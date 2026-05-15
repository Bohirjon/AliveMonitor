using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace AliveMonitor.Api.Middleware;

public sealed class HangfireBasicAuthFilter(string username, string password) : IDashboardAuthorizationFilter
{
    private readonly byte[] _userHash = SHA256.HashData(Encoding.UTF8.GetBytes(username));
    private readonly byte[] _passHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var header = http.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return Challenge(http);
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..].Trim()));
        }
        catch
        {
            return Challenge(http);
        }

        var separator = decoded.IndexOf(':');
        if (separator < 0)
        {
            return Challenge(http);
        }

        var providedUser = SHA256.HashData(Encoding.UTF8.GetBytes(decoded[..separator]));
        var providedPass = SHA256.HashData(Encoding.UTF8.GetBytes(decoded[(separator + 1)..]));

        if (CryptographicOperations.FixedTimeEquals(providedUser, _userHash) &&
            CryptographicOperations.FixedTimeEquals(providedPass, _passHash))
        {
            return true;
        }

        return Challenge(http);
    }

    private static bool Challenge(HttpContext http)
    {
        http.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire\"";
        http.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return false;
    }
}
