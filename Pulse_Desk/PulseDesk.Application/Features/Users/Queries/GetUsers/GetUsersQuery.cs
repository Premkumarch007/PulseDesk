using MediatR;
using PulseDesk.Shared.DTOs.Users;
using PulseDesk.Shared.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Queries.GetUsers
{
    public record GetUsersQuery(UserListRequestDto Request): IRequest<ApiResponse<PagedResponse<UserListItemDto>>>;
}
