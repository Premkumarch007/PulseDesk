using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Interfaces.Hubs
{
    [Authorize]
    public class DashboardHub : Hub<IDashboardHubClient>
    {
        private readonly ILogger<DashboardHub> _logger;
        public DashboardHub(ILogger<DashboardHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("userId")!.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("Hub connected — UserId: {UserId}, Role: [{Role}], ConnectionId: {ConnId}", userId, userRole, Context.ConnectionId);

            var isAdminOrManager = userRole?.Trim().Equals("Admin",
                               StringComparison.OrdinalIgnoreCase) == true
                        || userRole?.Trim().Equals("Manager",
                               StringComparison.OrdinalIgnoreCase) == true;
            if (isAdminOrManager)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId, "admin-dashboard");

                _logger.LogInformation(
                    "Added {ConnectionId} to admin-dashboard group",
                    Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning(
                    "User {UserId} with role {Role} NOT added to admin-dashboard",
                    userId, userRole);
            }
            if (userId is not null)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {   
            var userId = Context.User?.FindFirst("userId")?.Value;

            _logger.LogInformation("User {userId} disconnected from dashboardHub", userId);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
