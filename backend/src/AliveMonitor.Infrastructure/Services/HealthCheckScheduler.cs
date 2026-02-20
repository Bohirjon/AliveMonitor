using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace AliveMonitor.Infrastructure.Services;

public class HealthCheckScheduler(
    IRecurringJobManager recurringJobManager,
    IEndpointRepository endpointRepository,
    ILogger<HealthCheckScheduler> logger)
{
    public void ScheduleEndpoint(MonitoredEndpoint endpoint)
    {
        if (!endpoint.IsEnabled)
        {
            UnscheduleEndpoint(endpoint.Id);
            return;
        }

        var jobId = GetJobId(endpoint.Id);
        recurringJobManager.AddOrUpdate<HealthCheckExecutor>(
            jobId,
            executor => executor.ExecuteAsync(endpoint.Id),
            $"*/{endpoint.IntervalMinutes} * * * *");

        logger.LogInformation("Scheduled health check for {Name} every {Interval}m", endpoint.FriendlyName, endpoint.IntervalMinutes);
    }

    public void UnscheduleEndpoint(Guid endpointId)
    {
        var jobId = GetJobId(endpointId);
        recurringJobManager.RemoveIfExists(jobId);
        logger.LogInformation("Unscheduled health check for endpoint {EndpointId}", endpointId);
    }

    public async Task SyncAllSchedulesAsync()
    {
        var enabledEndpoints = await endpointRepository.GetAllEnabledAsync();
        foreach (var endpoint in enabledEndpoints)
        {
            ScheduleEndpoint(endpoint);
        }
        logger.LogInformation("Synced {Count} endpoint schedules on startup", enabledEndpoints.Count);
    }

    private static string GetJobId(Guid endpointId) => $"health-check-{endpointId}";
}
