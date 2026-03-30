using MediatR;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Domain.Exceptions;
using PulseDesk.Shared.DTOs;
using PulseDesk.Shared.DTOs.Auth;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserDto>>
    {
        private readonly IUserRepository _userRepo;
        public GetCurrentUserQueryHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        async Task<ApiResponse<UserDto>> IRequestHandler<GetCurrentUserQuery, ApiResponse<UserDto>>.Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByIdWithRoleAsync(request.UserId);
            if (user is null) throw new NotFoundException("User", request.UserId);

            var responseDto = new UserDto
            {
                Id = request.UserId,
                FirstName = user.FirstName,
                LastLoginAt = user.LastLoginAt,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name,
                Policies = user.Role.RolePolicies.Select(rp => rp.Policy.Name).ToList(),
                Department = user.Department,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive,
            };
            return ApiResponse<UserDto>.Ok(responseDto);
        }
    }
}
