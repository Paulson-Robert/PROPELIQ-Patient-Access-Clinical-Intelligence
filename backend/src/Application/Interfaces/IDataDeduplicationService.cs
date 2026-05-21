using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>A single de-duplicated field value selected as canonical.</summary>
public sealed record CanonicalRecord(
    ExtractedDataType DataType,
    string FieldName,
    string FieldValue,
    Guid SourceDocumentId,
    decimal Confidence,
    bool IsVerified);

/// <summary>A detected conflict between two sources for the same field.</summary>
public sealed record DetectedConflict(
    ExtractedDataType DataType,
    string FieldName,
    string Value1,
    Guid SourceDocumentId1,
    string Value2,
    Guid SourceDocumentId2);

/// <summary>Result returned by <see cref="IDataDeduplicationService.DeduplicateAsync"/>.</summary>
public sealed record DeduplicationResult(
    IReadOnlyList<CanonicalRecord> CanonicalRecords,
    IReadOnlyList<DetectedConflict> Conflicts);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Identifies duplicate field entries across multiple sources and selects
/// a canonical value per field, surfacing conflicts for human review (AC-02).
///
/// Selection strategy (highest priority first):
/// 1. EHR-sourced record with highest confidence.
/// 2. Intake record with highest confidence.
/// 3. Any other source, highest confidence.
///
/// When two records for the same field carry different values from different
/// sources a <see cref="DetectedConflict"/> is emitted (Edge Cases).
/// </summary>
public interface IDataDeduplicationService
{
    /// <summary>
    /// Deduplicates <paramref name="records"/> for a single patient,
    /// returning canonical values and any detected value conflicts.
    /// </summary>
    Task<DeduplicationResult> DeduplicateAsync(
        IReadOnlyList<ExtractedDataRecord> records,
        CancellationToken cancellationToken = default);
}
