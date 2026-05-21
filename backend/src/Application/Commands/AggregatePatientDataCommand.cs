using Application.Interfaces;
using MediatR;

namespace Application.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Merges NER-extracted records, intake data, and external source data into a
/// single aggregated <see cref="Domain.Entities.PatientView"/> for a patient (AC-01).
///
/// AC-01: Aggregation service merges data from NER, intake, and external sources.
/// AC-02: De-duplication identifies and merges duplicate records.
/// AC-03: Human verification status tracked per data point.
/// AC-04: Re-aggregation is triggered by the caller on document upload or deletion.
/// Edge:  Conflicting data from multiple sources is flagged for resolution.
///
/// The handler delegates all persistence and business logic to
/// <see cref="IPatientAggregationService"/> so the Application layer remains
/// free of Infrastructure dependencies.
/// </summary>
public sealed record AggregatePatientDataCommand(Guid PatientProfileId)
    : IRequest<AggregatePatientDataResult>;

/// <summary>Result returned by <see cref="AggregatePatientDataCommandHandler"/>.</summary>
public sealed record AggregatePatientDataResult(
    bool Success,
    string? FailureCode,
    string? FailureReason,
    int ConflictsDetected = 0);

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

internal sealed class AggregatePatientDataCommandHandler
    : IRequestHandler<AggregatePatientDataCommand, AggregatePatientDataResult>
{
    private readonly IPatientAggregationService _aggregation;

    public AggregatePatientDataCommandHandler(IPatientAggregationService aggregation)
    {
        _aggregation = aggregation;
    }

    public async Task<AggregatePatientDataResult> Handle(
        AggregatePatientDataCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PatientProfileId == Guid.Empty)
        {
            return new AggregatePatientDataResult(
                false, "INVALID_REQUEST", "PatientProfileId is required.");
        }

        var serviceResult = await _aggregation
            .AggregateAsync(
                new AggregatePatientDataRequest(request.PatientProfileId),
                cancellationToken)
            .ConfigureAwait(false);

        return new AggregatePatientDataResult(
            serviceResult.Success,
            serviceResult.FailureCode,
            serviceResult.FailureReason,
            serviceResult.ConflictsDetected);
    }
}
