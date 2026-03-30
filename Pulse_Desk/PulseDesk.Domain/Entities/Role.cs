using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = default;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<RolePolicy> RolePolicies { get; set; } = new List<RolePolicy>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
