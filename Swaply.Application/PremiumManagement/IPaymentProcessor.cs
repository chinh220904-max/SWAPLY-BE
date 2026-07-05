namespace Swaply.Application.PremiumManagement;

public interface IPaymentProcessor
{
    Task<bool> ProcessPaymentAsync(decimal amount, string currency, string paymentMethodId, CancellationToken cancellationToken = default);
}
