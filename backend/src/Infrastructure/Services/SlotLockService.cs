using Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services;

/// <summary>
/// Redis-backed slot lock service using SET NX EX semantics (AC-03, AC-05, AC-06).
/// The lock key is <c>slot:{slotId}</c> and the value is a UUID lock token
/// returned to the caller so only the holder can release it.
///
/// On Redis failure, AcquireAsync returns null (no lock granted) so the caller
/// receives a 409 response. Booking confirmation falls back to EF Core optimistic
/// concurrency (AvailabilitySlot.Version) as a secondary safety net (Edge Case).
/// </summary>
public sealed class SlotLockService : ISlotLockService
{
    private const int LockDurationSeconds = 30;

    // Atomically deletes the key only when its value matches the caller's token.
    private const string ReleaseScript =
        "if redis.call('get', KEYS[1]) == ARGV[1] then " +
            "return redis.call('del', KEYS[1]) " +
        "else " +
            "return 0 " +
        "end";

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SlotLockService> _logger;

    public SlotLockService(
        IConnectionMultiplexer redis,
        ILogger<SlotLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SlotLockResult?> AcquireAsync(
        Guid slotId,
        CancellationToken cancellationToken = default)
    {
        var key = LockKey(slotId);
        var lockToken = Guid.NewGuid().ToString("N");
        var expiry = TimeSpan.FromSeconds(LockDurationSeconds);

        try
        {
            var db = _redis.GetDatabase();
            var acquired = await db
                .StringSetAsync(key, lockToken, expiry, When.NotExists)
                .ConfigureAwait(false);

            if (!acquired)
            {
                _logger.LogDebug(
                    "Slot lock contention for slot {SlotId}. Lock already held.",
                    slotId);
                return null;
            }

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(LockDurationSeconds);

            _logger.LogInformation(
                "Slot lock acquired. SlotId={SlotId}, ExpiresAt={ExpiresAt}.",
                slotId, expiresAt);

            return new SlotLockResult(slotId, lockToken, expiresAt, LockDurationSeconds);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogCritical(ex,
                "Redis slot lock acquire failed for slot {SlotId}. No lock granted.",
                slotId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateAsync(
        Guid slotId,
        string lockToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockToken))
            return false;

        try
        {
            var db = _redis.GetDatabase();
            var storedToken = await db
                .StringGetAsync(LockKey(slotId))
                .ConfigureAwait(false);

            return storedToken.HasValue && storedToken == lockToken;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogCritical(ex,
                "Redis lock validation failed for slot {SlotId}. " +
                "Treating as expired — DB optimistic concurrency will guard.",
                slotId);
            // Return true so booking can proceed; DB Version check acts as fallback (Edge Case)
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ReleaseAsync(
        Guid slotId,
        string lockToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockToken))
            return false;

        try
        {
            var db = _redis.GetDatabase();
            var result = (long)await db
                .ScriptEvaluateAsync(
                    ReleaseScript,
                    keys: [LockKey(slotId)],
                    values: [(RedisValue)lockToken])
                .ConfigureAwait(false);

            return result == 1;
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogCritical(ex,
                "Redis lock release failed for slot {SlotId}. " +
                "Lock will expire naturally via TTL.",
                slotId);
            return false;
        }
    }

    private static string LockKey(Guid slotId) => $"slot:{slotId:N}";
}
