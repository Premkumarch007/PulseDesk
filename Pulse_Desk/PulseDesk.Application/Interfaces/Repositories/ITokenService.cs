using PulseDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Interfaces.Repositories
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateAccessToken(User user);
        Task<string> GenerateRefreshTokenAsync(int userID, string? ipAddress = null);
        Task<(bool isValid, int userId)> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null);
    }
}
