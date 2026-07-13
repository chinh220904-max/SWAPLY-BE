namespace Swaply.Application.ReportManagement;

public interface IReportService
{
    Task<ReportResponse> CreateReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken cancellationToken = default);
    Task<PagedReportResponse<ReportResponse>> GetMyReportsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ReportResponse> GetReportByIdAsync(Guid reportId, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedReportResponse<ReportResponse>> GetAllReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedReportResponse<ReportResponse>> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ReportResponse> ApproveReportAsync(Guid reportId, string adminNote, CancellationToken cancellationToken = default);
    Task<ReportResponse> RejectReportAsync(Guid reportId, string adminNote, CancellationToken cancellationToken = default);
}
