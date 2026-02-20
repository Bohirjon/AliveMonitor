using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
}
