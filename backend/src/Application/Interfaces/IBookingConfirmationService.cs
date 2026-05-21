namespace Application.Interfaces;

/// <summary>
/// DTO returned after a booking is confirmed (AC-04).
/// </summary>
public sealed record AppointmentConfirmationDto(
    Guid AppointmentId,
    string ProviderName,
    string Specialty,
    DateTime StartTime,
    int DurationMinutes,
    string Status,
    string PatientEmail,
    string? InsuranceProvider,
    string? InsurancePolicyNumber,
    string? InsuranceValidationWarning);

/// <summary>
/// Result wrapping either a confirmed appointment or a failure reason.
/// </summary>
public sealed record BookingConfirmationResult(
    bool Success,
    AppointmentConfirmationDto? Appointment,
    string? FailureReason,
    string? FailureCode);

public interface IBookingConfirmationService
{
    /// <summary>
    /// Validates the lock token, persists the appointment, marks the slot unavailable,
    /// releases the Redis lock, and enqueues PDF generation (AC-04).
    /// Returns failure when the lock has expired or the slot is no longer available.
    /// </summary>
    Task<BookingConfirmationResult> ConfirmAsync(
        Guid slotId,
        string lockToken,
        Guid patientUserId,
        string? insuranceProvider,
        string? insurancePolicyNumber,
        CancellationToken cancellationToken = default);
}
