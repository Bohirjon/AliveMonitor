using AliveMonitor.Core.Configuration;
using Microsoft.Extensions.Options;
using Google.Apis.Auth;

namespace AliveMonitor.Infrastructure.Services;

public record GoogleUserInfo(string GoogleId, string Email, string Name, string? AvatarUrl);

public class GoogleAuthService(IOptions<GoogleAuthSettings> settings)
{
    private readonly GoogleAuthSettings _settings = settings.Value;

    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.ClientId]
            });

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.Name,
                payload.Picture);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
