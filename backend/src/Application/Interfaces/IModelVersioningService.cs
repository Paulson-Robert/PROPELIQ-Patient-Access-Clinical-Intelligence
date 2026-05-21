namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// IModelVersioningService — NER model version tracking contract (AC-01, US_037)
// ---------------------------------------------------------------------------

/// <summary>
/// A snapshot of a model version deployment (AC-01).
/// </summary>
/// <param name="Version">Semantic version string, e.g. "NER-v1.2.0".</param>
/// <param name="DeployedAt">UTC timestamp when this version became active.</param>
/// <param name="ModelFilePath">Absolute path to the model zip file that was loaded.</param>
public sealed record ModelDeployment(
    string Version,
    DateTime DeployedAt,
    string ModelFilePath);

/// <summary>
/// Tracks which NER model version is currently active and maintains a deployment history (AC-01).
/// </summary>
public interface IModelVersioningService
{
    /// <summary>
    /// Returns the version string for the currently loaded NER model (AC-01).
    /// </summary>
    string GetCurrentVersion();

    /// <summary>
    /// Records that a model file was loaded / promoted to active (AC-01).
    /// Idempotent: calling twice with the same <paramref name="modelFilePath"/> is a no-op.
    /// </summary>
    /// <param name="modelFilePath">Absolute path to the model zip file.</param>
    void RecordDeployment(string modelFilePath);

    /// <summary>
    /// Returns the full deployment history ordered from newest to oldest (AC-01).
    /// </summary>
    IReadOnlyList<ModelDeployment> GetDeploymentHistory();
}
