using FluentValidation;
using Swaply.Application.NotificationManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;

namespace Swaply.Application.ReportManagement;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;
    private readonly INotificationService _notificationService;

    public ReportService(
        IReportRepository reportRepository,
        IUserRepository userRepository,
        IListingRepository listingRepository,
        INotificationService notificationService)
    {
        _reportRepository = reportRepository;
        _userRepository = userRepository;
        _listingRepository = listingRepository;
        _notificationService = notificationService;
    }

    public async Task<ReportResponse> CreateReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken cancellationToken = default)
    {
        var reporter = await _userRepository.GetByIdAsync(reporterId)
            ?? throw new InvalidOperationException("Reporter not found.");

        if (reporterId == request.TargetId)
            throw new InvalidOperationException("You cannot report yourself.");

        if (!await TargetExistsAsync(request.TargetType, request.TargetId, cancellationToken))
            throw new InvalidOperationException("Target entity not found.");

        if (await _reportRepository.ExistsPendingDuplicateAsync(reporterId, request.TargetType, request.TargetId, cancellationToken))
            throw new InvalidOperationException("You already have a pending report for this target.");

        var report = new Report(
            reporterId,
            request.TargetType,
            request.TargetId,
            request.Reason,
            request.Description);

        await _reportRepository.CreateAsync(report, cancellationToken);

        return MapToResponse(report);
    }

    public async Task<PagedReportResponse<ReportResponse>> GetMyReportsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _reportRepository.GetMyReportsAsync(userId, page, pageSize, cancellationToken);
        return CreatePagedResponse(items.Select(MapToResponse), totalCount, page, pageSize);
    }

    public async Task<ReportResponse> GetReportByIdAsync(Guid reportId, Guid userId, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report with ID {reportId} was not found.");

        if (report.ReporterId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return MapToResponse(report);
    }

    public async Task<PagedReportResponse<ReportResponse>> GetAllReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _reportRepository.GetAllReportsAsync(page, pageSize, cancellationToken);
        return CreatePagedResponse(items.Select(MapToResponse), totalCount, page, pageSize);
    }

    public async Task<PagedReportResponse<ReportResponse>> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _reportRepository.GetPendingReportsAsync(page, pageSize, cancellationToken);
        return CreatePagedResponse(items.Select(MapToResponse), totalCount, page, pageSize);
    }

    public async Task<ReportResponse> ApproveReportAsync(Guid reportId, string adminNote, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report with ID {reportId} was not found.");

        if (report.Status != ReportStatus.Pending)
            throw new InvalidOperationException("Only pending reports can be approved.");

        ValidateAdminNote(adminNote);

        await _reportRepository.BeginTransactionAsync(cancellationToken);

        try
        {
            report.Approve(adminNote);
            await _reportRepository.SaveChangesAsync(cancellationToken);

            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                UserId: report.ReporterId,
                Title: "Report Processed",
                Content: "Your report has been reviewed and approved.",
                Type: "ReportProcessed",
                RelatedEntityId: report.Id,
                RelatedEntityType: "Report"
            ), cancellationToken);

            await _reportRepository.CommitAsync(cancellationToken);
        }
        catch
        {
            await _reportRepository.RollbackAsync(cancellationToken);
            throw;
        }

        return MapToResponse(report);
    }

    public async Task<ReportResponse> RejectReportAsync(Guid reportId, string adminNote, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report with ID {reportId} was not found.");

        if (report.Status != ReportStatus.Pending)
            throw new InvalidOperationException("Only pending reports can be rejected.");

        ValidateAdminNote(adminNote);

        await _reportRepository.BeginTransactionAsync(cancellationToken);

        try
        {
            report.Reject(adminNote);
            await _reportRepository.SaveChangesAsync(cancellationToken);

            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                UserId: report.ReporterId,
                Title: "Report Processed",
                Content: "Your report has been reviewed and rejected.",
                Type: "ReportProcessed",
                RelatedEntityId: report.Id,
                RelatedEntityType: "Report"
            ), cancellationToken);

            await _reportRepository.CommitAsync(cancellationToken);
        }
        catch
        {
            await _reportRepository.RollbackAsync(cancellationToken);
            throw;
        }

        return MapToResponse(report);
    }

    private static ReportResponse MapToResponse(Report report) => new(
        report.Id,
        report.ReporterId,
        report.TargetType.ToString(),
        report.TargetId,
        report.Reason.ToString(),
        report.Description,
        report.Status.ToString(),
        report.AdminNote,
        report.CreatedAt,
        report.ProcessedAt);

    private async Task<bool> TargetExistsAsync(ReportTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        if (targetType == ReportTargetType.User)
            return await _userRepository.GetByIdAsync(targetId) is not null;

        return await _listingRepository.GetByIdAsync(targetId, cancellationToken) is not null;
    }

    private static void ValidateAdminNote(string? adminNote)
    {
        if (string.IsNullOrWhiteSpace(adminNote))
            throw new ArgumentException("Admin note is required.");

        if (adminNote.Length > 1000)
            throw new ArgumentException("Admin note must not exceed 1000 characters.");
    }

    private static PagedReportResponse<ReportResponse> CreatePagedResponse(IEnumerable<ReportResponse> items, int totalCount, int page, int pageSize)
    {
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedReportResponse<ReportResponse>(items.ToList(), totalCount, page, pageSize, totalPages);
    }
}
