using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Notifications;

/// <summary>
/// Delivers a single queued notification via Email or SMS.
/// Implements exponential backoff retry (max 3 total attempts) per AC-04.
/// Marks the notification Failed and alerts staff when all retries are exhausted (Edge Case).
/// Suppresses delivery when the appointment has been cancelled (Edge Case).
/// </summary>
public sealed class NotificationDeliveryService : INotificationDeliveryService
{
    private const int MaxAttempts = 3;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<NotificationDeliveryService> _logger;
    private readonly NotificationSettings _settings;

    public NotificationDeliveryService(
        IServiceScopeFactory scopeFactory,
        IBackgroundJobClient jobClient,
        ILogger<NotificationDeliveryService> logger,
        IOptions<NotificationSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _jobClient = jobClient;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public async Task DeliverAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();

        var notification = await db.Notifications
            .Include(n => n.Appointment)
                .ThenInclude(a => a.Slot)
            .Include(n => n.Appointment)
                .ThenInclude(a => a.Patient)
                    .ThenInclude(u => u.PatientProfile)
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken)
            .ConfigureAwait(false);

        if (notification is null)
        {
            _logger.LogWarning(
                "NotificationDeliveryService: Notification not found. NotificationId={NotificationId}.",
                notificationId);
            return;
        }

        // Edge case: suppress if appointment was cancelled after reminder was queued
        if (notification.Appointment.Status == AppointmentStatus.Cancelled)
        {
            notification.Status = NotificationStatus.Failed;
            notification.FailureReason = "Appointment was cancelled — reminder suppressed.";
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Reminder suppressed for cancelled appointment. NotificationId={NotificationId}, AppointmentId={AppointmentId}.",
                notificationId,
                notification.AppointmentId);
            return;
        }

        // Guard: all retries already exhausted from a prior run
        if (notification.RetryCount >= MaxAttempts)
        {
            await MarkFailedAndAlertStaffAsync(notification, db, emailService, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        notification.RetryCount++;
        notification.LastAttemptAt = DateTime.UtcNow;

        try
        {
            await SendAsync(notification, emailService, smsService, cancellationToken)
                .ConfigureAwait(false);

            notification.Status = NotificationStatus.Sent;
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Notification delivered. NotificationId={NotificationId}, Channel={Channel}, Attempt={Attempt}.",
                notificationId,
                notification.Channel,
                notification.RetryCount);
        }
        catch (Exception ex)
        {
            notification.FailureReason = ex.Message;

            _logger.LogWarning(
                ex,
                "Notification delivery failed. NotificationId={NotificationId}, Channel={Channel}, Attempt={Attempt}.",
                notificationId,
                notification.Channel,
                notification.RetryCount);

            if (notification.RetryCount >= MaxAttempts)
            {
                await MarkFailedAndAlertStaffAsync(notification, db, emailService, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            // Exponential backoff: 30 s, 60 s for retries 1 and 2
            var backoffDelay = TimeSpan.FromSeconds(30 * (int)Math.Pow(2, notification.RetryCount - 1));
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _jobClient.Schedule<INotificationDeliveryService>(
                s => s.DeliverAsync(notificationId, CancellationToken.None),
                backoffDelay);

            _logger.LogInformation(
                "Scheduled retry. NotificationId={NotificationId}, Attempt={Attempt}, DelaySeconds={DelaySeconds}.",
                notificationId,
                notification.RetryCount,
                backoffDelay.TotalSeconds);
        }
    }

    private static async Task SendAsync(
        Domain.Entities.Notification notification,
        IEmailService emailService,
        ISmsService smsService,
        CancellationToken cancellationToken)
    {
        var slot = notification.Appointment.Slot;
        var startTimeLocal = slot.StartTime.ToString("dddd, MMMM d 'at' h:mm tt");
        var providerName = slot.ProviderName;

        if (notification.Channel == NotificationChannel.Email)
        {
            var patientEmail = notification.Patient.Email;
            var (subject, body) = BuildEmailContent(notification.NotificationType, startTimeLocal, providerName, slot.Specialty);

            await emailService.SendConfirmationEmailAsync(
                patientEmail,
                subject,
                body,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        // SMS channel
        var phone = notification.Appointment.Patient.PatientProfile?.Phone;
        if (string.IsNullOrWhiteSpace(phone))
            throw new InvalidOperationException("Patient has no phone number on file for SMS reminder.");

        var smsBody = BuildSmsContent(notification.NotificationType, startTimeLocal, providerName);
        await smsService.SendReminderAsync(phone, smsBody, cancellationToken).ConfigureAwait(false);
    }

    private static (string subject, string body) BuildEmailContent(
        NotificationType type,
        string startTime,
        string providerName,
        string specialty)
    {
        return type switch
        {
            NotificationType.Reminder24Hour =>
                ($"Reminder: Appointment tomorrow with {providerName}",
                 $"This is a reminder that your appointment with {providerName} ({specialty}) is scheduled for {startTime}. " +
                 "If you need to reschedule, please contact us as soon as possible."),

            NotificationType.Reminder2Hour =>
                ($"Reminder: Appointment in 2 hours with {providerName}",
                 $"Your appointment with {providerName} ({specialty}) is in approximately 2 hours at {startTime}. " +
                 "Please ensure you arrive on time."),

            _ =>
                ("Appointment Reminder",
                 $"This is a reminder about your upcoming appointment with {providerName} ({specialty}) at {startTime}."),
        };
    }

    private static string BuildSmsContent(NotificationType type, string startTime, string providerName)
    {
        return type switch
        {
            NotificationType.Reminder24Hour =>
                $"Reminder: Appt with {providerName} tomorrow at {startTime}. Reply STOP to opt out.",

            NotificationType.Reminder2Hour =>
                $"Reminder: Appt with {providerName} in ~2 hrs at {startTime}. Reply STOP to opt out.",

            _ =>
                $"Reminder: Upcoming appt with {providerName} at {startTime}.",
        };
    }

    private async Task MarkFailedAndAlertStaffAsync(
        Domain.Entities.Notification notification,
        ApplicationDbContext db,
        IEmailService emailService,
        CancellationToken cancellationToken)
    {
        notification.Status = NotificationStatus.Failed;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogError(
            "All delivery attempts exhausted. NotificationId={NotificationId}, AppointmentId={AppointmentId}, Channel={Channel}, Reason={Reason}.",
            notification.NotificationId,
            notification.AppointmentId,
            notification.Channel,
            notification.FailureReason);

        if (string.IsNullOrWhiteSpace(_settings.StaffAlertEmail))
            return;

        try
        {
            await emailService.SendConfirmationEmailAsync(
                _settings.StaffAlertEmail,
                $"[ACTION REQUIRED] Reminder delivery failed — AppointmentId {notification.AppointmentId}",
                $"Notification {notification.NotificationId} for appointment {notification.AppointmentId} " +
                $"failed after {MaxAttempts} attempts via {notification.Channel}. " +
                $"Last failure: {notification.FailureReason}. Manual follow-up required.",
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception alertEx)
        {
            _logger.LogError(
                alertEx,
                "Failed to send staff alert email. NotificationId={NotificationId}.",
                notification.NotificationId);
        }
    }
}
