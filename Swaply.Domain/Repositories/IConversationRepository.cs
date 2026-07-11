using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByUsersAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByUsersAndListingAsync(Guid user1Id, Guid user2Id, Guid listingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
