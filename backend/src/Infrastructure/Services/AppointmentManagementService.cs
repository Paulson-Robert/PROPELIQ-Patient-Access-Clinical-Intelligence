using Application.EventHandlers;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Handles appointment cancellation and reschedule workflows with transactional
/// slot state updates and downstream event publication.
/// </summary>
public sealed class AppointmentManagementService : IAppointmentManagementService
{
    private readonly ApplicationDbContext _db;
    private readonly ISlotLockService _slotLock;
    private readonly IPublisher _publisher;
    private readonly ILogger<AppointmentManagementService> _logger;

    public AppointmentManagementService(
        ApplicationDbContext db,
        ISlotLockService slotLock,
        IPublisher publisher,
        ILogger<AppointmentManagementService> logger)
    {
        _db = db;
        _slotLock = slotLock;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<AppointmentMutationResult> CancelAsync(
        Guid appointmentId,
        Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(
                a => a.AppointmentId == appointmentId
                  && a.PatientId == patientUserId
                  && a.Status == AppointmentStatus.Scheduled,
                cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "Appointment not found or cannot be cancelled.",
                FailureCode: "NOT_FOUND");
        }

        var slot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == appointment.SlotId, cancellationToken)
            .ConfigureAwait(false);

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        try
        {
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = now;

            if (slot is not null)
            {
                slot.IsAvailable = true;
                slot.IsLocked = false;
                slot.LockExpiry = null;
                slot.Version++;
            }

            await EnqueueCalendarNotificationIfEnabledAsync(
                appointment.AppointmentId,
                patientUserId,
                NotificationType.Cancellation,
                now,
                cancellationToken)
                .ConfigureAwait(false);

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            await _publisher
                .Publish(new SlotCancelledNotification(appointment.SlotId), cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Appointment cancelled. AppointmentId={AppointmentId}, PatientUserId={PatientUserId}, FreedSlotId={FreedSlotId}.",
                appointment.AppointmentId,
                patientUserId,
                appointment.SlotId);

            return new AppointmentMutationResult(
                Success: true,
                Appointment: new AppointmentMutationDto(
                    appointment.AppointmentId,
                    appointment.SlotId,
                    nameof(AppointmentStatus.Cancelled),
                    appointment.UpdatedAt),
                FailureReason: null,
                FailureCode: null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning(ex,
                "Cancellation concurrency conflict. AppointmentId={AppointmentId}.",
                appointmentId);

            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "Appointment changed during cancellation. Please refresh and try again.",
                FailureCode: "CONFLICT");
        }
    }

    public async Task<AppointmentMutationResult> RescheduleAsync(
        Guid appointmentId,
        Guid newSlotId,
        string lockToken,
        Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        var lockValid = await _slotLock
            .ValidateAsync(newSlotId, lockToken, cancellationToken)
            .ConfigureAwait(false);

        if (!lockValid)
        {
            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "Slot hold expired — please select again.",
                FailureCode: "LOCK_EXPIRED");
        }

        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(
                a => a.AppointmentId == appointmentId
                  && a.PatientId == patientUserId
                  && a.Status == AppointmentStatus.Scheduled,
                cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            _ = _slotLock.ReleaseAsync(newSlotId, lockToken, CancellationToken.None);
            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "Appointment not found or cannot be rescheduled.",
                FailureCode: "NOT_FOUND");
        }

        if (appointment.SlotId == newSlotId)
        {
            _ = _slotLock.ReleaseAsync(newSlotId, lockToken, CancellationToken.None);
            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "You already have this slot.",
                FailureCode: "SAME_SLOT");
        }

        var previousSlotId = appointment.SlotId;

        var previousSlot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == previousSlotId, cancellationToken)
            .ConfigureAwait(false);

        var newSlot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == newSlotId, cancellationToken)
            .ConfigureAwait(false);

        if (newSlot is null || !newSlot.IsAvailable)
        {
            _ = _slotLock.ReleaseAsync(newSlotId, lockToken, CancellationToken.None);
            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "This slot is no longer available.",
                FailureCode: "SLOT_UNAVAILABLE");
        }

        await using var transaction = await _db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        try
        {
            appointment.SlotId = newSlotId;
            appointment.UpdatedAt = now;

            if (previousSlot is not null)
            {
                previousSlot.IsAvailable = true;
                previousSlot.IsLocked = false;
                previousSlot.LockExpiry = null;
                previousSlot.Version++;
            }

            newSlot.IsAvailable = false;
            newSlot.IsLocked = false;
            newSlot.LockExpiry = null;
            newSlot.Version++;

            await EnqueueCalendarNotificationIfEnabledAsync(
                appointment.AppointmentId,
                patientUserId,
                NotificationType.SlotSwap,
                now,
                cancellationToken)
                .ConfigureAwait(false);

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            _ = _slotLock.ReleaseAsync(newSlotId, lockToken, CancellationToken.None);

            await _publisher
                .Publish(new SlotCancelledNotification(previousSlotId), cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Appointment rescheduled. AppointmentId={AppointmentId}, PatientUserId={PatientUserId}, PreviousSlotId={PreviousSlotId}, NewSlotId={NewSlotId}.",
                appointment.AppointmentId,
                patientUserId,
                previousSlotId,
                newSlotId);

            return new AppointmentMutationResult(
                Success: true,
                Appointment: new AppointmentMutationDto(
                    appointment.AppointmentId,
                    appointment.SlotId,
                    nameof(AppointmentStatus.Scheduled),
                    appointment.UpdatedAt),
                FailureReason: null,
                FailureCode: null);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _ = _slotLock.ReleaseAsync(newSlotId, lockToken, CancellationToken.None);

            _logger.LogWarning(ex,
                "Reschedule concurrency conflict. AppointmentId={AppointmentId}, PreviousSlotId={PreviousSlotId}, NewSlotId={NewSlotId}. Old appointment preserved.",
                appointmentId,
                previousSlotId,
                newSlotId);

            return new AppointmentMutationResult(
                Success: false,
                Appointment: null,
                FailureReason: "This slot is no longer available.",
                FailureCode: "SLOT_UNAVAILABLE");
        }
    }

    private async Task EnqueueCalendarNotificationIfEnabledAsync(
        Guid appointmentId,
        Guid patientUserId,
        NotificationType notificationType,
        DateTime createdAt,
        CancellationToken cancellationToken)
    {
        var hasCalendarSync = await _db.CalendarSyncs
            .AnyAsync(c => c.UserId == patientUserId && c.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (!hasCalendarSync)
        {
            return;
        }

        _db.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            AppointmentId = appointmentId,
            PatientId = patientUserId,
            Channel = NotificationChannel.Email,
            NotificationType = notificationType,
            Status = NotificationStatus.Queued,
            RetryCount = 0,
            CreatedAt = createdAt,
        });
    }
}
