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
    public class RoleRepository : IRoleRepository
    {
        private readonly PulseDeskDbContext _context;
        public RoleRepository(PulseDeskDbContext context)
        {
            _context = context;
        }
        async Task<List<Role>> IRoleRepository.GetAllAsync()
          => await _context.Roles.Include(r => r.RolePolicies).ThenInclude(rp => rp.Policy).ToListAsync();

        async Task<Role?> IRoleRepository.GetByIdWithPoliciesAsync(int id)
        => await _context.Roles.Include(r => r.RolePolicies).ThenInclude(rp => rp.Policy).FirstOrDefaultAsync(x => x.Id == id);

        async Task<Role?> IRoleRepository.GetByNameAsync(string name)
        => await _context.Roles.Include(r => r.RolePolicies).ThenInclude(rp => rp.Policy).FirstOrDefaultAsync(x => x.Name == name);
    }
}
