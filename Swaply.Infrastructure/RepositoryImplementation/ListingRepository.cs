using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ListingRepository : IListingRepository
{
    private readonly SwaplyDbContext _context;

    public ListingRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = _context.Listings.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(listing);
    }

    public Task AddAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _context.Listings.Add(listing);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        var existing = _context.Listings.FirstOrDefault(x => x.Id == listing.Id);
        if (existing != null)
        {
            // In a real EF Core project, EF tracks entities. 
            // In our in-memory implementation, we remove and re-add or just do nothing since it's references.
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        var active = _context.Listings.Where(x => x.Status == ListingStatus.Active);
        return Task.FromResult<IEnumerable<Listing>>(active.ToList());
    }
}
