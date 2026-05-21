using Microsoft.ML.Data;

namespace Infrastructure.ML.Models;

// ---------------------------------------------------------------------------
// ML.NET data schema types for NER training and prediction (TR-010)
// These are internal to the Infrastructure layer and are NOT part of the
// public contract — use Application.Interfaces.ClinicalEntity for that.
// ---------------------------------------------------------------------------

/// <summary>
/// ML.NET column-mapped input for NER prediction.
/// The "Sentence" column is consumed by the TorchSharp NamedEntityRecognitionTrainer.
/// </summary>
internal sealed class NerInput
{
    [ColumnName("Sentence")]
    public string Sentence { get; set; } = string.Empty;
}

/// <summary>
/// ML.NET column-mapped output from the NER prediction engine.
/// Tags are IOB2 labels aligned to whitespace-delimited word tokens.
/// </summary>
internal sealed class NerOutput
{
    /// <summary>
    /// Predicted IOB2 tag per word token (e.g. "B-PERSON", "I-PERSON", "O").
    /// Length equals the number of tokens in the input sentence.
    /// Must reference the outputColumnName set on the NerTrainer ("PredictedTags").
    /// </summary>
    [ColumnName("PredictedTags")]
    public string[] Tags { get; set; } = [];
}
