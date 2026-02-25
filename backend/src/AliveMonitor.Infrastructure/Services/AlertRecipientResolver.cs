using AliveMonitor.Core.DTOs;
using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;

namespace AliveMonitor.Infrastructure.Services;

public class AlertRecipientResolver(ITeamRepository teamRepository, IUserRepository userRepository)
{
    public async Task<AlertRecipients> GetAlertRecipientsAsync(MonitoredEndpoint endpoint)
    {
        if (endpoint.TeamId is not null)
        {
            var team = await teamRepository.GetByIdAsync(endpoint.TeamId.Value, endpoint.UserId);
            if (team is not null && team.MemberEmails.Count > 0)
            {
                var telegramChatIds = team.TelegramChatId.HasValue
                    ? new List<long> { team.TelegramChatId.Value }
                    : new List<long>();
                return new AlertRecipients(team.MemberEmails, telegramChatIds);
            }
        }

        var user = await userRepository.GetByIdAsync(endpoint.UserId);
        if (user is not null)
        {
            var emails = new List<string> { user.AlertEmail };
            var telegramChatIds = user.TelegramChatId.HasValue
                ? new List<long> { user.TelegramChatId.Value }
                : new List<long>();
            return new AlertRecipients(emails, telegramChatIds);
        }

        return new AlertRecipients([], []);
    }
}
