using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Interfaces.Hubs
{
    public interface IDashboardHubContext
    {
        Task NotifyUserRegistered(UserRegisteredPayload payload);
        Task NotifyDashboardStatsUpdated(DashBoardStatsPayload stats);
        Task NotifyUserDeactivated(int userId);
        Task NotifyUserRoleUpdated(int userId,  string roleName);
    }
}
