namespace AliveMonitor.Core.DTOs;

public record CreateTeamRequest(
    string Name,
    List<string> MemberEmails,
    string? WebhookUrl = null);

public record UpdateTeamRequest(
    string Name,
    List<string> MemberEmails,
    string? WebhookUrl = null);

public record TeamResponse(
    Guid Id,
    string Name,
    List<string> MemberEmails,
    bool TelegramLinked,
    string? WebhookUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
