using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Append-only repository for AuditLog entries (AC-01, AC-02, NFR-005).
/// Implementations must never issue UPDATE or DELETE against the AuditLogs table.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Persists a single audit entry. Fire-and-forget safe — the caller should not
    /// await this on the hot request path when using the fire-and-forget overload.
    /// </summary>
    Task AddAsync(AuditLog entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a batch of audit entries in a single round-trip (edge case: bulk ops).
    /// </summary>
    Task AddRangeAsync(IEnumerable<AuditLog> entries, CancellationToken cancellationToken = default);
}
