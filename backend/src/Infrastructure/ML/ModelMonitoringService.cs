using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// ModelMonitoringService — AI-Human agreement rate computation (AC-02, AC-05, US_037)
// ---------------------------------------------------------------------------

/// <summary>
/// Minimum number of verified fields required before an agreement rate is reported
/// as meaningful (edge case: insufficient verification data).
/// </summary>
file static class MonitoringConstants
{
    /// <summary>
    /// Rolling window used for accuracy checks (AC-02, AC-03).
    /// Inferred decision: 30-day window per US_037 AC-03 spec.
    /// </summary>
    internal static readonly TimeSpan DefaultRollingWindow = TimeSpan.FromDays(30);

    /// <summary>
    /// Minimum verified field count before an agreement rate is considered statistically
    /// meaningful. Values below this threshold are reported as "Insufficient data" (edge case).
    /// Inferred decision: 50 per US_037 edge-case definition.
    /// </summary>
    internal const int MinimumVerifiedFields = 50;
}

/// <summary>
/// Computes AI–Human agreement rates for entity extractions (AC-02) and code mappings (AC-05).
///
/// Agreement rate (AC-02):
///   = verified fields that were accepted / total verified fields
///
/// For entity extractions, "accepted" means <see cref="Domain.Entities.ExtractedDataRecord.IsVerified"/>
/// is <c>true</c> — i.e. a staff member reviewed and kept the extraction (no rejection stored
/// in the current schema means the field was accepted after review).
///
/// For code mappings, "accepted" means <see cref="Domain.Entities.MedicalCodeMapping.IsVerified"/>
/// is <c>true</c> (AC-05).
///
/// Edge case — insufficient data:
///   When fewer than <see cref="MonitoringConstants.MinimumVerifiedFields"/> records exist in
///   the rolling window, <see cref="EntityAgreementResult.AgreementRate"/> is <c>null</c>
///   and <see cref="EntityAgreementResult.IsSufficientData"/> is <c>false</c>.
/// </summary>
public sealed class ModelMonitoringService : IModelMonitoringService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ModelMonitoringService> _logger;

    public ModelMonitoringService(
        ApplicationDbContext db,
        ILogger<ModelMonitoringService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<EntityAgreementResult> GetEntityAgreementRateAsync(
        string modelVersion,
        TimeSpan rollingWindow,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelVersion, nameof(modelVersion));

        var windowEnd = DateTime.UtcNow;
        var windowStart = windowEnd - rollingWindow;

        // Count all records that were verified within the rolling window.
        // VerifiedAt records when staff reviewed and accepted the extraction.
        var verifiedCount = await _db.ExtractedDataRecords
            .AsNoTracking()
            .CountAsync(
                r => r.IsVerified
                  && r.VerifiedAt.HasValue
                  && r.VerifiedAt.Value >= windowStart
                  && r.VerifiedAt.Value <= windowEnd,
                cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug(
            "ModelMonitoringService: entity window [{Start:O}, {End:O}] VerifiedCount={Count}",
            windowStart, windowEnd, verifiedCount);

        if (verifiedCount < MonitoringConstants.MinimumVerifiedFields)
        {
            _logger.LogInformation(
                "ModelMonitoringService: insufficient entity verification data " +
                "(VerifiedCount={Count} < MinRequired={Min}). Agreement rate not computed.",
                verifiedCount,
                MonitoringConstants.MinimumVerifiedFields);

            return new EntityAgreementResult(
                modelVersion,
                windowStart,
                windowEnd,
                VerifiedFieldCount: verifiedCount,
                AgreementRate: null,
                IsSufficientData: false);
        }

        // Total records created in the window (verified + unverified) represents the
        // AI prediction set. The agreement rate is verified / total for the window.
        // Inferred decision: denominator is total records in the window (not just verified)
        // because unverified records represent predictions staff did not act on yet.
        // Chosen to match US_037 AC-02: "(accepted + accepted-with-minor-edit) / total-verified-fields"
        // where total-verified-fields = all records touched by a human reviewer.
        var agreementRate = verifiedCount == 0 ? 0.0 : 1.0; // all verified = accepted

        // Fetch actual total fields reviewed in the window (IsVerified = true means accepted).
        // Any rejected extraction would have been deleted or flagged; remaining verified = accepted.
        var totalReviewed = await _db.ExtractedDataRecords
            .AsNoTracking()
            .CountAsync(
                r => r.VerifiedAt.HasValue
                  && r.VerifiedAt.Value >= windowStart
                  && r.VerifiedAt.Value <= windowEnd,
                cancellationToken)
            .ConfigureAwait(false);

        agreementRate = totalReviewed == 0
            ? 1.0
            : (double)verifiedCount / totalReviewed;

        _logger.LogInformation(
            "ModelMonitoringService: entity agreement rate. ModelVersion={ModelVersion} " +
            "Window=[{Start:O},{End:O}] Verified={Verified} TotalReviewed={Total} Rate={Rate:P2}",
            modelVersion, windowStart, windowEnd, verifiedCount, totalReviewed, agreementRate);

        return new EntityAgreementResult(
            modelVersion,
            windowStart,
            windowEnd,
            VerifiedFieldCount: verifiedCount,
            AgreementRate: agreementRate,
            IsSufficientData: true);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CodeAgreementResult>> GetCodeAgreementRatesAsync(
        TimeSpan rollingWindow,
        CancellationToken cancellationToken = default)
    {
        var windowEnd = DateTime.UtcNow;
        var windowStart = windowEnd - rollingWindow;

        // Group code mappings by CodeType and compute agreement per type (AC-05).
        var groups = await _db.MedicalCodeMappings
            .AsNoTracking()
            .Where(m => m.VerifiedAt.HasValue
                     && m.VerifiedAt.Value >= windowStart
                     && m.VerifiedAt.Value <= windowEnd)
            .GroupBy(m => m.CodeType)
            .Select(g => new
            {
                CodeType = g.Key,
                Accepted = g.Count(m => m.IsVerified),
                Total = g.Count()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var results = new List<CodeAgreementResult>(groups.Count);

        foreach (var group in groups)
        {
            var codeTypeName = group.CodeType.ToString();
            bool sufficient = group.Accepted >= MonitoringConstants.MinimumVerifiedFields;

            double? rate = sufficient
                ? (group.Total == 0 ? 1.0 : (double)group.Accepted / group.Total)
                : null;

            _logger.LogInformation(
                "ModelMonitoringService: code agreement. CodeType={CodeType} " +
                "Accepted={Accepted} Total={Total} Rate={Rate} Sufficient={Sufficient}",
                codeTypeName, group.Accepted, group.Total,
                rate.HasValue ? rate.Value.ToString("P2") : "N/A", sufficient);

            results.Add(new CodeAgreementResult(
                codeTypeName,
                group.Accepted,
                rate,
                sufficient));
        }

        return results.AsReadOnly();
    }
}
