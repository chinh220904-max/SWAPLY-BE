using Swaply.Domain.Entities;

namespace Swaply.Domain.DomainServices;

public interface IExchangeDomainService
{
    bool CanPerformExchange(Listing proposerListing, Listing receiverListing);
}
