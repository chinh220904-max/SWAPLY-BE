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

        return await MapToResponseAsync(report, cancellationToken);
    }

    public async Task<PagedReportResponse<ReportResponse>> GetMyReportsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _reportRepository.GetMyReportsAsync(userId, page, pageSize, cancellationToken);
        var responses = new List<ReportResponse>();
        foreach (var item in items)
        {
            responses.Add(await MapToResponseAsync(item, cancellationToken));
        }

        return CreatePagedResponse(responses, totalCount, page, pageSize);
    }

    public async Task<ReportResponse> GetReportByIdAsync(Guid reportId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report with ID {reportId} was not found.");

        if (userId.HasValue && report.ReporterId != userId.Value)
            throw new UnauthorizedAccessException("Access denied.");

        return await MapToResponseAsync(report, cancellationToken);
    }

    public async Task<PagedReportResponse<ReportResponse>> GetAllReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _reportRepository.GetAllReportsAsync(page, pageSize, cancellationToken);
        var responses = new List<ReportResponse>();
        foreach (var item in items)
        {
            responses.Add(await MapToResponseAsync(item, cancellationToken));
        }

        return CreatePagedResponse(responses, totalCount, page, pageSize);
    }

    public async Task<PagedReportResponse<ReportResponse>> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _reportRepository.GetPendingReportsAsync(page, pageSize, cancellationToken);
        var responses = new List<ReportResponse>();
        foreach (var item in items)
        {
            responses.Add(await MapToResponseAsync(item, cancellationToken));
        }

        return CreatePagedResponse(responses, totalCount, page, pageSize);
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

        return await MapToResponseAsync(report, cancellationToken);
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

        return await MapToResponseAsync(report, cancellationToken);
    }

    private async Task<ReportResponse> MapToResponseAsync(Report report, CancellationToken cancellationToken = default)
    {
        string? reporterName = report.Reporter != null
            ? (!string.IsNullOrWhiteSpace(report.Reporter.FullName) ? report.Reporter.FullName : report.Reporter.UserName)
            : null;

        if (string.IsNullOrWhiteSpace(reporterName) && report.ReporterId != Guid.Empty)
        {
            var reporter = await _userRepository.GetByIdAsync(report.ReporterId);
            if (reporter != null)
            {
                reporterName = !string.IsNullOrWhiteSpace(reporter.FullName) ? reporter.FullName : reporter.UserName;
            }
        }

        string? targetName = null;
        if (report.TargetType == ReportTargetType.User)
        {
            var targetUser = await _userRepository.GetByIdAsync(report.TargetId);
            if (targetUser != null)
            {
                targetName = !string.IsNullOrWhiteSpace(targetUser.FullName) ? targetUser.FullName : targetUser.UserName;
            }
        }
        else if (report.TargetType == ReportTargetType.Listing)
        {
            var targetListing = await _listingRepository.GetByIdAsync(report.TargetId, cancellationToken);
            if (targetListing != null)
            {
                targetName = targetListing.Title;
            }
        }

        return new ReportResponse(
            report.Id,
            report.ReporterId,
            reporterName,
            report.TargetType.ToString(),
            report.TargetId,
            targetName,
            report.Reason.ToString(),
            report.Description,
            report.Status.ToString(),
            report.AdminNote,
            report.CreatedAt,
            report.ProcessedAt);
    }

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
