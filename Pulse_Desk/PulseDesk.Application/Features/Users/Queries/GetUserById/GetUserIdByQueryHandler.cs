using MediatR;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Domain.Exceptions;
using PulseDesk.Shared.DTOs;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Queries.GetUserById
{
    public class GetUserIdByQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
    {
        private readonly IUserRepository _userRepo;
        public GetUserIdByQueryHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        async Task<ApiResponse<UserDto>> IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>.Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetByIdWithRoleAsync(request.UserId) ??
                throw new NotFoundException("User",request.UserId);

            var response = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                IsActive = user.IsActive,
                Role = user.Role.Name,
                Department = user.Department,
                JobTitle = user.JobTitle,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Policies = user.Role.RolePolicies.Select(rp => rp.Policy.Name).ToList(),
            };
            return ApiResponse<UserDto>.Ok(response);
        }
    }
}
