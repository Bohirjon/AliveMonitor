using System.Security.Claims;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliveMonitor.Api.Controllers;

[ApiController]
[Route("api/telegram")]
[Authorize]
public class TelegramController(
    TelegramLinkCodeService linkCodeService,
    IUserRepository userRepository,
    ITeamRepository teamRepository) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("link-code")]
    public async Task<ActionResult<LinkCodeResponse>> GenerateLinkCode([FromBody] GenerateLinkCodeRequest request)
    {
        var response = await linkCodeService.GenerateCodeAsync(UserId, request.TeamId);
        return Ok(response);
    }

    [HttpGet("status")]
    public async Task<ActionResult<TelegramStatusResponse>> GetStatus()
    {
        var user = await userRepository.GetByIdAsync(UserId);
        if (user is null) return NotFound();

        return Ok(new TelegramStatusResponse(user.TelegramChatId is not null, user.TelegramChatId));
    }

    [HttpGet("status/team/{teamId:guid}")]
    public async Task<ActionResult<TelegramStatusResponse>> GetTeamStatus(Guid teamId)
    {
        var team = await teamRepository.GetByIdAsync(teamId, UserId);
        if (team is null) return NotFound();

        return Ok(new TelegramStatusResponse(team.TelegramChatId is not null, team.TelegramChatId));
    }

    [HttpDelete("unlink")]
    public async Task<IActionResult> Unlink()
    {
        var user = await userRepository.GetByIdAsync(UserId);
        if (user is null) return NotFound();

        user.TelegramChatId = null;
        await userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpDelete("unlink/team/{teamId:guid}")]
    public async Task<IActionResult> UnlinkTeam(Guid teamId)
    {
        var team = await teamRepository.GetByIdAsync(teamId, UserId);
        if (team is null) return NotFound();

        team.TelegramChatId = null;
        await teamRepository.UpdateAsync(team);
        return NoContent();
    }
}
