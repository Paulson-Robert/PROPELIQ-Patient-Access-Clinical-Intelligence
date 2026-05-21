using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
/// INSERT-only persistence for AuditLog entries.
/// Calls AddAsync / AddRangeAsync only — never SaveChanges on any other tracked entity —
/// to ensure the audit write is isolated from the request's own unit of work (AC-02).
/// The trigger <c>trg_audit_logs_immutable</c> on the AuditLogs table provides the
/// database-level immutability guarantee; this class provides the application-level contract.
/// </summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    // A dedicated DbContext factory is used so that audit writes are isolated from the
    // ambient request DbContext. This prevents audit entries from being rolled back if
    // the caller's transaction fails, and avoids SaveChanges interferring with tracked entities.
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<AuditLogRepository> _logger;

    public AuditLogRepository(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<AuditLogRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AddAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        db.AuditLogs.Add(entry);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "Audit entry persisted: ActionType={ActionType} Actor={ActorUserId}",
            entry.ActionType,
            entry.ActorUserId);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(
        IEnumerable<AuditLog> entries,
        CancellationToken cancellationToken = default)
    {
        var batch = entries as IReadOnlyList<AuditLog> ?? entries.ToList();
        if (batch.Count == 0)
            return;

        await using var db = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        db.AuditLogs.AddRange(batch);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "Batch of {Count} audit entries persisted.",
            batch.Count);
    }
}
