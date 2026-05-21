using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Tracks human verification status for individual extracted data records
/// and computes the aggregate verification status of a patient's PatientView (AC-03).
///
/// Verification state transitions:
/// - <see cref="PatientViewVerificationStatus.Pending"/>        — no extracted records exist.
/// - <see cref="PatientViewVerificationStatus.RequiresReview"/> — at least one record is unverified.
/// - <see cref="PatientViewVerificationStatus.Verified"/>       — all records are marked verified.
/// </summary>
public sealed class VerificationTrackingService : IVerificationTrackingService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<VerificationTrackingService> _logger;

    public VerificationTrackingService(
        ApplicationDbContext db,
        ILogger<VerificationTrackingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<MarkVerifiedResult> MarkRecordVerifiedAsync(
        Guid recordId,
        Guid verifiedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (recordId == Guid.Empty)
        {
            return new MarkVerifiedResult(false, "INVALID_REQUEST", "RecordId is required.");
        }

        if (verifiedByUserId == Guid.Empty)
        {
            return new MarkVerifiedResult(false, "INVALID_REQUEST", "VerifiedByUserId is required.");
        }

        var record = await _db.ExtractedDataRecords
            .FirstOrDefaultAsync(r => r.RecordId == recordId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            return new MarkVerifiedResult(false, "NOT_FOUND", "Extracted data record not found.");
        }

        if (record.IsVerified)
        {
            // Idempotent: already verified — treat as success.
            return new MarkVerifiedResult(true, null, null);
        }

        record.IsVerified = true;
        record.VerifiedByUserId = verifiedByUserId;
        record.VerifiedAt = DateTime.UtcNow;

        // Update PatientView aggregate verification status in the same save.
        var newStatus = await ComputeVerificationStatusAsync(
            record.PatientProfileId, cancellationToken)
            .ConfigureAwait(false);

        var view = await _db.PatientViews
            .FirstOrDefaultAsync(
                v => v.PatientProfileId == record.PatientProfileId,
                cancellationToken)
            .ConfigureAwait(false);

        if (view is not null)
        {
            view.VerificationStatus = newStatus;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "VerificationTracking: RecordId {RecordId} marked verified by UserId {UserId}. " +
            "PatientView status → {Status}.",
            recordId, verifiedByUserId, newStatus);

        return new MarkVerifiedResult(true, null, null);
    }

    /// <inheritdoc/>
    public async Task<PatientViewVerificationStatus> ComputeVerificationStatusAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default)
    {
        var counts = await _db.ExtractedDataRecords
            .AsNoTracking()
            .Where(r => r.PatientProfileId == patientProfileId)
            .GroupBy(_ => true)
            .Select(g => new
            {
                Total = g.Count(),
                Verified = g.Count(r => r.IsVerified),
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (counts is null || counts.Total == 0)
            return PatientViewVerificationStatus.Pending;

        return counts.Verified == counts.Total
            ? PatientViewVerificationStatus.Verified
            : PatientViewVerificationStatus.RequiresReview;
    }
}
