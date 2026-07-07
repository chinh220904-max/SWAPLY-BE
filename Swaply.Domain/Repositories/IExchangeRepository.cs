using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IExchangeRepository
{
    Task<Exchange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Exchange exchange, CancellationToken cancellationToken = default);
    Task UpdateAsync(Exchange exchange, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exchange>> GetExchangesByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
