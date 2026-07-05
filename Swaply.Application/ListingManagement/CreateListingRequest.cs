namespace Swaply.Application.ListingManagement;

public record CreateListingRequest(
    string Title,
    string Description,
    string OwnerId,
    decimal EstimatedAmount,
    string Currency = "VND"
);
