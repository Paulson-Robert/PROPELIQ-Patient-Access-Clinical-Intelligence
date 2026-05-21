using MediatR;

namespace Application.Queries;

// ---------------------------------------------------------------------------
// Enums
// ---------------------------------------------------------------------------

/// <summary>Pre-defined date windows for the platform metrics query (US_046 AC-01).</summary>
public enum MetricsDateRange
{
    Last7Days = 0,
    Last30Days = 1,
    Last90Days = 2,
    YearToDate = 3,
}

/// <summary>Time granularity used to bucket trend data points (US_046 AC-02).</summary>
public enum MetricsGroupBy
{
    Day = 0,
    Week = 1,
    Month = 2,
}

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Aggregate KPI snapshot covering the requested date window (US_046 AC-01).</summary>
public sealed record MetricsSummaryDto(
    int TotalAppointments,
    double AvgWaitTimeMinutes,
    double NoShowRatePercent,
    int ActiveUsers);

/// <summary>Single data point in a time-series trend chart (US_046 AC-02).</summary>
public sealed record TrendPointDto(
    /// <summary>Human-readable period label (e.g., "05/01", "Wk 18", "May 2026").</summary>
    string Period,
    int Count);

/// <summary>Appointment count broken down by status.</summary>
public sealed record StatusBreakdownDto(string Status, int Count);

/// <summary>Average AI code-mapping confidence score for a time bucket (US_046 AC-02).</summary>
public sealed record ConfidenceTrendPointDto(string Period, double AvgConfidencePercent);

/// <summary>Full platform metrics result returned by <see cref="GetPlatformMetricsQuery"/>.</summary>
public sealed record PlatformMetricsResult(
    MetricsSummaryDto Summary,
    IReadOnlyList<TrendPointDto> DailyVolume,
    IReadOnlyList<StatusBreakdownDto> StatusBreakdown,
    IReadOnlyList<ConfidenceTrendPointDto> ConfidenceTrend);

// ---------------------------------------------------------------------------
// Query
// ---------------------------------------------------------------------------

/// <summary>
/// Returns platform-wide KPIs and trend data for the specified date window (US_046 AC-01, AC-02).
/// </summary>
/// <param name="Range">Date window selecting which appointments/records to include.</param>
/// <param name="GroupBy">
/// Time granularity for trend buckets.  Defaults to <see cref="MetricsGroupBy.Day"/>.
/// </param>
public sealed record GetPlatformMetricsQuery(
    MetricsDateRange Range,
    MetricsGroupBy GroupBy = MetricsGroupBy.Day) : IRequest<PlatformMetricsResult>;
