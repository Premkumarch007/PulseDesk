using MediatR;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Commands.DeActivateUser
{
    public record DeactivateUserCommand(int UserId, int RequestingUserId):IRequest<ApiResponse<bool>>;
}
