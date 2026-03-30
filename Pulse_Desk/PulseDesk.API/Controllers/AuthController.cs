using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PulseDesk.Application.Features.Auth.Commands.Login;
using PulseDesk.Application.Features.Auth.Commands.Register;
using PulseDesk.Application.Features.Auth.Queries.GetCurrentUser;
using PulseDesk.Shared.DTOs;
using PulseDesk.Shared.DTOs.Auth;
using PulseDesk.Shared.Wrappers;

namespace PulseDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _mediator.Send(new RegisterCommand(request));
            if (!result.Success) return BadRequest();
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginRequestDto request)
        {
            var result = await _mediator.Send(new LoginCommand(request));
            if (!result.Success) return Unauthorized();
            return Ok(result);
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto?>>> Me()
        {
            // Extracting userId from JWT claims
            var userIdClaims = User.FindFirst("userId")?.Value;
            if(userIdClaims is null || !int.TryParse(userIdClaims, out var userId))
                return Unauthorized();

            var result = await _mediator.Send(new GetCurrentUserQuery(userId));
            return Ok(result);
        }
    }
}
