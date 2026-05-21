using Application.Interfaces;
using Infrastructure.ML.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.TorchSharp;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// NerTrainingPipeline — ML.NET TorchSharp NER model training (AC-03, TR-010)
// ---------------------------------------------------------------------------

/// <summary>
/// Configuration options for NER model training and storage.
/// Bind from the "NerModel" section in appsettings.
/// </summary>
public sealed class NerModelOptions
{
    public const string SectionName = "NerModel";

    /// <summary>Directory where trained model zip files are persisted. Relative to the application's base directory.</summary>
    public string ModelDirectory { get; set; } = "ML/Models";

    /// <summary>
    /// Confidence threshold in [0, 1]. Extractions with max-token confidence below this
    /// value are flagged as <see cref="ClinicalEntity.NeedsReview"/> = true.
    /// Inferred decision: 0.85 chosen as conservative clinical default.
    /// </summary>
    public float ConfidenceThreshold { get; set; } = 0.85f;

    /// <summary>Number of fine-tuning epochs per training run. Default: 10.</summary>
    public int TrainingEpochs { get; set; } = 10;

    /// <summary>Mini-batch size during training. Default: 32.</summary>
    public int BatchSize { get; set; } = 32;
}

/// <summary>
/// Trains or retrains the clinical NER model using ML.NET TorchSharp (AC-03, TR-010).
///
/// Training data format: each <see cref="NerLabeledDocument"/> supplies a sentence and
/// a parallel array of IOB2 tags (one per whitespace-delimited word token).
///
/// IOB2 label inventory:
///   B-PERSON / I-PERSON   — patient or provider name
///   B-DATE   / I-DATE     — calendar date or date range
///   B-DIAGNOSIS / I-DIAGNOSIS — clinical diagnosis or condition
///   B-MEDICATION / I-MEDICATION — medication name
///   B-PROCEDURE / I-PROCEDURE — clinical procedure
///   O                     — outside any entity
///
/// Saved model files follow the naming convention:
///   ner-{modelVersion}_{timestamp:yyyyMMddHHmmss}.zip
/// </summary>
public sealed class NerTrainingPipeline : INerTrainingPipeline
{
    private readonly NerModelOptions _options;
    private readonly ILogger<NerTrainingPipeline> _logger;

    public NerTrainingPipeline(
        IOptions<NerModelOptions> options,
        ILogger<NerTrainingPipeline> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<string> TrainAsync(
        IReadOnlyList<NerLabeledDocument> labeledDocuments,
        string modelVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(labeledDocuments);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelVersion);

        if (labeledDocuments.Count == 0)
            throw new ArgumentException("Training corpus must contain at least one document.", nameof(labeledDocuments));

        // CPU-bound ML training — run on a thread pool thread so callers can remain async.
        return Task.Run(() => Train(labeledDocuments, modelVersion, cancellationToken), cancellationToken);
    }

    private string Train(
        IReadOnlyList<NerLabeledDocument> labeledDocuments,
        string modelVersion,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "NER training started. Documents: {Count}, Version: {Version}",
            labeledDocuments.Count, modelVersion);

        var mlContext = new MLContext(seed: 42);

        // Map labeled documents to the ML.NET training schema.
        var trainingRecords = labeledDocuments
            .Select(d => new NerTrainingRecord
            {
                Sentence = d.Sentence,
                Tags = d.Tags
            })
            .ToList();

        var dataView = mlContext.Data.LoadFromEnumerable(trainingRecords);

        cancellationToken.ThrowIfCancellationRequested();

        // Build the TorchSharp NER training pipeline (TR-010).
        // API: NamedEntityRecognition(labelColumnName, outputColumnName, sentence1ColumnName, ...)
        // labelColumnName must be a Vector<string> (IOB2 tags). outputColumnName is the prediction.
        var pipeline = mlContext.MulticlassClassification.Trainers.NamedEntityRecognition(
            labelColumnName: "Tags",
            outputColumnName: "PredictedTags",
            sentence1ColumnName: "Sentence");

        _logger.LogInformation(
            "Fitting NER model. Epochs: {Epochs}, BatchSize: {BatchSize}",
            _options.TrainingEpochs, _options.BatchSize);

        var model = pipeline.Fit(dataView);

        cancellationToken.ThrowIfCancellationRequested();

        var outputPath = BuildModelPath(modelVersion);
        EnsureModelDirectory(outputPath);

        mlContext.Model.Save(model, dataView.Schema, outputPath);

        _logger.LogInformation("NER model saved to {Path}", outputPath);

        return outputPath;
    }

    private string BuildModelPath(string modelVersion)
    {
        var baseDir = Path.IsPathRooted(_options.ModelDirectory)
            ? _options.ModelDirectory
            : Path.Combine(AppContext.BaseDirectory, _options.ModelDirectory);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var fileName = $"ner-{modelVersion}_{timestamp}.zip";
        return Path.Combine(baseDir, fileName);
    }

    private static void EnsureModelDirectory(string modelFilePath)
    {
        var dir = Path.GetDirectoryName(modelFilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }
}

/// <summary>
/// ML.NET training record mapping sentence text and IOB2 tags to column names expected
/// by the <see cref="NamedEntityRecognitionTrainer"/>.
/// </summary>
file sealed class NerTrainingRecord
{
    [Microsoft.ML.Data.ColumnName("Sentence")]
    public string Sentence { get; set; } = string.Empty;

    [Microsoft.ML.Data.ColumnName("Tags")]
    public string[] Tags { get; set; } = [];
}
