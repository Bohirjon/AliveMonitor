using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
        => await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == tokenHash && rt.RevokedAt == null);

    public async Task CreateAsync(RefreshToken refreshToken)
    {
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        await db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, DateTime.UtcNow));
    }
}
