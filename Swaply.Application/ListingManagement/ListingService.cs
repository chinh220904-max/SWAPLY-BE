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
    private readonly IReportRepository _reportRepository;

    public ListingService(IListingRepository listingRepository, IListingImageRepository listingImageRepository, IReportRepository reportRepository)
    {
        _listingRepository = listingRepository;
        _listingImageRepository = listingImageRepository;
        _reportRepository = reportRepository;
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

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

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

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

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

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

        listing.Publish();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> RenewListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

        listing.Renew();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

        listing.SubmitForReview();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> ApproveListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

        listing.Approve();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> RejectListingAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is deleted.");

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

    public async Task<PagedListResponse<AdminListingSummaryResponse>> SearchAdminListingsAsync(string? keyword, ListingStatus? status, string? sortBy, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var listings = await _listingRepository.SearchAdminListingsAsync(
            keyword,
            status,
            string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy,
            page,
            pageSize,
            cancellationToken);

        var totalCount = await _listingRepository.GetAdminTotalCountAsync(
            keyword,
            status,
            cancellationToken);

        var items = listings.Select(l => new AdminListingSummaryResponse(
            l.Id,
            l.Title,
            l.EstimatedValue.Amount,
            l.EstimatedValue.Currency,
            l.Condition,
            l.Location,
            l.FavoriteCount,
            l.Images.FirstOrDefault()?.ImageUrl ?? "",
            l.Owner?.FullName ?? "",
            l.Status.ToString(),
            l.CreatedAt,
            l.OwnerId
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedListResponse<AdminListingSummaryResponse>(items, totalCount, page, pageSize, totalPages);
    }

    public async Task<AdminListingDetailResponse?> GetAdminListingDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken);
        if (listing == null)
            return null;

        var reportCount = await _reportRepository.GetCountByTargetAsync(ReportTargetType.Listing, id, cancellationToken);

        return new AdminListingDetailResponse(
            listing.Id,
            listing.Title,
            listing.Description,
            listing.OwnerId,
            listing.Owner?.FullName ?? string.Empty,
            listing.CategoryId,
            listing.Category?.Name ?? string.Empty,
            listing.EstimatedValue.Amount,
            listing.EstimatedValue.Currency,
            listing.Condition,
            listing.Condition.ToString(),
            listing.Status.ToString(),
            listing.Brand,
            listing.ExchangeWish,
            listing.CashTopUp?.Amount,
            listing.CashTopUp?.Currency,
            listing.Location,
            listing.ViewCount,
            listing.FavoriteCount,
            listing.Images.Select(i => i.ImageUrl).ToList(),
            listing.CreatedAt,
            listing.UpdatedAt,
            reportCount
        );
    }

    public async Task<Listing> HideListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.Status == ListingStatus.Hidden)
            throw new InvalidOperationException("Listing is already hidden.");

        listing.Hide();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task<Listing> RestoreListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.Status != ListingStatus.Hidden)
            throw new InvalidOperationException("Only hidden listings can be restored.");

        listing.Publish();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        return listing;
    }

    public async Task SoftDeleteListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (listing.IsDeleted)
            throw new InvalidOperationException("Listing is already deleted.");

        listing.SoftDelete();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task<Listing> PermanentlyDeleteListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdIncludingDeletedAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (!listing.IsDeleted)
            throw new InvalidOperationException("Listing must be soft-deleted before permanent deletion.");

        await _listingRepository.DeleteAsync(id, cancellationToken);
        return listing;
    }

    public async Task RestoreDeletedListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdIncludingDeletedAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        if (!listing.IsDeleted)
            throw new InvalidOperationException("Listing is not deleted.");

        listing.Restore();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetDeletedListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _listingRepository.GetDeletedListingsAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ListingNotFoundException(id);

        listing.IncrementViewCount();
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }
}
