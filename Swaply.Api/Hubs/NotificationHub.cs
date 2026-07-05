using Microsoft.AspNetCore.SignalR;

namespace Swaply.Api.Hubs;

public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        await Clients.Caller.SendAsync("GroupJoined", userId);
    }
}
