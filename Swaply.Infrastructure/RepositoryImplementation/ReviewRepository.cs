using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ReviewRepository : IReviewRepository
{
    private readonly SwaplyDbContext _context;

    public ReviewRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Include(r => r.Exchange)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Review> CreateAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _context.Reviews.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<IEnumerable<Review>> GetReviewsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Include(r => r.Exchange)
            .Where(r => r.RevieweeId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserReviewedExchangeAsync(Guid exchangeId, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ExchangeId == exchangeId && r.ReviewerId == reviewerId, cancellationToken);
    }

    public async Task<Review?> GetExchangeReviewAsync(Guid exchangeId, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .FirstOrDefaultAsync(r => r.ExchangeId == exchangeId && r.ReviewerId == reviewerId, cancellationToken);
    }

    public async Task<(double AverageRating, int TotalReviews)> GetUserRatingAggregateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var reviews = _context.Reviews.Where(r => r.RevieweeId == userId);

        var totalReviews = await reviews.CountAsync(cancellationToken);
        if (totalReviews == 0)
            return (AverageRating: 0, TotalReviews: 0);

        var averageRating = await reviews.AverageAsync(r => r.Rating, cancellationToken);
        return (AverageRating: averageRating, TotalReviews: totalReviews);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
