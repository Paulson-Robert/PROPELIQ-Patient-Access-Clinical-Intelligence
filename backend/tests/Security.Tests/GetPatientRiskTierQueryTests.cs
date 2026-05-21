using Application.Interfaces;
using Application.Queries;
using Xunit;

namespace Security.Tests;

// ---------------------------------------------------------------------------
// GetPatientRiskTierQueryTests
// AC-01: tier classification thresholds (0–30 Low, 31–70 Medium, 71–100 High)
// AC-02: tier response includes contributing factor breakdown
// Edge Case: boundary scores produce consistent tier assignment
// ---------------------------------------------------------------------------

public sealed class GetPatientRiskTierQueryTests
{
    // -----------------------------------------------------------------------
    // Stub
    // -----------------------------------------------------------------------

    private sealed class StubRiskScoring : IRiskScoringService
    {
        public RiskAppointmentDataDto? Data { get; set; }

        public Task<RiskScoringResult> CalculateAndPersistAsync(
            Guid appointmentId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<RiskAppointmentDataDto?> GetAppointmentRiskDataAsync(
            Guid appointmentId, CancellationToken ct)
            => Task.FromResult(Data);
    }

    private static GetPatientRiskTierQueryHandler BuildHandler(RiskAppointmentDataDto? data)
    {
        var stub = new StubRiskScoring { Data = data };
        return new GetPatientRiskTierQueryHandler(stub);
    }

    private static RiskAppointmentDataDto ScoredData(decimal score, bool pending = false)
        => new(score, pending, [
            new("historical_no_show_count", 16m, "2"),
            new("appointment_lead_time",    20m, "2.0 days"),
            new("time_of_day",             10m, "unknown"),
            new("new_patient",             15m, "true"),
        ]);

    // -----------------------------------------------------------------------
    // AC-01 — tier classification
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0,   "Low")]
    [InlineData(15,  "Low")]
    [InlineData(30,  "Low")]    // boundary — inclusive upper end of Low
    [InlineData(31,  "Medium")] // boundary — first Medium score
    [InlineData(50,  "Medium")]
    [InlineData(70,  "Medium")] // boundary — inclusive upper end of Medium
    [InlineData(71,  "High")]   // boundary — first High score
    [InlineData(85,  "High")]
    [InlineData(100, "High")]
    public async Task Handle_ClassifiesTierCorrectly(int score, string expectedTier)
    {
        var handler = BuildHandler(ScoredData(score));
        var result = await handler.Handle(
            new GetPatientRiskTierQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedTier, result.Tier);
        Assert.Equal(score, (int)result.Score);
    }

    // -----------------------------------------------------------------------
    // AC-02 — contributing factor breakdown
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ReturnsFourContributingFactors()
    {
        var handler = BuildHandler(ScoredData(55m));
        var result = await handler.Handle(
            new GetPatientRiskTierQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(4, result.ContributingFactors.Count);
    }

    [Fact]
    public async Task Handle_FactorNamesMatchExpectedKeys()
    {
        var handler = BuildHandler(ScoredData(55m));
        var result = await handler.Handle(
            new GetPatientRiskTierQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(result);
        var names = result.ContributingFactors.Select(f => f.FactorName).ToArray();
        Assert.Contains("historical_no_show_count", names);
        Assert.Contains("appointment_lead_time",    names);
        Assert.Contains("time_of_day",              names);
        Assert.Contains("new_patient",              names);
    }

    // -----------------------------------------------------------------------
    // Edge Case — appointment not found → null
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_ReturnsNull_WhenAppointmentNotFound()
    {
        var handler = BuildHandler(data: null);
        var result = await handler.Handle(
            new GetPatientRiskTierQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    // -----------------------------------------------------------------------
    // Edge Case — data pending
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_SetsIsDataPending_WhenFactorsAbsent()
    {
        var pendingData = new RiskAppointmentDataDto(50m, IsDataPending: true, ContributingFactors: []);
        var handler = BuildHandler(pendingData);
        var result = await handler.Handle(
            new GetPatientRiskTierQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsDataPending);
        Assert.Equal("Medium", result.Tier); // 50 falls in Medium band
        Assert.Empty(result.ContributingFactors);
    }

    // -----------------------------------------------------------------------
    // Edge Case — boundary scores: exact 30, 31, 70, 71 (AC-01 explicit boundaries)
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(30,  "Low")]
    [InlineData(31,  "Medium")]
    [InlineData(70,  "Medium")]
    [InlineData(71,  "High")]
    public async Task Handle_BoundaryScores_ProduceConsistentTier(int score, string expectedTier)
    {
        var handler = BuildHandler(ScoredData(score));
        var result = await handler.Handle(
            new GetPatientRiskTierQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedTier, result.Tier);
    }
}
