namespace AliveMonitor.Core.Entities;

public class HealthCheckLog
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }
    public DateTime CheckedAt { get; set; }
    public int? HttpStatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool IsHealthy { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryAttempts { get; set; }

    public MonitoredEndpoint Endpoint { get; set; } = default!;
}
