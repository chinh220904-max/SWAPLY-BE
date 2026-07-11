using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.ValueObjects;

namespace Swaply.Domain.Repositories;

public interface IListingRepository
{
    // Basic CRUD
    Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Queries
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByStatusAsync(Guid ownerId, ListingStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByStatusListAsync(ListingStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> SearchListingsAsync(
        string? keyword,
        Guid? categoryId,
        ItemCondition? condition,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        ListingStatus? status,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(
        string? keyword,
        Guid? categoryId,
        ItemCondition? condition,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        ListingStatus? status,
        CancellationToken cancellationToken = default);
}
