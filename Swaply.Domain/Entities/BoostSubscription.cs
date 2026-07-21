using Swaply.Domain.Enums;

namespace Swaply.Domain.Entities;

public class BoostSubscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BoostPackageId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int TotalQuota { get; private set; }
    public int UsedQuota { get; private set; }
    public BoostSubscriptionStatus Status { get; private set; }

    public User? User { get; private set; }
    public BoostPackage? BoostPackage { get; private set; }

    private readonly List<BoostHistory> _boostHistories = new();
    public IReadOnlyCollection<BoostHistory> BoostHistories => _boostHistories.AsReadOnly();

    private BoostSubscription() { }

    public BoostSubscription(Guid userId, Guid boostPackageId, int durationDays, int totalQuota)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (boostPackageId == Guid.Empty)
            throw new ArgumentException("BoostPackageId is required.", nameof(boostPackageId));
        if (durationDays <= 0)
            throw new ArgumentException("Duration must be positive.", nameof(durationDays));
        if (totalQuota <= 0)
            throw new ArgumentException("TotalQuota must be positive.", nameof(totalQuota));

        Id = Guid.NewGuid();
        UserId = userId;
        BoostPackageId = boostPackageId;
        StartedAt = DateTime.UtcNow;
        ExpiresAt = StartedAt.AddDays(durationDays);
        TotalQuota = totalQuota;
        UsedQuota = 0;
        Status = BoostSubscriptionStatus.Active; // Default to Active, not 0
    }

    public bool IsActive()
    {
        return Status == BoostSubscriptionStatus.Active && DateTime.UtcNow < ExpiresAt;
    }

    public int RemainingQuota()
    {
        return TotalQuota - UsedQuota;
    }

    public bool CanUseQuota()
    {
        return IsActive() && RemainingQuota() > 0;
    }

    public bool UseQuota(int amount = 1)
    {
        if (!CanUseQuota())
            return false;

        UsedQuota += amount;
        return true;
    }

    public void Cancel()
    {
        Status = BoostSubscriptionStatus.Cancelled;
    }

    public void Expire()
    {
        Status = BoostSubscriptionStatus.Expired;
    }
}
