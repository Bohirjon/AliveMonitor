using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class TelegramLinkCodeRepository(AppDbContext db) : ITelegramLinkCodeRepository
{
    public async Task<TelegramLinkCode?> GetByCodeAsync(string code)
        => await db.TelegramLinkCodes
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Code == code && t.ExpiresAt > DateTime.UtcNow);

    public async Task CreateAsync(TelegramLinkCode linkCode)
    {
        db.TelegramLinkCodes.Add(linkCode);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TelegramLinkCode linkCode)
    {
        db.TelegramLinkCodes.Remove(linkCode);
        await db.SaveChangesAsync();
    }

    public async Task DeleteExpiredAsync()
    {
        await db.TelegramLinkCodes
            .Where(t => t.ExpiresAt <= DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }
}
