using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Validates and persists a completed manual intake form submission (AC-01).
///
/// Validation rules enforced:
/// - <see cref="ReasonForVisit"/> must be non-empty (required field in SCR-010).
/// - PatientUserId and AppointmentId must be valid GUIDs (non-empty).
///
/// Idempotency: if a completed <c>IntakeRecord</c> already exists for the
/// appointment, the handler returns a success result with
/// <c>FailureCode = "ALREADY_SUBMITTED"</c> rather than creating a duplicate
/// (Edge Case: duplicate submission).
/// </summary>
public sealed record SubmitManualIntakeCommand(
    Guid PatientUserId,
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
        if (request.PatientUserId == Guid.Empty || request.AppointmentId == Guid.Empty)
        {
            return Task.FromResult(new ManualIntakeResult(
                Success: false,
                IntakeId: null,
                FailureReason: "PatientUserId and AppointmentId are required.",
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
                request.PatientUserId,
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
