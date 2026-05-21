namespace Application.Interfaces;

/// <summary>
/// Data required to create a walk-in booking.
/// </summary>
public sealed record CreateWalkInRequest(
    Guid SlotId,
    Guid StaffUserId,
    Guid? ExistingPatientUserId,
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? Email,
    string? Phone,
    bool CreateAccount);

/// <summary>
/// Result payload returned after creating a walk-in booking.
/// </summary>
public sealed record WalkInBookingResultDto(
    Guid AppointmentId,
    Guid PatientUserId,
    Guid QueueId,
    string BookingType,
    string Status,
    DateTime CreatedAtUtc,
    bool TemporaryPatientRecordCreated);

public interface IWalkInBookingService
{
    /// <summary>
    /// Creates a walk-in appointment and inserts it into same-day queue tracking.
    /// </summary>
    Task<WalkInBookingResultDto> CreateAsync(
        CreateWalkInRequest request,
        CancellationToken cancellationToken = default);
}