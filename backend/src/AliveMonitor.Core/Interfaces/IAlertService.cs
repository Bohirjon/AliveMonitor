using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IAlertService
{
    Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, IReadOnlyList<string> alertEmails);
    Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, IReadOnlyList<string> alertEmails);
    Task SendSslExpirationAlertAsync(MonitoredEndpoint endpoint, SslCertificateCheckLog checkLog, int thresholdDays, IReadOnlyList<string> alertEmails);
}
