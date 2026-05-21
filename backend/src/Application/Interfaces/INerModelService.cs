namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// NER model service contract — clinical entity extraction (AC-01, TR-010)
// ---------------------------------------------------------------------------

/// <summary>
/// Clinical entity type extracted by the NER pipeline (AC-01).
/// Maps to the IOB2 label scheme used during training.
/// </summary>
public enum ClinicalEntityType
{
    /// <summary>Patient or provider person name.</summary>
    PatientName,

    /// <summary>Calendar date or date range (e.g. "March 5, 2024").</summary>
    Date,

    /// <summary>Clinical diagnosis or condition (ICD-10 mappable).</summary>
    Diagnosis,

    /// <summary>Medication name or drug reference.</summary>
    Medication,

    /// <summary>Clinical procedure name (CPT mappable).</summary>
    Procedure,

    /// <summary>Extracted entity whose type could not be classified with confidence.</summary>
    Unknown
}

/// <summary>
/// A single clinical entity span extracted from free-form text (AC-01).
/// </summary>
/// <param name="EntityType">Semantic category of the entity.</param>
/// <param name="Text">The verbatim extracted text.</param>
/// <param name="StartIndex">Zero-based character index of the first character in the source text.</param>
/// <param name="EndIndex">Zero-based character index of the last character (inclusive) in the source text.</param>
/// <param name="Confidence">Model confidence score in [0, 1]. Values below the configured threshold set <see cref="NeedsReview"/>.</param>
/// <param name="NeedsReview">True when <see cref="Confidence"/> is below the configured threshold (edge case: low-confidence extraction).</param>
public sealed record ClinicalEntity(
    ClinicalEntityType EntityType,
    string Text,
    int StartIndex,
    int EndIndex,
    float Confidence,
    bool NeedsReview);

/// <summary>
/// A single labeled training document for the NER pipeline (AC-03).
/// Uses IOB2 tag format: B-PERSON, I-PERSON, B-DATE, B-DIAGNOSIS, B-MEDICATION, B-PROCEDURE, O.
/// </summary>
public sealed class NerLabeledDocument
{
    /// <summary>The full sentence text. Must not be null or empty.</summary>
    public string Sentence { get; set; } = string.Empty;

    /// <summary>
    /// IOB2 label for each whitespace-delimited word token in <see cref="Sentence"/>.
    /// Length must equal the token count of <see cref="Sentence"/>.
    /// </summary>
    public string[] Tags { get; set; } = [];
}

/// <summary>
/// Extracts clinical entities from free-form text using a trained ML.NET NER model (AC-01).
/// </summary>
public interface INerModelService
{
    /// <summary>
    /// Extracts named entities from <paramref name="text"/>.
    /// Entities with confidence below the configured threshold are returned with <see cref="ClinicalEntity.NeedsReview"/> = true.
    /// </summary>
    /// <param name="text">Raw clinical text (must not be null).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Ordered list of extracted entities sorted by <see cref="ClinicalEntity.StartIndex"/>.</returns>
    Task<IReadOnlyList<ClinicalEntity>> ExtractEntitiesAsync(
        string text,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Trains or retrains the ML.NET NER model from labeled documents and persists the resulting model (AC-03).
/// </summary>
public interface INerTrainingPipeline
{
    /// <summary>
    /// Trains a new NER model on <paramref name="labeledDocuments"/> and saves it to the configured model directory.
    /// </summary>
    /// <param name="labeledDocuments">Labeled training corpus. Must contain at least one document per entity type.</param>
    /// <param name="modelVersion">Human-readable version label (e.g. "v2"). Appended to the output filename.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Absolute path of the saved model zip file.</returns>
    Task<string> TrainAsync(
        IReadOnlyList<NerLabeledDocument> labeledDocuments,
        string modelVersion,
        CancellationToken cancellationToken = default);
}
