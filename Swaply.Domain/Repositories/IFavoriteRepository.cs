using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByUserAndListingAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Favorite> AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool IsFavorited, int FavoriteCount)> ToggleFavoriteAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
}
