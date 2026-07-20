using Swaply.Domain.Enums;

namespace Swaply.Domain.Entities;

public class Subscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PackageId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Package? Package { get; private set; }

    // 1:1 relationship — each subscription has one payment
    public Swaply.Domain.Entities.Payment? Payment { get; private set; }

    // EF Core constructor
    private Subscription() { }

    public Subscription(Guid userId, Guid packageId, int durationDays)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PackageId = packageId;
        StartedAt = DateTime.UtcNow;
        ExpiresAt = StartedAt.AddDays(durationDays);
        Status = SubscriptionStatus.Active;
    }

    public bool IsActive()
    {
        return Status == SubscriptionStatus.Active && DateTime.UtcNow < ExpiresAt;
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
    }

    public void Expire()
    {
        Status = SubscriptionStatus.Expired;
    }

    public void AttachPayment(Swaply.Domain.Entities.Payment payment)
    {
        Payment = payment;
    }
}
