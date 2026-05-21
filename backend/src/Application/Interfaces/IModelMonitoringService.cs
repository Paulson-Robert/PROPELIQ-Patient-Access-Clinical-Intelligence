namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// IModelMonitoringService — NER accuracy monitoring contract (AC-02, AC-03, AC-05, US_037)
// ---------------------------------------------------------------------------

/// <summary>
/// Represents the computed accuracy for one model version over a rolling window (AC-02).
/// </summary>
/// <param name="ModelVersion">Model version identifier, e.g. "NER-v1.2.0".</param>
/// <param name="WindowStart">Start of the rolling window (UTC).</param>
/// <param name="WindowEnd">End of the rolling window (UTC).</param>
/// <param name="VerifiedFieldCount">Number of human-verified fields in the window.</param>
/// <param name="AgreementRate">
/// Agreement rate in [0, 1]: (accepted fields) / (verified fields).
/// <c>null</c> when <see cref="VerifiedFieldCount"/> is below the minimum threshold (edge case: insufficient data).
/// </param>
/// <param name="IsSufficientData">
/// <c>true</c> when the window contains enough verified fields to compute a meaningful rate.
/// </param>
public sealed record EntityAgreementResult(
    string ModelVersion,
    DateTime WindowStart,
    DateTime WindowEnd,
    int VerifiedFieldCount,
    double? AgreementRate,
    bool IsSufficientData);

/// <summary>
/// Accuracy result for code-mapping verifications (AC-05).
/// </summary>
/// <param name="CodeType">e.g. "ICD10" or "CPT".</param>
/// <param name="VerifiedCodeCount">Number of human-verified code mappings in the window.</param>
/// <param name="AgreementRate">
/// Agreement rate in [0, 1].
/// <c>null</c> when <see cref="VerifiedCodeCount"/> is below the minimum threshold (edge case).
/// </param>
/// <param name="IsSufficientData">
/// <c>true</c> when the window contains enough verified codes to compute a meaningful rate.
/// </param>
public sealed record CodeAgreementResult(
    string CodeType,
    int VerifiedCodeCount,
    double? AgreementRate,
    bool IsSufficientData);

/// <summary>
/// Computes AI–Human agreement rates for entity extractions and code mappings (AC-02, AC-05).
/// </summary>
public interface IModelMonitoringService
{
    /// <summary>
    /// Calculates the entity extraction agreement rate over a rolling window for the
    /// currently active model version (AC-02).
    /// Returns a result with <see cref="EntityAgreementResult.IsSufficientData"/> = false
    /// when fewer than the minimum verified fields exist in the window (edge case).
    /// </summary>
    Task<EntityAgreementResult> GetEntityAgreementRateAsync(
        string modelVersion,
        TimeSpan rollingWindow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the code-mapping agreement rate over a rolling window, per code type (AC-05).
    /// Returns a result with <see cref="CodeAgreementResult.IsSufficientData"/> = false
    /// when fewer than the minimum verified codes exist in the window (edge case).
    /// </summary>
    Task<IReadOnlyList<CodeAgreementResult>> GetCodeAgreementRatesAsync(
        TimeSpan rollingWindow,
        CancellationToken cancellationToken = default);
}
