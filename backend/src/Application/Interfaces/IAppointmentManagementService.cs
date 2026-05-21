namespace Application.Interfaces;

/// <summary>
/// DTO describing a mutable appointment state change.
/// </summary>
public sealed record AppointmentMutationDto(
    Guid AppointmentId,
    Guid SlotId,
    string Status,
    DateTime UpdatedAtUtc);

/// <summary>
/// Result wrapper for cancellation and reschedule operations.
/// </summary>
public sealed record AppointmentMutationResult(
    bool Success,
    AppointmentMutationDto? Appointment,
    string? FailureReason,
    string? FailureCode);

public interface IAppointmentManagementService
{
    /// <summary>
    /// Cancels a scheduled appointment, releases its slot, and triggers
    /// swap/calendar downstream processing.
    /// </summary>
    Task<AppointmentMutationResult> CancelAsync(
        Guid appointmentId,
        Guid patientUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically moves an appointment to a newly locked slot.
    /// </summary>
    Task<AppointmentMutationResult> RescheduleAsync(
        Guid appointmentId,
        Guid newSlotId,
        string lockToken,
        Guid patientUserId,
        CancellationToken cancellationToken = default);
}
