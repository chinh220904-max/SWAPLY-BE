using Microsoft.AspNetCore.SignalR;
using Swaply.Api.Hubs;
using Swaply.Application.NotificationManagement;

namespace Swaply.Api.Services;

public class SignalRNotificationService : 
    global::Swaply.Application.ChatManagement.INotificationService,
    Swaply.Application.NotificationManagement.IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    Task global::Swaply.Application.ChatManagement.INotificationService.SendNotificationToUserAsync(string userId, string message, CancellationToken cancellationToken)
    {
        return _hubContext.Clients
            .Group(userId)
            .SendAsync("ReceiveNotification", new { message }, cancellationToken);
    }

    async Task Swaply.Application.NotificationManagement.IRealTimeNotificationService.SendNotificationToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients
            .Group(userId)
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}
