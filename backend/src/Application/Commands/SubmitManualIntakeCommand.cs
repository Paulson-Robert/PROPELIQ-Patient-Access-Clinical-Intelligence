using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Validates and persists a completed manual intake form submission (AC-01).
///
/// Validation rules enforced:
/// - <see cref="ReasonForVisit"/> must be non-empty (required field in SCR-010).
/// - ActorUserId, ActorRole, and AppointmentId must be present.
///
/// Completed submissions are append-only history entries. If an unfinished
/// manual draft exists for the appointment, the service completes that draft;
/// otherwise it creates a new completed intake record.
/// </summary>
public sealed record SubmitManualIntakeCommand(
    Guid ActorUserId,
    string ActorRole,
    Guid AppointmentId,
    string? ChronicConditions,
    string? PastSurgeries,
    string? FamilyHistory,
    string? SymptomsDescription,
    string? SymptomOnset,
    string? SymptomSeverity,
    string? CurrentMedications,
    string? KnownAllergies,
    string ReasonForVisit) : IRequest<ManualIntakeResult>;

internal sealed class SubmitManualIntakeCommandHandler
    : IRequestHandler<SubmitManualIntakeCommand, ManualIntakeResult>
{
    private readonly IManualIntakeService _intake;

    public SubmitManualIntakeCommandHandler(IManualIntakeService intake)
    {
        _intake = intake;
    }

    public Task<ManualIntakeResult> Handle(
        SubmitManualIntakeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.ActorRole) ||
            request.AppointmentId == Guid.Empty)
        {
            return Task.FromResult(new ManualIntakeResult(
                Success: false,
                IntakeId: null,
                FailureReason: "ActorUserId, ActorRole, and AppointmentId are required.",
                FailureCode: "INVALID_REQUEST"));
        }

        if (string.IsNullOrWhiteSpace(request.ReasonForVisit))
        {
            return Task.FromResult(new ManualIntakeResult(
                Success: false,
                IntakeId: null,
                FailureReason: "Reason for visit is required.",
                FailureCode: "VALIDATION_ERROR"));
        }

        return _intake.SubmitAsync(
            new SubmitManualIntakeRequest(
                request.ActorUserId,
                request.ActorRole,
                request.AppointmentId,
                request.ChronicConditions,
                request.PastSurgeries,
                request.FamilyHistory,
                request.SymptomsDescription,
                request.SymptomOnset,
                request.SymptomSeverity,
                request.CurrentMedications,
                request.KnownAllergies,
                request.ReasonForVisit),
            cancellationToken);
    }
}
