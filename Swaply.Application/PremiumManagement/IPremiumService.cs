namespace Swaply.Application.PremiumManagement;

public interface IPremiumService
{
    Task<bool> UpgradeToPremiumAsync(string userId, string paymentMethodId, CancellationToken cancellationToken = default);
    Task<bool> IsPremiumUserAsync(string userId, CancellationToken cancellationToken = default);
}
