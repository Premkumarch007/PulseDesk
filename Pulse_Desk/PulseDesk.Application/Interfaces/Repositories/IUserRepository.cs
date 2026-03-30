using PulseDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int Id);
        Task<User?> GetByIdWithRoleAsync(int Id);
        Task<User?> GetByEmailAsync(string Email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> IsExistsAsync(string Email);
    }
}
