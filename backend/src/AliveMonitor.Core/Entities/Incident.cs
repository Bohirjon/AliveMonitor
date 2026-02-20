namespace AliveMonitor.Core.Entities;

public class Incident
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime LastNotifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int FailureCount { get; set; }

    public bool IsOpen => ResolvedAt is null;

    public MonitoredEndpoint Endpoint { get; set; } = default!;
}
