using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Request payload for the patient data aggregation pipeline.</summary>
public sealed record AggregatePatientDataRequest(Guid PatientProfileId);

/// <summary>Result returned by <see cref="IPatientAggregationService.AggregateAsync"/>.</summary>
public sealed record AggregatePatientDataServiceResult(
    bool Success,
    string? FailureCode,
    string? FailureReason,
    int ConflictsDetected = 0);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Orchestrates the full patient data aggregation pipeline (US_038):
/// 1. Loads NER-extracted records, intake data, and external sources (AC-01).
/// 2. Runs de-duplication across sources (AC-02).
/// 3. Flags conflicting data for human review (Edge Cases).
/// 4. Builds and upserts the aggregated <see cref="PatientView"/> (AC-01).
/// 5. Computes and persists the aggregate verification status (AC-03).
///
/// Re-aggregation is triggered by callers (document upload / deletion) (AC-04).
/// </summary>
public interface IPatientAggregationService
{
    Task<AggregatePatientDataServiceResult> AggregateAsync(
        AggregatePatientDataRequest request,
        CancellationToken cancellationToken = default);
}
