using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class TeamRepository(AppDbContext db) : ITeamRepository
{
    public async Task<Team?> GetByIdAsync(Guid id, Guid userId)
        => await db.Teams
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<List<Team>> GetAllAsync(Guid userId)
        => await db.Teams
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();

    public async Task<Team> CreateAsync(Team team)
    {
        db.Teams.Add(team);
        await db.SaveChangesAsync();
        return team;
    }

    public async Task UpdateAsync(Team team)
    {
        team.UpdatedAt = DateTime.UtcNow;
        db.Teams.Update(team);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Team team)
    {
        db.Teams.Remove(team);
        await db.SaveChangesAsync();
    }
}
