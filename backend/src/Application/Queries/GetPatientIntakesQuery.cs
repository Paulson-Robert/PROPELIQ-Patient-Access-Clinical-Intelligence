using MediatR;

namespace Application.Queries;

/// <summary>
/// DTO representing a completed or in-progress intake record for the patient's intake history.
/// </summary>
public sealed record PatientIntakeDto(
    Guid IntakeId,
    Guid AppointmentId,
    string IntakeMode,
    string? ReasonForVisit,
    DateTime? CompletedAt,
    DateTime LastModifiedAt);

/// <summary>
/// Returns all intake records for the authenticated patient, ordered by last-modified descending.
/// </summary>
public sealed record GetPatientIntakesQuery(
    Guid PatientUserId) : IRequest<IReadOnlyList<PatientIntakeDto>>;
