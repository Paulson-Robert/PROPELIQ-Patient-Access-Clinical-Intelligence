using MediatR;

namespace Application.Commands;

/// <summary>
/// Registers a patient's preferred slot swap preference in the DB and Redis queue (AC-01).
/// Returns false when the patient already has a waiting preference for the same slot,
/// or when the appointment does not belong to the requesting patient.
/// </summary>
public sealed record RegisterSwapPreferenceCommand(
    Guid AppointmentId,
    Guid PreferredSlotId,
    Guid PatientUserId) : IRequest<bool>;
