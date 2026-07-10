using Swaply.Domain.Entities;
using Swaply.Domain.ValueObjects;

namespace Swaply.Application.ListingManagement;

public record CreateListingRequest(
    string Title,
    string Description,
    Guid OwnerId,
    Guid CategoryId,
    decimal EstimatedAmount,
    string Currency = "VND",
    ItemCondition Condition = ItemCondition.Good,
    string Brand = "",
    string ExchangeWish = "",
    decimal? CashTopUpAmount = null,
    string Location = ""
);

public record UpdateListingRequest(
    string Title,
    string Description,
    Guid CategoryId,
    decimal EstimatedAmount,
    string Currency = "VND",
    ItemCondition Condition = ItemCondition.Good,
    string Brand = "",
    string ExchangeWish = "",
    decimal? CashTopUpAmount = null,
    string Location = ""
);

public record SearchListingRequest(
    string? Keyword = null,
    Guid? CategoryId = null,
    ItemCondition? Condition = null,
    string? Location = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string SortBy = "newest",
    int Page = 1,
    int PageSize = 20
);

public record ListingResponse(
    Guid Id,
    string Title,
    string Description,
    Guid OwnerId,
    string OwnerName,
    string OwnerAvatar,
    Guid CategoryId,
    string CategoryName,
    decimal EstimatedValue,
    string Currency,
    ItemCondition Condition,
    string ConditionName,
    ListingStatus Status,
    string Brand,
    string ExchangeWish,
    decimal? CashTopUpAmount,
    string CashTopUpCurrency,
    string Location,
    int ViewCount,
    int FavoriteCount,
    List<ListingImageResponse> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ExpiresAt
);

public record ListingImageResponse(
    Guid Id,
    string ImageUrl,
    bool IsPrimary,
    int DisplayOrder
);

public record ListingSummaryResponse(
    Guid Id,
    string Title,
    decimal EstimatedValue,
    string Currency,
    ItemCondition Condition,
    string Location,
    int FavoriteCount,
    string PrimaryImageUrl,
    string OwnerName,
    DateTime CreatedAt
);

public record PagedListResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record UpdateStatusRequest(ListingStatus Status);

public record FavoriteResponse(
    Guid ListingId,
    bool IsFavorited,
    int FavoriteCount
);
