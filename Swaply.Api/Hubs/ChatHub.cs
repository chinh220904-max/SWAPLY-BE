using Microsoft.AspNetCore.SignalR;
using Swaply.Application.ChatManagement;

namespace Swaply.Api.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessage(string receiverId, string messageContent)
    {
        var senderId = Context.ConnectionId; // Mocking sender using ConnectionId
        await _chatService.SendMessageAsync(senderId, receiverId, messageContent);
        
        // Push message back to caller and receiver
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, messageContent);
        await Clients.Client(receiverId).SendAsync("ReceiveMessage", senderId, messageContent);
    }
}
