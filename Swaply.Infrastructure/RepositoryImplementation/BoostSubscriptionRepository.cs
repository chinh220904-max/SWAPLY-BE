using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class BoostSubscriptionRepository : IBoostSubscriptionRepository
{
    private readonly SwaplyDbContext _context;

    public BoostSubscriptionRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<BoostSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.BoostSubscriptions
            .Where(s => s.UserId == userId && s.Status == BoostSubscriptionStatus.Active && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BoostSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BoostSubscriptions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<BoostSubscription>> GetExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BoostSubscriptions
            .Where(s => s.Status == BoostSubscriptionStatus.Active && s.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BoostSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.BoostSubscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BoostSubscription subscription, CancellationToken cancellationToken = default)
    {
        _context.BoostSubscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
