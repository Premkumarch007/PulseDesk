using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Shared.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
