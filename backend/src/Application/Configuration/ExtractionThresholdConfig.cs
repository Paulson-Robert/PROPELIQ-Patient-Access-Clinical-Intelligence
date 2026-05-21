namespace Application.Configuration;

// ---------------------------------------------------------------------------
// ExtractionThresholdConfig — configurable thresholds for NER confidence scoring
// (AC-02, US_036)
// ---------------------------------------------------------------------------

/// <summary>
/// Configurable confidence thresholds for NER extraction decisions.
/// Bind from the "ExtractionThresholds" configuration section.
///
/// Threshold semantics (AC-02):
///   Confidence ≥ <see cref="AutoAcceptThreshold"/> → auto-accept
///   Confidence ≥ <see cref="ReviewThreshold"/>     → queue for human review
///   Confidence below <see cref="ReviewThreshold"/> → reject
///
/// Supports hot-reload via <c>IOptionsMonitor</c> — threshold changes apply to all
/// subsequent extraction calls without requiring a redeployment (edge case: US_036).
/// </summary>
public sealed class ExtractionThresholdConfig
{
    /// <summary>Configuration section key bound in appsettings.</summary>
    public const string SectionName = "ExtractionThresholds";

    /// <summary>
    /// Confidence score at or above which an extraction is automatically accepted.
    /// Boundary is inclusive. Default: 0.9 (AC-02).
    /// </summary>
    public float AutoAcceptThreshold { get; set; } = 0.9f;

    /// <summary>
    /// Confidence score at or above which an extraction is queued for human review.
    /// Values strictly below this threshold are rejected. Boundary is inclusive.
    /// Default: 0.7 (AC-02).
    /// </summary>
    public float ReviewThreshold { get; set; } = 0.7f;
}
