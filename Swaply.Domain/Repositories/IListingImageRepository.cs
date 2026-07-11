using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IListingImageRepository
{
    Task<ListingImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ListingImage>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task AddAsync(ListingImage image, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ListingImage> images, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
}
