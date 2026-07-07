using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Domain.ValueObjects;

namespace Swaply.Application.ListingManagement;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;

    public ListingService(IListingRepository listingRepository)
    {
        _listingRepository = listingRepository;
    }

    public async Task<Listing> CreateListingAsync(CreateListingRequest request, CancellationToken cancellationToken = default)
    {
        var price = new Money(request.EstimatedAmount, request.Currency);
        var listing = new Listing(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.OwnerId,
            request.CategoryId,
            price,
            request.Condition
        );

        await _listingRepository.AddAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing?> GetListingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _listingRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _listingRepository.GetActiveListingsAsync(cancellationToken);
    }
}
