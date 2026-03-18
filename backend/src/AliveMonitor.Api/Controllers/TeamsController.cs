using System.Security.Claims;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliveMonitor.Api.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController(ITeamRepository teamRepository) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<TeamResponse>>> GetAll()
    {
        var teams = await teamRepository.GetAllAsync(UserId);
        return Ok(teams.Select(MapToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamResponse>> GetById(Guid id)
    {
        var team = await teamRepository.GetByIdAsync(id, UserId);
        if (team is null) return NotFound();
        return Ok(MapToResponse(team));
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponse>> Create([FromBody] CreateTeamRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Team name is required" });
        if (request.MemberEmails is null || request.MemberEmails.Count == 0)
            return BadRequest(new { message = "At least one member email is required" });

        var team = new Team
        {
            UserId = UserId,
            Name = request.Name,
            MemberEmails = request.MemberEmails,
            WebhookUrl = request.WebhookUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await teamRepository.CreateAsync(team);
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, MapToResponse(team));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TeamResponse>> Update(Guid id, [FromBody] UpdateTeamRequest request)
    {
        var team = await teamRepository.GetByIdAsync(id, UserId);
        if (team is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Team name is required" });
        if (request.MemberEmails is null || request.MemberEmails.Count == 0)
            return BadRequest(new { message = "At least one member email is required" });

        team.Name = request.Name;
        team.MemberEmails = request.MemberEmails;
        team.WebhookUrl = request.WebhookUrl;

        await teamRepository.UpdateAsync(team);
        return Ok(MapToResponse(team));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var team = await teamRepository.GetByIdAsync(id, UserId);
        if (team is null) return NotFound();

        await teamRepository.DeleteAsync(team);
        return NoContent();
    }

    private static TeamResponse MapToResponse(Team t) => new(
        t.Id,
        t.Name,
        t.MemberEmails,
        t.TelegramChatId is not null,
        t.WebhookUrl,
        t.CreatedAt,
        t.UpdatedAt);
}
