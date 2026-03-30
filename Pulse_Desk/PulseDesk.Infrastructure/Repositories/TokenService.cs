using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Domain.Entities;
using PulseDesk.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Infrastructure.Repositories
{
    public class TokenService : ITokenService
    {
        private readonly PulseDeskDbContext _context;
        private readonly IConfiguration _configuration;
        public TokenService(PulseDeskDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        (string token, DateTime expiresAt) ITokenService.GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var expiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "15");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new(ClaimTypes.Name , user.FullName),
                new("role", user.Role.Name),
                new("userId", user.Id.ToString())
            };

            foreach (var rolePolicy in user.Role.RolePolicies)
            {
                claims.Add(new Claim("policy", rolePolicy.Policy.Name));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            return (new JwtSecurityTokenHandler().WriteToken(token),expiresAt);
        }

        async Task<string> ITokenService.GenerateRefreshTokenAsync(int userID, string? ipAddress)
        {
            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

            var refreshToken = new RefreshToken
            {
                UserId = userID,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                CreatedByIp = ipAddress,
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return token;
        }

        async Task ITokenService.RevokeRefreshTokenAsync(string token, string? replacedByToken)
        {
           var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken is null) return;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReplacedBy = replacedByToken;
        }

        async Task<(bool isValid, int userId)> ITokenService.ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
            if (refreshToken is null || !refreshToken.IsActive)
                return (false, 0);
            return (true, refreshToken.UserId);
        }
    }
}
