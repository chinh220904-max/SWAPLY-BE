using Swaply.Domain.Entities;

namespace Swaply.Application.ConversationManagement;

public interface IConversationService
{
    Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationDto> CreateConversationAsync(Guid currentUserId, CreateConversationRequest request, CancellationToken cancellationToken = default);
    Task<ConversationDto?> GetOrCreateConversationAsync(Guid currentUserId, CreateConversationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<MessageDto> SendMessageAsync(Guid conversationId, Guid senderId, CreateMessageRequest request, CancellationToken cancellationToken = default);
}
