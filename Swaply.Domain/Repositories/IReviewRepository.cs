using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Review> CreateAsync(Review review, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetReviewsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetReviewsReceivedByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetReviewsGivenByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasUserReviewedExchangeAsync(Guid exchangeId, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<Review?> GetExchangeReviewAsync(Guid exchangeId, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<(double AverageRating, int TotalReviews)> GetUserRatingAggregateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
