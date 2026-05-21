using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

/// <summary>
/// Orchestrates the appointment reminder pipeline (US_027).
/// Creates Notification records and schedules Hangfire delivery jobs for the
/// 24 h reminder (AC-01), 2 h reminder (AC-02), and the additional SMS escalation
/// for High-risk patients (AC-03).
/// </summary>
public sealed class ReminderPipelineService : IReminderPipelineService
{
    private readonly ApplicationDbContext _db;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<ReminderPipelineService> _logger;

    public ReminderPipelineService(
        ApplicationDbContext db,
        IBackgroundJobClient jobClient,
        ILogger<ReminderPipelineService> logger)
    {
        _db = db;
        _jobClient = jobClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ScheduleAppointmentRemindersAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _db.Appointments
            .AsNoTracking()
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            _logger.LogWarning(
                "ReminderPipelineService: Appointment not found. AppointmentId={AppointmentId}.",
                appointmentId);
            return;
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            _logger.LogInformation(
                "Skipping reminder scheduling for cancelled appointment. AppointmentId={AppointmentId}.",
                appointmentId);
            return;
        }

        var now = DateTime.UtcNow;
        var startTime = appointment.Slot.StartTime;

        await Schedule24HourReminderAsync(appointment, startTime, now, cancellationToken)
            .ConfigureAwait(false);

        await Schedule2HourReminderAsync(appointment, startTime, now, cancellationToken)
            .ConfigureAwait(false);

        if (appointment.NoShowRiskTier == NoShowRiskTier.High)
        {
            await ScheduleHighRiskSmsEscalationAsync(appointment, startTime, now, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task Schedule24HourReminderAsync(
        Appointment appointment,
        DateTime startTime,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var alreadyScheduled = await ReminderExistsAsync(
                appointment.AppointmentId,
                NotificationType.Reminder24Hour,
                cancellationToken)
            .ConfigureAwait(false);

        if (alreadyScheduled)
            return;

        var deliverAt = startTime.AddHours(-24);

        // Skip if the 24 h window has already passed and less than 2 h remain (2 h reminder covers it)
        if (deliverAt <= now && startTime.Subtract(now).TotalHours < 2)
            return;

        var notification = CreateNotification(
            appointment,
            NotificationChannel.Email,
            NotificationType.Reminder24Hour);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var delay = deliverAt > now ? deliverAt - now : TimeSpan.Zero;
        _jobClient.Schedule<INotificationDeliveryService>(
            s => s.DeliverAsync(notification.NotificationId, CancellationToken.None),
            delay);

        _logger.LogInformation(
            "Scheduled 24 h reminder. NotificationId={NotificationId}, AppointmentId={AppointmentId}, DeliverAt={DeliverAt:O}.",
            notification.NotificationId,
            appointment.AppointmentId,
            now.Add(delay));
    }

    private async Task Schedule2HourReminderAsync(
        Appointment appointment,
        DateTime startTime,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var alreadyScheduled = await ReminderExistsAsync(
                appointment.AppointmentId,
                NotificationType.Reminder2Hour,
                cancellationToken)
            .ConfigureAwait(false);

        if (alreadyScheduled)
            return;

        var deliverAt = startTime.AddHours(-2);

        // Skip if the appointment has already started or the 2 h window has passed
        if (startTime <= now || deliverAt <= now.AddMinutes(-5))
            return;

        var notification = CreateNotification(
            appointment,
            NotificationChannel.Email,
            NotificationType.Reminder2Hour);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var delay = deliverAt > now ? deliverAt - now : TimeSpan.Zero;
        _jobClient.Schedule<INotificationDeliveryService>(
            s => s.DeliverAsync(notification.NotificationId, CancellationToken.None),
            delay);

        _logger.LogInformation(
            "Scheduled 2 h reminder. NotificationId={NotificationId}, AppointmentId={AppointmentId}, DeliverAt={DeliverAt:O}.",
            notification.NotificationId,
            appointment.AppointmentId,
            now.Add(delay));
    }

    private async Task ScheduleHighRiskSmsEscalationAsync(
        Appointment appointment,
        DateTime startTime,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var alreadyScheduled = await _db.Notifications
            .AsNoTracking()
            .AnyAsync(
                n => n.AppointmentId == appointment.AppointmentId
                  && n.Channel == NotificationChannel.SMS
                  && n.NotificationType == NotificationType.Reminder2Hour
                  && n.Status != NotificationStatus.Failed,
                cancellationToken)
            .ConfigureAwait(false);

        if (alreadyScheduled)
            return;

        var deliverAt = startTime.AddHours(-2);

        if (startTime <= now || deliverAt <= now.AddMinutes(-5))
            return;

        var notification = CreateNotification(
            appointment,
            NotificationChannel.SMS,
            NotificationType.Reminder2Hour);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var delay = deliverAt > now ? deliverAt - now : TimeSpan.Zero;
        _jobClient.Schedule<INotificationDeliveryService>(
            s => s.DeliverAsync(notification.NotificationId, CancellationToken.None),
            delay);

        _logger.LogInformation(
            "Scheduled High-risk SMS escalation. NotificationId={NotificationId}, AppointmentId={AppointmentId}.",
            notification.NotificationId,
            appointment.AppointmentId);
    }

    private Task<bool> ReminderExistsAsync(
        Guid appointmentId,
        NotificationType type,
        CancellationToken cancellationToken)
    {
        return _db.Notifications
            .AsNoTracking()
            .AnyAsync(
                n => n.AppointmentId == appointmentId
                  && n.NotificationType == type
                  && n.Status != NotificationStatus.Failed,
                cancellationToken);
    }

    private static Notification CreateNotification(
        Appointment appointment,
        NotificationChannel channel,
        NotificationType type)
    {
        return new Notification
        {
            NotificationId = Guid.NewGuid(),
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            Channel = channel,
            NotificationType = type,
            Status = NotificationStatus.Queued,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
