using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IBoostPackageRepository
{
    Task<IReadOnlyList<BoostPackage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BoostPackage>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<BoostPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(BoostPackage package, CancellationToken cancellationToken = default);
    Task UpdateAsync(BoostPackage package, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
