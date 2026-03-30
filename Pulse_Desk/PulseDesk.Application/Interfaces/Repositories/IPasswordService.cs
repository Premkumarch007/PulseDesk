using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Interfaces.Repositories
{
    public interface IPasswordService
    {
        string Hash(string password);
        bool VerifyPassword(string password, string hash);
    }
}
