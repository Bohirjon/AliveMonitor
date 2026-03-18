namespace AliveMonitor.Core.DTOs;

public record GoogleSignInRequest(string IdToken);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);

public record UserDto(Guid Id, string Email, string Name, string? AvatarUrl, string AlertEmail, bool TelegramLinked, string? WebhookUrl);
