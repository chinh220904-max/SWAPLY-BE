namespace Swaply.Application.ReviewManagement;

public record CreateReviewRequest(
    Guid ExchangeId,
    Guid RevieweeId,
    int Rating,
    string? Comment = null
);

public record ReviewDto(
    Guid Id,
    Guid ExchangeId,
    Guid ReviewerId,
    string ReviewerName,
    Guid RevieweeId,
    string RevieweeName,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);

public record UserRatingDto(
    double AverageRating,
    int TotalReviews
);
