using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class MessageRepository : IMessageRepository
{
    private readonly SwaplyDbContext _context;

    public MessageRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
        // SaveChanges is intentionally NOT called here.
        // Caller is responsible for calling SaveChanges once to commit all changes.
        return message;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
