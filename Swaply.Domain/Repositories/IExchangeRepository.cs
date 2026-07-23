using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IExchangeRepository
{
    Task<Exchange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Exchange exchange, CancellationToken cancellationToken = default);
    Task UpdateAsync(Exchange exchange, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Exchange>> GetMyExchangesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exchange>> GetOutgoingExchangesAsync(Guid proposerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exchange>> GetIncomingExchangesAsync(Guid receiverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find an active exchange (Pending or Accepted) by listing pair regardless of direction.
    /// </summary>
    Task<Exchange?> GetActiveByListingPairAsync(Guid listingId1, Guid listingId2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find all pending exchanges that share at least one listing with the given exchange.
    /// Used for auto-cancel when an exchange is accepted.
    /// </summary>
    Task<IEnumerable<Exchange>> GetCompetingPendingExchangesAsync(Guid exchangeId, Guid listing1Id, Guid listing2Id, CancellationToken cancellationToken = default);
}
