using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Caching;

/// <summary>
/// Creates and validates <see cref="IConnectionMultiplexer"/> instances for Upstash Redis.
/// Encapsulates TLS configuration, retry policy, and connect-time error handling (AC-01).
/// </summary>
public static class RedisConnectionFactory
{
    /// <summary>
    /// Builds a connected <see cref="IConnectionMultiplexer"/> from the supplied
    /// Upstash connection string.  TLS is enforced and a connect retry of 3 is applied.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is blank.</exception>
    /// <exception cref="RedisConnectionException">Propagated on connection failure; callers should
    /// catch and switch to degraded mode.</exception>
    public static IConnectionMultiplexer Create(string connectionString, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Redis connection string must not be empty.", nameof(connectionString));

        var options = ConfigurationOptions.Parse(connectionString, ignoreUnknown: true);

        // Upstash requires TLS; enforce it regardless of what the connection string contains.
        options.Ssl = true;
        options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                             | System.Security.Authentication.SslProtocols.Tls13;
        options.AbortOnConnectFail = false;
        options.ConnectRetry = 3;
        options.ConnectTimeout = 5_000;   // ms
        options.SyncTimeout = 3_000;      // ms
        options.AsyncTimeout = 3_000;     // ms
        options.ReconnectRetryPolicy = new ExponentialRetry(baseDelayMilliseconds: 500);

        logger?.LogInformation("Connecting to Redis endpoint(s): {Endpoints}", options.EndPoints);

        return ConnectionMultiplexer.Connect(options);
    }
}
