using Swaply.Application.AdminManagement;
using Swaply.Application.ListingManagement;

namespace Swaply.Application.AdminManagement;

public interface IAdminUserService
{
    Task<AdminUserListItemResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedListResponse<AdminUserListItemResponse>> SearchUsersAsync(string? keyword, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminUserDetailResponse?> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);
    Task LockUserAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default);
    Task UnlockUserAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default);
}
