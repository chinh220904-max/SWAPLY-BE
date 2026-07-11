using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ListingImageRepository : IListingImageRepository
{
    private readonly SwaplyDbContext _context;

    public ListingImageRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<ListingImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ListingImages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ListingImage>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _context.ListingImages
            .Where(x => x.ListingId == listingId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ListingImage image, CancellationToken cancellationToken = default)
    {
        await _context.ListingImages.AddAsync(image, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ListingImage> images, CancellationToken cancellationToken = default)
    {
        await _context.ListingImages.AddRangeAsync(images, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await _context.ListingImages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (image != null)
        {
            _context.ListingImages.Remove(image);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var images = await _context.ListingImages
            .Where(x => x.ListingId == listingId)
            .ToListAsync(cancellationToken);
        _context.ListingImages.RemoveRange(images);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
