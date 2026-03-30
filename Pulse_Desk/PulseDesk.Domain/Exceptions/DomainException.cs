using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Domain.Exceptions
{
    public class DomainException: Exception
    {
        public DomainException(string message): base(message) { }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key):base($"{entityName} with key '{key}' was not found.") { }
        public NotFoundException(string message): base(message)
        { }
    }
    public class  ForbiddenException: Exception
    {
        public ForbiddenException(string message = "You do not have permission to perform this operation"):base(message)
        {
            
        }
    }
}
