using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Irreversibly deletes all persisted data related to a patient profile.
/// </summary>
public sealed record DeletePatientDataCommand(
    Guid PatientProfileId,
    Guid? ActorUserId,
    string ActorRole,
    string? IpAddress = null) : IRequest<PatientDataDeletionResult>;