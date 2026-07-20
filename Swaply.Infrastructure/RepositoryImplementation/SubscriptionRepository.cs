using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SwaplyDbContext _dbContext;

    public SubscriptionRepository(SwaplyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Package> GetActivePackageAsync(CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.Packages.FirstOrDefaultAsync(p => p.IsActive, cancellationToken);
        if (plan == null)
        {
            throw new InvalidOperationException("No active package found.");
        }

        return plan;
    }

    public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.Subscriptions
            .Include(s => s.Package)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.ExpiresAt > now, cancellationToken);
    }

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _dbContext.Subscriptions.AddAsync(subscription, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _dbContext.Subscriptions.Update(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
