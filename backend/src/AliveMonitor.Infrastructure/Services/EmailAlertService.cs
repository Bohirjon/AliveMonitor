using System.Net;
using System.Net.Mail;
using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AliveMonitor.Infrastructure.Services;

public class EmailAlertService(IOptions<AlertSettings> alertSettings, ILogger<EmailAlertService> logger) : IAlertService
{
    private readonly EmailSettings _emailSettings = alertSettings.Value.Email;

    public async Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, string alertEmail)
    {
        var isRepeat = incident.FailureCount > 1;
        var downtime = DateTime.UtcNow - incident.OpenedAt;

        var subject = $"\u26a0\ufe0f AliveMonitor Alert: {endpoint.FriendlyName} is Unhealthy";
        var body = $"""
            <h2>Endpoint Health Check Failed</h2>
            <p><strong>Endpoint:</strong> {endpoint.FriendlyName}</p>
            <p><strong>URL:</strong> {endpoint.Url}</p>
            <p><strong>Time:</strong> {checkLog.CheckedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
            <p><strong>HTTP Status:</strong> {checkLog.HttpStatusCode?.ToString() ?? "N/A"}</p>
            <p><strong>Error:</strong> {checkLog.ErrorMessage ?? "N/A"}</p>
            {(isRepeat ? $"<p><strong>Consecutive Failures:</strong> {incident.FailureCount}</p>" : "")}
            {(isRepeat ? $"<p><strong>Downtime:</strong> {FormatDuration(downtime)}</p>" : "")}
            <br/>
            <p><em>Sent by AliveMonitor</em></p>
            """;

        await SendEmailAsync(alertEmail, subject, body);
    }

    public async Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, string alertEmail)
    {
        var downtime = (incident.ResolvedAt ?? DateTime.UtcNow) - incident.OpenedAt;

        var subject = $"\u2705 AliveMonitor: {endpoint.FriendlyName} has Recovered";
        var body = $"""
            <h2>Endpoint Recovered</h2>
            <p><strong>Endpoint:</strong> {endpoint.FriendlyName}</p>
            <p><strong>URL:</strong> {endpoint.Url}</p>
            <p><strong>Recovered At:</strong> {incident.ResolvedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
            <p><strong>Total Downtime:</strong> {FormatDuration(downtime)}</p>
            <br/>
            <p><em>Sent by AliveMonitor</em></p>
            """;

        await SendEmailAsync(alertEmail, subject, body);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = true,
            };

            if (!string.IsNullOrEmpty(_emailSettings.Username))
            {
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
            }

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderAddress, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            logger.LogInformation("Alert email sent to {Email}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send alert email to {Email}", to);
        }
    }
}
