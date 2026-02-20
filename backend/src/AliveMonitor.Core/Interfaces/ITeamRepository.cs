using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, Guid userId);
    Task<List<Team>> GetAllAsync(Guid userId);
    Task<Team> CreateAsync(Team team);
    Task UpdateAsync(Team team);
    Task DeleteAsync(Team team);
}
