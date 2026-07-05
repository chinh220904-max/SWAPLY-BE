namespace Swaply.Application.ChatManagement;

public class ChatService : IChatService
{
    private readonly INotificationService _notificationService;
    private readonly List<string> _mockMessages = new(); // Temporary mock storage

    public ChatService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task SendMessageAsync(string senderId, string receiverId, string content, CancellationToken cancellationToken = default)
    {
        var message = $"[{DateTime.UtcNow:HH:mm}] {senderId}: {content}";
        _mockMessages.Add(message);

        // Notify receiver in real time via SignalR/Notification service
        await _notificationService.SendNotificationToUserAsync(
            receiverId, 
            $"New message from {senderId}: {content}", 
            cancellationToken
        );
    }

    public Task<IEnumerable<string>> GetConversationHistoryAsync(string userId, string otherUserId, CancellationToken cancellationToken = default)
    {
        // Return simulated conversation history
        return Task.FromResult<IEnumerable<string>>(_mockMessages);
    }
}
