using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<Package> GetActivePackageAsync(CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
