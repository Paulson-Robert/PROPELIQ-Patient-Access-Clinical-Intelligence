using Domain.Enums;

namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// ICodeMappingService — ICD-10/CPT code mapping contract (AC-01 – AC-04)
// ---------------------------------------------------------------------------

/// <summary>
/// Staff action when reviewing an AI-suggested code mapping (AC-03).
/// </summary>
public enum VerifyCodeAction
{
    /// <summary>Accept the suggested code as-is (AC-03).</summary>
    Verify,

    /// <summary>Replace the suggested code with a corrected value and record the reason (AC-03, AC-04).</summary>
    Modify,

    /// <summary>Discard the suggested code and record the reason (AC-03, AC-04).</summary>
    Reject
}

/// <summary>
/// A single ranked ICD-10 or CPT code suggestion produced by the mapping engine (AC-01, AC-02).
/// Multiple candidates per input entity represent the ambiguous diagnosis edge case.
/// </summary>
/// <param name="InputText">The original clinical entity text used for mapping.</param>
/// <param name="CodeType">ICD-10 or CPT (AC-01).</param>
/// <param name="CodeValue">The suggested code value, e.g. "E11.9" or "99213".</param>
/// <param name="CodeDescription">Human-readable code description.</param>
/// <param name="ConfidenceScore">Confidence in [0, 1] — higher is more certain (AC-02).</param>
/// <param name="Source">Mapping source label, e.g. "Rule-based", "ML model".</param>
/// <param name="CodeSetVersion">Code set version string, e.g. "ICD-10-CM 2024".</param>
public sealed record CodeMappingCandidate(
    string InputText,
    MedicalCodeType CodeType,
    string CodeValue,
    string CodeDescription,
    decimal ConfidenceScore,
    string Source,
    string CodeSetVersion);

/// <summary>
/// Request payload for persisting a staff verification decision (AC-03, AC-04).
/// </summary>
/// <param name="MappingId">Primary key of the <see cref="Domain.Entities.MedicalCodeMapping"/> row.</param>
/// <param name="StaffUserId">User ID of the staff member performing the action (AC-04 audit).</param>
/// <param name="StaffRole">Role of the staff member (AC-04 audit).</param>
/// <param name="Action">Verify, Modify, or Reject (AC-03).</param>
/// <param name="ModifiedCodeValue">Replacement code when <see cref="Action"/> is Modify (AC-03).</param>
/// <param name="Reason">Mandatory for Modify and Reject — logged to audit trail (AC-04).</param>
public sealed record VerifyCodeMappingRequest(
    Guid MappingId,
    Guid StaffUserId,
    string StaffRole,
    VerifyCodeAction Action,
    string? ModifiedCodeValue,
    string? Reason);

/// <summary>Result of a verification operation (AC-03).</summary>
public sealed record VerifyCodeMappingResult(
    bool Success,
    string? FailureCode = null,
    string? FailureReason = null);

/// <summary>
/// Maps clinical entities extracted by the NER pipeline to ICD-10/CPT codes
/// and persists staff verification decisions.
/// </summary>
public interface ICodeMappingService
{
    /// <summary>
    /// Maps <paramref name="entities"/> to ranked ICD-10/CPT candidates (AC-01, AC-02).
    ///
    /// Each entity of type <see cref="ClinicalEntityType.Diagnosis"/> or
    /// <see cref="ClinicalEntityType.Procedure"/> is processed independently.
    /// Entities of other types are silently skipped.
    ///
    /// Edge case — ambiguous diagnosis: multiple candidates are returned per entity,
    /// ranked in descending confidence order.
    /// </summary>
    /// <param name="entities">Clinical entities from the NER pipeline.</param>
    /// <param name="patientId">PHI boundary identifier — scopes the operation to one patient.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<CodeMappingCandidate>> MapAsync(
        IReadOnlyList<ClinicalEntity> entities,
        string patientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a staff verification decision and appends an immutable audit log entry (AC-03, AC-04).
    /// </summary>
    Task<VerifyCodeMappingResult> VerifyAsync(
        VerifyCodeMappingRequest request,
        CancellationToken cancellationToken = default);
}
