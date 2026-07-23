using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ConversationRepository : IConversationRepository
{
    private readonly SwaplyDbContext _context;

    public ConversationRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.RelatedListing)
            .Include(c => c.RelatedExchange)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Conversation?> GetByUsersAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        var normalizedUser1 = user1Id.CompareTo(user2Id) < 0 ? user1Id : user2Id;
        var normalizedUser2 = user1Id.CompareTo(user2Id) < 0 ? user2Id : user1Id;

        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.RelatedListing)
            .Include(c => c.RelatedExchange)
            .FirstOrDefaultAsync(c => c.User1Id == normalizedUser1 && c.User2Id == normalizedUser2, cancellationToken);
    }

    public async Task<Conversation?> GetByUsersAndListingAsync(Guid user1Id, Guid user2Id, Guid listingId, CancellationToken cancellationToken = default)
    {
        var normalizedUser1 = user1Id.CompareTo(user2Id) < 0 ? user1Id : user2Id;
        var normalizedUser2 = user1Id.CompareTo(user2Id) < 0 ? user2Id : user1Id;

        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.RelatedListing)
            .Include(c => c.RelatedExchange)
            .FirstOrDefaultAsync(c =>
                c.User1Id == normalizedUser1 &&
                c.User2Id == normalizedUser2 &&
                c.RelatedListingId == listingId, cancellationToken);
    }

    public async Task<Conversation?> GetByExchangeIdAsync(Guid exchangeId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.RelatedListing)
            .Include(c => c.RelatedExchange)
            .FirstOrDefaultAsync(c => c.RelatedExchangeId == exchangeId, cancellationToken);
    }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.RelatedListing)
            .Include(c => c.RelatedExchange)
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
