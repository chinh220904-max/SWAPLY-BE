namespace Swaply.Domain.Exceptions;

public class ListingNotFoundException : DomainException
{
    public ListingNotFoundException(Guid listingId) 
        : base($"Listing with ID {listingId} was not found.")
    {
    }
}
