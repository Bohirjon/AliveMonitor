using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;

namespace AliveMonitor.Infrastructure.Services;

public class CompositeAlertService(EmailAlertService emailAlertService, TelegramAlertService telegramAlertService, WebhookAlertService webhookAlertService) : IAlertService
{
    public async Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, AlertRecipients recipients)
    {
        var tasks = new List<Task>();

        if (recipients.Emails.Count > 0)
            tasks.Add(emailAlertService.SendFailureAlertAsync(endpoint, incident, checkLog, recipients));

        if (recipients.TelegramChatIds.Count > 0)
            tasks.Add(telegramAlertService.SendFailureAlertAsync(endpoint, incident, checkLog, recipients));

        if (recipients.WebhookUrls.Count > 0)
            tasks.Add(webhookAlertService.SendFailureAlertAsync(endpoint, incident, checkLog, recipients));

        await Task.WhenAll(tasks);
    }

    public async Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, AlertRecipients recipients)
    {
        var tasks = new List<Task>();

        if (recipients.Emails.Count > 0)
            tasks.Add(emailAlertService.SendRecoveryAlertAsync(endpoint, incident, recipients));

        if (recipients.TelegramChatIds.Count > 0)
            tasks.Add(telegramAlertService.SendRecoveryAlertAsync(endpoint, incident, recipients));

        if (recipients.WebhookUrls.Count > 0)
            tasks.Add(webhookAlertService.SendRecoveryAlertAsync(endpoint, incident, recipients));

        await Task.WhenAll(tasks);
    }

    public async Task SendSslExpirationAlertAsync(MonitoredEndpoint endpoint, SslCertificateCheckLog checkLog, int thresholdDays, AlertRecipients recipients)
    {
        var tasks = new List<Task>();

        if (recipients.Emails.Count > 0)
            tasks.Add(emailAlertService.SendSslExpirationAlertAsync(endpoint, checkLog, thresholdDays, recipients));

        if (recipients.TelegramChatIds.Count > 0)
            tasks.Add(telegramAlertService.SendSslExpirationAlertAsync(endpoint, checkLog, thresholdDays, recipients));

        if (recipients.WebhookUrls.Count > 0)
            tasks.Add(webhookAlertService.SendSslExpirationAlertAsync(endpoint, checkLog, thresholdDays, recipients));

        await Task.WhenAll(tasks);
    }
}
