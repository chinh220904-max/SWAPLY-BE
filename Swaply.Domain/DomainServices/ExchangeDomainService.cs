using Swaply.Domain.Entities;
using Swaply.Domain.Enums;

namespace Swaply.Domain.DomainServices;

public class ExchangeDomainService : IExchangeDomainService
{
    public bool CanPerformExchange(Listing proposerListing, Listing receiverListing)
    {
        // Business rule: Both listings must be Active and not deleted to be exchanged
        return proposerListing.Status == ListingStatus.Active &&
               receiverListing.Status == ListingStatus.Active &&
               !proposerListing.IsDeleted &&
               !receiverListing.IsDeleted;
    }
}
