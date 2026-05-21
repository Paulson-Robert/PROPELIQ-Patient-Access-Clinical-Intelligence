using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Applies the result of a ClamAV malware scan to a <c>ClinicalDocument</c> (US_033).
///
/// Outcomes:
/// - <see cref="MalwareScanOutcome.Clean"/>: marks the document clean and advances
///   <see cref="DocumentProcessingStatus.Processing"/> so the NER pipeline picks it up (AC-04).
/// - <see cref="MalwareScanOutcome.Infected"/>: quarantines the document by deleting
///   the physical file and setting <see cref="DocumentProcessingStatus.Failed"/> (AC-02).
/// - <see cref="MalwareScanOutcome.Unavailable"/> / <see cref="MalwareScanOutcome.Error"/>:
///   leaves <see cref="MalwareScanStatus.Pending"/> so the next job run retries (edge case).
/// </summary>
public sealed record ProcessScanResultCommand(
    Guid DocumentId,
    MalwareScanOutcome Outcome) : IRequest<ProcessScanResultResult>;

/// <summary>Result returned by <see cref="ProcessScanResultCommand"/>.</summary>
public sealed record ProcessScanResultResult(
    bool Success,
    string? FailureReason);

// ---------------------------------------------------------------------------
// Handler — lives in Infrastructure (requires EF Core + file-system access)
// ---------------------------------------------------------------------------
