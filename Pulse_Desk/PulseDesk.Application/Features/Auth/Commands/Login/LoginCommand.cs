using MediatR;
using PulseDesk.Shared.DTOs.Auth;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginRequestDto Request) : IRequest<ApiResponse<AuthResponseDto>>;
}
