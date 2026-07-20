using Swaply.Domain.Entities;

namespace Swaply.Application.PremiumManagement;

public interface IPremiumService
{
    Task<Payment> StartUpgradeAsync(string userId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> IsPremiumUserAsync(string userId, CancellationToken cancellationToken = default);
}
