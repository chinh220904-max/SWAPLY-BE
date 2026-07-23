using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class BoostPackageGoldenHourRepository : IBoostPackageGoldenHourRepository
{
    private readonly SwaplyDbContext _context;

    public BoostPackageGoldenHourRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BoostPackageGoldenHour>> GetByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        return await _context.BoostPackageGoldenHours
            .Where(gh => gh.BoostPackageId == packageId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<BoostPackageGoldenHour> goldenHours, CancellationToken cancellationToken = default)
    {
        await _context.BoostPackageGoldenHours.AddRangeAsync(goldenHours, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        var goldenHours = await _context.BoostPackageGoldenHours
            .Where(gh => gh.BoostPackageId == packageId)
            .ToListAsync(cancellationToken);

        _context.BoostPackageGoldenHours.RemoveRange(goldenHours);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
