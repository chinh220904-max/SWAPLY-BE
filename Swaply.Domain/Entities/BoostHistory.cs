namespace Swaply.Domain.Entities;

public class BoostHistory
{
    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BoostSubscriptionId { get; private set; }
    public DateTime BoostedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int Priority { get; private set; }

    public Listing? Listing { get; private set; }
    public User? User { get; private set; }
    public BoostSubscription? BoostSubscription { get; private set; }

    private BoostHistory() { }

    public BoostHistory(Guid listingId, Guid userId, Guid boostSubscriptionId, int priority, DateTime expiresAt)
    {
        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId is required.", nameof(listingId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (boostSubscriptionId == Guid.Empty)
            throw new ArgumentException("BoostSubscriptionId is required.", nameof(boostSubscriptionId));

        Id = Guid.NewGuid();
        ListingId = listingId;
        UserId = userId;
        BoostSubscriptionId = boostSubscriptionId;
        BoostedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        Priority = priority;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }
}
