using System.Security.Claims;
using System.Text.Json;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Enums;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliveMonitor.Api.Controllers;

[ApiController]
[Route("api/endpoints")]
[Authorize]
public class EndpointsController(
    IEndpointRepository endpointRepository,
    HealthCheckScheduler scheduler) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<EndpointResponse>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] EndpointStatus? status = null)
    {
        var endpoints = await endpointRepository.GetAllAsync(UserId, search, status);
        return Ok(endpoints.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EndpointResponse>> GetById(Guid id)
    {
        var endpoint = await endpointRepository.GetByIdAsync(id, UserId);
        if (endpoint is null) return NotFound();
        return Ok(MapToResponse(endpoint));
    }

    [HttpPost]
    public async Task<ActionResult<EndpointResponse>> Create([FromBody] CreateEndpointRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FriendlyName))
            return BadRequest(new { message = "Friendly name is required" });
        if (string.IsNullOrWhiteSpace(request.Url) || !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            return BadRequest(new { message = "A valid HTTP/HTTPS URL is required" });
        if (request.IntervalMinutes < 1)
            return BadRequest(new { message = "Interval must be at least 1 minute" });
        if (request.SslCheckEnabled && uri.Scheme != "https")
            return BadRequest(new { message = "SSL certificate monitoring requires an HTTPS URL" });

        var endpoint = new MonitoredEndpoint
        {
            UserId = UserId,
            FriendlyName = request.FriendlyName,
            Url = request.Url,
            IntervalMinutes = request.IntervalMinutes,
            TimeoutSeconds = request.TimeoutSeconds,
            IsEnabled = false,
            CustomHeadersJson = request.CustomHeaders is not null ? JsonSerializer.Serialize(request.CustomHeaders) : null,
            ExpectedStatusCode = request.ExpectedStatusCode,
            JsonPropertyName = request.JsonPropertyName,
            JsonPropertyExpectedValue = request.JsonPropertyExpectedValue,
            TeamId = request.TeamId,
            SslCheckEnabled = request.SslCheckEnabled,
            CurrentStatus = EndpointStatus.Disabled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await endpointRepository.CreateAsync(endpoint);
        return CreatedAtAction(nameof(GetById), new { id = endpoint.Id }, MapToResponse(endpoint));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EndpointResponse>> Update(Guid id, [FromBody] UpdateEndpointRequest request)
    {
        var endpoint = await endpointRepository.GetByIdAsync(id, UserId);
        if (endpoint is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.FriendlyName))
            return BadRequest(new { message = "Friendly name is required" });
        if (string.IsNullOrWhiteSpace(request.Url) || !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            return BadRequest(new { message = "A valid HTTP/HTTPS URL is required" });
        if (request.IntervalMinutes < 1)
            return BadRequest(new { message = "Interval must be at least 1 minute" });
        if (request.SslCheckEnabled && uri.Scheme != "https")
            return BadRequest(new { message = "SSL certificate monitoring requires an HTTPS URL" });

        endpoint.FriendlyName = request.FriendlyName;
        endpoint.Url = request.Url;
        endpoint.IntervalMinutes = request.IntervalMinutes;
        endpoint.TimeoutSeconds = request.TimeoutSeconds;
        endpoint.CustomHeadersJson = request.CustomHeaders is not null ? JsonSerializer.Serialize(request.CustomHeaders) : null;
        endpoint.ExpectedStatusCode = request.ExpectedStatusCode;
        endpoint.JsonPropertyName = request.JsonPropertyName;
        endpoint.JsonPropertyExpectedValue = request.JsonPropertyExpectedValue;
        endpoint.TeamId = request.TeamId;
        endpoint.SslCheckEnabled = request.SslCheckEnabled;

        await endpointRepository.UpdateAsync(endpoint);

        if (endpoint.IsEnabled)
            scheduler.ScheduleEndpoint(endpoint);

        return Ok(MapToResponse(endpoint));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var endpoint = await endpointRepository.GetByIdAsync(id, UserId);
        if (endpoint is null) return NotFound();

        scheduler.UnscheduleEndpoint(endpoint.Id);
        await endpointRepository.DeleteAsync(endpoint);
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<ActionResult<EndpointResponse>> Toggle(Guid id)
    {
        var endpoint = await endpointRepository.GetByIdAsync(id, UserId);
        if (endpoint is null) return NotFound();

        endpoint.IsEnabled = !endpoint.IsEnabled;
        endpoint.CurrentStatus = endpoint.IsEnabled ? EndpointStatus.Healthy : EndpointStatus.Disabled;

        await endpointRepository.UpdateAsync(endpoint);
        scheduler.ScheduleEndpoint(endpoint);

        return Ok(MapToResponse(endpoint));
    }

    private static EndpointResponse MapToResponse(MonitoredEndpoint e) => new(
        e.Id,
        e.FriendlyName,
        e.Url,
        e.IntervalMinutes,
        e.TimeoutSeconds,
        e.IsEnabled,
        e.CustomHeadersJson is not null ? JsonSerializer.Deserialize<Dictionary<string, string>>(e.CustomHeadersJson) : null,
        e.ExpectedStatusCode,
        e.JsonPropertyName,
        e.JsonPropertyExpectedValue,
        e.CurrentStatus,
        e.LastCheckedAt,
        e.CreatedAt,
        e.UpdatedAt,
        e.TeamId,
        e.Team?.Name,
        e.SslCheckEnabled,
        e.SslLastCheckedAt,
        e.SslCertificateExpiresAt,
        e.SslCertificateExpiresAt.HasValue ? (int)(e.SslCertificateExpiresAt.Value - DateTime.UtcNow).TotalDays : null);
}
