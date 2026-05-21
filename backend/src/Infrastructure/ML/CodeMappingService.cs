using Application.Configuration;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// CodeMappingService — ICD-10/CPT mapping engine (AC-01, AC-02, AC-03, AC-04)
// ---------------------------------------------------------------------------

/// <summary>
/// Implements <see cref="ICodeMappingService"/> using a two-stage strategy:
///
///   Stage 1 — Rule-based (via <see cref="RuleBasedCodeMapper"/>):
///     Exact and partial lookups against a curated table; confidence ≥ 0.82.
///
///   Stage 2 — ML.NET (text classification fallback):
///     Applied when rule-based yields no match. Loads a trained model from
///     <see cref="CodeMappingOptions.ModelDirectory"/>. When the model file
///     is absent the service degrades gracefully — no exception is thrown and
///     no candidates are returned for that entity (AC-01 edge case).
///
/// Edge case — ambiguous diagnosis: multiple candidates are returned per entity
/// in descending confidence order (see AC-01 edge case in task spec).
///
/// Thread safety: <see cref="PredictionEngine{TSrc,TDst}"/> is not thread-safe;
/// a <see cref="SemaphoreSlim"/> serialises concurrent ML predictions.
///
/// PHI boundary (security): every call is scoped to a single <c>patientId</c>;
/// entities from different patients must never share a call.
/// </summary>
public sealed class CodeMappingService : ICodeMappingService, IDisposable
{
    private readonly RuleBasedCodeMapper _ruleMapper = new();
    private readonly CodeMappingOptions _options;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CodeMappingService> _logger;

    // Lazy ML.NET engine — loaded on first ML fallback call.
    private readonly Lazy<PredictionEngine<CodeMappingInput, CodeMappingPrediction>?> _mlEngine;

    // Serialises access to the non-thread-safe PredictionEngine.
    private readonly SemaphoreSlim _gate = new(1, 1);

    private bool _disposed;

    public CodeMappingService(
        IOptions<CodeMappingOptions> options,
        ApplicationDbContext db,
        ILogger<CodeMappingService> logger)
    {
        _options = options.Value;
        _db = db;
        _logger = logger;
        _mlEngine = new Lazy<PredictionEngine<CodeMappingInput, CodeMappingPrediction>?>(
            TryLoadMlEngine,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    // -----------------------------------------------------------------------
    // AC-01, AC-02: Map clinical entities to ranked code candidates
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CodeMappingCandidate>> MapAsync(
        IReadOnlyList<ClinicalEntity> entities,
        string patientId,
        CancellationToken cancellationToken = default)
    {
        // PHI boundary: every call must be scoped to one patient.
        ArgumentException.ThrowIfNullOrWhiteSpace(patientId, nameof(patientId));
        ArgumentNullException.ThrowIfNull(entities);

        if (entities.Count == 0)
            return [];

        var results = new List<CodeMappingCandidate>();

        foreach (var entity in entities)
        {
            // Only Diagnosis and Procedure entities are ICD-10/CPT mappable.
            if (entity.EntityType is not (ClinicalEntityType.Diagnosis or ClinicalEntityType.Procedure))
                continue;

            // Stage 1: rule-based lookup
            var ruleCandidates = _ruleMapper.Map(entity.Text, _options.Icd10Version, _options.CptVersion);
            if (ruleCandidates.Count > 0)
            {
                results.AddRange(ruleCandidates);
                continue;
            }

            // Stage 2: ML.NET fallback
            var mlCandidates = await MapWithMlAsync(entity, cancellationToken).ConfigureAwait(false);
            if (mlCandidates.Count > 0)
            {
                results.AddRange(mlCandidates);
                continue;
            }

            _logger.LogDebug(
                "No mapping found for entity PatientId={PatientId} EntityType={EntityType} Text={Text}",
                patientId, entity.EntityType, entity.Text);
        }

        _logger.LogInformation(
            "Code mapping complete PatientId={PatientId} InputEntities={InputCount} Candidates={CandidateCount}",
            patientId, entities.Count, results.Count);

        return results.AsReadOnly();
    }

    // -----------------------------------------------------------------------
    // AC-03, AC-04: Persist staff verification decision + audit log
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<VerifyCodeMappingResult> VerifyAsync(
        VerifyCodeMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // AC-04: reason is mandatory for Modify and Reject
        if (request.Action is VerifyCodeAction.Modify or VerifyCodeAction.Reject &&
            string.IsNullOrWhiteSpace(request.Reason))
        {
            return new VerifyCodeMappingResult(false, "REASON_REQUIRED",
                "A reason is required when modifying or rejecting a code mapping.");
        }

        var mapping = await _db.MedicalCodeMappings
            .FindAsync([request.MappingId], cancellationToken)
            .ConfigureAwait(false);

        if (mapping is null)
        {
            return new VerifyCodeMappingResult(false, "MAPPING_NOT_FOUND",
                $"MedicalCodeMapping {request.MappingId} was not found.");
        }

        var oldCodeValue = mapping.CodeValue;

        switch (request.Action)
        {
            case VerifyCodeAction.Verify:
                mapping.IsVerified = true;
                mapping.VerifiedByUserId = request.StaffUserId;
                mapping.VerifiedAt = DateTime.UtcNow;
                break;

            case VerifyCodeAction.Modify:
                // AC-03: update code value when staff supplies a replacement
                if (!string.IsNullOrWhiteSpace(request.ModifiedCodeValue))
                    mapping.CodeValue = request.ModifiedCodeValue.Trim();
                mapping.IsVerified = true;
                mapping.VerifiedByUserId = request.StaffUserId;
                mapping.VerifiedAt = DateTime.UtcNow;
                break;

            case VerifyCodeAction.Reject:
                // Rejection leaves IsVerified = false — record is preserved for audit.
                // The rejected status is captured in the immutable AuditLog below (AC-04).
                break;
        }

        // AC-04: immutable audit trail — append only, never update or delete
        _db.AuditLogs.Add(new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            ActorUserId = request.StaffUserId,
            ActorRole = request.StaffRole,
            ActionType = $"CodeMapping{request.Action}",
            ResourceType = nameof(MedicalCodeMapping),
            ResourceId = request.MappingId.ToString(),
            Details = JsonSerializer.Serialize(new
            {
                reason = request.Reason,
                oldCodeValue,
                newCodeValue = mapping.CodeValue,
            }),
        });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Code mapping {Action} persisted MappingId={MappingId} StaffUserId={StaffUserId}",
            request.Action, request.MappingId, request.StaffUserId);

        return new VerifyCodeMappingResult(true);
    }

    // -----------------------------------------------------------------------
    // ML.NET helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Runs the ML.NET prediction engine against a single entity.
    /// Returns an empty list when the model is unavailable — never throws.
    /// </summary>
    private async Task<IReadOnlyList<CodeMappingCandidate>> MapWithMlAsync(
        ClinicalEntity entity,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var engine = _mlEngine.Value;
            if (engine is null)
                return [];

            var prediction = engine.Predict(new CodeMappingInput
            {
                EntityText = entity.Text,
                EntityType = entity.EntityType.ToString(),
            });

            if (string.IsNullOrEmpty(prediction.PredictedLabel) ||
                prediction.Score is null ||
                prediction.Score.Length == 0)
            {
                return [];
            }

            // PredictedLabel format: "ICD10:E11.9:Type 2 diabetes mellitus without complications"
            // or "CPT:99213:Office visit, established patient, low-mod complexity"
            var parts = prediction.PredictedLabel.Split(':', 3);
            if (parts.Length != 3)
                return [];

            if (!Enum.TryParse<MedicalCodeType>(parts[0], ignoreCase: true, out var codeType))
                return [];

            var confidence = (decimal)(prediction.Score.Max());

            return new[]
            {
                new CodeMappingCandidate(
                    entity.Text,
                    codeType,
                    parts[1],
                    parts[2],
                    Math.Round(confidence, 2),
                    "ML model",
                    codeType == MedicalCodeType.ICD10 ? _options.Icd10Version : _options.CptVersion),
            };
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Loads the ML.NET model from disk. Returns null when the model file is absent —
    /// the service then operates in rule-based-only mode.
    /// </summary>
    private PredictionEngine<CodeMappingInput, CodeMappingPrediction>? TryLoadMlEngine()
    {
        var modelPath = Path.Combine(_options.ModelDirectory, "code-mapping-model.zip");

        if (!File.Exists(modelPath))
        {
            _logger.LogWarning(
                "Code mapping ML model not found at {ModelPath}. " +
                "Service will operate in rule-based-only mode.",
                modelPath);
            return null;
        }

        try
        {
            var mlContext = new MLContext(seed: 42);
            var model = mlContext.Model.Load(modelPath, out _);
            _logger.LogInformation("Code mapping ML model loaded from {ModelPath}", modelPath);
            return mlContext.Model.CreatePredictionEngine<CodeMappingInput, CodeMappingPrediction>(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load code mapping ML model from {ModelPath}", modelPath);
            return null;
        }
    }

    // -----------------------------------------------------------------------
    // ML.NET model schema — private to this service
    // -----------------------------------------------------------------------

    private sealed class CodeMappingInput
    {
        [LoadColumn(0)]
        public string EntityText { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string EntityType { get; set; } = string.Empty;
    }

    private sealed class CodeMappingPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = string.Empty;

        public float[] Score { get; set; } = [];
    }

    // -----------------------------------------------------------------------
    // IDisposable
    // -----------------------------------------------------------------------

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gate.Dispose();
        if (_mlEngine.IsValueCreated)
            _mlEngine.Value?.Dispose();
    }
}
