using Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Locking;

/// <summary>
/// Redis-backed <see cref="IDistributedLockService"/> using SET NX EX semantics (AC-03).
/// Only the first caller for a given resource acquires the lock.
/// Release uses a Lua script to prevent a caller from releasing a lock it does not own.
/// </summary>
public sealed class RedisDistributedLockService : IDistributedLockService
{
    // Atomically deletes the key only when its value matches the expected lock ID.
    private const string ReleaseScript =
        "if redis.call('get', KEYS[1]) == ARGV[1] then " +
            "return redis.call('del', KEYS[1]) " +
        "else " +
            "return 0 " +
        "end";

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLockService> _logger;

    public RedisDistributedLockService(
        IConnectionMultiplexer redis,
        ILogger<RedisDistributedLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc/>
    /// Uses SET resource lockId NX EX <seconds> — atomic acquire with TTL.
    public async Task<bool> AcquireAsync(
        string resource,
        string lockId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Lock resource key must not be empty.", nameof(resource));

        if (string.IsNullOrWhiteSpace(lockId))
            throw new ArgumentException("Lock ID must not be empty.", nameof(lockId));

        try
        {
            var db = _redis.GetDatabase();
            var acquired = await db
                .StringSetAsync(LockKey(resource), lockId, expiry, When.NotExists)
                .ConfigureAwait(false);

            if (!acquired)
            {
                _logger.LogDebug(
                    "Slot lock contention: resource {Resource} already held. Lock ID {LockId} rejected.",
                    resource, lockId);
            }

            return acquired;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogCritical(ex,
                "Redis lock acquire failed for resource {Resource}. Returning false (no lock granted).",
                resource);
            return false;
        }
    }

    /// <inheritdoc/>
    /// Uses a Lua script to delete the key only when the stored value equals <paramref name="lockId"/>.
    public async Task<bool> ReleaseAsync(
        string resource,
        string lockId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Lock resource key must not be empty.", nameof(resource));

        if (string.IsNullOrWhiteSpace(lockId))
            throw new ArgumentException("Lock ID must not be empty.", nameof(lockId));

        try
        {
            var db = _redis.GetDatabase();
            var result = (long)await db
                .ScriptEvaluateAsync(
                    ReleaseScript,
                    keys: [LockKey(resource)],
                    values: [(RedisValue)lockId])
                .ConfigureAwait(false);

            return result == 1;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogCritical(ex,
                "Redis lock release failed for resource {Resource}. " +
                "Lock may expire naturally via TTL.", resource);
            return false;
        }
    }

    private static string LockKey(string resource) => $"lock:{resource}";
}
