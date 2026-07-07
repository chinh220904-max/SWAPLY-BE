using Swaply.Domain.ValueObjects;

namespace Swaply.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = Money.Zero();
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    // Navigation property (1:1 from Payment side)
    public Subscription? Subscription { get; private set; }

    // EF Core constructor
    private Payment() { }

    public Payment(Guid subscriptionId, Money amount, PaymentMethod method)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscriptionId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        TransactionId = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required.", nameof(transactionId));

        Status = PaymentStatus.Paid;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
    }
}
