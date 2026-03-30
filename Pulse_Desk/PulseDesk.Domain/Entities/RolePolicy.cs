using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Domain.Entities
{
    public class RolePolicy
    {
        public int RoleId { get; set; }
        public int PolicyId { get; set; }

        public Role Role { get; set; } = default;
        public Policy Policy { get; set; } = default;
    }
}
