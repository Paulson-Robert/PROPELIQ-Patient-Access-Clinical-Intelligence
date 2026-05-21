using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

// ---------------------------------------------------------------------------
// ModelAccuracyCheckJob — periodic NER accuracy monitoring (AC-02, AC-03, US_037)
// ---------------------------------------------------------------------------

/// <summary>
/// Hangfire recurring job that computes the AI–Human agreement rate for the active
/// NER model and triggers an alert when the rate drops below the 98 % threshold (AC-03).
///
/// Alert behaviour (AC-03):
///   Rate &lt; AlertThreshold → send staff notification to all Admin users + log Critical
///   Rate ≥ AlertThreshold after prior alert → auto-resolve with recovery notification
///
/// Edge case — insufficient data (US_037):
///   Fewer than 50 verified fields in the rolling window → monitoring paused with a warning.
///   No alert is triggered to prevent false positives.
///
/// Scoped services (DB, monitoring) are resolved per-execution via
/// <see cref="IServiceScopeFactory"/> following the existing Hangfire job pattern.
/// </summary>
public sealed class ModelAccuracyCheckJob
{
    /// <summary>Agreement rate below which an alert is triggered (AC-03).</summary>
    private const double AlertThreshold = 0.98;

    /// <summary>Rolling window for the agreement rate calculation (AC-03, US_037).</summary>
    private static readonly TimeSpan RollingWindow = TimeSpan.FromDays(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ModelAccuracyCheckJob> _logger;

    // Tracks whether the previous run was in an alerted state to detect recovery (AC-03).
    // In-process state is acceptable: a fresh alert on restart is benign and preferable
    // to persisting alert state across deployments. (Inferred decision: US_037 AC-03.)
    private bool _previousRunWasAlerted;

    public ModelAccuracyCheckJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ModelAccuracyCheckJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes one accuracy check cycle.
    /// Called by the Hangfire scheduler — register via <c>RecurringJob.AddOrUpdate</c>.
    /// </summary>
    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var versioning = scope.ServiceProvider.GetRequiredService<IModelVersioningService>();
        var monitoring = scope.ServiceProvider.GetRequiredService<IModelMonitoringService>();
        var notifications = scope.ServiceProvider.GetRequiredService<IStaffNotificationService>();
        var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.ApplicationDbContext>();

        var currentVersion = versioning.GetCurrentVersion();

        // --- Entity extraction accuracy (AC-02, AC-03) ---
        var entityResult = await monitoring.GetEntityAgreementRateAsync(
                currentVersion, RollingWindow)
            .ConfigureAwait(false);

        if (!entityResult.IsSufficientData)
        {
            _logger.LogWarning(
                "ModelAccuracyCheckJob: insufficient entity verification data for version {Version}. " +
                "VerifiedCount={Count}. Monitoring paused for this cycle.",
                currentVersion,
                entityResult.VerifiedFieldCount);
            return;
        }

        var rate = entityResult.AgreementRate!.Value;

        _logger.LogInformation(
            "ModelAccuracyCheckJob: entity agreement rate. Version={Version} Rate={Rate:P2} " +
            "Window=[{Start:O},{End:O}]",
            currentVersion, rate, entityResult.WindowStart, entityResult.WindowEnd);

        if (rate < AlertThreshold)
        {
            await SendAlertAsync(db, notifications, currentVersion, rate, cancellationToken: default)
                .ConfigureAwait(false);
            _previousRunWasAlerted = true;
        }
        else if (_previousRunWasAlerted)
        {
            await SendRecoveryAsync(db, notifications, currentVersion, rate, cancellationToken: default)
                .ConfigureAwait(false);
            _previousRunWasAlerted = false;
        }

        // --- Code mapping accuracy (AC-05) — independent threshold check ---
        var codeResults = await monitoring.GetCodeAgreementRatesAsync(RollingWindow)
            .ConfigureAwait(false);

        foreach (var codeResult in codeResults)
        {
            if (!codeResult.IsSufficientData)
            {
                _logger.LogWarning(
                    "ModelAccuracyCheckJob: insufficient code verification data for CodeType={CodeType}. " +
                    "VerifiedCount={Count}. Monitoring paused for this code type.",
                    codeResult.CodeType,
                    codeResult.VerifiedCodeCount);
                continue;
            }

            var codeRate = codeResult.AgreementRate!.Value;

            _logger.LogInformation(
                "ModelAccuracyCheckJob: code agreement rate. CodeType={CodeType} Rate={Rate:P2}",
                codeResult.CodeType, codeRate);

            if (codeRate < AlertThreshold)
            {
                _logger.LogCritical(
                    "ModelAccuracyCheckJob: code mapping agreement rate below threshold. " +
                    "CodeType={CodeType} Rate={Rate:P2} Threshold={Threshold:P2}",
                    codeResult.CodeType, codeRate, AlertThreshold);

                await SendCodeAlertAsync(db, notifications, codeResult.CodeType, codeRate, cancellationToken: default)
                    .ConfigureAwait(false);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task SendAlertAsync(
        Infrastructure.Data.ApplicationDbContext db,
        IStaffNotificationService notifications,
        string modelVersion,
        double rate,
        CancellationToken cancellationToken)
    {
        _logger.LogCritical(
            "ModelAccuracyCheckJob: entity agreement rate below threshold. " +
            "Version={Version} Rate={Rate:P2} Threshold={Threshold:P2}",
            modelVersion, rate, AlertThreshold);

        var adminIds = await GetAdminUserIdsAsync(db, cancellationToken).ConfigureAwait(false);
        if (adminIds.Count == 0)
            return;

        var request = new CreateStaffNotificationRequest(
            adminIds,
            Variant: "error",
            Title: "NER Model Accuracy Alert",
            Message: $"Agreement rate for {modelVersion} is {rate:P1} — below the 98% threshold. " +
                     "Review recent extractions immediately.");

        await notifications.CreateAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendRecoveryAsync(
        Infrastructure.Data.ApplicationDbContext db,
        IStaffNotificationService notifications,
        string modelVersion,
        double rate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ModelAccuracyCheckJob: entity agreement rate recovered. Version={Version} Rate={Rate:P2}",
            modelVersion, rate);

        var adminIds = await GetAdminUserIdsAsync(db, cancellationToken).ConfigureAwait(false);
        if (adminIds.Count == 0)
            return;

        var request = new CreateStaffNotificationRequest(
            adminIds,
            Variant: "success",
            Title: "NER Model Accuracy Recovered",
            Message: $"Agreement rate for {modelVersion} has recovered to {rate:P1} (above 98% threshold).");

        await notifications.CreateAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendCodeAlertAsync(
        Infrastructure.Data.ApplicationDbContext db,
        IStaffNotificationService notifications,
        string codeType,
        double rate,
        CancellationToken cancellationToken)
    {
        var adminIds = await GetAdminUserIdsAsync(db, cancellationToken).ConfigureAwait(false);
        if (adminIds.Count == 0)
            return;

        var request = new CreateStaffNotificationRequest(
            adminIds,
            Variant: "error",
            Title: $"{codeType} Code Mapping Accuracy Alert",
            Message: $"{codeType} code mapping agreement rate is {rate:P1} — below the 98% threshold.");

        await notifications.CreateAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<Guid>> GetAdminUserIdsAsync(
        Infrastructure.Data.ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(u => u.Role == Domain.Enums.UserRole.Admin && u.IsActive)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
