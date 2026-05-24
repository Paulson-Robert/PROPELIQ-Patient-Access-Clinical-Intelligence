using MediatR;

namespace Application.Queries;

/// <summary>
/// DTO representing a single appointment in the patient's list view.
/// </summary>
public sealed record PatientAppointmentDto(
    Guid AppointmentId,
    Guid SlotId,
    string ProviderName,
    string Specialty,
    DateTime StartTime,
    DateTime EndTime,
    int DurationMinutes,
    string Status,
    string? InsuranceProvider);

/// <summary>
/// Returns all appointments for the authenticated patient, ordered by StartTime descending.
/// Supports optional status filter to retrieve e.g. only upcoming (Scheduled) appointments.
/// </summary>
public sealed record GetPatientAppointmentsQuery(
    Guid PatientUserId,
    string? StatusFilter = null) : IRequest<IReadOnlyList<PatientAppointmentDto>>;
