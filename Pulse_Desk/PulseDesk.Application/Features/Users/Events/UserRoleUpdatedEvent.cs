using MediatR;
using PulseDesk.Application.Interfaces.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Events
{
    public record UserRoleUpdatedEvent(int UserId, string roleName):INotification;
    public class UserRoleUpdatedEventHandler : INotificationHandler<UserRoleUpdatedEvent>
    {
        private readonly IDashboardHubContext _context;
        public UserRoleUpdatedEventHandler(IDashboardHubContext context)
        {
            _context = context;
        }

        async Task INotificationHandler<UserRoleUpdatedEvent>.Handle(UserRoleUpdatedEvent notification, CancellationToken cancellationToken)
            => await _context.NotifyUserRoleUpdated(notification.UserId, notification.roleName);
    }
}
