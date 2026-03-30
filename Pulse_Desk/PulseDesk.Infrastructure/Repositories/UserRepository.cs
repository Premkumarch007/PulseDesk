using Microsoft.EntityFrameworkCore;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Domain.Entities;
using PulseDesk.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PulseDeskDbContext _context;
        public UserRepository(PulseDeskDbContext context)
        {
            _context = context;
        }
        async Task<User> IUserRepository.AddAsync(User user)
        {
             _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        async Task<User?> IUserRepository.GetByEmailAsync(string Email)
        => await _context.Users.Include(u => u.Role).ThenInclude(r => r.RolePolicies).ThenInclude(rp => rp.Policy).FirstOrDefaultAsync(x => x.Email == Email.ToLowerInvariant());

        async Task<User?> IUserRepository.GetByIdAsync(int Id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);

        async Task<User?> IUserRepository.GetByIdWithRoleAsync(int Id)
        => await _context.Users.Include(u => u.Role).ThenInclude(r => r.RolePolicies).ThenInclude(rp => rp.Policy).FirstOrDefaultAsync(u => u.Id == Id);

        async Task<bool> IUserRepository.IsExistsAsync(string Email)
        => await _context.Users.AnyAsync(u => u.Email == Email.ToLowerInvariant());

        async Task IUserRepository.UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
