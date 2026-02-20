using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string tokenHash);
    Task CreateAsync(RefreshToken refreshToken);
    Task RevokeAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(Guid userId);
}
