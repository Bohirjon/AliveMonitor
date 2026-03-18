using AliveMonitor.Core.Configuration;
using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AliveMonitor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    GoogleAuthService googleAuthService,
    ITokenService tokenService,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> GoogleSignIn([FromBody] GoogleSignInRequest request)
    {
        var googleUser = await googleAuthService.ValidateGoogleTokenAsync(request.IdToken);
        if (googleUser is null)
            return Unauthorized(new { message = "Invalid Google token" });

        var user = await userRepository.GetByGoogleIdAsync(googleUser.GoogleId);
        if (user is null)
        {
            user = new User
            {
                GoogleId = googleUser.GoogleId,
                Email = googleUser.Email,
                Name = googleUser.Name,
                AvatarUrl = googleUser.AvatarUrl,
                AlertEmail = googleUser.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await userRepository.CreateAsync(user);
        }
        else
        {
            user.Name = googleUser.Name;
            user.AvatarUrl = googleUser.AvatarUrl;
            user.Email = googleUser.Email;
            await userRepository.UpdateAsync(user);
        }

        return await GenerateAuthResponse(user);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = tokenService.HashToken(request.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenAsync(tokenHash);

        if (storedToken is null || !storedToken.IsActive)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        await refreshTokenRepository.RevokeAsync(storedToken);

        return await GenerateAuthResponse(storedToken.User);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = tokenService.HashToken(request.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenAsync(tokenHash);

        if (storedToken is not null)
            await refreshTokenRepository.RevokeAsync(storedToken);

        return NoContent();
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = tokenService.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
        };

        await refreshTokenRepository.CreateAsync(refreshTokenEntity);

        var userDto = new UserDto(user.Id, user.Email, user.Name, user.AvatarUrl, user.AlertEmail, user.TelegramChatId is not null, user.WebhookUrl);
        return new AuthResponse(accessToken, refreshToken, userDto);
    }
}
