namespace AliveMonitor.Core.DTOs;

public record AlertRecipients(IReadOnlyList<string> Emails, IReadOnlyList<long> TelegramChatIds);
