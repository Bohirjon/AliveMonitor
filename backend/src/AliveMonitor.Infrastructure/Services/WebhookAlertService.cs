using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AliveMonitor.Infrastructure.Services;

public class WebhookAlertService(ILogger<WebhookAlertService> logger) : IAlertService
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, AlertRecipients recipients)
    {
        if (recipients.WebhookUrls.Count == 0) return Task.CompletedTask;

        var payload = new
        {
            EventType = "failure",
            Timestamp = DateTime.UtcNow,
            Endpoint = new { endpoint.Id, Name = endpoint.FriendlyName, endpoint.Url },
            Status = endpoint.CurrentStatus.ToString(),
            HttpStatusCode = checkLog.HttpStatusCode,
            ResponseTimeMs = checkLog.ResponseTimeMs,
            ErrorMessage = checkLog.ErrorMessage,
            Incident = new
            {
                incident.Id,
                incident.OpenedAt,
                incident.ResolvedAt,
                incident.FailureCount,
                DowntimeSeconds = (int)(DateTime.UtcNow - incident.OpenedAt).TotalSeconds,
            },
        };

        return PostToWebhooksAsync(recipients.WebhookUrls, payload);
    }

    public Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, AlertRecipients recipients)
    {
        if (recipients.WebhookUrls.Count == 0) return Task.CompletedTask;

        var payload = new
        {
            EventType = "recovery",
            Timestamp = DateTime.UtcNow,
            Endpoint = new { endpoint.Id, Name = endpoint.FriendlyName, endpoint.Url },
            Status = endpoint.CurrentStatus.ToString(),
            Incident = new
            {
                incident.Id,
                incident.OpenedAt,
                incident.ResolvedAt,
                incident.FailureCount,
                DowntimeSeconds = (int)((incident.ResolvedAt ?? DateTime.UtcNow) - incident.OpenedAt).TotalSeconds,
            },
        };

        return PostToWebhooksAsync(recipients.WebhookUrls, payload);
    }

    public Task SendSslExpirationAlertAsync(MonitoredEndpoint endpoint, SslCertificateCheckLog checkLog, int thresholdDays, AlertRecipients recipients)
    {
        if (recipients.WebhookUrls.Count == 0) return Task.CompletedTask;

        var payload = new
        {
            EventType = "ssl_expiry",
            Timestamp = DateTime.UtcNow,
            Endpoint = new { endpoint.Id, Name = endpoint.FriendlyName, endpoint.Url },
            Ssl = new
            {
                ExpiresAt = checkLog.ExpiresAt,
                DaysUntilExpiry = checkLog.DaysUntilExpiry,
                ThresholdDays = thresholdDays,
            },
        };

        return PostToWebhooksAsync(recipients.WebhookUrls, payload);
    }

    private async Task PostToWebhooksAsync(IReadOnlyList<string> webhookUrls, object payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        foreach (var url in webhookUrls)
        {
            try
            {
                var response = await HttpClient.PostAsync(url, content);
                logger.LogInformation("Webhook sent to {Url}, status {StatusCode}", url, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send webhook to {Url}", url);
            }
        }
    }
}
