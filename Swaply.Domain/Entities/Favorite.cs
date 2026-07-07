namespace Swaply.Domain.Entities;

public class Favorite
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ListingId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Listing? Listing { get; private set; }

    // EF Core constructor
    private Favorite() { }

    public Favorite(Guid userId, Guid listingId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ListingId = listingId;
        CreatedAt = DateTime.UtcNow;
    }
}
