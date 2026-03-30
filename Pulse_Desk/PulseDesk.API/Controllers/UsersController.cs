using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PulseDesk.Application.Features.Users.Commands.DeActivateUser;
using PulseDesk.Application.Features.Users.Commands.UpdateUserRole;
using PulseDesk.Application.Features.Users.Queries.GetUserById;
using PulseDesk.Application.Features.Users.Queries.GetUsers;
using PulseDesk.Shared.DTOs;
using PulseDesk.Shared.DTOs.Users;
using PulseDesk.Shared.Wrappers;

namespace PulseDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        [Authorize(Policy ="CanManageUsers")]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserListItemDto>>>> GetUsers([FromQuery] UserListRequestDto request)
        {
            var result = await _mediator.Send(new GetUsersQuery(request));
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy ="CanManageUsers")]
        public async Task<ActionResult<ApiResponse<UserDto>>>GetUser(int id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(result);
        }

        [HttpPut("{id:int}/role")]
        [Authorize(Policy ="CanManageRoles")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateRole(int id, [FromBody] UpdateRoleRequestDto request)
        {
            var result = await _mediator.Send(new UpdateUserRoleCommand(id, request.RoleName));
            return Ok(result);
        }

        [HttpPut("{id:int}/deactivate")]
        [Authorize("CanManageUsers")]
        public async Task<ActionResult<ApiResponse<bool>>>DeactivateUser(int id)
        {
            var requestingUser = int.Parse(User.FindFirst("userId")!.Value);
            var result = await _mediator.Send(new DeactivateUserCommand(id, requestingUser));
            return Ok(result);
        }
    }
}
