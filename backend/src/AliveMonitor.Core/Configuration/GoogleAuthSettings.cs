namespace AliveMonitor.Core.Configuration;

public class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";

    public string ClientId { get; set; } = default!;
}
