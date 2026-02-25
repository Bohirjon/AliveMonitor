using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface ITelegramLinkCodeRepository
{
    Task<TelegramLinkCode?> GetByCodeAsync(string code);
    Task CreateAsync(TelegramLinkCode linkCode);
    Task DeleteAsync(TelegramLinkCode linkCode);
    Task DeleteExpiredAsync();
}
