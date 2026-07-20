using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Domain.ValueObjects;

namespace Swaply.Application.PremiumManagement;

public class PremiumService : IPremiumService
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public PremiumService(IPaymentGateway paymentGateway, IPaymentRepository paymentRepository, ISubscriptionRepository subscriptionRepository)
    {
        _paymentGateway = paymentGateway;
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Payment> StartUpgradeAsync(string userId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var money = new Money(amount, "VND");

        var plan = await _subscriptionRepository.GetActivePackageAsync(cancellationToken)
            ?? throw new InvalidOperationException("Premium plan is not configured.");

        var subscription = new Subscription(userGuid, plan.Id, plan.DurationDays);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        var payment = new Payment(userGuid, subscription.Id, money, PaymentMethod.VNPay, "Upgrade premium", subscription.ExpiresAt);
        await _paymentRepository.AddAsync(payment, cancellationToken);

        var payUrl = await _paymentGateway.CreatePaymentAsync(payment, cancellationToken);
        payment.SetPayUrl(payUrl);

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return payment;
    }

    public async Task<bool> IsPremiumUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return false;
        }

        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userGuid, cancellationToken);
        return subscription is not null;
    }
}
