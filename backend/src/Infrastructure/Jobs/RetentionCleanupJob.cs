using Infrastructure.Data;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that enforces the document retention policy by
/// removing orphaned physical files — files present on disk that have no
/// corresponding <c>ClinicalDocument</c> record in the database (US_034, AC-03).
///
/// Orphaned files arise when:
/// - A document deletion removes the DB record but the physical file delete fails.
/// - A file write succeeds but the subsequent metadata save fails (upload edge case).
/// - Manual file restoration without a matching DB record (operational incident).
///
/// The job is designed to be idempotent and safe to re-run at any time.
/// It never removes files that have a matching <c>StoragePath</c> in the database.
/// </summary>
public sealed class RetentionCleanupJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetentionCleanupJob> _logger;

    public RetentionCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RetentionCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Scans the document storage directory and deletes any files that do not
    /// correspond to an active <c>ClinicalDocument</c> row.
    /// </summary>
    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storageOptions = scope.ServiceProvider
            .GetRequiredService<IOptions<DocumentStorageOptions>>().Value;

        var basePath = storageOptions.BasePath;

        if (!Directory.Exists(basePath))
        {
            _logger.LogInformation(
                "RetentionCleanupJob: storage directory '{BasePath}' does not exist — nothing to clean.",
                basePath);
            return;
        }

        // Load the set of StoragePaths that are referenced by active DB records.
        // Using HashSet for O(1) lookup when comparing against disk files.
        var knownPaths = await db.ClinicalDocuments
            .AsNoTracking()
            .Select(d => d.StoragePath)
            .ToHashSetAsync()
            .ConfigureAwait(false);

        var files = Directory.EnumerateFiles(basePath, "*", SearchOption.TopDirectoryOnly);

        int deletedCount = 0;
        int errorCount = 0;

        foreach (var filePath in files)
        {
            // Normalise path separators for comparison
            var normalised = Path.GetFullPath(filePath);

            if (knownPaths.Contains(normalised))
                continue;

            // File has no matching DB record — it is orphaned.
            try
            {
                File.Delete(filePath);
                deletedCount++;

                _logger.LogInformation(
                    "RetentionCleanupJob: deleted orphaned file '{FilePath}'.",
                    filePath);
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogError(ex,
                    "RetentionCleanupJob: failed to delete orphaned file '{FilePath}'.",
                    filePath);
            }
        }

        _logger.LogInformation(
            "RetentionCleanupJob completed. OrphanedFilesDeleted={Deleted}, Errors={Errors}.",
            deletedCount, errorCount);
    }
}
