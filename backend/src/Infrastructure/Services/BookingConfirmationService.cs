using Application.EventHandlers;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// Validates the Redis lock token, persists the appointment record, marks the
/// AvailabilitySlot unavailable, releases the lock, and enqueues PDF delivery
/// via Hangfire (AC-04). Uses EF Core optimistic concurrency on
/// AvailabilitySlot.Version as a secondary guard when Redis is degraded (Edge Case).
/// </summary>
public sealed class BookingConfirmationService : IBookingConfirmationService
{
    private static readonly Regex PolicyNumberPattern = new(
        "^[A-Za-z0-9]{2,8}-?[A-Za-z0-9]{4,12}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ApplicationDbContext _db;
    private readonly ISlotLockService _slotLock;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IPublisher _publisher;
    private readonly ILogger<BookingConfirmationService> _logger;

    public BookingConfirmationService(
        ApplicationDbContext db,
        ISlotLockService slotLock,
        IBackgroundJobClient jobClient,
        IPublisher publisher,
        ILogger<BookingConfirmationService> logger)
    {
        _db = db;
        _slotLock = slotLock;
        _jobClient = jobClient;
        _publisher = publisher;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BookingConfirmationResult> ConfirmAsync(
        Guid slotId,
        string lockToken,
        Guid patientUserId,
        string? insuranceProvider,
        string? insurancePolicyNumber,
        CancellationToken cancellationToken = default)
    {
        // AC-05: Validate the caller still owns the Redis lock
        var lockValid = await _slotLock.ValidateAsync(slotId, lockToken, cancellationToken)
            .ConfigureAwait(false);

        if (!lockValid)
        {
            return new BookingConfirmationResult(
                Success: false,
                Appointment: null,
                FailureReason: "Slot hold expired — please select again.",
                FailureCode: "LOCK_EXPIRED");
        }

        // Load slot with row-level lock via FOR UPDATE equivalent (optimistic concurrency guard)
        var slot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == slotId, cancellationToken)
            .ConfigureAwait(false);

        if (slot is null || !slot.IsAvailable)
        {
            await _slotLock.ReleaseAsync(slotId, lockToken, cancellationToken).ConfigureAwait(false);

            return new BookingConfirmationResult(
                Success: false,
                Appointment: null,
                FailureReason: "This slot is no longer available.",
                FailureCode: "SLOT_UNAVAILABLE");
        }

        // Load patient email for confirmation
        var patientEmail = await _db.Users
            .Where(u => u.UserId == patientUserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? string.Empty;

        var now = DateTime.UtcNow;
        var trimmedInsuranceProvider = string.IsNullOrWhiteSpace(insuranceProvider)
            ? null
            : insuranceProvider.Trim();
        var trimmedInsurancePolicyNumber = string.IsNullOrWhiteSpace(insurancePolicyNumber)
            ? null
            : insurancePolicyNumber.Trim();
        var insuranceValidationWarning = ValidatePolicyNumber(trimmedInsurancePolicyNumber);

        var appointment = new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            PatientId = patientUserId,
            ProviderId = slot.ProviderId,
            SlotId = slotId,
            Status = AppointmentStatus.Scheduled,
            BookingType = BookingType.Online,
            InsuranceProvider = trimmedInsuranceProvider,
            InsurancePolicyNumber = trimmedInsurancePolicyNumber,
            CreatedByUserId = patientUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (insuranceValidationWarning is not null)
        {
            _logger.LogInformation(
                "Soft insurance policy format warning for PatientId={PatientId}, SlotId={SlotId}: {Warning}",
                patientUserId,
                slotId,
                insuranceValidationWarning);
        }

        // Mark slot as booked
        slot.IsAvailable = false;
        slot.IsLocked = false;
        slot.LockExpiry = null;
        slot.Version++;

        _db.Appointments.Add(appointment);

        try
        {
            // EF Core optimistic concurrency: will throw DbUpdateConcurrencyException
            // if another transaction committed a Version change between our read and write
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Optimistic concurrency conflict booking slot {SlotId}. Another booking committed first.",
                slotId);

            await _slotLock.ReleaseAsync(slotId, lockToken, cancellationToken).ConfigureAwait(false);

            return new BookingConfirmationResult(
                Success: false,
                Appointment: null,
                FailureReason: "This slot is no longer available.",
                FailureCode: "SLOT_UNAVAILABLE");
        }

        // Release Redis lock — fire-and-forget (TTL auto-expires as fallback)
        _ = _slotLock.ReleaseAsync(slotId, lockToken, CancellationToken.None);

        // Enqueue PDF generation and email delivery (AC-04, Edge Case: PDF failure queued for retry)
        _jobClient.Enqueue<IBookingPdfService>(
            svc => svc.GenerateAndDeliverAsync(appointment.AppointmentId, CancellationToken.None));

        // Trigger no-show risk scoring on appointment creation (US_041, AC-01, AC-03)
        await _publisher
            .Publish(new AppointmentRiskScoreRequested(appointment.AppointmentId), cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Booking confirmed. AppointmentId={AppointmentId}, SlotId={SlotId}, PatientId={PatientId}.",
            appointment.AppointmentId, slotId, patientUserId);

        return new BookingConfirmationResult(
            Success: true,
            Appointment: new AppointmentConfirmationDto(
                appointment.AppointmentId,
                slot.ProviderName,
                slot.Specialty,
                slot.StartTime,
                (int)(slot.EndTime - slot.StartTime).TotalMinutes,
                nameof(AppointmentStatus.Scheduled),
                patientEmail,
                appointment.InsuranceProvider,
                appointment.InsurancePolicyNumber,
                insuranceValidationWarning),
            FailureReason: null,
            FailureCode: null);
    }

    private static string? ValidatePolicyNumber(string? policyNumber)
    {
        if (policyNumber is null)
        {
            return null;
        }

        return PolicyNumberPattern.IsMatch(policyNumber)
            ? null
            : "Insurance policy number format looks unusual. This is a soft validation warning only.";
    }
}
