namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// IConfidenceScoringService — NER extraction scoring contract (AC-01, AC-02, AC-03)
// ---------------------------------------------------------------------------

/// <summary>
/// Threshold decision applied to a NER extraction based on its confidence score (AC-02).
/// </summary>
public enum ExtractionDecision
{
    /// <summary>Confidence ≥ AutoAcceptThreshold — extraction accepted without mandatory review (AC-02).</summary>
    AutoAccept,

    /// <summary>Confidence ≥ ReviewThreshold and below AutoAcceptThreshold — queued for human review (AC-02).</summary>
    Review,

    /// <summary>Confidence below ReviewThreshold — extraction is discarded (AC-02).</summary>
    Reject
}

/// <summary>
/// A clinical entity paired with its threshold-based scoring decision (AC-01, AC-02).
/// </summary>
/// <param name="Entity">The NER-extracted entity carrying its confidence score in [0, 1] (AC-01).</param>
/// <param name="Decision">The threshold decision applied to this entity (AC-02).</param>
public sealed record ScoredExtraction(
    ClinicalEntity Entity,
    ExtractionDecision Decision);

/// <summary>
/// Applies configurable confidence thresholds to NER extraction results.
/// Enforces PHI patient boundary — every call is scoped to a single patient identifier (AC-03).
/// </summary>
public interface IConfidenceScoringService
{
    /// <summary>
    /// Classifies each entity in <paramref name="entities"/> with a threshold decision.
    /// </summary>
    /// <param name="entities">Entities produced by the NER pipeline; each carries a confidence score (AC-01).</param>
    /// <param name="patientId">
    /// Patient record identifier — enforces the PHI boundary that extractions are
    /// always scoped to a single patient and never cross patient records (AC-03).
    /// Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// One <see cref="ScoredExtraction"/> per input entity in the original order,
    /// including rejected extractions so callers can audit them.
    /// </returns>
    IReadOnlyList<ScoredExtraction> Score(
        IReadOnlyList<ClinicalEntity> entities,
        string patientId);
}
