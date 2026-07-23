using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.ValueObjects;

namespace Swaply.Application.ListingManagement;

public interface IListingService
{
    // CRUD
    Task<Listing> CreateListingAsync(CreateListingRequest request, CancellationToken cancellationToken = default);
    Task<Listing?> GetListingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<Listing> UpdateListingAsync(Guid id, UpdateListingRequest request, CancellationToken cancellationToken = default);
    Task SoftDeleteListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> PermanentlyDeleteListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteListingAsync(Guid id, CancellationToken cancellationToken = default);

    // Status
    Task<Listing> UpdateStatusAsync(Guid id, ListingStatus status, CancellationToken cancellationToken = default);
    Task<Listing> PublishListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> RenewListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> ApproveListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> RejectListingAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default);

    // Admin
    Task<IEnumerable<Listing>> GetPendingListingsAsync(CancellationToken cancellationToken = default);
    Task<PagedListResponse<AdminListingSummaryResponse>> SearchAdminListingsAsync(string? keyword, ListingStatus? status, string? sortBy, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminListingDetailResponse?> GetAdminListingDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> HideListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Listing> RestoreListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task RestoreDeletedListingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetDeletedListingsAsync(CancellationToken cancellationToken = default);

    // User's listings
    Task<IEnumerable<Listing>> GetMyListingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetMyListingsByStatusAsync(Guid userId, ListingStatus status, CancellationToken cancellationToken = default);

    // Search & Filter
    Task<PagedListResponse<ListingSummaryResponse>> SearchListingsAsync(SearchListingRequest request, CancellationToken cancellationToken = default);

    // View count
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
}

public record AdminListingSummaryResponse(
    Guid Id,
    string Title,
    decimal EstimatedValue,
    string Currency,
    ItemCondition Condition,
    string Location,
    int FavoriteCount,
    string PrimaryImageUrl,
    string OwnerName,
    string Status,
    DateTime CreatedAt,
    Guid OwnerId
);

public record AdminListingDetailResponse(
    Guid Id,
    string Title,
    string Description,
    Guid OwnerId,
    string OwnerName,
    Guid CategoryId,
    string CategoryName,
    decimal EstimatedValue,
    string Currency,
    ItemCondition Condition,
    string ConditionName,
    string Status,
    string Brand,
    string ExchangeWish,
    decimal? CashTopUpAmount,
    string? CashTopUpCurrency,
    string Location,
    int ViewCount,
    int FavoriteCount,
    List<string> ImageUrls,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int ReportCount
);
