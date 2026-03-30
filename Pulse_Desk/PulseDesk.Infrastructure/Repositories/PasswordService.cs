using PulseDesk.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Infrastructure.Repositories
{
    public class PasswordService : IPasswordService
    {
        private const int workFactor = 12;
        string IPasswordService.Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password,workFactor);

        bool IPasswordService.VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password,hash);
    }
}
