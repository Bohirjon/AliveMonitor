using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AliveMonitor.Infrastructure.Services;

public class TelegramBotService(
    IServiceScopeFactory scopeFactory,
    IOptions<AlertSettings> alertSettings,
    ILogger<TelegramBotService> logger) : BackgroundService
{
    private readonly TelegramSettings _settings = alertSettings.Value.Telegram;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Telegram bot is disabled, skipping");
            return;
        }

        var bot = new TelegramBotClient(_settings.BotToken);
        var offset = 0;

        logger.LogInformation("Telegram bot started polling");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await bot.GetUpdates(offset, timeout: 30, cancellationToken: stoppingToken);

                foreach (var update in updates)
                {
                    offset = update.Id + 1;

                    if (update.Message?.Text is not null)
                    {
                        await HandleMessageAsync(bot, update.Message, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Telegram bot polling loop");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task HandleMessageAsync(TelegramBotClient bot, Message message, CancellationToken ct)
    {
        var text = message.Text!.Trim();
        var chatId = message.Chat.Id;

        try
        {
            if (text.StartsWith("/start"))
            {
                var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    await HandleStartCommandAsync(bot, chatId, parts[1].Trim(), ct);
                else
                    await bot.SendMessage(chatId, "Welcome to AliveMonitor Bot!\n\nUse /help to see available commands.", cancellationToken: ct);
            }
            else if (text.StartsWith("/status"))
            {
                await HandleStatusCommandAsync(bot, chatId, ct);
            }
            else if (text.StartsWith("/help"))
            {
                await HandleHelpCommandAsync(bot, chatId, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling Telegram message from chat {ChatId}", chatId);
        }
    }

    private async Task HandleStartCommandAsync(TelegramBotClient bot, long chatId, string code, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var linkCodeRepo = scope.ServiceProvider.GetRequiredService<ITelegramLinkCodeRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var teamRepo = scope.ServiceProvider.GetRequiredService<ITeamRepository>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var linkCode = await linkCodeRepo.GetByCodeAsync(code);
        if (linkCode is null)
        {
            await bot.SendMessage(chatId, "Invalid or expired link code. Please generate a new one from the AliveMonitor dashboard.", cancellationToken: ct);
            return;
        }

        if (linkCode.TeamId.HasValue)
        {
            var team = await teamRepo.GetByIdAsync(linkCode.TeamId.Value, linkCode.UserId);
            if (team is not null)
            {
                team.TelegramChatId = chatId;
                await teamRepo.UpdateAsync(team);
                await linkCodeRepo.DeleteAsync(linkCode);
                await bot.SendMessage(chatId, $"Telegram linked to team \"{team.Name}\" successfully! You will now receive alerts here.", cancellationToken: ct);
                logger.LogInformation("Telegram chat {ChatId} linked to team {TeamId}", chatId, team.Id);
                return;
            }
        }

        var user = await userRepo.GetByIdAsync(linkCode.UserId);
        if (user is not null)
        {
            user.TelegramChatId = chatId;
            await userRepo.UpdateAsync(user);
            await linkCodeRepo.DeleteAsync(linkCode);
            await bot.SendMessage(chatId, "Telegram linked to your AliveMonitor account successfully! You will now receive alerts here.", cancellationToken: ct);
            logger.LogInformation("Telegram chat {ChatId} linked to user {UserId}", chatId, user.Id);
        }
    }

    private async Task HandleStatusCommandAsync(TelegramBotClient bot, long chatId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, ct);
        var teams = await db.Teams.Where(t => t.TelegramChatId == chatId).ToListAsync(ct);

        // If no direct user link, resolve user through linked teams
        if (user is null && teams.Count > 0)
        {
            var teamOwnerId = teams[0].UserId;
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == teamOwnerId, ct);
        }

        if (user is null && teams.Count == 0)
        {
            await bot.SendMessage(chatId, "This chat is not linked to any AliveMonitor account. Link it from the Settings page.", cancellationToken: ct);
            return;
        }

        var lines = new List<string>();

        if (user is not null)
        {
            var endpoints = await db.MonitoredEndpoints
                .Where(e => e.UserId == user.Id && e.IsEnabled)
                .OrderBy(e => e.FriendlyName)
                .ToListAsync(ct);

            lines.Add($"<b>User: {EscapeHtml(user.Name)}</b>");
            lines.Add("");
            if (endpoints.Count == 0)
            {
                lines.Add("No enabled endpoints found.");
            }
            else
            {
                foreach (var ep in endpoints)
                {
                    var statusIcon = ep.CurrentStatus.ToString() switch
                    {
                        "Healthy" => "✅",
                        "Unhealthy" => "🔴",
                        _ => "⚪",
                    };
                    var lastChecked = ep.LastCheckedAt?.ToString("HH:mm:ss UTC") ?? "Never";
                    lines.Add($"{statusIcon} <b>{EscapeHtml(ep.FriendlyName)}</b> — {lastChecked}");
                }
            }
        }

        foreach (var team in teams)
        {
            if (lines.Count > 0) lines.Add("");

            var teamEndpoints = await db.MonitoredEndpoints
                .Where(e => e.TeamId == team.Id && e.IsEnabled)
                .OrderBy(e => e.FriendlyName)
                .ToListAsync(ct);

            lines.Add($"<b>Team: {EscapeHtml(team.Name)}</b>");
            lines.Add("");
            if (teamEndpoints.Count == 0)
            {
                lines.Add("No enabled endpoints found.");
            }
            else
            {
                foreach (var ep in teamEndpoints)
                {
                    var statusIcon = ep.CurrentStatus.ToString() switch
                    {
                        "Healthy" => "✅",
                        "Unhealthy" => "🔴",
                        _ => "⚪",
                    };
                    var lastChecked = ep.LastCheckedAt?.ToString("HH:mm:ss UTC") ?? "Never";
                    lines.Add($"{statusIcon} <b>{EscapeHtml(ep.FriendlyName)}</b> — {lastChecked}");
                }
            }
        }

        await bot.SendMessage(chatId, string.Join("\n", lines), parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private static async Task HandleHelpCommandAsync(TelegramBotClient bot, long chatId, CancellationToken ct)
    {
        var message = """
            <b>AliveMonitor Bot Commands</b>

            /start &lt;code&gt; — Link this chat to your account
            /status — View your endpoint statuses
            /help — Show this help message
            """;

        await bot.SendMessage(chatId, message, parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private static string EscapeHtml(string text)
        => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
