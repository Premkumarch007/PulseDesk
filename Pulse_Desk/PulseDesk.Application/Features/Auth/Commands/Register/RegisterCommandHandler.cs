using MediatR;
using Microsoft.Extensions.Logging;
using PulseDesk.Application.Features.Users.Events;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Domain.Entities;
using PulseDesk.Domain.Exceptions;
using PulseDesk.Shared.DTOs;
using PulseDesk.Shared.DTOs.Auth;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IMediator _mediator;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(IUserRepository userRepo, IRoleRepository roleRepo, ITokenService tokenService, IPasswordService passwordService, IMediator mediator, ILogger<RegisterCommandHandler> logger)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
        {
            var dto = request.request;

            // Checking email uniqueness
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser is not null)
            {
                _logger.LogWarning("Registration attempt with existing email {Email}", dto.Email);
                throw new DomainException("Email is already in use");
            }

            var userRole = await _roleRepo.GetByNameAsync("User");
            if (userRole is null)
                throw new InvalidOperationException("Defined role User is not found in Db");

            // Creating user Entity
            var user = new User
            {
                Email = dto.Email.ToLowerInvariant(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = _passwordService.Hash(dto.Password),
                RoleId = userRole.Id,
                Department = dto.Department,
                JobTitle = dto.JobTitle,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _userRepo.AddAsync(user);

            // Load user with role + policy for token generating
            var createdUser = await _userRepo.GetByIdWithRoleAsync(user.Id);

            // Generating accessToken
            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(createdUser!);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(createdUser!.Id);

            _logger.LogInformation(
            "New user registered: {Email} with role {Role}",
            user.Email, userRole.Name);

            // Publishing Mediator Notification - fire and forget
            // All Handlers react independently, won't wait for them to respond.
            await _mediator.Publish(new UserRegisteredEvent(
                UserId: createdUser.Id,
                FullName: createdUser.FullName,
                Email: createdUser.Email,
                Role: createdUser.Role.Name,
                Department: createdUser.Department,
                RegisteredAt: createdUser.CreatedAt
                ), ct
            );

            return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    FullName = createdUser.FullName,
                    Email = createdUser.Email,
                    Role = createdUser.Role.Name,
                    Policies = createdUser.Role.RolePolicies.Select(rp => rp.Policy.Name).ToList(),
                    Department = createdUser.Department,
                    JobTitle = createdUser.JobTitle,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt,
                }
            }, "User Registration Successful"
            );
        }
    }
}
