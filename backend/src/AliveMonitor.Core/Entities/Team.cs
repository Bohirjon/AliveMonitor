namespace AliveMonitor.Core.Entities;

public class Team
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
    public List<string> MemberEmails { get; set; } = [];
    public long? TelegramChatId { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = default!;
    public ICollection<MonitoredEndpoint> MonitoredEndpoints { get; set; } = [];
}
