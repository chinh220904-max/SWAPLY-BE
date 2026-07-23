using Swaply.Domain.Entities;
using Swaply.Domain.Enums;

namespace Swaply.Domain.Repositories;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(Report report, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Report> Items, int TotalCount)> GetMyReportsAsync(Guid reporterId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Report> Items, int TotalCount)> GetAllReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Report> Items, int TotalCount)> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsPendingDuplicateAsync(Guid reporterId, ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);
    Task<int> GetCountByTargetAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
