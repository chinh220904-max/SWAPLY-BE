using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ExchangeRepository : IExchangeRepository
{
    private readonly SwaplyDbContext _context;

    public ExchangeRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public Task<Exchange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exchange = _context.Exchanges.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(exchange);
    }

    public Task AddAsync(Exchange exchange, CancellationToken cancellationToken = default)
    {
        _context.Exchanges.Add(exchange);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Exchange exchange, CancellationToken cancellationToken = default)
    {
        // In-memory update does not require extra actions since reference is same
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Exchange>> GetExchangesByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var exchanges = _context.Exchanges.Where(x => x.ProposerId == userId || x.ReceiverId == userId);
        return Task.FromResult<IEnumerable<Exchange>>(exchanges.ToList());
    }
}
