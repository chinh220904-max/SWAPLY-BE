using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
