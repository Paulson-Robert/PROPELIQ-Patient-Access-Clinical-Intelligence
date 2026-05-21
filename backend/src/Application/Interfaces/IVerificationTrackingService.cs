using Domain.Enums;

namespace Application.Interfaces;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Result returned by <see cref="IVerificationTrackingService.MarkRecordVerifiedAsync"/>.</summary>
public sealed record MarkVerifiedResult(
    bool Success,
    string? FailureCode,
    string? FailureReason);

// ---------------------------------------------------------------------------
// Interface
// ---------------------------------------------------------------------------

/// <summary>
/// Tracks and updates the human verification status of extracted data
/// records and the aggregate <see cref="Domain.Entities.PatientView"/> (AC-03).
///
/// A PatientView is considered:
/// - <see cref="PatientViewVerificationStatus.Verified"/> — every record is marked verified.
/// - <see cref="PatientViewVerificationStatus.RequiresReview"/> — one or more records unverified.
/// - <see cref="PatientViewVerificationStatus.Pending"/> — no records exist yet.
/// </summary>
public interface IVerificationTrackingService
{
    /// <summary>
    /// Marks a single <see cref="Domain.Entities.ExtractedDataRecord"/> as
    /// human-verified and updates the parent PatientView verification status.
    /// </summary>
    Task<MarkVerifiedResult> MarkRecordVerifiedAsync(
        Guid recordId,
        Guid verifiedByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the aggregate verification status for all extracted data
    /// records belonging to <paramref name="patientProfileId"/>.
    /// </summary>
    Task<PatientViewVerificationStatus> ComputeVerificationStatusAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default);
}
