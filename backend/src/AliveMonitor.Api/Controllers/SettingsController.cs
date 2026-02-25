using System.Security.Claims;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliveMonitor.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(IUserRepository userRepository) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var user = await userRepository.GetByIdAsync(UserId);
        if (user is null) return NotFound();

        return Ok(new UserDto(user.Id, user.Email, user.Name, user.AvatarUrl, user.AlertEmail, user.TelegramChatId is not null));
    }

    [HttpPut("alert-email")]
    public async Task<IActionResult> UpdateAlertEmail([FromBody] UpdateAlertEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AlertEmail))
            return BadRequest(new { message = "Alert email is required" });

        var user = await userRepository.GetByIdAsync(UserId);
        if (user is null) return NotFound();

        user.AlertEmail = request.AlertEmail;
        await userRepository.UpdateAsync(user);

        return NoContent();
    }
}

public record UpdateAlertEmailRequest(string AlertEmail);
