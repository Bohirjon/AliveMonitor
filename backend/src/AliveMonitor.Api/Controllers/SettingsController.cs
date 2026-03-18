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

        return Ok(new UserDto(user.Id, user.Email, user.Name, user.AvatarUrl, user.AlertEmail, user.TelegramChatId is not null, user.WebhookUrl));
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

    [HttpPut("webhook-url")]
    public async Task<IActionResult> UpdateWebhookUrl([FromBody] UpdateWebhookUrlRequest request)
    {
        var user = await userRepository.GetByIdAsync(UserId);
        if (user is null) return NotFound();

        var url = request.WebhookUrl?.Trim();
        if (!string.IsNullOrEmpty(url))
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
                return BadRequest(new { message = "Webhook URL must be a valid HTTP or HTTPS URL" });
        }

        user.WebhookUrl = string.IsNullOrEmpty(url) ? null : url;
        await userRepository.UpdateAsync(user);

        return NoContent();
    }
}

public record UpdateAlertEmailRequest(string AlertEmail);
public record UpdateWebhookUrlRequest(string? WebhookUrl);
