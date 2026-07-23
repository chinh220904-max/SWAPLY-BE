using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly SwaplyDbContext _context;

    public FavoriteRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public Task<Favorite?> GetByUserAndListingAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var favorite = _context.Favorites.FirstOrDefault(f => f.UserId == userId && f.ListingId == listingId);
        return Task.FromResult(favorite);
    }

    public Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var favorites = _context.Favorites.Where(f => f.UserId == userId);
        return Task.FromResult<IEnumerable<Favorite>>(favorites.ToList());
    }

    public Task<Favorite> AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
    {
        _context.Favorites.Add(favorite);
        return Task.FromResult(favorite);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var favorite = _context.Favorites.FirstOrDefault(f => f.Id == id);
        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
        }
        return Task.CompletedTask;
    }

    public async Task<(bool IsFavorited, int FavoriteCount)> ToggleFavoriteAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var existing = await GetByUserAndListingAsync(userId, listingId, cancellationToken);

        if (existing != null)
        {
            _context.Favorites.Remove(existing);
        }
        else
        {
            var favorite = new Favorite(userId, listingId);
            await AddAsync(favorite, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var count = _context.Favorites.Count(f => f.ListingId == listingId);
        return (existing == null, count);
    }
}
