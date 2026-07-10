using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Domain.ValueObjects;
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
            _context.Listings.Remove(existing);
        }
        _context.Listings.Add(listing);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = _context.Listings.FirstOrDefault(x => x.Id == id);
        if (listing != null)
        {
            _context.Listings.Remove(listing);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        var active = _context.Listings.Where(x => x.Status == ListingStatus.Active);
        return Task.FromResult<IEnumerable<Listing>>(active.ToList());
    }

    public Task<IEnumerable<Listing>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var listings = _context.Listings.Where(x => x.OwnerId == ownerId);
        return Task.FromResult<IEnumerable<Listing>>(listings.ToList());
    }

    public Task<IEnumerable<Listing>> GetByStatusAsync(Guid ownerId, ListingStatus status, CancellationToken cancellationToken = default)
    {
        var listings = _context.Listings.Where(x => x.OwnerId == ownerId && x.Status == status);
        return Task.FromResult<IEnumerable<Listing>>(listings.ToList());
    }

    public Task<IEnumerable<Listing>> SearchListingsAsync(
        string? keyword,
        Guid? categoryId,
        ItemCondition? condition,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        ListingStatus? status,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings.AsQueryable();

        // Filter by status (default to Active if not specified)
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        else
            query = query.Where(x => x.Status == ListingStatus.Active);

        // Keyword search
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                x.Description.ToLower().Contains(keyword) ||
                x.Brand.ToLower().Contains(keyword) ||
                x.ExchangeWish.ToLower().Contains(keyword));
        }

        // Category filter
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        // Condition filter
        if (condition.HasValue)
            query = query.Where(x => x.Condition == condition.Value);

        // Location filter
        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(x => x.Location.ToLower().Contains(location.ToLower()));

        // Price range
        if (minPrice.HasValue)
            query = query.Where(x => x.EstimatedValue.Amount >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(x => x.EstimatedValue.Amount <= maxPrice.Value);

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "newest" => query.OrderByDescending(x => x.CreatedAt),
            "oldest" => query.OrderBy(x => x.CreatedAt),
            "price_asc" => query.OrderBy(x => x.EstimatedValue.Amount),
            "price_desc" => query.OrderByDescending(x => x.EstimatedValue.Amount),
            "popular" => query.OrderByDescending(x => x.ViewCount),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        // Pagination
        var result = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<IEnumerable<Listing>>(result);
    }

    public Task<int> GetTotalCountAsync(
        string? keyword,
        Guid? categoryId,
        ItemCondition? condition,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        ListingStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings.AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        else
            query = query.Where(x => x.Status == ListingStatus.Active);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                x.Description.ToLower().Contains(keyword));
        }

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (condition.HasValue)
            query = query.Where(x => x.Condition == condition.Value);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(x => x.Location.ToLower().Contains(location.ToLower()));

        if (minPrice.HasValue)
            query = query.Where(x => x.EstimatedValue.Amount >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(x => x.EstimatedValue.Amount <= maxPrice.Value);

        return Task.FromResult(query.Count());
    }
}
