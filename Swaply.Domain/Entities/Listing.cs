using Swaply.Domain.Enums;
using Swaply.Domain.ValueObjects;

namespace Swaply.Domain.Entities;

public class Listing
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public Money EstimatedValue { get; private set; } = Money.Zero();
    public ListingStatus Status { get; private set; } = ListingStatus.Draft;
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private Listing() { }

    public Listing(Guid id, string title, string description, string ownerId, Money estimatedValue)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = title;
        Description = description;
        OwnerId = ownerId;
        EstimatedValue = estimatedValue;
        Status = ListingStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsExchanged()
    {
        Status = ListingStatus.Exchanged;
    }

    public void Deactivate()
    {
        Status = ListingStatus.Inactive;
    }
}
