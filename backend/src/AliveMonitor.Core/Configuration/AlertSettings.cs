namespace AliveMonitor.Core.Configuration;

public class AlertSettings
{
    public const string SectionName = "Alerts";

    public string Provider { get; set; } = "Email";
    public int ThrottleIntervalMinutes { get; set; } = 10;
    public EmailSettings Email { get; set; } = new();
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = default!;
    public int SmtpPort { get; set; } = 587;
    public string SenderAddress { get; set; } = default!;
    public string SenderName { get; set; } = "AliveMonitor";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
