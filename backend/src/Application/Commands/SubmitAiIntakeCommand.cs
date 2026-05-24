using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Persists the structured summary collected by the AI-assisted intake UI.
/// </summary>
public sealed record SubmitAiIntakeCommand(
    Guid ActorUserId,
    string ActorRole,
    Guid AppointmentId,
    string? ChronicConditions,
    string? CurrentMedications,
    string? Allergies,
    string? SurgicalHistory,
    string ReasonForVisit) : IRequest<AiIntakeSubmissionResult>;

internal sealed class SubmitAiIntakeCommandHandler
    : IRequestHandler<SubmitAiIntakeCommand, AiIntakeSubmissionResult>
{
    private readonly IAiIntakePersistenceService _persistence;

    public SubmitAiIntakeCommandHandler(IAiIntakePersistenceService persistence)
    {
        _persistence = persistence;
    }

    public Task<AiIntakeSubmissionResult> Handle(
        SubmitAiIntakeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorUserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.ActorRole) ||
            request.AppointmentId == Guid.Empty)
        {
            return Task.FromResult(new AiIntakeSubmissionResult(
                Success: false,
                IntakeId: null,
                FailureReason: "ActorUserId, ActorRole, and AppointmentId are required.",
                FailureCode: "INVALID_REQUEST"));
        }

        if (string.IsNullOrWhiteSpace(request.ReasonForVisit))
        {
            return Task.FromResult(new AiIntakeSubmissionResult(
                Success: false,
                IntakeId: null,
                FailureReason: "Reason for visit is required.",
                FailureCode: "VALIDATION_ERROR"));
        }

        return _persistence.PersistAsync(
            request.ActorUserId,
            request.ActorRole,
            request.AppointmentId,
            new AiIntakeSummary(
                request.ChronicConditions,
                request.CurrentMedications,
                request.Allergies,
                request.SurgicalHistory,
                request.ReasonForVisit.Trim()),
            cancellationToken);
    }
}
