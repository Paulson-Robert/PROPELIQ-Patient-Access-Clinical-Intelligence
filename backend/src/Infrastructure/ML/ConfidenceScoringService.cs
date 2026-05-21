using Application.Configuration;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// ConfidenceScoringService — threshold-based NER extraction scoring
// (AC-01, AC-02, AC-03, TR-010)
// ---------------------------------------------------------------------------

/// <summary>
/// Applies configurable auto-accept / review / reject thresholds to NER extraction results (AC-02).
///
/// Decision rules (AC-02):
///   Confidence ≥ AutoAcceptThreshold → <see cref="ExtractionDecision.AutoAccept"/>
///   Confidence ≥ ReviewThreshold     → <see cref="ExtractionDecision.Review"/>
///   Confidence below ReviewThreshold → <see cref="ExtractionDecision.Reject"/>
///
/// Boundary values are inclusive: a score exactly equal to a threshold meets it (US_036 edge case).
///
/// PHI boundary enforcement (AC-03): every call to <see cref="Score"/> must supply a non-empty
/// <c>patientId</c>. The service is stateless and processes one patient's extractions per call,
/// preventing cross-patient entity contamination at the API boundary.
///
/// Threshold changes are applied to subsequent calls without redeployment because the service
/// reads <see cref="IOptionsMonitor{TOptions}.CurrentValue"/> at call time (US_036 edge case).
/// </summary>
public sealed class ConfidenceScoringService : IConfidenceScoringService
{
    private readonly IOptionsMonitor<ExtractionThresholdConfig> _thresholds;
    private readonly ILogger<ConfidenceScoringService> _logger;

    public ConfidenceScoringService(
        IOptionsMonitor<ExtractionThresholdConfig> thresholds,
        ILogger<ConfidenceScoringService> logger)
    {
        _thresholds = thresholds;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ScoredExtraction> Score(
        IReadOnlyList<ClinicalEntity> entities,
        string patientId)
    {
        // PHI boundary enforcement (AC-03): each scoring call must be tied to a specific patient.
        ArgumentException.ThrowIfNullOrWhiteSpace(patientId, nameof(patientId));
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return [];

        // Snapshot thresholds once per call — config changes apply to the next invocation.
        var config = _thresholds.CurrentValue;
        var autoAccept = config.AutoAcceptThreshold;
        var review = config.ReviewThreshold;

        var results = new List<ScoredExtraction>(entities.Count);

        foreach (var entity in entities)
        {
            // AC-01: confidence score in [0, 1] is carried on the entity from the NER pipeline.
            var decision = ClassifyConfidence(entity.Confidence, autoAccept, review);

            if (decision == ExtractionDecision.Reject)
            {
                _logger.LogDebug(
                    "Rejected extraction PatientId={PatientId} EntityType={EntityType} Confidence={Confidence:F3}",
                    patientId, entity.EntityType, entity.Confidence);
            }

            results.Add(new ScoredExtraction(entity, decision));
        }

        _logger.LogInformation(
            "Scored {Total} extractions for PatientId={PatientId}: AutoAccept={AutoAccept} Review={Review} Reject={Reject}",
            entities.Count,
            patientId,
            results.Count(r => r.Decision == ExtractionDecision.AutoAccept),
            results.Count(r => r.Decision == ExtractionDecision.Review),
            results.Count(r => r.Decision == ExtractionDecision.Reject));

        return results.AsReadOnly();
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps a confidence score to a threshold decision.
    /// Boundary comparisons are inclusive (AC-02, US_036 edge case).
    /// </summary>
    private static ExtractionDecision ClassifyConfidence(float confidence, float autoAccept, float review)
    {
        if (confidence >= autoAccept)
            return ExtractionDecision.AutoAccept;

        if (confidence >= review)
            return ExtractionDecision.Review;

        return ExtractionDecision.Reject;
    }
}
