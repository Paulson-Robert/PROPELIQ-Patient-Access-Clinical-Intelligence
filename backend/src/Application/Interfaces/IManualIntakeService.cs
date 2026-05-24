namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Structured fields stored in the MedicalHistory JSONB column for manual intake.</summary>
public sealed record ManualMedicalHistory(
    string? ChronicConditions,
    string? PastSurgeries,
    string? FamilyHistory);

/// <summary>Structured fields stored in the CurrentSymptoms JSONB column for manual intake.</summary>
public sealed record ManualSymptoms(
    string? Description,
    string? Onset,
    string? Severity);

/// <summary>Full intake payload sent on final submission (AC-01).</summary>
public sealed record SubmitManualIntakeRequest(
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
    string ReasonForVisit);

/// <summary>Partial payload used for draft auto-save between steps (AC-02).</summary>
public sealed record SaveIntakeDraftRequest(
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
    string? ReasonForVisit);

/// <summary>Draft data returned to the client for form restoration.</summary>
public sealed record IntakeDraftDto(
    Guid IntakeId,
    string? ChronicConditions,
    string? PastSurgeries,
    string? FamilyHistory,
    string? SymptomsDescription,
    string? SymptomOnset,
    string? SymptomSeverity,
    string? CurrentMedications,
    string? KnownAllergies,
    string? ReasonForVisit,
    bool IsSubmitted,
    DateTime LastModifiedAt);

/// <summary>Result returned from intake submission.</summary>
public sealed record ManualIntakeResult(
    bool Success,
    Guid? IntakeId,
    string? FailureReason,
    string? FailureCode);

/// <summary>Result returned from draft save.</summary>
public sealed record IntakeDraftResult(
    bool Success,
    Guid? IntakeId,
    string? FailureReason,
    string? FailureCode);

// ---------------------------------------------------------------------------
// Service contract
// ---------------------------------------------------------------------------

/// <summary>
/// Handles manual intake persistence: submission (AC-01), draft save (AC-02),
/// and draft retrieval (AC-02). The actor may be the patient or staff; the
/// service resolves the stored patient profile from the appointment. Completed
/// submissions are append-only; only unfinished drafts are updated in place.
/// </summary>
public interface IManualIntakeService
{
    /// <summary>
    /// Validates and persists a completed manual intake (AC-01).
    /// Completes an unfinished draft when one exists; otherwise creates a new
    /// completed history record.
    /// </summary>
    Task<ManualIntakeResult> SubmitAsync(
        SubmitManualIntakeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a partial draft for the given appointment (AC-02).
    /// Only updates fields that are not null in the request; existing data is preserved.
    /// </summary>
    Task<IntakeDraftResult> SaveDraftAsync(
        SaveIntakeDraftRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the unfinished draft intake record, or <c>null</c> if none exists.</summary>
    Task<IntakeDraftDto?> GetDraftAsync(
        Guid actorUserId,
        string actorRole,
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
