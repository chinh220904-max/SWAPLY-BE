using Microsoft.AspNetCore.Http;
using Swaply.Domain.Entities;

namespace Swaply.Application.PremiumManagement;

public interface IPaymentGateway
{
    Task<string> CreatePaymentAsync(Payment payment, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> HandleReturnAsync(IQueryCollection query, CancellationToken cancellationToken = default);
    Task<bool> HandleIpnAsync(IQueryCollection query, CancellationToken cancellationToken = default);
}
