using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Shared.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
    }
}
