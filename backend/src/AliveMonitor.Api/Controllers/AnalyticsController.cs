using System.Security.Claims;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliveMonitor.Api.Controllers;

[ApiController]
[Route("api/endpoints/{endpointId:guid}")]
[Authorize]
public class AnalyticsController(
    IEndpointRepository endpointRepository,
    IHealthCheckLogRepository checkLogRepository,
    IIncidentRepository incidentRepository) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsSummaryResponse>> GetAnalytics(
        Guid endpointId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
    {
        var endpoint = await endpointRepository.GetByIdAsync(endpointId, UserId);
        if (endpoint is null) return NotFound();

        var fromDate = from?.UtcDateTime ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to?.UtcDateTime ?? DateTime.UtcNow;

        var logs = await checkLogRepository.GetForAnalyticsAsync(endpointId, fromDate, toDate);
        var incidents = await incidentRepository.GetByEndpointAsync(endpointId, fromDate, toDate);

        var totalChecks = logs.Count;
        var healthyChecks = logs.Count(l => l.IsHealthy);
        var uptimePercentage = totalChecks > 0 ? (double)healthyChecks / totalChecks * 100 : 0;
        var avgResponseTime = totalChecks > 0 ? logs.Average(l => l.ResponseTimeMs) : 0;

        return Ok(new AnalyticsSummaryResponse(
            Math.Round(uptimePercentage, 2),
            Math.Round(avgResponseTime, 2),
            totalChecks,
            incidents.Count));
    }

    [HttpGet("checks")]
    public async Task<ActionResult<PaginatedResponse<CheckLogResponse>>> GetChecks(
        Guid endpointId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var endpoint = await endpointRepository.GetByIdAsync(endpointId, UserId);
        if (endpoint is null) return NotFound();

        var fromDate = from?.UtcDateTime ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to?.UtcDateTime ?? DateTime.UtcNow;

        var totalCount = await checkLogRepository.GetCountAsync(endpointId, fromDate, toDate);
        var logs = await checkLogRepository.GetByEndpointAsync(endpointId, fromDate, toDate, page, pageSize);

        var items = logs.Select(l => new CheckLogResponse(
            l.Id, l.CheckedAt, l.HttpStatusCode, l.ResponseTimeMs, l.IsHealthy, l.ErrorMessage, l.RetryAttempts)).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedResponse<CheckLogResponse>(items, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("incidents")]
    public async Task<ActionResult<List<IncidentResponse>>> GetIncidents(
        Guid endpointId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
    {
        var endpoint = await endpointRepository.GetByIdAsync(endpointId, UserId);
        if (endpoint is null) return NotFound();

        var fromDate = from?.UtcDateTime ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to?.UtcDateTime ?? DateTime.UtcNow;

        var incidents = await incidentRepository.GetByEndpointAsync(endpointId, fromDate, toDate);

        return Ok(incidents.Select(i => new IncidentResponse(
            i.Id, i.OpenedAt, i.LastNotifiedAt, i.ResolvedAt, i.FailureCount)).ToList());
    }
}
