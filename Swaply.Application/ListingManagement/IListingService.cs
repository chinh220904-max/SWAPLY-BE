using Swaply.Domain.Entities;

namespace Swaply.Application.ListingManagement;

public interface IListingService
{
    Task<Listing> CreateListingAsync(CreateListingRequest request, CancellationToken cancellationToken = default);
    Task<Listing?> GetListingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
}
