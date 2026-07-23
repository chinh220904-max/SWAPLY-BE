using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class UserMonthlyQuotaRepository : IUserMonthlyQuotaRepository
{
    private readonly SwaplyDbContext _context;

    public UserMonthlyQuotaRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<UserMonthlyQuota?> GetByUserAndPeriodAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.UserMonthlyQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId && q.Year == year && q.Month == month, cancellationToken);
    }

    public async Task<UserMonthlyQuota?> GetCurrentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await GetByUserAndPeriodAsync(userId, now.Year, now.Month, cancellationToken);
    }

    public async Task AddAsync(UserMonthlyQuota quota, CancellationToken cancellationToken = default)
    {
        await _context.UserMonthlyQuotas.AddAsync(quota, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserMonthlyQuota quota, CancellationToken cancellationToken = default)
    {
        _context.UserMonthlyQuotas.Update(quota);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
