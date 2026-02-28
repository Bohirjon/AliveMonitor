using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace AliveMonitor.Infrastructure.Services;

public class TelegramAlertService(IOptions<AlertSettings> alertSettings, ILogger<TelegramAlertService> logger) : IAlertService
{
    private readonly TelegramSettings _settings = alertSettings.Value.Telegram;
    private readonly TelegramBotClient? _bot = alertSettings.Value.Telegram.Enabled
        && !string.IsNullOrWhiteSpace(alertSettings.Value.Telegram.BotToken)
        ? new TelegramBotClient(alertSettings.Value.Telegram.BotToken)
        : null;

    public async Task SendFailureAlertAsync(MonitoredEndpoint endpoint, Incident incident, HealthCheckLog checkLog, AlertRecipients recipients)
    {
        if (_bot is null || recipients.TelegramChatIds.Count == 0) return;

        var isRepeat = incident.FailureCount > 1;
        var downtime = DateTime.UtcNow - incident.OpenedAt;

        var message = $"""
            <b>⚠️ Endpoint Health Check Failed</b>

            <b>Endpoint:</b> {EscapeHtml(endpoint.FriendlyName)}
            <b>URL:</b> {EscapeHtml(endpoint.Url)}
            <b>Time:</b> {checkLog.CheckedAt:yyyy-MM-dd HH:mm:ss} UTC
            <b>HTTP Status:</b> {checkLog.HttpStatusCode?.ToString() ?? "N/A"}
            <b>Error:</b> {EscapeHtml(checkLog.ErrorMessage ?? "N/A")}
            """;

        if (isRepeat)
        {
            message += $"""

                <b>Consecutive Failures:</b> {incident.FailureCount}
                <b>Downtime:</b> {FormatDuration(downtime)}
                """;
        }

        await SendToAllAsync(recipients.TelegramChatIds, message);
    }

    public async Task SendRecoveryAlertAsync(MonitoredEndpoint endpoint, Incident incident, AlertRecipients recipients)
    {
        if (_bot is null || recipients.TelegramChatIds.Count == 0) return;

        var downtime = (incident.ResolvedAt ?? DateTime.UtcNow) - incident.OpenedAt;

        var message = $"""
            <b>✅ Endpoint Recovered</b>

            <b>Endpoint:</b> {EscapeHtml(endpoint.FriendlyName)}
            <b>URL:</b> {EscapeHtml(endpoint.Url)}
            <b>Recovered At:</b> {incident.ResolvedAt:yyyy-MM-dd HH:mm:ss} UTC
            <b>Total Downtime:</b> {FormatDuration(downtime)}
            """;

        await SendToAllAsync(recipients.TelegramChatIds, message);
    }

    public async Task SendSslExpirationAlertAsync(MonitoredEndpoint endpoint, SslCertificateCheckLog checkLog, int thresholdDays, AlertRecipients recipients)
    {
        if (_bot is null || recipients.TelegramChatIds.Count == 0) return;

        var (urgency, icon) = thresholdDays switch
        {
            <= 1 => ("CRITICAL", "🚨"),
            <= 7 => ("Warning", "⚠️"),
            _ => ("Notice", "🔒"),
        };

        var message = $"""
            <b>{icon} SSL Certificate {urgency}</b>

            <b>Endpoint:</b> {EscapeHtml(endpoint.FriendlyName)}
            <b>URL:</b> {EscapeHtml(endpoint.Url)}
            <b>Certificate Subject:</b> {EscapeHtml(checkLog.SubjectName ?? "N/A")}
            <b>Issuer:</b> {EscapeHtml(checkLog.IssuerName ?? "N/A")}
            <b>Expires:</b> {checkLog.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC
            <b>Days Remaining:</b> {checkLog.DaysUntilExpiry}
            """;

        await SendToAllAsync(recipients.TelegramChatIds, message);
    }

    private async Task SendToAllAsync(IReadOnlyList<long> chatIds, string message)
    {
        foreach (var chatId in chatIds)
        {
            try
            {
                await _bot!.SendMessage(chatId, message, parseMode: ParseMode.Html);
                logger.LogInformation("Telegram alert sent to chat {ChatId}", chatId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send Telegram alert to chat {ChatId}", chatId);
            }
        }
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
    }

    private static string EscapeHtml(string text)
        => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
