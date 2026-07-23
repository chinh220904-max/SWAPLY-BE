using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class BoostPackageRepository : IBoostPackageRepository
{
    private readonly SwaplyDbContext _context;

    public BoostPackageRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BoostPackage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BoostPackages.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BoostPackage>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BoostPackages
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<BoostPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BoostPackages.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(BoostPackage package, CancellationToken cancellationToken = default)
    {
        await _context.BoostPackages.AddAsync(package, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BoostPackage package, CancellationToken cancellationToken = default)
    {
        _context.BoostPackages.Update(package);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await GetByIdAsync(id, cancellationToken);
        if (package != null)
        {
            _context.BoostPackages.Remove(package);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
