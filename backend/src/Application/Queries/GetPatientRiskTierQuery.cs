using Application.Interfaces;
using MediatR;

namespace Application.Queries;

// ---------------------------------------------------------------------------
// DTOs — AC-02: tier response with contributing factor breakdown
// ---------------------------------------------------------------------------

/// <summary>Individual factor contribution surfaced in the risk tier response.</summary>
/// <param name="FactorName">Machine-readable factor identifier.</param>
/// <param name="ContributionPoints">Points this factor contributed to the overall score.</param>
/// <param name="RawValue">Human-readable raw value used in scoring.</param>
public sealed record RiskTierFactor(
    string FactorName,
    decimal ContributionPoints,
    string RawValue);

/// <summary>
/// Response DTO returned by <see cref="GetPatientRiskTierQuery"/>.
/// </summary>
/// <param name="Tier">Classified risk tier: Low, Medium, or High (AC-01).</param>
/// <param name="Score">Numeric risk score (0–100).</param>
/// <param name="IsDataPending">True when risk factors were unavailable at scoring time.</param>
/// <param name="ContributingFactors">Per-factor contribution breakdown (AC-02).</param>
public sealed record RiskTierDto(
    string Tier,
    decimal Score,
    bool IsDataPending,
    IReadOnlyList<RiskTierFactor> ContributingFactors);

// ---------------------------------------------------------------------------
// Query
// ---------------------------------------------------------------------------

/// <summary>
/// Returns the risk tier and contributing factor breakdown for a scored appointment (US_042).
/// Reads persisted score — does not recalculate.
/// Returns <c>null</c> when the appointment does not exist.
/// </summary>
public sealed record GetPatientRiskTierQuery(Guid AppointmentId) : IRequest<RiskTierDto?>;

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

internal sealed class GetPatientRiskTierQueryHandler
    : IRequestHandler<GetPatientRiskTierQuery, RiskTierDto?>
{
    // AC-01: tier thresholds — Low: 0–30, Medium: 31–70, High: 71–100
    // Edge Case: boundary values are inclusive at the upper end of each band
    //   score 30  → Low
    //   score 31  → Medium
    //   score 70  → Medium
    //   score 71  → High
    private const decimal LowMax    = 30m;
    private const decimal MediumMax = 70m;

    private readonly IRiskScoringService _riskScoring;

    public GetPatientRiskTierQueryHandler(IRiskScoringService riskScoring)
    {
        _riskScoring = riskScoring;
    }

    public async Task<RiskTierDto?> Handle(
        GetPatientRiskTierQuery request,
        CancellationToken cancellationToken)
    {
        var data = await _riskScoring
            .GetAppointmentRiskDataAsync(request.AppointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
            return null;

        var tier = ClassifyTier(data.Score);

        IReadOnlyList<RiskTierFactor> factors = data.ContributingFactors
            .Select(f => new RiskTierFactor(f.FactorName, f.ContributionPoints, f.RawValue))
            .ToList()
            .AsReadOnly();

        return new RiskTierDto(tier, data.Score, data.IsDataPending, factors);
    }

    // internal static — accessible in unit tests via InternalsVisibleTo or tested via Handle
    internal static string ClassifyTier(decimal score) => score switch
    {
        <= LowMax    => "Low",
        <= MediumMax => "Medium",
        _            => "High",
    };
}
