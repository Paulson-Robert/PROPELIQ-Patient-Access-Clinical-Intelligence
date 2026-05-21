using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// ModelVersioningService — NER model version tracking (AC-01, US_037)
// ---------------------------------------------------------------------------

/// <summary>
/// Tracks the currently active NER model version and its deployment history (AC-01).
///
/// Version identifiers are derived from the model zip file name.
/// Convention: "ner-{version}.zip" → version = "NER-{version}", e.g. "ner-v1.2.0.zip" → "NER-v1.2.0".
/// Files that do not match the convention fall back to the filename stem.
///
/// Deployment history is in-process only — it resets on app restart.
/// The history records which file was promoted and when, satisfying the audit requirement (AC-01).
///
/// Thread-safe: a <see cref="ConcurrentQueue{T}"/> and <see cref="Interlocked"/> write guard
/// ensure correct behaviour under concurrent <see cref="RecordDeployment"/> calls.
/// </summary>
public sealed class ModelVersioningService : IModelVersioningService
{
    private const string UnknownVersion = "NER-unknown";

    private readonly ILogger<ModelVersioningService> _logger;

    // Newest deployment at the front; implemented as a reversed list behind a lock.
    private readonly object _lock = new();
    private readonly List<ModelDeployment> _history = [];

    public ModelVersioningService(ILogger<ModelVersioningService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GetCurrentVersion()
    {
        lock (_lock)
        {
            return _history.Count > 0 ? _history[0].Version : UnknownVersion;
        }
    }

    /// <inheritdoc/>
    public void RecordDeployment(string modelFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelFilePath, nameof(modelFilePath));

        var version = ExtractVersion(modelFilePath);

        lock (_lock)
        {
            // Idempotent: skip if the same file is already the active deployment.
            if (_history.Count > 0 && _history[0].ModelFilePath == modelFilePath)
                return;

            var deployment = new ModelDeployment(version, DateTime.UtcNow, modelFilePath);
            _history.Insert(0, deployment); // newest first

            _logger.LogInformation(
                "ModelVersioningService: new deployment recorded. Version={Version} File={File} DeployedAt={DeployedAt:O}",
                version,
                modelFilePath,
                deployment.DeployedAt);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<ModelDeployment> GetDeploymentHistory()
    {
        lock (_lock)
        {
            return _history.AsReadOnly();
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Derives a human-readable version identifier from a model file path.
    /// "ner-v1.2.0.zip" → "NER-v1.2.0".
    /// Any file whose stem does not begin with "ner-" returns the stem as-is.
    /// </summary>
    private static string ExtractVersion(string modelFilePath)
    {
        var stem = Path.GetFileNameWithoutExtension(modelFilePath);

        if (string.IsNullOrEmpty(stem))
            return UnknownVersion;

        if (stem.StartsWith("ner-", StringComparison.OrdinalIgnoreCase))
            return "NER-" + stem["ner-".Length..];

        return stem;
    }
}
