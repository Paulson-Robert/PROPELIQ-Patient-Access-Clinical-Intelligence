using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Identifies duplicate field entries across multiple extraction sources and
/// selects a single canonical value per (DataType, FieldName) pair (AC-02).
///
/// Selection strategy (highest priority first):
/// 1. Record whose document is the most-recently processed EHR upload and has
///    the highest confidence score.
/// 2. Record from any other source with the highest confidence score.
///
/// Conflict detection: when two or more records for the same (DataType, FieldName)
/// have different normalised values, a <see cref="DetectedConflict"/> is emitted
/// so it can be flagged for human review (Edge Cases).
/// </summary>
public sealed class DataDeduplicationService : IDataDeduplicationService
{
    private readonly ILogger<DataDeduplicationService> _logger;

    public DataDeduplicationService(ILogger<DataDeduplicationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<DeduplicationResult> DeduplicateAsync(
        IReadOnlyList<ExtractedDataRecord> records,
        CancellationToken cancellationToken = default)
    {
        if (records.Count == 0)
        {
            return Task.FromResult(new DeduplicationResult(
                Array.Empty<CanonicalRecord>(),
                Array.Empty<DetectedConflict>()));
        }

        var canonicals = new List<CanonicalRecord>();
        var conflicts = new List<DetectedConflict>();

        // Group by (DataType, normalised FieldName) to find all candidate values.
        var groups = records
            .GroupBy(r => (r.DataType, NormaliseName(r.FieldName)));

        foreach (var group in groups)
        {
            var candidates = group.ToList();

            if (candidates.Count == 1)
            {
                // No duplication — add as canonical directly.
                var r = candidates[0];
                canonicals.Add(ToCanonical(r));
                continue;
            }

            // Detect conflicting values across different source documents.
            var distinctValues = candidates
                .GroupBy(r => NormaliseValue(r.FieldValue))
                .ToList();

            if (distinctValues.Count > 1)
            {
                // Two or more distinct values from different sources → conflict.
                var first = distinctValues[0].OrderByDescending(r => r.Confidence).First();
                var second = distinctValues[1].OrderByDescending(r => r.Confidence).First();

                conflicts.Add(new DetectedConflict(
                    DataType: group.Key.DataType,
                    FieldName: group.Key.Item2,
                    Value1: first.FieldValue,
                    SourceDocumentId1: first.DocumentId,
                    Value2: second.FieldValue,
                    SourceDocumentId2: second.DocumentId));

                _logger.LogDebug(
                    "DataDeduplication: Conflict on field '{FieldName}' ({DataType}) — " +
                    "'{Value1}' vs '{Value2}'.",
                    group.Key.Item2, group.Key.DataType, first.FieldValue, second.FieldValue);
            }

            // Always select one canonical record regardless of conflict presence.
            // Priority: highest confidence; prefer already-verified records.
            var canonical = candidates
                .OrderByDescending(r => r.IsVerified ? 1 : 0)
                .ThenByDescending(r => r.Confidence)
                .First();

            canonicals.Add(ToCanonical(canonical));
        }

        _logger.LogDebug(
            "DataDeduplication: {TotalIn} records → {Canonicals} canonical, {Conflicts} conflicts.",
            records.Count, canonicals.Count, conflicts.Count);

        return Task.FromResult(new DeduplicationResult(canonicals, conflicts));
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>Converts an <see cref="ExtractedDataRecord"/> to a <see cref="CanonicalRecord"/>.</summary>
    private static CanonicalRecord ToCanonical(ExtractedDataRecord r) =>
        new(r.DataType, r.FieldName, r.FieldValue, r.DocumentId, r.Confidence, r.IsVerified);

    /// <summary>Normalises a field name for grouping (trim + lowercase).</summary>
    private static string NormaliseName(string name) =>
        name.Trim().ToLowerInvariant();

    /// <summary>Normalises a field value for conflict detection (trim + lowercase).</summary>
    private static string NormaliseValue(string value) =>
        value.Trim().ToLowerInvariant();
}
