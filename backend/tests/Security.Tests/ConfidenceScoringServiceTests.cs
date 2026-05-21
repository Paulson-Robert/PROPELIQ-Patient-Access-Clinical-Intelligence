using Application.Configuration;
using Application.Interfaces;
using Infrastructure.ML;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Security.Tests;

// ---------------------------------------------------------------------------
// ConfidenceScoringServiceTests — scoring logic, threshold classification, PHI boundary
// (AC-01, AC-02, AC-03)
// ---------------------------------------------------------------------------

public sealed class ConfidenceScoringServiceTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ConfidenceScoringService BuildService(float autoAccept = 0.9f, float review = 0.7f)
    {
        var config = new ExtractionThresholdConfig
        {
            AutoAcceptThreshold = autoAccept,
            ReviewThreshold = review
        };
        var monitor = new StaticOptionsMonitor<ExtractionThresholdConfig>(config);
        return new ConfidenceScoringService(monitor, NullLogger<ConfidenceScoringService>.Instance);
    }

    private static ClinicalEntity Entity(float confidence, ClinicalEntityType type = ClinicalEntityType.Diagnosis) =>
        new(type, "test", 0, 3, confidence, confidence < 0.85f);

    // -----------------------------------------------------------------------
    // AC-01: confidence score carried on each entity
    // -----------------------------------------------------------------------

    [Fact]
    public void Score_ReturnsOneResultPerInputEntity_PreservingOrder()
    {
        var service = BuildService();
        var entities = new[]
        {
            Entity(0.95f, ClinicalEntityType.Diagnosis),
            Entity(0.75f, ClinicalEntityType.Medication),
            Entity(0.50f, ClinicalEntityType.Date)
        };

        var results = service.Score(entities, "patient-1");

        Assert.Equal(3, results.Count);
        Assert.Equal(entities[0], results[0].Entity);
        Assert.Equal(entities[1], results[1].Entity);
        Assert.Equal(entities[2], results[2].Entity);
    }

    [Fact]
    public void Score_EmptyEntities_ReturnsEmptyList()
    {
        var service = BuildService();
        var results = service.Score([], "patient-1");
        Assert.Empty(results);
    }

    // -----------------------------------------------------------------------
    // AC-02: threshold classification — auto-accept / review / reject
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0.9f, ExtractionDecision.AutoAccept)]   // exactly at boundary — inclusive
    [InlineData(0.95f, ExtractionDecision.AutoAccept)]
    [InlineData(1.0f, ExtractionDecision.AutoAccept)]
    public void Score_ConfidenceAtOrAboveAutoAccept_ReturnsAutoAccept(float confidence, ExtractionDecision expected)
    {
        var service = BuildService(autoAccept: 0.9f, review: 0.7f);
        var results = service.Score([Entity(confidence)], "patient-1");
        Assert.Equal(expected, results[0].Decision);
    }

    [Theory]
    [InlineData(0.7f, ExtractionDecision.Review)]   // exactly at review boundary — inclusive
    [InlineData(0.8f, ExtractionDecision.Review)]
    [InlineData(0.89f, ExtractionDecision.Review)]
    public void Score_ConfidenceInReviewBand_ReturnsReview(float confidence, ExtractionDecision expected)
    {
        var service = BuildService(autoAccept: 0.9f, review: 0.7f);
        var results = service.Score([Entity(confidence)], "patient-1");
        Assert.Equal(expected, results[0].Decision);
    }

    [Theory]
    [InlineData(0.69f, ExtractionDecision.Reject)]
    [InlineData(0.5f, ExtractionDecision.Reject)]
    [InlineData(0.0f, ExtractionDecision.Reject)]
    public void Score_ConfidenceBelowReview_ReturnsReject(float confidence, ExtractionDecision expected)
    {
        var service = BuildService(autoAccept: 0.9f, review: 0.7f);
        var results = service.Score([Entity(confidence)], "patient-1");
        Assert.Equal(expected, results[0].Decision);
    }

    [Fact]
    public void Score_ThresholdChange_AppliesOnlyToSubsequentCall()
    {
        // Demonstrates that threshold snapshot is per-call and does not bleed across calls.
        var config = new ExtractionThresholdConfig { AutoAcceptThreshold = 0.9f, ReviewThreshold = 0.7f };
        var monitor = new MutableOptionsMonitor<ExtractionThresholdConfig>(config);
        var service = new ConfidenceScoringService(monitor, NullLogger<ConfidenceScoringService>.Instance);

        // First call with high threshold — 0.8f lands in Review.
        var firstResult = service.Score([Entity(0.8f)], "patient-1");
        Assert.Equal(ExtractionDecision.Review, firstResult[0].Decision);

        // Raise AutoAccept threshold so 0.8f would become AutoAccept.
        monitor.Update(new ExtractionThresholdConfig { AutoAcceptThreshold = 0.75f, ReviewThreshold = 0.5f });

        // Second call — uses updated thresholds.
        var secondResult = service.Score([Entity(0.8f)], "patient-1");
        Assert.Equal(ExtractionDecision.AutoAccept, secondResult[0].Decision);
    }

    // -----------------------------------------------------------------------
    // AC-03: PHI boundary enforcement
    // -----------------------------------------------------------------------

    [Fact]
    public void Score_NullPatientId_ThrowsArgumentNullException()
    {
        var service = BuildService();
        Assert.Throws<ArgumentNullException>(() => service.Score([Entity(0.9f)], null!));
    }

    [Fact]
    public void Score_EmptyPatientId_ThrowsArgumentException()
    {
        var service = BuildService();
        Assert.Throws<ArgumentException>(() => service.Score([Entity(0.9f)], ""));
    }

    [Fact]
    public void Score_WhitespacePatientId_ThrowsArgumentException()
    {
        var service = BuildService();
        Assert.Throws<ArgumentException>(() => service.Score([Entity(0.9f)], "   "));
    }

    [Fact]
    public void Score_DifferentPatientIds_ProducesIsolatedResults()
    {
        // Verifies statelessness — separate calls with different patient IDs return independent results.
        var service = BuildService();
        var entityA = Entity(0.95f, ClinicalEntityType.PatientName);
        var entityB = Entity(0.65f, ClinicalEntityType.Medication);

        var resultsA = service.Score([entityA], "patient-A");
        var resultsB = service.Score([entityB], "patient-B");

        Assert.Single(resultsA);
        Assert.Equal(ExtractionDecision.AutoAccept, resultsA[0].Decision);

        Assert.Single(resultsB);
        Assert.Equal(ExtractionDecision.Reject, resultsB[0].Decision);

        // Ensure no cross-patient entity contamination.
        Assert.Equal(entityA, resultsA[0].Entity);
        Assert.Equal(entityB, resultsB[0].Entity);
    }
}

// ---------------------------------------------------------------------------
// Test doubles for IOptionsMonitor
// ---------------------------------------------------------------------------

/// <summary>Returns a fixed options value (no change notifications).</summary>
file sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
    public T CurrentValue => value;
    public T Get(string? name) => value;
    public IDisposable? OnChange(Action<T, string?> listener) => null;
}

/// <summary>Allows in-test mutation of the monitored value to verify hot-reload behaviour.</summary>
file sealed class MutableOptionsMonitor<T>(T initial) : IOptionsMonitor<T>
{
    private T _current = initial;

    public T CurrentValue => _current;
    public T Get(string? name) => _current;
    public void Update(T next) => _current = next;
    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
