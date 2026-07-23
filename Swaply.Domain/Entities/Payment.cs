using Swaply.Domain.Enums;
using Swaply.Domain.ValueObjects;

namespace Swaply.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;
    public string ProviderTransactionId { get; private set; } = string.Empty;
    public string OrderInfo { get; private set; } = string.Empty;
    public string PayUrl { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? ReturnQuery { get; private set; }
    public string? IpnQuery { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // EF Core constructor
    private Payment() { }

    public Payment(Guid userId, Guid subscriptionId, Money amount, PaymentMethod method, string orderInfo, DateTime? expiresAt = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        Method = method;
        OrderInfo = orderInfo;
        Status = PaymentStatus.Pending;
        TransactionId = string.Empty;
        ProviderTransactionId = string.Empty;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public void SetPayUrl(string payUrl)
    {
        if (string.IsNullOrWhiteSpace(payUrl))
            throw new ArgumentException("Pay URL is required.", nameof(payUrl));

        PayUrl = payUrl;
    }

    public void SetIpAddress(string ipAddress)
    {
        IpAddress = ipAddress;
    }

    public void SetReturnQuery(string returnQuery)
    {
        ReturnQuery = returnQuery;
    }

    public void SetIpnQuery(string ipnQuery)
    {
        IpnQuery = ipnQuery;
    }

    public void MarkAsPaid(string transactionId, string providerTransactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required.", nameof(transactionId));

        Status = PaymentStatus.Paid;
        TransactionId = transactionId;
        ProviderTransactionId = providerTransactionId;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
    }

    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
    }
}
