using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Events
{
    public record UserRegisteredEvent(int UserId, string FullName, string Email, string Role, string? Department, DateTime RegisteredAt):INotification;
}
