using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PulseDesk.Application.Interfaces.Hubs;
using PulseDesk.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Events
{
    public class UserRegisteredEventHandler:INotificationHandler<UserRegisteredEvent>
    {
        private readonly IDashboardHubContext _hubContext;
        private readonly IDbConnectionFactory _db;
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(IDashboardHubContext hubContext, IDbConnectionFactory db, ILogger<UserRegisteredEventHandler> _logger)
        {
            _hubContext = hubContext;
            _db = db;
            this._logger = _logger;
        }

        async Task INotificationHandler<UserRegisteredEvent>.Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Pushing new user to Dashboard Immediatly
                await _hubContext.NotifyUserRegistered(new UserRegisteredPayload
                {
                    Id = notification.UserId,
                    FullName = notification.FullName,
                    Email = notification.Email,
                    Role = notification.Role,
                    Department = notification.Department,
                    RegisteredAt = notification.RegisteredAt,
                });

                // 2. Re calculating user stats and pushing updated number
                // Using Dapper query - much faster than EF Core for aggrigations
                var stats = await GetDashboardStatsAsync();
                await _hubContext.NotifyDashboardStatsUpdated(stats);

                _logger.LogInformation("SignalR: Pushed UserRegistered event for {userId} to {hubName}", notification.UserId, "admin-dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserRegisteredEventHandler for {UserId}", notification.UserId);
            }
        }

        private async Task<DashBoardStatsPayload> GetDashboardStatsAsync()
        {
            const string sql = @"
            SELECT
                COUNT(*)                                            AS TotalUsers,
                SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END)      AS ActiveUsers,
                SUM(CASE WHEN CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
                         THEN 1 ELSE 0 END)                        AS NewUsersToday,
                SUM(CASE WHEN CreatedAt >= DATEADD(day, -7, GETUTCDATE())
                         THEN 1 ELSE 0 END)                        AS NewUsersThisWeek
            FROM PulseDesk.Users;";

            using var connection = _db.CreateConnection();
            return await connection.QuerySingleAsync<DashBoardStatsPayload>(sql);
        }
    }
}
