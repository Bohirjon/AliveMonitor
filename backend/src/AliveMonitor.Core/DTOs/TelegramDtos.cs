namespace AliveMonitor.Core.DTOs;

public record GenerateLinkCodeRequest(Guid? TeamId = null);

public record LinkCodeResponse(string Code, string DeepLink, DateTime ExpiresAt);

public record TelegramStatusResponse(bool IsLinked, string? ChatId);
