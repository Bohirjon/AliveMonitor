using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IAlertService
{
    Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, string alertEmail);
    Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, string alertEmail);
}
