using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IAlertService
{
    Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, AlertRecipients recipients);
    Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, AlertRecipients recipients);
    Task SendSslExpirationAlertAsync(MonitoredEndpoint endpoint, SslCertificateCheckLog checkLog, int thresholdDays, AlertRecipients recipients);
}
