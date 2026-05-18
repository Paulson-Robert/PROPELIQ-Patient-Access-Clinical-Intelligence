using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Infrastructure.HealthChecks;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var pong = await db.PingAsync();

            var data = new Dictionary<string, object>
            {
                ["latencyMs"] = pong.TotalMilliseconds,
                ["isConnected"] = _redis.IsConnected,
            };

            return HealthCheckResult.Healthy("Redis is reachable.", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis health check failed.",
                exception: ex);
        }
    }
}
