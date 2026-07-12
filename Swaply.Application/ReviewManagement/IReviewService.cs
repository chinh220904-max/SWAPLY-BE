namespace Swaply.Application.ReviewManagement;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid reviewerId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserRatingDto> GetUserRatingAsync(Guid userId, CancellationToken cancellationToken = default);
}
