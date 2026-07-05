namespace Swaply.Application.ChatManagement;

public interface IChatService
{
    Task SendMessageAsync(string senderId, string receiverId, string content, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetConversationHistoryAsync(string userId, string otherUserId, CancellationToken cancellationToken = default);
}
