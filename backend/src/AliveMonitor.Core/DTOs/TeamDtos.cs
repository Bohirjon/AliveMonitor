namespace AliveMonitor.Core.DTOs;

public record CreateTeamRequest(
    string Name,
    List<string> MemberEmails);

public record UpdateTeamRequest(
    string Name,
    List<string> MemberEmails);

public record TeamResponse(
    Guid Id,
    string Name,
    List<string> MemberEmails,
    bool TelegramLinked,
    DateTime CreatedAt,
    DateTime UpdatedAt);
