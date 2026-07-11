using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Domain.ValueObjects;
using Swaply.Domain.Exceptions;

namespace Swaply.Application.ListingManagement;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;
    private readonly IListingImageRepository _listingImageRepository;

    public ListingService(IListingRepository listingRepository, IListingImageRepository listingImageRepository)
    {
        _listingRepository = listingRepository;
        _listingImageRepository = listingImageRepository;
    }

    public async Task<Listing> CreateListingAsync(CreateListingRequest request, CancellationToken cancellationToken = default)
    {
        var price = new Money(request.EstimatedAmount, request.Currency);
        Money? cashTopUp = request.CashTopUpAmount.HasValue
            ? new Money(request.CashTopUpAmount.Value, request.Currency)
            : null;

        var listing = new Listing(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.OwnerId,
            request.CategoryId,
            price,
            request.Condition,
            request.Brand,
            request.ExchangeWish,
            cashTopUp,
            request.Location
        );

        await _listingRepository.AddAsync(listing, cancellationToken);

        // Add images if provided
        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            var images = request.ImageUrls.Select((url, index) =>
                new ListingImage(listing.Id, url, "", index)).ToList();
            await _listingImageRepository.AddRangeAsync(images, cancellationToken);
        }

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

    public async Task<Listing> UpdateListingAsync(Guid id, UpdateListingRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        var price = new Money(request.EstimatedAmount, request.Currency);
        Money? cashTopUp = request.CashTopUpAmount.HasValue
            ? new Money(request.CashTopUpAmount.Value, request.Currency)
            : null;

        listing.Update(
            request.Title,
            request.Description,
            price,
            request.Condition,
            request.Brand,
            request.ExchangeWish,
            cashTopUp,
            request.Location
        );

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task DeleteListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _listingRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<Listing> UpdateStatusAsync(Guid id, ListingStatus status, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        switch (status)
        {
            case ListingStatus.Active:
                listing.Publish();
                break;
            case ListingStatus.Inactive:
                listing.Deactivate();
                break;
            case ListingStatus.Hidden:
                listing.Hide();
                break;
            case ListingStatus.Expired:
                listing.Renew();
                break;
            default:
                throw new InvalidOperationException($"Cannot set status to {status}");
        }

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> PublishListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.Publish();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> RenewListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.Renew();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.SubmitForReview();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> ApproveListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.Approve();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> RejectListingAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.Reject(reason);
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<IEnumerable<Listing>> GetPendingListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _listingRepository.GetByStatusListAsync(ListingStatus.Pending, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetMyListingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _listingRepository.GetByOwnerIdAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetMyListingsByStatusAsync(Guid userId, ListingStatus status, CancellationToken cancellationToken = default)
    {
        return await _listingRepository.GetByStatusAsync(userId, status, cancellationToken);
    }

    public async Task<PagedListResponse<ListingSummaryResponse>> SearchListingsAsync(SearchListingRequest request, CancellationToken cancellationToken = default)
    {
        var listings = await _listingRepository.SearchListingsAsync(
            request.Keyword,
            request.CategoryId,
            request.Condition,
            request.Location,
            request.MinPrice,
            request.MaxPrice,
            ListingStatus.Active,
            request.SortBy,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _listingRepository.GetTotalCountAsync(
            request.Keyword,
            request.CategoryId,
            request.Condition,
            request.Location,
            request.MinPrice,
            request.MaxPrice,
            ListingStatus.Active,
            cancellationToken);

        var items = listings.Select(l => new ListingSummaryResponse(
            l.Id,
            l.Title,
            l.EstimatedValue.Amount,
            l.EstimatedValue.Currency,
            l.Condition,
            l.Location,
            l.FavoriteCount,
            l.Images.FirstOrDefault()?.ImageUrl ?? "",
            l.Owner?.FullName ?? "",
            l.CreatedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedListResponse<ListingSummaryResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }

    public async Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.IncrementViewCount();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }
}
