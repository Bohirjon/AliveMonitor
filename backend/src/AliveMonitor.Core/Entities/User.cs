namespace AliveMonitor.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string GoogleId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public string AlertEmail { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public long? TelegramChatId { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<MonitoredEndpoint> MonitoredEndpoints { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
}
