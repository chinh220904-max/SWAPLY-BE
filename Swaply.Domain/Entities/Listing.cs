using Swaply.Domain.ValueObjects;

namespace Swaply.Domain.Entities;

public class Listing
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Money EstimatedValue { get; private set; } = Money.Zero();
    public ItemCondition Condition { get; private set; } = ItemCondition.Good;
    public ListingStatus Status { get; private set; } = ListingStatus.Draft;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public User? Owner { get; private set; }
    public Category? Category { get; private set; }

    private readonly List<ListingImage> _images = new();
    public IReadOnlyCollection<ListingImage> Images => _images.AsReadOnly();

    private readonly List<Exchange> _proposedExchanges = new();
    public IReadOnlyCollection<Exchange> ProposedExchanges => _proposedExchanges.AsReadOnly();

    private readonly List<Exchange> _receivedExchanges = new();
    public IReadOnlyCollection<Exchange> ReceivedExchanges => _receivedExchanges.AsReadOnly();

    private readonly List<MatchingHistory> _sourceMatches = new();
    public IReadOnlyCollection<MatchingHistory> SourceMatches => _sourceMatches.AsReadOnly();

    private readonly List<MatchingHistory> _matchedAsSource = new();
    public IReadOnlyCollection<MatchingHistory> MatchedAsSource => _matchedAsSource.AsReadOnly();

    private readonly List<Favorite> _favorites = new();
    public IReadOnlyCollection<Favorite> Favorites => _favorites.AsReadOnly();

    // EF Core constructor
    private Listing() { }

    public Listing(Guid id, string title, string description, Guid ownerId, Guid categoryId, Money estimatedValue, ItemCondition condition)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId cannot be empty.", nameof(ownerId));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = title;
        Description = description;
        OwnerId = ownerId;
        CategoryId = categoryId;
        EstimatedValue = estimatedValue;
        Condition = condition;
        Status = ListingStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsExchanged()
    {
        Status = ListingStatus.Exchanged;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = ListingStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, Money estimatedValue, ItemCondition condition)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title;
        Description = description;
        EstimatedValue = estimatedValue;
        Condition = condition;
        UpdatedAt = DateTime.UtcNow;
    }
}
