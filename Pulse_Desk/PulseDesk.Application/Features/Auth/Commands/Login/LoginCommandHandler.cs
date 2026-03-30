using MediatR;
using Microsoft.Extensions.Logging;
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

namespace PulseDesk.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler: IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService, IPasswordService passwordService, ILogger<LoginCommandHandler> logger)
        {
            _logger = logger;
            _userRepo = userRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        async Task<ApiResponse<AuthResponseDto>> IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>.Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Request;
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            // Checking Password and user existance at once. Not revealing what fails

            if (user is null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed Login attempt for email {email}", dto.Email);
                return ApiResponse<AuthResponseDto>.Fail("Invalid or Email or Password");
            }
            if (!user.IsActive)
                throw new DomainException("User has been de activated");


            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.Name,
                    Policies = user.Role.RolePolicies
                                      .Select(rp => rp.Policy.Name)
                                      .ToList(),
                    Department = user.Department,
                    JobTitle = user.JobTitle,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                }
            });
        }
    }
}
