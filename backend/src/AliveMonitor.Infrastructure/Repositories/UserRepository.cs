using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
        => await db.Users.FindAsync(id);

    public async Task<User?> GetByGoogleIdAsync(string googleId)
        => await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }
}
