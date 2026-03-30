using Microsoft.AspNetCore.SignalR;
using PulseDesk.Application.Interfaces.Hubs;

namespace PulseDesk.API.Hubs
{
    public class DashboardHubContext:IDashboardHubContext
    {
        private readonly IHubContext<DashboardHub, IDashboardHubClient> _hubContext;
        private readonly  ILogger<DashboardHubContext> _logger;
        private static readonly string hubName = "admin-dashboard";
        public DashboardHubContext(IHubContext<DashboardHub, IDashboardHubClient> _hubContext, ILogger<DashboardHubContext> logger)
        {
            this._hubContext = _hubContext;
            _logger = logger;
        }

        async Task IDashboardHubContext.NotifyDashboardStatsUpdated(DashBoardStatsPayload stats)
        => await _hubContext.Clients.Group(hubName).DashboardStatsUpdated(stats);

        async Task IDashboardHubContext.NotifyUserDeactivated(int userId)
            => await _hubContext.Clients.Group(hubName).UserDeactivated(userId);

        async Task IDashboardHubContext.NotifyUserRegistered(UserRegisteredPayload payload)
        {
            _logger.LogInformation(
            "Pushing UserRegistered to admin-dashboard group. UserId: {UserId}",
            payload.Id); await _hubContext.Clients.Group(hubName).UserRegistered(payload);
            _logger.LogInformation("User Registered Push Completed");
        }

        async Task IDashboardHubContext.NotifyUserRoleUpdated(int userId, string roleName)
            => await _hubContext.Clients.Group(hubName).UserRoleupdated(userId, roleName);
    }
}
