using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that scans upcoming appointments and ensures reminder notifications
/// are scheduled for both the 24 h (AC-01) and 2 h (AC-02) delivery windows.
/// Acts as a safety net for appointments that were booked before the reminder pipeline existed
/// and for any bookings where reminder scheduling was skipped at confirmation time.
/// </summary>
public sealed class ScheduleRemindersJob
{
    /// <summary>Scan window — appointments starting within this horizon are candidates.</summary>
    private static readonly TimeSpan ScanHorizon = TimeSpan.FromHours(26);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduleRemindersJob> _logger;

    public ScheduleRemindersJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduleRemindersJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pipeline = scope.ServiceProvider.GetRequiredService<IReminderPipelineService>();

        var now = DateTime.UtcNow;
        var scanUntil = now.Add(ScanHorizon);

        // Load appointment IDs for all scheduled appointments within the scan horizon
        // that do not yet have any non-failed reminder notification scheduled.
        var appointmentIds = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Scheduled
                     && a.Slot.StartTime > now
                     && a.Slot.StartTime <= scanUntil
                     && !a.Notifications.Any(n =>
                            (n.NotificationType == NotificationType.Reminder24Hour
                             || n.NotificationType == NotificationType.Reminder2Hour)
                         && n.Status != NotificationStatus.Failed))
            .Select(a => a.AppointmentId)
            .ToListAsync()
            .ConfigureAwait(false);

        _logger.LogInformation(
            "ScheduleRemindersJob started. CandidateAppointments={Count}, ScanUntil={ScanUntil:O}.",
            appointmentIds.Count,
            scanUntil);

        foreach (var appointmentId in appointmentIds)
        {
            await pipeline
                .ScheduleAppointmentRemindersAsync(appointmentId)
                .ConfigureAwait(false);
        }

        _logger.LogInformation(
            "ScheduleRemindersJob completed. Processed={Count}.",
            appointmentIds.Count);
    }
}
