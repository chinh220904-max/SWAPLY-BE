using Swaply.Domain.Entities;

namespace Swaply.Application.ListingManagement;

public record CreateListingRequest(
    string Title,
    string Description,
    Guid OwnerId,
    Guid CategoryId,
    decimal EstimatedAmount,
    string Currency = "VND",
    ItemCondition Condition = ItemCondition.Good
);
