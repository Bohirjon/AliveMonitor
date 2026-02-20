namespace AliveMonitor.Core.Entities;

public class SslCertificateCheckLog
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }
    public DateTime CheckedAt { get; set; }
    public bool IsValid { get; set; }
    public string? SubjectName { get; set; }
    public string? IssuerName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public string? ErrorMessage { get; set; }

    public MonitoredEndpoint Endpoint { get; set; } = default!;
}
