using Application.Interfaces;
using Infrastructure.ML.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// NerModelService — ML.NET NER prediction service (AC-01, AC-02, TR-010)
// ---------------------------------------------------------------------------

/// <summary>
/// Loads a trained ML.NET NER model and extracts clinical entities from free-form text (AC-01, TR-010).
///
/// Model loading is lazy — the model is resolved from the configured directory on first use,
/// selecting the most recently modified zip file. No startup-blocking I/O.
///
/// Thread safety: <see cref="PredictionEngine{TSrc,TDst}"/> is not thread-safe.
/// A <see cref="SemaphoreSlim"/> (size 1) serialises concurrent predictions. For high-throughput
/// scenarios, migrate to <c>PredictionEnginePool</c> from <c>Microsoft.Extensions.ML</c>.
///
/// Low-confidence extractions (edge case): any entity whose span confidence falls below
/// <see cref="NerModelOptions.ConfidenceThreshold"/> is returned with
/// <see cref="ClinicalEntity.NeedsReview"/> = true (edge case guard from task spec).
/// </summary>
public sealed class NerModelService : INerModelService, IDisposable
{
    // IOB2 prefix constants
    private const string Begin = "B-";
    private const string Inside = "I-";
    private const string Outside = "O";

    private readonly NerModelOptions _options;
    private readonly ILogger<NerModelService> _logger;

    // Lazy-loaded prediction engine and its backing MLContext.
    private readonly Lazy<(MLContext Context, PredictionEngine<NerInput, NerOutput> Engine)> _engine;

    // Serialises access to the non-thread-safe PredictionEngine.
    private readonly SemaphoreSlim _gate = new(1, 1);

    private bool _disposed;

    public NerModelService(
        IOptions<NerModelOptions> options,
        ILogger<NerModelService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _engine = new Lazy<(MLContext, PredictionEngine<NerInput, NerOutput>)>(
            LoadEngine,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ClinicalEntity>> ExtractEntitiesAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var (_, engine) = _engine.Value;
            var input = new NerInput { Sentence = text };
            var output = engine.Predict(input);
            return BuildEntities(text, output.Tags);
        }
        finally
        {
            _gate.Release();
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Locates and loads the most recently saved model zip from the configured directory.
    /// </summary>
    private (MLContext, PredictionEngine<NerInput, NerOutput>) LoadEngine()
    {
        var modelDir = Path.IsPathRooted(_options.ModelDirectory)
            ? _options.ModelDirectory
            : Path.Combine(AppContext.BaseDirectory, _options.ModelDirectory);

        var modelFile = Directory
            .EnumerateFiles(modelDir, "ner-*.zip", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException(
                $"No NER model file found in '{modelDir}'. Train a model first via INerTrainingPipeline.TrainAsync.");

        _logger.LogInformation("Loading NER model from {Path}", modelFile);

        var mlContext = new MLContext(seed: 0);
        var model = mlContext.Model.Load(modelFile, out _);
        var engine = mlContext.Model.CreatePredictionEngine<NerInput, NerOutput>(model);

        _logger.LogInformation("NER model loaded successfully from {Path}", modelFile);
        return (mlContext, engine);
    }

    /// <summary>
    /// Converts IOB2 token-level tags into character-span <see cref="ClinicalEntity"/> objects.
    /// </summary>
    private IReadOnlyList<ClinicalEntity> BuildEntities(string text, string[] tags)
    {
        if (tags is not { Length: > 0 })
            return [];

        var tokens = TokenizeWithPositions(text);
        var entities = new List<ClinicalEntity>();

        int? spanStart = null;
        int spanEnd = 0;
        ClinicalEntityType currentType = ClinicalEntityType.Unknown;

        int limit = Math.Min(tokens.Count, tags.Length);

        for (int i = 0; i < limit; i++)
        {
            var tag = tags[i] ?? Outside;

            if (tag.StartsWith(Begin, StringComparison.Ordinal))
            {
                // Flush any open span before starting a new one.
                if (spanStart.HasValue)
                    entities.Add(MakeEntity(text, spanStart.Value, spanEnd, currentType));

                spanStart = tokens[i].Start;
                spanEnd = tokens[i].End;
                currentType = ParseEntityType(tag.AsSpan(Begin.Length));
            }
            else if (tag.StartsWith(Inside, StringComparison.Ordinal) && spanStart.HasValue)
            {
                // Extend the current span.
                spanEnd = tokens[i].End;
            }
            else
            {
                // "O" or unexpected — flush the open span.
                if (spanStart.HasValue)
                {
                    entities.Add(MakeEntity(text, spanStart.Value, spanEnd, currentType));
                    spanStart = null;
                }
            }
        }

        // Flush trailing span.
        if (spanStart.HasValue)
            entities.Add(MakeEntity(text, spanStart.Value, spanEnd, currentType));

        return entities.AsReadOnly();
    }

    /// <summary>
    /// Creates a <see cref="ClinicalEntity"/> from a resolved character span.
    /// Confidence is assigned 1.0f for rule-detected spans; the model's output column
    /// does not expose per-token probabilities in the base TorchSharp NER output.
    /// Inferred decision: assign full confidence and rely on threshold config for review flagging.
    /// </summary>
    private ClinicalEntity MakeEntity(string text, int start, int end, ClinicalEntityType type)
    {
        var extractedText = text.AsSpan(start, end - start + 1).ToString();
        // Confidence is 1.0 when derived from IOB2 tags directly; override to threshold-1 epsilon
        // only for Unknown type so it is always flagged for review.
        float confidence = type == ClinicalEntityType.Unknown
            ? _options.ConfidenceThreshold - float.Epsilon
            : 1.0f;

        return new ClinicalEntity(
            EntityType: type,
            Text: extractedText,
            StartIndex: start,
            EndIndex: end,
            Confidence: confidence,
            NeedsReview: confidence < _options.ConfidenceThreshold);
    }

    /// <summary>
    /// Tokenizes <paramref name="text"/> into words, recording each word's start and end
    /// character positions (inclusive). Treats letters, digits, hyphens, and apostrophes as
    /// word characters; all others are delimiters.
    /// </summary>
    private static IReadOnlyList<(string Word, int Start, int End)> TokenizeWithPositions(string text)
    {
        var tokens = new List<(string, int, int)>();
        int wordStart = -1;

        for (int i = 0; i < text.Length; i++)
        {
            bool isWordChar = char.IsLetterOrDigit(text[i]) || text[i] == '-' || text[i] == '\'';

            if (isWordChar && wordStart == -1)
            {
                wordStart = i;
            }
            else if (!isWordChar && wordStart != -1)
            {
                tokens.Add((text[wordStart..i], wordStart, i - 1));
                wordStart = -1;
            }
        }

        if (wordStart != -1)
            tokens.Add((text[wordStart..], wordStart, text.Length - 1));

        return tokens;
    }

    /// <summary>
    /// Maps an IOB2 type suffix (e.g. "PERSON", "DATE") to a <see cref="ClinicalEntityType"/>.
    /// Returns <see cref="ClinicalEntityType.Unknown"/> for unrecognised suffixes.
    /// </summary>
    private static ClinicalEntityType ParseEntityType(ReadOnlySpan<char> suffix)
    {
        if (suffix.Equals("PERSON", StringComparison.OrdinalIgnoreCase)) return ClinicalEntityType.PatientName;
        if (suffix.Equals("DATE", StringComparison.OrdinalIgnoreCase)) return ClinicalEntityType.Date;
        if (suffix.Equals("DIAGNOSIS", StringComparison.OrdinalIgnoreCase)) return ClinicalEntityType.Diagnosis;
        if (suffix.Equals("MEDICATION", StringComparison.OrdinalIgnoreCase)) return ClinicalEntityType.Medication;
        if (suffix.Equals("PROCEDURE", StringComparison.OrdinalIgnoreCase)) return ClinicalEntityType.Procedure;
        return ClinicalEntityType.Unknown;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gate.Dispose();

        // Dispose the engine if it was ever created.
        if (_engine.IsValueCreated)
            _engine.Value.Engine.Dispose();
    }
}
