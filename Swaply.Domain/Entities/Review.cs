namespace Swaply.Domain.Entities;

public class Review
{
    public Guid Id { get; private set; }
    public Guid ExchangeId { get; private set; }
    public Guid ReviewerId { get; private set; }
    public Guid RevieweeId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Exchange? Exchange { get; private set; }
    public User? Reviewer { get; private set; }
    public User? Reviewee { get; private set; }

    // EF Core constructor
    private Review() { }

    public Review(Guid exchangeId, Guid reviewerId, Guid revieweeId, int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
        if (reviewerId == revieweeId)
            throw new ArgumentException("A user cannot review themselves.");

        Id = Guid.NewGuid();
        ExchangeId = exchangeId;
        ReviewerId = reviewerId;
        RevieweeId = revieweeId;
        Rating = rating;
        Comment = comment?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
        Rating = rating;
        Comment = comment?.Trim();
    }

    // Fluent API unique constraint: (ExchangeId, ReviewerId)
    // This ensures each participant can only review an exchange once.
}
