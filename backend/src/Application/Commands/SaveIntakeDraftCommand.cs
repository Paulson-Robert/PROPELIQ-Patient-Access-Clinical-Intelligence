using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Persists a partial manual intake draft between steps (AC-02).
///
/// All clinical fields are optional — only fields with non-null values
/// are written; existing data for omitted fields is preserved. This
/// allows the client to auto-save a single step without overwriting
/// data from steps not yet sent.
///
/// PatientUserId and AppointmentId are required for record ownership.
/// </summary>
public sealed record SaveIntakeDraftCommand(
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
    string? ReasonForVisit) : IRequest<IntakeDraftResult>;

internal sealed class SaveIntakeDraftCommandHandler
    : IRequestHandler<SaveIntakeDraftCommand, IntakeDraftResult>
{
    private readonly IManualIntakeService _intake;

    public SaveIntakeDraftCommandHandler(IManualIntakeService intake)
    {
        _intake = intake;
    }

    public Task<IntakeDraftResult> Handle(
        SaveIntakeDraftCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PatientUserId == Guid.Empty || request.AppointmentId == Guid.Empty)
        {
            return Task.FromResult(new IntakeDraftResult(
                Success: false,
                IntakeId: null,
                FailureReason: "PatientUserId and AppointmentId are required."));
        }

        return _intake.SaveDraftAsync(
            new SaveIntakeDraftRequest(
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
