using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PulseDesk.Application.Features.Dashboard.Queries.GetDashboardStats;
using PulseDesk.Application.Interfaces.Hubs;
using PulseDesk.Shared.Wrappers;

namespace PulseDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("stats")]
        [Authorize(Policy = "CanViewReports")]
        public async Task<ActionResult<ApiResponse<DashBoardStatsPayload>>> GetStats()
        {
            var stats = await _mediator.Send(new GetDashboardStatsQuery());
            return Ok(stats);
        }
    }
}
