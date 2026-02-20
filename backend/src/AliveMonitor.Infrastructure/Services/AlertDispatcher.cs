using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AliveMonitor.Infrastructure.Services;

public class AlertDispatcher(
    IIncidentRepository incidentRepository,
    IAlertService alertService,
    AlertEmailResolver alertEmailResolver,
    IOptions<AlertSettings> alertSettings,
    ILogger<AlertDispatcher> logger)
{
    private readonly AlertSettings _alertSettings = alertSettings.Value;

    public async Task ProcessCheckResultAsync(MonitoredEndpoint endpoint, HealthCheckLog checkLog)
    {
        var openIncident = await incidentRepository.GetOpenIncidentAsync(endpoint.Id);

        if (checkLog.IsHealthy)
        {
            if (openIncident is not null)
            {
                // Resolve incident
                openIncident.ResolvedAt = DateTime.UtcNow;
                await incidentRepository.UpdateAsync(openIncident);

                var alertEmails = await alertEmailResolver.GetAlertEmailsAsync(endpoint);
                if (alertEmails.Count > 0)
                {
                    await alertService.SendRecoveryAlertAsync(endpoint, openIncident, alertEmails);
                    logger.LogInformation("Recovery alert sent for {Name}", endpoint.FriendlyName);
                }
            }
        }
        else
        {
            if (openIncident is null)
            {
                // Create new incident
                var incident = new Incident
                {
                    EndpointId = endpoint.Id,
                    OpenedAt = DateTime.UtcNow,
                    LastNotifiedAt = DateTime.UtcNow,
                    FailureCount = 1,
                };
                await incidentRepository.CreateAsync(incident);

                var alertEmails = await alertEmailResolver.GetAlertEmailsAsync(endpoint);
                if (alertEmails.Count > 0)
                {
                    await alertService.SendFailureAlertAsync(endpoint, incident, checkLog, alertEmails);
                    logger.LogInformation("Failure alert sent for {Name}", endpoint.FriendlyName);
                }
            }
            else
            {
                // Ongoing incident
                openIncident.FailureCount++;
                var timeSinceLastNotification = DateTime.UtcNow - openIncident.LastNotifiedAt;

                if (timeSinceLastNotification.TotalMinutes >= _alertSettings.ThrottleIntervalMinutes)
                {
                    openIncident.LastNotifiedAt = DateTime.UtcNow;
                    await incidentRepository.UpdateAsync(openIncident);

                    var alertEmails = await alertEmailResolver.GetAlertEmailsAsync(endpoint);
                    if (alertEmails.Count > 0)
                    {
                        await alertService.SendFailureAlertAsync(endpoint, openIncident, checkLog, alertEmails);
                        logger.LogInformation("Throttled failure alert sent for {Name} ({Count} failures)", endpoint.FriendlyName, openIncident.FailureCount);
                    }
                }
                else
                {
                    await incidentRepository.UpdateAsync(openIncident);
                    logger.LogDebug("Alert throttled for {Name}, next notification in {Minutes}m",
                        endpoint.FriendlyName,
                        _alertSettings.ThrottleIntervalMinutes - (int)timeSinceLastNotification.TotalMinutes);
                }
            }
        }
    }

}
