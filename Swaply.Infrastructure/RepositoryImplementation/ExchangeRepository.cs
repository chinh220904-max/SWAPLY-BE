using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
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

    public async Task<Exchange?> GetActiveByListingPairAsync(Guid listingId1, Guid listingId2, CancellationToken cancellationToken = default)
    {
        // Find exchange with matching listing pair regardless of direction (proposer/receiver)
        // Only return if status is Pending or Accepted
        return await _context.Exchanges
            .FirstOrDefaultAsync(x =>
                ((x.ProposerListingId == listingId1 && x.ReceiverListingId == listingId2) ||
                (x.ProposerListingId == listingId2 && x.ReceiverListingId == listingId1))
                && (x.Status == ExchangeStatus.Pending || x.Status == ExchangeStatus.Accepted),
                cancellationToken);
    }

    public async Task<IEnumerable<Exchange>> GetCompetingPendingExchangesAsync(
        Guid exchangeId, Guid listing1Id, Guid listing2Id, CancellationToken cancellationToken = default)
    {
        // Find all pending exchanges that:
        // 1. Are not the current exchange
        // 2. Share at least one listing with the current exchange
        // 3. Are in Pending status
        return await _context.Exchanges
            .Where(x => x.Id != exchangeId
                && x.Status == ExchangeStatus.Pending
                && (x.ProposerListingId == listing1Id
                    || x.ReceiverListingId == listing1Id
                    || x.ProposerListingId == listing2Id
                    || x.ReceiverListingId == listing2Id))
            .ToListAsync(cancellationToken);
    }
}
