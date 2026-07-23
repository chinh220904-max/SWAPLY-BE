using Swaply.Domain.Enums;
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

    // Additional fields for Swaply
    public string Brand { get; private set; } = string.Empty;
    public string ExchangeWish { get; private set; } = string.Empty;
    public Money? CashTopUp { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public int ViewCount { get; private set; } = 0;
    public int FavoriteCount { get; private set; } = 0;
    public DateTime? ExpiresAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public bool IsDeleted { get; private set; } = false;

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

    // Boost fields
    public Guid? BoostSubscriptionId { get; private set; }
    public DateTime? BoostedAt { get; private set; }
    public DateTime? BoostExpiresAt { get; private set; }
    public int? BoostPriority { get; private set; }

    private readonly List<Favorite> _favorites = new();
    public IReadOnlyCollection<Favorite> Favorites => _favorites.AsReadOnly();

    // EF Core constructor
    private Listing() { }

    public Listing(
        Guid id,
        string title,
        string description,
        Guid ownerId,
        Guid categoryId,
        Money estimatedValue,
        ItemCondition condition,
        string brand = "",
        string exchangeWish = "",
        Money? cashTopUp = null,
        string location = "")
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
        Brand = brand;
        ExchangeWish = exchangeWish;
        CashTopUp = cashTopUp;
        Location = location;
        Status = ListingStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.AddDays(30);
    }

    public void Publish()
    {
        Status = ListingStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(30);
    }

    public void SubmitForReview()
    {
        if (Status != ListingStatus.Draft)
            throw new InvalidOperationException("Only draft listings can be submitted for review.");
        Status = ListingStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != ListingStatus.Pending)
            throw new InvalidOperationException("Only pending listings can be approved.");
        Status = ListingStatus.Active;
        ExpiresAt = DateTime.UtcNow.AddDays(30);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? reason = null)
    {
        if (Status != ListingStatus.Pending)
            throw new InvalidOperationException("Only pending listings can be rejected.");
        Status = ListingStatus.Draft;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
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

    public void Hide()
    {
        Status = ListingStatus.Hidden;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (IsDeleted)
        {
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Renew()
    {
        if (Status != ListingStatus.Expired)
            throw new InvalidOperationException("Only expired listings can be renewed.");
        Status = ListingStatus.Active;
        ExpiresAt = DateTime.UtcNow.AddDays(30);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, Money estimatedValue, ItemCondition condition,
        string brand, string exchangeWish, Money? cashTopUp, string location)
    {
        if (Status != ListingStatus.Draft)
            throw new InvalidOperationException("Only draft listings can be updated.");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title;
        Description = description;
        EstimatedValue = estimatedValue;
        Condition = condition;
        Brand = brand;
        ExchangeWish = exchangeWish;
        CashTopUp = cashTopUp;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void AddImage(ListingImage image)
    {
        _images.Add(image);
    }

    public void RemoveImage(ListingImage image)
    {
        _images.Remove(image);
    }

    public void ClearImages()
    {
        _images.Clear();
    }

    public void UpdateFavoriteCount()
    {
        FavoriteCount = _favorites.Count;
    }

    public void SetBoost(Guid boostSubscriptionId, DateTime boostedAt, DateTime expiresAt, int priority)
    {
        BoostSubscriptionId = boostSubscriptionId;
        BoostedAt = boostedAt;
        BoostExpiresAt = expiresAt;
        BoostPriority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearBoost()
    {
        BoostSubscriptionId = null;
        BoostedAt = null;
        BoostExpiresAt = null;
        BoostPriority = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
