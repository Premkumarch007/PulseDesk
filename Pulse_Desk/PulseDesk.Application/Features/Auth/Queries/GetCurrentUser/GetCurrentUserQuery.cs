using MediatR;
using PulseDesk.Shared.DTOs;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Auth.Queries.GetCurrentUser
{
    public record GetCurrentUserQuery(int UserId) : IRequest<ApiResponse<UserDto>>;
}
