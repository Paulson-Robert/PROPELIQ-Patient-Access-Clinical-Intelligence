using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Caching;

/// <summary>
/// Redis-backed <see cref="ICacheService"/> with 15-minute sliding expiry (AC-02).
/// On any Redis failure the operation is transparently retried against the
/// <see cref="InMemoryCacheService"/> fallback (AC-05).
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(15);

    private readonly IConnectionMultiplexer _redis;
    private readonly IMemoryCache _fallback;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        IMemoryCache fallback,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _fallback = fallback;
        _logger = logger;
    }

    /// <inheritdoc/>
    /// Sliding expiry: every successful GET resets the key TTL to 15 minutes.
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var raw = await db.StringGetAsync(key).ConfigureAwait(false);

            if (raw.IsNullOrEmpty)
                return default;

            // Reset TTL on each access — sliding window behaviour.
            await db.KeyExpireAsync(key, DefaultExpiry).ConfigureAwait(false);

            return JsonSerializer.Deserialize<T>(raw.ToString());
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            _logger.LogCritical(ex,
                "Redis GET failed for key {Key}; falling back to in-memory cache. " +
                "Verify Upstash quota and connectivity.", key);

            return _fallback.TryGetValue(key, out T? cached) ? cached : default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var ttl = expiry ?? DefaultExpiry;
        var serialized = JsonSerializer.Serialize(value);

        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, serialized, ttl).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            _logger.LogCritical(ex,
                "Redis SET failed for key {Key}; persisting to in-memory fallback.", key);

            _fallback.Set(key, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = ttl
            });
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            _logger.LogCritical(ex,
                "Redis DELETE failed for key {Key}; removing from in-memory fallback.", key);

            _fallback.Remove(key);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            _logger.LogCritical(ex,
                "Redis EXISTS failed for key {Key}; checking in-memory fallback.", key);

            return _fallback.TryGetValue(key, out _);
        }
    }

    // Catches connection errors, server-side errors (e.g. Upstash quota), and timeouts.
    private static bool IsRedisFailure(Exception ex) =>
        ex is RedisException or TimeoutException;
}
