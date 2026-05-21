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
    string ReasonForVisit);

/// <summary>Partial payload used for draft auto-save between steps (AC-02).</summary>
public sealed record SaveIntakeDraftRequest(
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
    string? FailureReason);

// ---------------------------------------------------------------------------
// Service contract
// ---------------------------------------------------------------------------

/// <summary>
/// Handles manual intake persistence: submission (AC-01), draft save (AC-02),
/// and draft retrieval (AC-02). Idempotency is enforced at the service level
/// (Edge Case: duplicate submission).
/// </summary>
public interface IManualIntakeService
{
    /// <summary>
    /// Validates and persists a completed manual intake (AC-01).
    /// Returns <c>FailureCode = "ALREADY_SUBMITTED"</c> when a completed record
    /// already exists for the appointment (idempotency guard).
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

    /// <summary>Returns the draft (or submitted) intake record, or <c>null</c> if none exists.</summary>
    Task<IntakeDraftDto?> GetDraftAsync(
        Guid patientUserId,
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
