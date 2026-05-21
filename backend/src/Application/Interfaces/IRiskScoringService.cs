namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Result returned by <see cref="IRiskScoringService.CalculateAndPersistAsync"/>.</summary>
/// <param name="Score">Computed risk score (0–100).</param>
/// <param name="Tier">Classified risk tier derived from Score.</param>
/// <param name="IsDataPending">True when the risk factor record was absent; score defaults to Medium.</param>
/// <param name="MissingFactors">List of factor names that could not be evaluated (AC-06 / Edge Case).</param>
public sealed record RiskScoringResult(
    decimal Score,
    Domain.Enums.NoShowRiskTier Tier,
    bool IsDataPending,
    IReadOnlyList<string> MissingFactors);

// ---------------------------------------------------------------------------
// Read DTOs (US_042 AC-02: tier with contributing factor breakdown)
// ---------------------------------------------------------------------------

/// <summary>Individual factor contribution to the no-show risk score.</summary>
/// <param name="FactorName">Machine-readable factor identifier.</param>
/// <param name="ContributionPoints">Points awarded to this factor (0–max weight).</param>
/// <param name="RawValue">Human-readable raw value used in scoring.</param>
public sealed record RiskFactorBreakdownItem(
    string FactorName,
    decimal ContributionPoints,
    string RawValue);

/// <summary>Read-only risk data for a scored appointment (US_042 AC-02).</summary>
/// <param name="Score">Persisted composite risk score (0–100).</param>
/// <param name="IsDataPending">True when risk factors were absent at calculation time.</param>
/// <param name="ContributingFactors">Per-factor contribution breakdown.</param>
public sealed record RiskAppointmentDataDto(
    decimal Score,
    bool IsDataPending,
    IReadOnlyList<RiskFactorBreakdownItem> ContributingFactors);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Deterministic 4-factor weighted no-show risk scoring algorithm (US_041, FR-033, AIR-007).
///
/// Factors (weights):
///   1. Historical no-show count  — 40 pts
///   2. Appointment lead time     — 25 pts (inverse: shorter lead time = higher risk)
///   3. Time-of-day pattern       — 20 pts
///   4. New-patient flag          — 15 pts
///
/// Total range: 0–100.
/// Score is reproducible given identical inputs (AC-02).
/// </summary>
public interface IRiskScoringService
{
    /// <summary>
    /// Calculates the no-show risk score for an appointment, persists it to
    /// <c>Appointment.NoShowRiskScore</c> and <c>Appointment.NoShowRiskTier</c>,
    /// and updates <c>NoShowRiskFactor.LastCalculatedAt</c>.
    /// </summary>
    /// <param name="appointmentId">The appointment to score.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Scoring result including score, tier, and any data-pending flags.</returns>
    Task<RiskScoringResult> CalculateAndPersistAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the persisted risk score and per-factor contribution breakdown for an appointment.
    /// Does not recalculate — reads already-persisted data (US_042 AC-02).
    /// Returns <c>null</c> when the appointment does not exist.
    /// Returns <see cref="RiskAppointmentDataDto.IsDataPending"/> = <c>true</c> when factors are absent.
    /// </summary>
    /// <param name="appointmentId">The appointment whose stored risk data to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<RiskAppointmentDataDto?> GetAppointmentRiskDataAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
