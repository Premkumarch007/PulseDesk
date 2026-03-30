using Dapper;
using MediatR;
using PulseDesk.Application.Interfaces.Hubs;
using PulseDesk.Application.Interfaces.Services;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, ApiResponse<DashBoardStatsPayload>>
    {
        private readonly IDbConnectionFactory _db;
        public GetDashboardStatsQueryHandler(IDbConnectionFactory db)
        {
            _db = db;
        }
        async Task<ApiResponse<DashBoardStatsPayload>> IRequestHandler<GetDashboardStatsQuery, ApiResponse<DashBoardStatsPayload>>.Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
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
            var stats = await connection.QuerySingleAsync<DashBoardStatsPayload>(sql);
            return ApiResponse<DashBoardStatsPayload>.Ok(stats);
        }
    }
}
