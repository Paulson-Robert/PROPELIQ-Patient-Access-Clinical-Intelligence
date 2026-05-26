using Application.Queries;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Handlers;

/// <summary>
/// Executes the platform metrics aggregation query (US_046 AC-01, AC-02).
///
/// Aggregation strategy:
///   • KPI summary — four scalar queries, each AsNoTracking and bounded by the date range.
///   • DailyVolume  — GroupBy on Appointment.CreatedAt (year/month/day) pushed to the DB.
///   • StatusBreakdown — GroupBy on Appointment.Status pushed to the DB.
///   • ConfidenceTrend — GroupBy on ClinicalDocument.UploadedAt (year/month/day) joined to
///     ExtractedDataRecords; confidence averaging pushed to the DB.
///   • Week/Month bucketing is done in-memory after fetching day-level DB groups to avoid
///     database-specific date functions and keep queries portable.
///
/// Edge case (large date range / pre-aggregated materialized data):
///   For YearToDate the query set is bounded and the DB GroupBy limits returned rows.
///   A materialized view or summary table can be added later as an optimisation once
///   query latency is measured against production data volumes.
/// </summary>
public sealed class GetPlatformMetricsQueryHandler
    : IRequestHandler<GetPlatformMetricsQuery, PlatformMetricsResult>
{
    private readonly ApplicationDbContext _dbContext;

    public GetPlatformMetricsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlatformMetricsResult> Handle(
        GetPlatformMetricsQuery request,
        CancellationToken cancellationToken)
    {
        var (fromUtc, toUtc) = ResolveWindow(request.Range);

        // DbContext is not thread-safe: run each query sequentially to avoid
        // "A second operation was started on this context" concurrency errors.
        var summary = await BuildSummaryAsync(fromUtc, toUtc, cancellationToken).ConfigureAwait(false);
        var rawVolume = await BuildDailyVolumeAsync(fromUtc, toUtc, cancellationToken).ConfigureAwait(false);
        var statusBreakdown = await BuildStatusBreakdownAsync(fromUtc, toUtc, cancellationToken).ConfigureAwait(false);
        var rawConfidence = await BuildConfidenceTrendAsync(fromUtc, toUtc, cancellationToken).ConfigureAwait(false);

        var volumePoints = ApplyGroupBy(rawVolume, request.GroupBy);
        var confidenceGrouped = ApplyConfidenceGroupBy(rawConfidence, request.GroupBy);

        return new PlatformMetricsResult(
            summary,
            volumePoints,
            statusBreakdown,
            confidenceGrouped);
    }

    // -------------------------------------------------------------------------
    // Date window
    // -------------------------------------------------------------------------

    internal static (DateTime From, DateTime To) ResolveWindow(MetricsDateRange range)
    {
        var today = DateTime.UtcNow.Date;
        var from = range switch
        {
            MetricsDateRange.Last7Days => today.AddDays(-7),
            MetricsDateRange.Last30Days => today.AddDays(-30),
            MetricsDateRange.Last90Days => today.AddDays(-90),
            MetricsDateRange.YearToDate => new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => today.AddDays(-30),
        };
        // Include the full current day
        var to = today.AddDays(1).AddTicks(-1);
        return (from, to);
    }

    // -------------------------------------------------------------------------
    // KPI summary
    // -------------------------------------------------------------------------

    private async Task<MetricsSummaryDto> BuildSummaryAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct)
    {
        // AC-01: Total appointments in window
        var totalAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync(a => a.CreatedAt >= fromUtc && a.CreatedAt <= toUtc, ct)
            .ConfigureAwait(false);

        // AC-01: Average wait time — appointments that completed and recorded an arrival
        var completedWithArrival = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a =>
                a.CreatedAt >= fromUtc &&
                a.CreatedAt <= toUtc &&
                a.Status == AppointmentStatus.Completed &&
                a.ArrivalTimestamp.HasValue)
            .Select(a => new { a.ArrivalTimestamp, a.UpdatedAt })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var avgWaitMin = completedWithArrival.Count > 0
            ? completedWithArrival.Average(a => (a.UpdatedAt - a.ArrivalTimestamp!.Value).TotalMinutes)
            : 0d;

        // AC-01: No-show rate — noShows / (completed + cancelled + no-show) * 100
        var statusCounts = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.CreatedAt >= fromUtc && a.CreatedAt <= toUtc)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var noShows = statusCounts.FirstOrDefault(s => s.Status == AppointmentStatus.NoShow)?.Count ?? 0;
        var closedTotal = statusCounts
            .Where(s => s.Status is AppointmentStatus.Completed
                         or AppointmentStatus.Cancelled
                         or AppointmentStatus.NoShow)
            .Sum(s => s.Count);

        var noShowRate = closedTotal > 0 ? noShows * 100d / closedTotal : 0d;

        // AC-01: Active users — platform-wide active account count
        var activeUsers = await _dbContext.Users
            .AsNoTracking()
            .CountAsync(u => u.IsActive, ct)
            .ConfigureAwait(false);

        return new MetricsSummaryDto(
            totalAppointments,
            Math.Round(avgWaitMin, 1),
            Math.Round(noShowRate, 1),
            activeUsers);
    }

    // -------------------------------------------------------------------------
    // Daily appointment volume (AC-02)
    // -------------------------------------------------------------------------

    private async Task<List<(int Year, int Month, int Day, int Count)>> BuildDailyVolumeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct)
    {
        var rows = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.CreatedAt >= fromUtc && a.CreatedAt <= toUtc)
            .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month, a.CreatedAt.Day })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                Count = g.Count(),
            })
            .OrderBy(r => r.Year).ThenBy(r => r.Month).ThenBy(r => r.Day)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return rows.Select(r => (r.Year, r.Month, r.Day, r.Count)).ToList();
    }

    // -------------------------------------------------------------------------
    // Appointment status breakdown (AC-01)
    // -------------------------------------------------------------------------

    private async Task<IReadOnlyList<StatusBreakdownDto>> BuildStatusBreakdownAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct)
    {
        var rows = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.CreatedAt >= fromUtc && a.CreatedAt <= toUtc)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return rows
            .OrderByDescending(r => r.Count)
            .Select(r => new StatusBreakdownDto(r.Status.ToString(), r.Count))
            .ToList();
    }

    // -------------------------------------------------------------------------
    // AI confidence trend (AC-02)
    // -------------------------------------------------------------------------

    private async Task<List<(int Year, int Month, int Day, double AvgConfidence)>> BuildConfidenceTrendAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct)
    {
        var rows = await (
            from record in _dbContext.ExtractedDataRecords.AsNoTracking()
            join doc in _dbContext.ClinicalDocuments on record.DocumentId equals doc.DocumentId
            where doc.UploadedAt >= fromUtc && doc.UploadedAt <= toUtc
            group record by new { doc.UploadedAt.Year, doc.UploadedAt.Month, doc.UploadedAt.Day } into g
            orderby g.Key.Year, g.Key.Month, g.Key.Day
            select new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                AvgConfidence = g.Average(r => (double)r.Confidence) * 100d,
            }
        ).ToListAsync(ct).ConfigureAwait(false);

        return rows.Select(r => (r.Year, r.Month, r.Day, r.AvgConfidence)).ToList();
    }

    // -------------------------------------------------------------------------
    // In-memory GroupBy helpers
    // -------------------------------------------------------------------------

    private static IReadOnlyList<TrendPointDto> ApplyGroupBy(
        List<(int Year, int Month, int Day, int Count)> daily,
        MetricsGroupBy groupBy)
    {
        return groupBy switch
        {
            MetricsGroupBy.Day => daily
                .Select(r => new TrendPointDto(
                    $"{r.Month:D2}/{r.Day:D2}",
                    r.Count))
                .ToList(),

            MetricsGroupBy.Week => daily
                .GroupBy(r => new { r.Year, Week = GetIsoWeek(r.Year, r.Month, r.Day) })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                .Select(g => new TrendPointDto($"Wk {g.Key.Week}", g.Sum(r => r.Count)))
                .ToList(),

            MetricsGroupBy.Month => daily
                .GroupBy(r => new { r.Year, r.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new TrendPointDto(
                    new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    g.Sum(r => r.Count)))
                .ToList(),

            _ => daily
                .Select(r => new TrendPointDto($"{r.Month:D2}/{r.Day:D2}", r.Count))
                .ToList(),
        };
    }

    private static IReadOnlyList<ConfidenceTrendPointDto> ApplyConfidenceGroupBy(
        List<(int Year, int Month, int Day, double AvgConfidence)> daily,
        MetricsGroupBy groupBy)
    {
        return groupBy switch
        {
            MetricsGroupBy.Day => daily
                .Select(r => new ConfidenceTrendPointDto(
                    $"{r.Month:D2}/{r.Day:D2}",
                    Math.Round(r.AvgConfidence, 1)))
                .ToList(),

            MetricsGroupBy.Week => daily
                .GroupBy(r => new { r.Year, Week = GetIsoWeek(r.Year, r.Month, r.Day) })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                .Select(g => new ConfidenceTrendPointDto(
                    $"Wk {g.Key.Week}",
                    Math.Round(g.Average(r => r.AvgConfidence), 1)))
                .ToList(),

            MetricsGroupBy.Month => daily
                .GroupBy(r => new { r.Year, r.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new ConfidenceTrendPointDto(
                    new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Math.Round(g.Average(r => r.AvgConfidence), 1)))
                .ToList(),

            _ => daily
                .Select(r => new ConfidenceTrendPointDto(
                    $"{r.Month:D2}/{r.Day:D2}",
                    Math.Round(r.AvgConfidence, 1)))
                .ToList(),
        };
    }

    /// <summary>
    /// Returns ISO 8601 week-of-year for the given date.
    /// Uses <see cref="System.Globalization.ISOWeek"/> to avoid off-by-one errors at year boundaries.
    /// </summary>
    private static int GetIsoWeek(int year, int month, int day) =>
        System.Globalization.ISOWeek.GetWeekOfYear(new DateTime(year, month, day));
}
