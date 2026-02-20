using AliveMonitor.Core.Enums;

namespace AliveMonitor.Core.DTOs;

public record CreateEndpointRequest(
    string FriendlyName,
    string Url,
    int IntervalMinutes,
    int TimeoutSeconds = 30,
    Dictionary<string, string>? CustomHeaders = null,
    int ExpectedStatusCode = 200,
    string? JsonPropertyName = null,
    string? JsonPropertyExpectedValue = null);

public record UpdateEndpointRequest(
    string FriendlyName,
    string Url,
    int IntervalMinutes,
    int TimeoutSeconds = 30,
    Dictionary<string, string>? CustomHeaders = null,
    int ExpectedStatusCode = 200,
    string? JsonPropertyName = null,
    string? JsonPropertyExpectedValue = null);

public record EndpointResponse(
    Guid Id,
    string FriendlyName,
    string Url,
    int IntervalMinutes,
    int TimeoutSeconds,
    bool IsEnabled,
    Dictionary<string, string>? CustomHeaders,
    int ExpectedStatusCode,
    string? JsonPropertyName,
    string? JsonPropertyExpectedValue,
    EndpointStatus CurrentStatus,
    DateTime? LastCheckedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
