using MediatR;

namespace Application.Queries;

/// <summary>
/// DTO returned for a single appointment detail view (SCR-007).
/// </summary>
public sealed record AppointmentDetailDto(
    Guid AppointmentId,
    Guid SlotId,
    string ProviderName,
    string Specialty,
    DateTime StartTime,
    DateTime EndTime,
    int DurationMinutes,
    string Status,
    string PatientEmail,
    string? InsuranceProvider,
    string? InsurancePolicyNumber);

/// <summary>
/// Returns the full detail for a single appointment by ID.
/// Patients may only view their own appointments (ownership guard).
/// </summary>
public sealed record GetAppointmentQuery(
    Guid AppointmentId,
    Guid PatientUserId) : IRequest<AppointmentDetailDto?>;
