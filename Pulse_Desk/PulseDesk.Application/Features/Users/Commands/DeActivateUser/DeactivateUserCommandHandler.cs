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

namespace PulseDesk.Application.Features.Users.Commands.DeActivateUser
{
    public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<DeactivateUserCommandHandler> _logger;
        public DeactivateUserCommandHandler(IUserRepository userRepo, IMediator mediator ,ILogger<DeactivateUserCommandHandler> logger)
        {
            _userRepo = userRepo;
            _mediator = mediator;
            _logger = logger;
        }
        async Task<ApiResponse<bool>> IRequestHandler<DeactivateUserCommand, ApiResponse<bool>>.Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            // Preventing Self-Deactivation
            if (request.UserId == request.RequestingUserId)
                throw new DomainException("You cannot deactivate your own account.");

            var user = await _userRepo.GetByIdWithRoleAsync(request.UserId)
                ?? throw new NotFoundException("User not found with user Id: {userId}", request.UserId);

            if (!user.IsActive)
                throw new DomainException("User is already Deactivated");

            // De-activating 
            user.IsActive = false;
            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("User {user} successfully deactivated by {reqUserId}", user.Id, request.RequestingUserId);
            await _mediator.Publish(new UserDeactivatedEvent(request.UserId));
            return ApiResponse<bool>.Ok(true, "Deactivation successful");
        }
    }
}
