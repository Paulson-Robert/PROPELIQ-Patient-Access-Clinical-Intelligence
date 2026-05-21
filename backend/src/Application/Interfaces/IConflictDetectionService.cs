using Domain.Enums;

namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Read-only projection of a <see cref="Domain.Entities.DataConflict"/> row.</summary>
public sealed record DataConflictDto(
    Guid ConflictId,
    Guid PatientProfileId,
    string FieldName,
    string Value1,
    Guid SourceDocumentId1,
    string Value2,
    Guid SourceDocumentId2,
    ResolutionStatus ResolutionStatus,
    Guid? ResolvedByUserId,
    DateTime? ResolvedAt,
    string? ResolutionNotes);

/// <summary>Summary counts returned by <see cref="IConflictDetectionService.DetectAndPersistAsync"/>.</summary>
public sealed record ConflictDetectionResult(
    int NewConflictsInserted,
    int ResolvedConflictsReopened);

// ---------------------------------------------------------------------------
// Resolution request/result DTOs
// ---------------------------------------------------------------------------

/// <summary>Request payload for <see cref="IConflictDetectionService.ResolveAsync"/>.</summary>
public sealed record ResolveConflictRequest(
    Guid ConflictId,
    Guid ResolvedByUserId,
    string ResolvedByRole,
    string AcceptedValue,
    string? Notes = null);

/// <summary>Result returned by <see cref="IConflictDetectionService.ResolveAsync"/>.</summary>
public sealed record ConflictResolutionResult(
    bool Success,
    string? FailureCode,
    string? FailureReason);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Manages the full conflict lifecycle for a patient (US_039):
/// <list type="bullet">
///   <item>AC-01: Detects value mismatches across extracted sources by running
///     de-duplication on the patient's <see cref="Domain.Entities.ExtractedDataRecord"/> rows.</item>
///   <item>AC-01: Surfaces open conflicts via <see cref="GetOpenConflictsAsync"/>.</item>
///   <item>AC-02: Persists the selected value via <see cref="ResolveAsync"/>.</item>
///   <item>AC-03: Appends an immutable audit trail entry via <see cref="ResolveAsync"/>.</item>
///   <item>Edge case: Re-opens a previously resolved conflict when incoming data
///     contradicts the accepted value, and notifies staff.</item>
/// </list>
/// </summary>
public interface IConflictDetectionService
{
    /// <summary>
    /// Returns all open <see cref="Domain.Entities.DataConflict"/> rows for
    /// <paramref name="patientProfileId"/>.
    /// </summary>
    Task<IReadOnlyList<DataConflictDto>> GetOpenConflictsAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the patient's extracted records, runs de-duplication, and persists the
    /// resulting conflict state:
    /// <list type="bullet">
    ///   <item>New conflicts → inserted as <see cref="Domain.Enums.ResolutionStatus.Open"/>.</item>
    ///   <item>Already-open conflicts → unchanged (idempotent).</item>
    ///   <item>Resolved conflicts with new contradicting data → re-opened and staff notified
    ///     (edge case).</item>
    /// </list>
    /// </summary>
    Task<ConflictDetectionResult> DetectAndPersistAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an open conflict by persisting the accepted value (AC-02) and
    /// appending an immutable audit log entry (AC-03).
    /// </summary>
    Task<ConflictResolutionResult> ResolveAsync(
        ResolveConflictRequest request,
        CancellationToken cancellationToken = default);
}
