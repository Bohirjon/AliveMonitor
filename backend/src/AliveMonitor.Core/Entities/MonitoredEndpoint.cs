using AliveMonitor.Core.Enums;

namespace AliveMonitor.Core.Entities;

public class MonitoredEndpoint
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FriendlyName { get; set; } = default!;
    public string Url { get; set; } = default!;
    public int IntervalMinutes { get; set; } = 1;
    public int TimeoutSeconds { get; set; } = 30;
    public bool IsEnabled { get; set; }
    public string? CustomHeadersJson { get; set; }
    public int ExpectedStatusCode { get; set; } = 200;
    public string? JsonPropertyName { get; set; }
    public string? JsonPropertyExpectedValue { get; set; }
    public EndpointStatus CurrentStatus { get; set; } = EndpointStatus.Disabled;
    public DateTime? LastCheckedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = default!;
    public ICollection<HealthCheckLog> HealthCheckLogs { get; set; } = [];
    public ICollection<Incident> Incidents { get; set; } = [];
}
