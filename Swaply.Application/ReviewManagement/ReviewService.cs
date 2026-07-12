using Swaply.Application.NotificationManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Application.ReviewManagement;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IExchangeRepository _exchangeRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IExchangeRepository exchangeRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _reviewRepository = reviewRepository;
        _exchangeRepository = exchangeRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid reviewerId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        if (reviewerId == request.RevieweeId)
            throw new ArgumentException("You cannot review yourself.");

        if (request.Rating < 1 || request.Rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.");

        var exchange = await _exchangeRepository.GetByIdAsync(request.ExchangeId, cancellationToken);
        if (exchange == null)
            throw new InvalidOperationException("Exchange not found.");

        if (exchange.Status != ExchangeStatus.Completed)
            throw new InvalidOperationException("You can only review completed exchanges.");

        var isParticipant = exchange.ProposerId == reviewerId || exchange.ReceiverId == reviewerId;
        if (!isParticipant)
            throw new UnauthorizedAccessException("Only exchange participants can leave reviews.");

        var isCorrectReviewee = request.RevieweeId == exchange.ProposerId || request.RevieweeId == exchange.ReceiverId;
        if (!isCorrectReviewee)
            throw new InvalidOperationException("Invalid reviewee. The reviewee must be the other exchange participant.");

        var revieweeIsReviewer = request.RevieweeId == reviewerId;
        if (revieweeIsReviewer)
            throw new ArgumentException("You cannot review yourself.");

        var revieweeExists = await _userRepository.GetByIdAsync(request.RevieweeId);
        if (revieweeExists == null)
            throw new InvalidOperationException("Target user not found.");

        var hasReviewed = await _reviewRepository.HasUserReviewedExchangeAsync(request.ExchangeId, reviewerId, cancellationToken);
        if (hasReviewed)
            throw new InvalidOperationException("You have already reviewed this exchange.");

        var review = new Review(
            exchangeId: request.ExchangeId,
            reviewerId: reviewerId,
            revieweeId: request.RevieweeId,
            rating: request.Rating,
            comment: request.Comment
        );

        var created = await _reviewRepository.CreateAsync(review, cancellationToken);

        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
            UserId: request.RevieweeId,
            Title: "New Review",
            Content: "You received a new review.",
            Type: "NewReview",
            RelatedEntityId: created.Id,
            RelatedEntityType: "Review"
        ), cancellationToken);

        return ToReviewDto(created);
    }

    public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetReviewsForUserAsync(userId, cancellationToken);
        return reviews.Select(ToReviewDto);
    }

    public async Task<UserRatingDto> GetUserRatingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (averageRating, totalReviews) = await _reviewRepository.GetUserRatingAggregateAsync(userId, cancellationToken);

        if (totalReviews == 0)
            return new UserRatingDto(AverageRating: 0, TotalReviews: 0);

        return new UserRatingDto(AverageRating: Math.Round(averageRating, 1), TotalReviews: totalReviews);
    }

    private static ReviewDto ToReviewDto(Review review)
    {
        return new ReviewDto(
            Id: review.Id,
            ExchangeId: review.ExchangeId,
            ReviewerId: review.ReviewerId,
            ReviewerName: review.Reviewer?.UserName ?? "Unknown",
            RevieweeId: review.RevieweeId,
            RevieweeName: review.Reviewee?.UserName ?? "Unknown",
            Rating: review.Rating,
            Comment: review.Comment,
            CreatedAt: review.CreatedAt
        );
    }
}
