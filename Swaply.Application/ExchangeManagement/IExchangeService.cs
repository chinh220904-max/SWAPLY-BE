using Swaply.Domain.Entities;

namespace Swaply.Application.ExchangeManagement;

public interface IExchangeService
{
    Task<Exchange> ProposeExchangeAsync(Guid proposerListingId, Guid receiverListingId, Guid proposerId, CancellationToken cancellationToken = default);
    Task<bool> AcceptExchangeAsync(Guid exchangeId, CancellationToken cancellationToken = default);
    Task<bool> RejectExchangeAsync(Guid exchangeId, CancellationToken cancellationToken = default);
}
