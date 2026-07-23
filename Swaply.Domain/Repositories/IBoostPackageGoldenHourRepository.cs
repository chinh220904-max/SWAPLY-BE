using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IBoostPackageGoldenHourRepository
{
    Task<IReadOnlyList<BoostPackageGoldenHour>> GetByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<BoostPackageGoldenHour> goldenHours, CancellationToken cancellationToken = default);
    Task DeleteByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);
}
