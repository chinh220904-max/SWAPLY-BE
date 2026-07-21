using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class BoostHistoryRepository : IBoostHistoryRepository
{
    private readonly SwaplyDbContext _context;

    public BoostHistoryRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BoostHistory>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.BoostHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.BoostedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BoostHistory>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _context.BoostHistories
            .Where(h => h.ListingId == listingId)
            .OrderByDescending(h => h.BoostedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BoostHistory?> GetLatestByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _context.BoostHistories
            .Where(h => h.ListingId == listingId)
            .OrderByDescending(h => h.BoostedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BoostHistory>> GetActiveBoostsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BoostHistories
            .Where(h => h.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(h => h.Priority)
            .ThenBy(h => h.BoostedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BoostHistory history, CancellationToken cancellationToken = default)
    {
        await _context.BoostHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expiredHistories = await _context.BoostHistories
            .Where(h => h.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.BoostHistories.RemoveRange(expiredHistories);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
