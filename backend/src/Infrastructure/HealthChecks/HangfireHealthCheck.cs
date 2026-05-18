using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public sealed class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var statistics = monitoringApi.GetStatistics();

            var data = new Dictionary<string, object>
            {
                ["servers"] = statistics.Servers,
                ["queues"] = statistics.Queues,
                ["processing"] = statistics.Processing,
                ["scheduled"] = statistics.Scheduled,
                ["failed"] = statistics.Failed,
            };

            if (statistics.Servers == 0)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "No Hangfire servers are running.",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Hangfire is operational.", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Hangfire health check failed.",
                exception: ex));
        }
    }
}
