using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.RateLimiting;

/// <summary>
/// Configuration options for the Redis sliding-window rate limiter.
/// Bind from the "RateLimiting" configuration section.
/// </summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Maximum requests allowed per <see cref="WindowSeconds"/>.</summary>
    public int Limit { get; set; } = 100;

    /// <summary>Duration of the sliding window in seconds.</summary>
    public int WindowSeconds { get; set; } = 60;
}

/// <summary>
/// Result returned by <see cref="RedisSlidingWindowCounter.CheckAsync"/>.
/// </summary>
public sealed record RateLimitResult(bool IsAllowed, int RemainingRequests, int RetryAfterSeconds);

/// <summary>
/// Sliding-window rate-limiting counter backed by Redis sorted sets (AC-04).
/// Each caller identity + endpoint combination gets its own counter key.
/// </summary>
public sealed class RedisSlidingWindowCounter
{
    // Lua script: removes expired members, adds current timestamp, returns count.
    // Uses millisecond epoch timestamps as both score and member value,
    // appending a random suffix to guarantee uniqueness within the same millisecond.
    private const string SlidingWindowScript = @"
local key      = KEYS[1]
local now      = tonumber(ARGV[1])
local window   = tonumber(ARGV[2])
local limit    = tonumber(ARGV[3])
local member   = ARGV[4]

redis.call('ZREMRANGEBYSCORE', key, 0, now - window)
local count = tonumber(redis.call('ZCARD', key))

if count < limit then
    redis.call('ZADD', key, now, member)
    redis.call('PEXPIRE', key, window)
    return count + 1
else
    return -(count)
end";

    private readonly IConnectionMultiplexer _redis;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RedisSlidingWindowCounter> _logger;

    public RedisSlidingWindowCounter(
        IConnectionMultiplexer redis,
        IOptions<RateLimitOptions> options,
        ILogger<RedisSlidingWindowCounter> logger)
    {
        _redis = redis;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Checks whether the caller is within the configured rate limit.
    /// Increments the counter when allowed.
    /// </summary>
    /// <param name="clientId">Stable client identifier (e.g. IP or user ID).</param>
    /// <param name="endpoint">Endpoint discriminator (e.g. request path).</param>
    public async Task<RateLimitResult> CheckAsync(
        string clientId,
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        var key = $"ratelimit:{clientId}:{endpoint}";
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowMs = _options.WindowSeconds * 1_000L;
        var member = $"{nowMs}-{Guid.NewGuid():N}";

        try
        {
            var db = _redis.GetDatabase();
            var result = (long)await db.ScriptEvaluateAsync(
                SlidingWindowScript,
                keys: [key],
                values: [nowMs, windowMs, _options.Limit, member])
                .ConfigureAwait(false);

            if (result > 0)
            {
                var remaining = _options.Limit - (int)result;
                return new RateLimitResult(IsAllowed: true, RemainingRequests: remaining, RetryAfterSeconds: 0);
            }

            // Negative result means the limit was exceeded; abs value is current count.
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. " +
                "Count={Count} Limit={Limit}",
                clientId, endpoint, Math.Abs(result), _options.Limit);

            var retryAfterSeconds = _options.WindowSeconds;
            var oldestEntries = await db.SortedSetRangeByRankWithScoresAsync(key, 0, 0, Order.Ascending)
                .ConfigureAwait(false);

            if (oldestEntries.Length > 0)
            {
                var oldestTimestampMs = (long)oldestEntries[0].Score;
                var retryAfterMs = (oldestTimestampMs + windowMs) - nowMs;

                retryAfterSeconds = retryAfterMs > 0
                    ? Math.Max(1, (int)Math.Ceiling(retryAfterMs / 1000d))
                    : 1;
            }

            return new RateLimitResult(
                IsAllowed: false,
                RemainingRequests: 0,
                RetryAfterSeconds: retryAfterSeconds);
        }
        catch (Exception ex) when (ex is RedisException or TimeoutException)
        {
            _logger.LogCritical(ex,
                "Redis rate-limit counter unavailable for client {ClientId}. Allowing request (fail-open).",
                clientId);

            // Fail-open on Redis failure to avoid blocking all traffic.
            return new RateLimitResult(IsAllowed: true, RemainingRequests: _options.Limit, RetryAfterSeconds: 0);
        }
    }
}

/// <summary>
/// ASP.NET Core middleware that enforces Redis sliding-window rate limits.
/// Returns HTTP 429 with a Retry-After header when limits are exceeded (AC-04).
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds Redis-backed sliding-window rate limiting to the request pipeline.
    /// Must be called after <c>UseRouting()</c> so the request path is resolved.
    /// </summary>
    public static IApplicationBuilder UseRedisRateLimiting(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var counter = context.RequestServices
                .GetService(typeof(RedisSlidingWindowCounter)) as RedisSlidingWindowCounter;

            // Rate limiting is optional — if the service is not registered, continue.
            if (counter is null)
            {
                await next(context);
                return;
            }

            // Prefer authenticated user ID for per-user limiting; fall back to IP for anonymous traffic.
            var clientId = context.User.Identity?.IsAuthenticated == true
                ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown"
                : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var endpoint = context.Request.Path.Value ?? "/";

            var result = await counter.CheckAsync(clientId, endpoint, context.RequestAborted);

            if (!result.IsAllowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.RetryAfter = result.RetryAfterSeconds.ToString();
                await context.Response.WriteAsync("Too Many Requests", context.RequestAborted);
                return;
            }

            await next(context);
        });
    }
}
