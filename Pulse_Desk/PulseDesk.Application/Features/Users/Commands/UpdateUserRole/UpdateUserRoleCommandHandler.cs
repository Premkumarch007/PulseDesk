using MediatR;
using Microsoft.Extensions.Logging;
using PulseDesk.Application.Features.Users.Events;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Domain.Exceptions;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Commands.UpdateUserRole
{
    public class UpdateUserRoleCommandHandler:IRequestHandler<UpdateUserRoleCommand,ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateUserRoleCommandHandler> _logger;
        public UpdateUserRoleCommandHandler(IUserRepository userRepo, IRoleRepository roleRepo, IMediator mediator ,ILogger<UpdateUserRoleCommandHandler> logger)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _mediator = mediator;
            _logger = logger;
        }
        async Task<ApiResponse<bool>> IRequestHandler<UpdateUserRoleCommand, ApiResponse<bool>>.Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByIdAsync(request.UserID)
                ?? throw new NotFoundException("User", request.UserID);

            var role = await _roleRepo.GetByNameAsync(request.RoleName)
                ?? throw new NotFoundException($"Role: {request.RoleName} not found");

            var oldRole = user.RoleId;
            user.RoleId = role.Id;

            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("User {user} role changed from {oldRole} to {newRole}",user.FullName,oldRole, role.Name);

            // Publishing to MediatR
            await _mediator.Publish(new UserRoleUpdatedEvent(request.UserID, role.Name));

            return ApiResponse<bool>.Ok(true, $"Role successfully updated to {role.Name}");
        }
    }
}
