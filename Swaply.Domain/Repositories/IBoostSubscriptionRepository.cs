using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IBoostSubscriptionRepository
{
    Task<BoostSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BoostSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BoostSubscription>> GetExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(BoostSubscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(BoostSubscription subscription, CancellationToken cancellationToken = default);
}
