using MediatR;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Commands.UpdateUserRole
{
    public record UpdateUserRoleCommand(int UserID, string RoleName): IRequest<ApiResponse<bool>>;
}
