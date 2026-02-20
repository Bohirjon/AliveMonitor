using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;

namespace AliveMonitor.Infrastructure.Services;

public class AlertEmailResolver(ITeamRepository teamRepository, IUserRepository userRepository)
{
    public async Task<IReadOnlyList<string>> GetAlertEmailsAsync(MonitoredEndpoint endpoint)
    {
        if (endpoint.TeamId is not null)
        {
            var team = await teamRepository.GetByIdAsync(endpoint.TeamId.Value, endpoint.UserId);
            if (team is not null && team.MemberEmails.Count > 0)
                return team.MemberEmails;
        }

        var user = await userRepository.GetByIdAsync(endpoint.UserId);
        return user is not null ? [user.AlertEmail] : [];
    }
}
