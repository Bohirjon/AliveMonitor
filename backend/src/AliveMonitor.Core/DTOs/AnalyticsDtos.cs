namespace AliveMonitor.Core.DTOs;

public record AnalyticsSummaryResponse(
    double UptimePercentage,
    double AvgResponseTimeMs,
    int TotalChecks,
    int TotalIncidents);

public record CheckLogResponse(
    Guid Id,
    DateTime CheckedAt,
    int? HttpStatusCode,
    long ResponseTimeMs,
    bool IsHealthy,
    string? ErrorMessage);

public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record IncidentResponse(
    Guid Id,
    DateTime OpenedAt,
    DateTime LastNotifiedAt,
    DateTime? ResolvedAt,
    int FailureCount);
