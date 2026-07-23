using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IUserMonthlyQuotaRepository
{
    Task<UserMonthlyQuota?> GetByUserAndPeriodAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
    Task<UserMonthlyQuota?> GetCurrentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserMonthlyQuota quota, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserMonthlyQuota quota, CancellationToken cancellationToken = default);
}
