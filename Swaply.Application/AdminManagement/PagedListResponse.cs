namespace Swaply.Application.AdminManagement;

public record PagedListResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
