using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IBoostHistoryRepository
{
    Task<IReadOnlyList<BoostHistory>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BoostHistory>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<BoostHistory?> GetLatestByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BoostHistory>> GetActiveBoostsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(BoostHistory history, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
