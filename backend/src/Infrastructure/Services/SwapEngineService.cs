using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// FCFS swap engine: dequeues the earliest waiting candidate for a freed slot,
/// acquires a Redis lock, executes the booking swap, and dispatches notifications
/// to both the patient (email + SMS) and staff (AC-02, AC-03, AC-04, AC-05).
/// </summary>
public sealed class SwapEngineService : ISwapEngineService
{
    private readonly ApplicationDbContext _db;
    private readonly ISwapQueueService _swapQueue;
    private readonly ISlotLockService _slotLock;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<SwapEngineService> _logger;

    public SwapEngineService(
        ApplicationDbContext db,
        ISwapQueueService swapQueue,
        ISlotLockService slotLock,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<SwapEngineService> logger)
    {
        _db = db;
        _swapQueue = swapQueue;
        _slotLock = slotLock;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SwapEngineResult> ProcessFreedSlotAsync(
        Guid freedSlotId,
        CancellationToken cancellationToken = default)
    {
        // Retrieve FCFS-ordered candidates from Redis (AC-03)
        var candidates = await _swapQueue
            .GetWaitingEntriesAsync(freedSlotId, cancellationToken)
            .ConfigureAwait(false);

        if (candidates.Count == 0)
        {
            return new SwapEngineResult(
                SwapExecuted: false,
                WinningAppointmentId: null,
                WinningPatientUserId: null,
                FailureReason: "No waiting preferences for slot.");
        }

        // Acquire Redis lock on the freed slot to prevent concurrent swap attempts (Edge Case: rapid toggling)
        var lockResult = await _slotLock.AcquireAsync(freedSlotId, cancellationToken).ConfigureAwait(false);
        if (lockResult is null)
        {
            _logger.LogWarning(
                "Swap engine could not acquire lock on freed slot {FreedSlotId}. Another process may be handling it.",
                freedSlotId);
            return new SwapEngineResult(
                SwapExecuted: false,
                WinningAppointmentId: null,
                WinningPatientUserId: null,
                FailureReason: "Slot lock unavailable.");
        }

        try
        {
            return await ExecuteSwapAsync(freedSlotId, lockResult.LockToken, candidates, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            // Always release lock — fire-and-forget; TTL auto-expires as fallback
            _ = _slotLock.ReleaseAsync(freedSlotId, lockResult.LockToken, CancellationToken.None);
        }
    }

    private async Task<SwapEngineResult> ExecuteSwapAsync(
        Guid freedSlotId,
        string lockToken,
        IReadOnlyList<SwapQueueEntry> candidates,
        CancellationToken cancellationToken)
    {
        var freedSlot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == freedSlotId, cancellationToken)
            .ConfigureAwait(false);

        if (freedSlot is null || !freedSlot.IsAvailable)
        {
            _logger.LogWarning(
                "Freed slot {FreedSlotId} is no longer available when swap engine ran.",
                freedSlotId);
            return new SwapEngineResult(
                SwapExecuted: false,
                WinningAppointmentId: null,
                WinningPatientUserId: null,
                FailureReason: "Freed slot no longer available.");
        }

        // Iterate FCFS candidates until a valid swap is found (AC-03, AC-05)
        foreach (var candidate in candidates)
        {
            var result = await TrySwapCandidateAsync(
                candidate, freedSlot, lockToken, cancellationToken)
                .ConfigureAwait(false);

            if (result is not null)
                return result;
        }

        return new SwapEngineResult(
            SwapExecuted: false,
            WinningAppointmentId: null,
            WinningPatientUserId: null,
            FailureReason: "All candidates exhausted — no valid swap found.");
    }

    private async Task<SwapEngineResult?> TrySwapCandidateAsync(
        SwapQueueEntry candidate,
        AvailabilitySlot freedSlot,
        string lockToken,
        CancellationToken cancellationToken)
    {
        // Load appointment with navigation for patient data
        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(
                a => a.AppointmentId == candidate.AppointmentId,
                cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            _logger.LogWarning(
                "Swap candidate appointment {AppointmentId} not found. Skipping.",
                candidate.AppointmentId);
            await ExpireQueueEntryAsync(candidate, cancellationToken).ConfigureAwait(false);
            return null;
        }

        // Edge Case: patient cancelled appointment while queued → mark Expired and skip
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            _logger.LogInformation(
                "Appointment {AppointmentId} was cancelled while in swap queue. Expiring queue entry.",
                candidate.AppointmentId);
            await ExpireQueueEntryAsync(candidate, cancellationToken).ConfigureAwait(false);
            return null;
        }

        // Load original slot to release
        var originalSlot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == appointment.SlotId, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        // Execute the swap: reassign appointment, update slots, update queue status
        var originalSlotId = appointment.SlotId;
        appointment.SlotId = freedSlot.SlotId;
        appointment.UpdatedAt = now;

        // Mark freed slot as booked
        freedSlot.IsAvailable = false;
        freedSlot.IsLocked = false;
        freedSlot.LockExpiry = null;
        freedSlot.Version++;

        // Release the original slot back to availability
        if (originalSlot is not null)
        {
            originalSlot.IsAvailable = true;
            originalSlot.IsLocked = false;
            originalSlot.LockExpiry = null;
            originalSlot.Version++;
        }

        // Update queue entry to Swapped
        var queueEntry = await _db.PreferredSlotQueues
            .FirstOrDefaultAsync(
                q => q.AppointmentId == candidate.AppointmentId
                  && q.PreferredSlotId == freedSlot.SlotId
                  && q.Status == QueueStatus.Waiting,
                cancellationToken)
            .ConfigureAwait(false);

        if (queueEntry is not null)
            queueEntry.Status = QueueStatus.Swapped;

        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Optimistic concurrency conflict during swap for slot {FreedSlotId} / appointment {AppointmentId}. Skipping candidate.",
                freedSlot.SlotId, candidate.AppointmentId);
            return null;
        }

        // Remove from Redis queue — swap complete
        if (queueEntry is not null)
        {
            await _swapQueue.RemoveAsync(queueEntry.QueueId, freedSlot.SlotId, CancellationToken.None)
                .ConfigureAwait(false);
        }

        _logger.LogInformation(
            "Slot swap executed. AppointmentId={AppointmentId}, PatientUserId={PatientUserId}, OriginalSlotId={OriginalSlotId}, NewSlotId={NewSlotId}.",
            appointment.AppointmentId, candidate.PatientUserId, originalSlotId, freedSlot.SlotId);

        // Notify patient and staff asynchronously — failures do NOT reverse the swap (AC-05, Edge Case: notification failure)
        await DispatchNotificationsAsync(appointment, freedSlot, originalSlotId, now, cancellationToken)
            .ConfigureAwait(false);

        return new SwapEngineResult(
            SwapExecuted: true,
            WinningAppointmentId: appointment.AppointmentId,
            WinningPatientUserId: candidate.PatientUserId,
            FailureReason: null);
    }

    private async Task DispatchNotificationsAsync(
        Appointment appointment,
        AvailabilitySlot newSlot,
        Guid originalSlotId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var patientEmail = appointment.Patient?.Email ?? string.Empty;
        var patientPhone = appointment.Patient?.MfaPhoneNumber;

        // Patient email notification (AC-02, AC-05)
        if (!string.IsNullOrWhiteSpace(patientEmail))
        {
            try
            {
                await _emailService.SendConfirmationEmailAsync(
                    patientEmail,
                    subject: "Your appointment has been swapped",
                    body: $"Your appointment has been successfully swapped to {newSlot.StartTime:f} UTC with {newSlot.ProviderName} ({newSlot.Specialty}). Booking reference: {appointment.AppointmentId}.",
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Swap notification email failed for AppointmentId={AppointmentId}. Swap is NOT reversed.",
                    appointment.AppointmentId);
            }
        }

        // Patient SMS notification (AC-02) — SmsService logs only; swap is not reversed on failure
        if (!string.IsNullOrWhiteSpace(patientPhone))
        {
            try
            {
                await _smsService.SendVerificationCodeAsync(
                    patientPhone,
                    $"Appointment swapped to {newSlot.StartTime:g} UTC with {newSlot.ProviderName}. Ref: {appointment.AppointmentId}",
                    cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Swap notification SMS failed for AppointmentId={AppointmentId}. Swap is NOT reversed.",
                    appointment.AppointmentId);
            }
        }

        // Staff notification: persist a Notification record for the appointment (AC-04)
        var staffNotification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            Channel = NotificationChannel.Email,
            NotificationType = NotificationType.SlotSwap,
            Status = NotificationStatus.Queued,
            RetryCount = 0,
            CreatedAt = now,
        };

        _db.Notifications.Add(staffNotification);

        try
        {
            await _db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist staff notification for AppointmentId={AppointmentId}. Swap is NOT reversed.",
                appointment.AppointmentId);
        }
    }

    private async Task ExpireQueueEntryAsync(SwapQueueEntry candidate, CancellationToken cancellationToken)
    {
        var entry = await _db.PreferredSlotQueues
            .FirstOrDefaultAsync(
                q => q.AppointmentId == candidate.AppointmentId
                  && q.PreferredSlotId == candidate.PreferredSlotId
                  && q.Status == QueueStatus.Waiting,
                cancellationToken)
            .ConfigureAwait(false);

        if (entry is null)
            return;

        entry.Status = QueueStatus.Expired;

        try
        {
            await _db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            await _swapQueue.RemoveAsync(entry.QueueId, candidate.PreferredSlotId, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to expire queue entry for AppointmentId={AppointmentId}.",
                candidate.AppointmentId);
        }
    }
}
