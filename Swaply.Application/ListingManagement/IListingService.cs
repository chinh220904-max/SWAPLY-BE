using Swaply.Domain.Entities;

namespace Swaply.Application.ListingManagement;

public interface IListingService
{
    // CRUD
    Task<Listing> CreateListingAsync(CreateListingRequest request, CancellationToken cancellationToken = default);
    Task<Listing?> GetListingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<Listing> UpdateListingAsync(Guid id, UpdateListingRequest request, CancellationToken cancellationToken = default);
    Task DeleteListingAsync(Guid id, CancellationToken cancellationToken = default);

    // Status
    Task<Listing> UpdateStatusAsync(Guid id, ListingStatus status, CancellationToken cancellationToken = default);
    Task<Listing> PublishListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> RenewListingAsync(Guid id, CancellationToken cancellationToken = default);

    // User's listings
    Task<IEnumerable<Listing>> GetMyListingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetMyListingsByStatusAsync(Guid userId, ListingStatus status, CancellationToken cancellationToken = default);

    // Search & Filter
    Task<PagedListResponse<ListingSummaryResponse>> SearchListingsAsync(SearchListingRequest request, CancellationToken cancellationToken = default);

    // View count
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
}
