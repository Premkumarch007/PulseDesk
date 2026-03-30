using MediatR;
using PulseDesk.Application.Interfaces.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Events
{
    public record UserDeactivatedEvent(int UserId):INotification;
    public class UserDeactivatedEventHandler : INotificationHandler<UserDeactivatedEvent>
    {
        private readonly IDashboardHubContext dashboardHubContext;
        public UserDeactivatedEventHandler(IDashboardHubContext dashboardHubContext)
        {
            this.dashboardHubContext = dashboardHubContext;
        }
        async Task INotificationHandler<UserDeactivatedEvent>.Handle(UserDeactivatedEvent notification, CancellationToken cancellationToken)
            => await dashboardHubContext.NotifyUserDeactivated(notification.UserId);
    }
}
