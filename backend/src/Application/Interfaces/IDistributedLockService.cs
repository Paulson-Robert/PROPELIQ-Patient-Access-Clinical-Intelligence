namespace Application.Interfaces;

/// <summary>
/// Abstraction over a distributed lock used for slot-booking concurrency control (DR-002, AC-03).
/// Implementations use SETNX semantics: only the first caller acquires the lock.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Attempts to acquire a lock on <paramref name="resource"/>.
    /// The caller MUST supply a unique <paramref name="lockId"/> so it can release its own lock.
    /// </summary>
    /// <returns>True when the lock was acquired; false when already held.</returns>
    Task<bool> AcquireAsync(
        string resource,
        string lockId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the lock on <paramref name="resource"/> only when
    /// the stored value matches <paramref name="lockId"/> (prevents foreign-owner release).
    /// </summary>
    /// <returns>True when the lock was released; false when not owned by the caller.</returns>
    Task<bool> ReleaseAsync(
        string resource,
        string lockId,
        CancellationToken cancellationToken = default);
}
