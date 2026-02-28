namespace AliveMonitor.Core.Entities;

public class TelegramLinkCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TeamId { get; set; }
    public string Code { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
}
