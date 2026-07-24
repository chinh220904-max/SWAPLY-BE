using Swaply.Domain.Enums;

namespace Swaply.Application.ReportManagement;

public record CreateReportRequest(
    ReportTargetType TargetType,
    Guid TargetId,
    ReportReason Reason,
    string? Description = null
);

public record ProcessReportRequest(string AdminNote);

public record ReportResponse(
    Guid Id,
    Guid ReporterId,
    string? ReporterName,
    string TargetType,
    Guid TargetId,
    string? TargetName,
    string Reason,
    string? Description,
    string Status,
    string? AdminNote,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record PagedReportResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
