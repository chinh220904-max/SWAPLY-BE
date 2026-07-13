using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class ReportRepository : IReportRepository
{
    private readonly SwaplyDbContext _context;
    private IDbContextTransaction? _transaction;

    public ReportRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Include(r => r.Reporter)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task CreateAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Report> Items, int TotalCount)> GetMyReportsAsync(Guid reporterId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Report> Items, int TotalCount)> GetAllReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Report> Items, int TotalCount)> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> ExistsPendingDuplicateAsync(Guid reporterId, ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .AnyAsync(r => r.ReporterId == reporterId &&
                           r.TargetType == targetType &&
                           r.TargetId == targetId &&
                           r.Status == ReportStatus.Pending, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
