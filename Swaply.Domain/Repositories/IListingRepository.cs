using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
}
