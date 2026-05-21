using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// EF Core implementation of the same-day queue operations (US_024).
/// </summary>
public sealed class QueueService : IQueueService
{
    // Assumed average consultation time used for estimated wait calculation.
    // Decision logged: no consultation duration spec in FR-009 — using 20 min default.
    private const int AverageConsultationMinutes = 20;

    private readonly ApplicationDbContext _db;
    private readonly ILogger<QueueService> _logger;

    public QueueService(ApplicationDbContext db, ILogger<QueueService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<QueueResponseDto> GetTodayQueueAsync(
        CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var rows = await _db.Appointments
            .AsNoTracking()
            .Where(a =>
                a.Slot.StartTime >= todayUtc &&
                a.Slot.StartTime < tomorrowUtc &&
                a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.QueuePosition == null)   // nulls last
            .ThenBy(a => a.QueuePosition)
            .ThenBy(a => a.Slot.StartTime)
            .Select(a => new
            {
                a.AppointmentId,
                a.PatientId,
                PatientFirstName = a.Patient.PatientProfile!.FirstName,
                PatientLastName = a.Patient.PatientProfile!.LastName,
                a.Slot.StartTime,
                a.Slot.ProviderName,
                a.Status,
                a.BookingType,
                a.ArrivalTimestamp,
                a.NoShowRiskTier,
                a.QueuePosition,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var entries = rows
            .Select((row, index) =>
            {
                var position = index + 1;
                var status = MapStatus(row.Status, row.BookingType);
                var riskLevel = MapRisk(row.NoShowRiskTier);
                var wait = position > 1 ? (position - 1) * AverageConsultationMinutes : 0;

                return new QueueEntryDto(
                    AppointmentId: row.AppointmentId,
                    Position: position,
                    PatientId: row.PatientId,
                    PatientName: $"{row.PatientFirstName} {row.PatientLastName}".Trim(),
                    AppointmentTime: row.StartTime.ToString("HH:mm"),
                    ProviderName: row.ProviderName,
                    Status: status,
                    BookingType: row.BookingType == BookingType.WalkIn ? "WalkIn" : "Scheduled",
                    ArrivalTimestamp: row.ArrivalTimestamp?.ToString("O"),
                    RiskLevel: riskLevel,
                    EstimatedWaitMinutes: wait);
            })
            .ToList();

        var summary = BuildSummary(entries);

        return new QueueResponseDto(entries, summary);
    }

    /// <inheritdoc/>
    public async Task<MarkArrivedResultDto> MarkArrivedAsync(
        Guid appointmentId,
        Guid staffUserId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(
                a => a.AppointmentId == appointmentId,
                cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            throw new InvalidOperationException($"Appointment {appointmentId} not found.");
        }

        // Idempotent: already arrived — return existing timestamp
        if (appointment.Status == AppointmentStatus.Arrived && appointment.ArrivalTimestamp.HasValue)
        {
            return new MarkArrivedResultDto(
                appointment.AppointmentId,
                appointment.ArrivalTimestamp.Value.ToString("O"));
        }

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Cannot mark appointment {appointmentId} as arrived — current status: {appointment.Status}.");
        }

        var now = DateTime.UtcNow;
        appointment.Status = AppointmentStatus.Arrived;
        appointment.ArrivalTimestamp = now;
        appointment.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Appointment marked arrived. AppointmentId={AppointmentId}, StaffUserId={StaffUserId}, ArrivalTimestamp={ArrivalTimestamp}.",
            appointmentId,
            staffUserId,
            now);

        return new MarkArrivedResultDto(appointmentId, now.ToString("O"));
    }

    /// <inheritdoc/>
    public async Task<ReorderQueueResultDto> ReorderAsync(
        Guid appointmentId,
        int newPosition,
        string reason,
        Guid staffUserId,
        CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        // Load the full today queue as tracked entities for reordering
        var todayAppointments = await _db.Appointments
            .Where(a =>
                a.Slot.StartTime >= todayUtc &&
                a.Slot.StartTime < tomorrowUtc &&
                a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.QueuePosition == null)
            .ThenBy(a => a.QueuePosition)
            .ThenBy(a => a.Slot.StartTime)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var target = todayAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
        if (target is null)
        {
            throw new InvalidOperationException(
                $"Appointment {appointmentId} is not in today's queue.");
        }

        // Remove target from current list and insert at new position (1-based, clamped)
        todayAppointments.Remove(target);
        var insertIndex = Math.Clamp(newPosition - 1, 0, todayAppointments.Count);
        todayAppointments.Insert(insertIndex, target);

        var now = DateTime.UtcNow;

        // Update positions and timestamps on all affected rows
        for (var i = 0; i < todayAppointments.Count; i++)
        {
            todayAppointments[i].QueuePosition = i + 1;
            todayAppointments[i].UpdatedAt = now;
        }

        // Append-only audit log entry for the reorder action
        _db.AuditLogs.Add(new AuditLog
        {
            Timestamp = now,
            ActorUserId = staffUserId,
            ActorRole = "Staff",
            ActionType = "QueueReorder",
            ResourceType = "Appointment",
            ResourceId = appointmentId.ToString(),
            Details = $"{{\"newPosition\":{newPosition},\"reason\":{System.Text.Json.JsonSerializer.Serialize(reason)}}}",
        });

        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(
                ex,
                "Concurrent queue reorder conflict. AppointmentId={AppointmentId}, StaffUserId={StaffUserId}.",
                appointmentId,
                staffUserId);

            throw new QueueConcurrencyException();
        }

        _logger.LogInformation(
            "Queue reordered. AppointmentId={AppointmentId}, NewPosition={NewPosition}, StaffUserId={StaffUserId}.",
            appointmentId,
            newPosition,
            staffUserId);

        return await GetTodayReorderResultAsync(cancellationToken).ConfigureAwait(false);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<ReorderQueueResultDto> GetTodayReorderResultAsync(
        CancellationToken cancellationToken)
    {
        var response = await GetTodayQueueAsync(cancellationToken).ConfigureAwait(false);
        return new ReorderQueueResultDto(response.Entries, response.Summary);
    }

    private static string MapStatus(AppointmentStatus status, BookingType bookingType)
        => status switch
        {
            AppointmentStatus.Arrived => "Arrived",
            AppointmentStatus.Completed => "Completed",
            AppointmentStatus.Cancelled => "Cancelled",
            AppointmentStatus.Scheduled when bookingType == BookingType.WalkIn => "Waiting",
            _ => "Scheduled",
        };

    private static string MapRisk(NoShowRiskTier? tier)
        => tier switch
        {
            NoShowRiskTier.High => "High",
            NoShowRiskTier.Medium => "Medium",
            _ => "Low",
        };

    private static QueueSummaryDto BuildSummary(IReadOnlyList<QueueEntryDto> entries)
    {
        var arrivedCount = entries.Count(e => e.Status == "Arrived");
        var walkInCount = entries.Count(e => e.BookingType == "WalkIn");
        var avgWait = entries.Count == 0
            ? 0
            : (int)Math.Round(entries.Average(e => e.EstimatedWaitMinutes));

        return new QueueSummaryDto(
            TotalInQueue: entries.Count,
            WalkInCount: walkInCount,
            ArrivedCount: arrivedCount,
            AvgWaitMinutes: avgWait);
    }
}
