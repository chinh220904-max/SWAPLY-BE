namespace Swaply.Application.PremiumManagement;

public class PremiumService : IPremiumService
{
    private readonly IPaymentProcessor _paymentProcessor;

    public PremiumService(IPaymentProcessor paymentProcessor)
    {
        _paymentProcessor = paymentProcessor;
    }

    public async Task<bool> UpgradeToPremiumAsync(string userId, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        // 99,000 VND fee for premium upgrade
        var result = await _paymentProcessor.ProcessPaymentAsync(99000, "VND", paymentMethodId, cancellationToken);
        if (result)
        {
            // Update user status in database (mocked)
            return true;
        }
        return false;
    }

    public Task<bool> IsPremiumUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Simulated check
        return Task.FromResult(userId == "premium_user");
    }
}
