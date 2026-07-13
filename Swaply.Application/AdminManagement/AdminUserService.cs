using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swaply.Application.AdminManagement;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;

namespace Swaply.Application.AdminManagement;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IExchangeRepository _exchangeRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IPremiumService _premiumService;

    public AdminUserService(
        IUserRepository userRepository,
        IListingRepository listingRepository,
        IExchangeRepository exchangeRepository,
        IReviewRepository reviewRepository,
        IReportRepository reportRepository,
        IPremiumService premiumService)
    {
        _userRepository = userRepository;
        _listingRepository = listingRepository;
        _exchangeRepository = exchangeRepository;
        _reviewRepository = reviewRepository;
        _reportRepository = reportRepository;
        _premiumService = premiumService;
    }

    public async Task<AdminUserListItemResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return null;

        var role = await _userRepository.GetRoleByNameAsync("Admin", cancellationToken);
        var isPremium = await _premiumService.IsPremiumUserAsync(user.Id.ToString(), cancellationToken);

        return new AdminUserListItemResponse(
            user.Id,
            user.FullName ?? string.Empty,
            user.Email,
            role != null && user.RoleId == role.Id ? "Admin" : "User",
            user.IsBanned ? "Locked" : "Active",
            isPremium,
            user.CreatedAt
        );
    }

    public async Task<PagedListResponse<AdminUserListItemResponse>> SearchUsersAsync(string? keyword, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await _userRepository.SearchUsersAsync(keyword, page, pageSize, cancellationToken);
        var totalCount = await _userRepository.SearchUsersCountAsync(keyword, cancellationToken);
        var roleAdmin = await _userRepository.GetRoleByNameAsync("Admin", cancellationToken);

        var results = items.Select(u => new AdminUserListItemResponse(
            u.Id,
            u.FullName ?? string.Empty,
            u.Email,
            roleAdmin != null && u.RoleId == roleAdmin.Id ? "Admin" : "User",
            u.IsBanned ? "Locked" : "Active",
            false,
            u.CreatedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedListResponse<AdminUserListItemResponse>(results, totalCount, page, pageSize, totalPages);
    }

    public async Task LockUserAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        if (userId == adminUserId)
            throw new InvalidOperationException("You cannot lock yourself.");

        var target = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (target == null)
            throw new KeyNotFoundException("User not found.");

        var targetRole = await _userRepository.GetRoleByNameAsync("Admin", cancellationToken);
        var isTargetAdmin = targetRole != null && target.RoleId == targetRole.Id;
        if (isTargetAdmin)
            throw new InvalidOperationException("Cannot lock another admin.");

        if (target.IsBanned)
            throw new InvalidOperationException("User is already locked.");

        target.Ban("Locked by admin");
        await _userRepository.UpdateAsync(target, cancellationToken);
    }

    public async Task UnlockUserAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var target = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (target == null)
            throw new KeyNotFoundException("User not found.");

        if (!target.IsBanned)
            throw new InvalidOperationException("User is not locked.");

        target.Unban();
        await _userRepository.UpdateAsync(target, cancellationToken);
    }

    public async Task<AdminUserDetailResponse?> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return null;

        var roleAdmin = await _userRepository.GetRoleByNameAsync("Admin", cancellationToken);

        var listings = await _listingRepository.GetByOwnerIdAsync(userId, cancellationToken);
        var listingCount = listings.Count();

        var exchanges = await _exchangeRepository.GetMyExchangesAsync(userId, cancellationToken);
        var completedExchanges = exchanges.Count(e => e.Status == ExchangeStatus.Completed);

        var reviews = await _reviewRepository.GetReviewsForUserAsync(userId, cancellationToken);
        var reviewCount = reviews.Count();
        var averageRating = reviewCount == 0 ? 0 : reviews.Average(r => r.Rating);

        var reports = await _reportRepository.GetMyReportsAsync(userId, 1, int.MaxValue, cancellationToken);
        var reportCount = reports.Items.Count();

        var isPremium = await _premiumService.IsPremiumUserAsync(user.Id.ToString(), cancellationToken);

        return new AdminUserDetailResponse(
            user.Id,
            user.FullName ?? string.Empty,
            user.Email,
            roleAdmin != null && user.RoleId == roleAdmin.Id ? "Admin" : "User",
            user.IsBanned ? "Locked" : "Active",
            isPremium,
            user.CreatedAt,
            listingCount,
            completedExchanges,
            Math.Round(averageRating, 2),
            reviewCount,
            reportCount
        );
    }
}
