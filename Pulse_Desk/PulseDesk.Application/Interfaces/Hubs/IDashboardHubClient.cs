using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Interfaces.Hubs
{
    public interface IDashboardHubClient
    {
        Task UserRegistered(UserRegisteredPayload payload);
        Task DashboardStatsUpdated(DashBoardStatsPayload payload);
        Task UserDeactivated(int userId);
        Task UserRoleupdated(int userId, string newRole);
    }
    public class UserRegisteredPayload
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string? Department { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class DashBoardStatsPayload
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
    }
}
