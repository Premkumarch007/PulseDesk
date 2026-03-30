using MediatR;
using PulseDesk.Application.Interfaces.Hubs;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public record GetDashboardStatsQuery : IRequest<ApiResponse<DashBoardStatsPayload>>;
}
