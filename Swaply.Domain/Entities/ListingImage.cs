namespace Swaply.Domain.Entities;

public class ListingImage
{
    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string CloudinaryPublicId { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public Listing? Listing { get; private set; }

    // EF Core constructor
    private ListingImage() { }

    public ListingImage(Guid listingId, string imageUrl, string cloudinaryPublicId, int displayOrder)
    {
        Id = Guid.NewGuid();
        ListingId = listingId;
        ImageUrl = imageUrl;
        CloudinaryPublicId = cloudinaryPublicId;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }
}
