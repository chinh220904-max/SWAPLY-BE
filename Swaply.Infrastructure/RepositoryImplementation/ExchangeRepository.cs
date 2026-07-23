using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ExchangeRepository : IExchangeRepository
{
    private readonly SwaplyDbContext _context;

    public ExchangeRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<Exchange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Exchanges
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Exchange exchange, CancellationToken cancellationToken = default)
    {
        await _context.Exchanges.AddAsync(exchange, cancellationToken);
    }

    public Task UpdateAsync(Exchange exchange, CancellationToken cancellationToken = default)
    {
        _context.Exchanges.Update(exchange);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Exchange>> GetMyExchangesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Exchanges
            .Where(x => x.ProposerId == userId || x.ReceiverId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Exchange>> GetOutgoingExchangesAsync(Guid proposerId, CancellationToken cancellationToken = default)
    {
        return await _context.Exchanges
            .Where(x => x.ProposerId == proposerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Exchange>> GetIncomingExchangesAsync(Guid receiverId, CancellationToken cancellationToken = default)
    {
        return await _context.Exchanges
            .Where(x => x.ReceiverId == receiverId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
