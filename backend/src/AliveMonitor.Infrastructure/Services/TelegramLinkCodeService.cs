using System.Security.Cryptography;
using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace AliveMonitor.Infrastructure.Services;

public class TelegramLinkCodeService(
    ITelegramLinkCodeRepository linkCodeRepository,
    IOptions<AlertSettings> alertSettings)
{
    private readonly TelegramSettings _telegramSettings = alertSettings.Value.Telegram;

    public async Task<LinkCodeResponse> GenerateCodeAsync(Guid userId, Guid? teamId = null)
    {
        var code = GenerateSecureCode(8);
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var linkCode = new TelegramLinkCode
        {
            UserId = userId,
            TeamId = teamId,
            Code = code,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
        };

        await linkCodeRepository.CreateAsync(linkCode);

        var deepLink = $"https://t.me/{_telegramSettings.BotUsername}?start={code}";
        return new LinkCodeResponse(code, deepLink, expiresAt);
    }

    private static string GenerateSecureCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var result = new char[length];
        for (var i = 0; i < length; i++)
            result[i] = chars[bytes[i] % chars.Length];
        return new string(result);
    }
}
