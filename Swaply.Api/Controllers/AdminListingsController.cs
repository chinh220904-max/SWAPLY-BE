using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ListingManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/admin/listings")]
[Authorize]
public class AdminListingsController : ControllerBase
{
    private readonly IListingRepository _listingRepository;
    private readonly IListingService _listingService;
    private readonly IUserRepository _userRepository;

    public AdminListingsController(
        IListingRepository listingRepository,
        IListingService listingService,
        IUserRepository userRepository)
    {
        _listingRepository = listingRepository;
        _listingService = listingService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> SearchListings([FromQuery] AdminListingSearchRequest? request)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        request ??= new AdminListingSearchRequest();

        var result = await _listingService.SearchAdminListingsAsync(
            request.Keyword,
            request.Status,
            request.SortBy,
            request.Page,
            request.PageSize);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetListingDetail(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var detail = await _listingService.GetAdminListingDetailAsync(id);
        if (detail == null)
            return NotFound(new { error = "Listing not found." });

        return Ok(detail);
    }

    [HttpPut("{id:guid}/hide")]
    public async Task<IActionResult> HideListing(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        try
        {
            var listing = await _listingService.HideListingAsync(id);
            return Ok(new { id = listing.Id, status = listing.Status.ToString() });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Listing not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<IActionResult> RestoreListing(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        try
        {
            var listing = await _listingService.RestoreListingAsync(id);
            return Ok(new { id = listing.Id, status = listing.Status.ToString() });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Listing not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDeleteListing(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        try
        {
            await _listingService.SoftDeleteListingAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Listing not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedListings([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var deleted = await _listingService.GetDeletedListingsAsync();
        var items = deleted.Select(MapToDeletedSummary).ToList();

        return Ok(new PagedListResponse<object>(
            items.Cast<object>().ToList(),
            items.Count,
            page,
            pageSize,
            (int)Math.Ceiling(items.Count / (double)pageSize)
        ));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveListing(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
            return NotFound(new { error = "Listing not found." });

        try
        {
            var approved = await _listingService.ApproveListingAsync(id);
            return Ok(MapToDetailResponse(approved));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectListing(Guid id, [FromBody] RejectListingRequest? request)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var listing = await _listingRepository.GetByIdAsync(id);
        if (listing == null)
            return NotFound(new { error = "Listing not found." });

        try
        {
            var rejected = await _listingService.RejectListingAsync(id, request?.Reason);
            return Ok(new
            {
                id = rejected.Id,
                status = rejected.Status.ToString(),
                rejectionReason = rejected.RejectionReason
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingListings()
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var pending = await _listingService.GetPendingListingsAsync();
        return Ok(pending.Select(MapToSummaryResponse));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }

    private async Task<bool> IsAdminAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        var role = await _userRepository.GetRoleByNameAsync("Admin");
        return role != null && user.RoleId == role.Id;
    }

    private static object MapToSummaryResponse(Listing l) => new
    {
        id = l.Id,
        title = l.Title,
        description = l.Description,
        estimatedAmount = l.EstimatedValue.Amount,
        currency = l.EstimatedValue.Currency,
        condition = l.Condition.ToString(),
        status = l.Status.ToString(),
        brand = l.Brand,
        location = l.Location,
        ownerName = l.Owner?.FullName ?? string.Empty,
        ownerId = l.OwnerId,
        categoryId = l.CategoryId,
        categoryName = l.Category?.Name ?? string.Empty,
        imageUrls = l.Images.Select(i => i.ImageUrl).ToList(),
        createdAt = l.CreatedAt
    };

    private static object MapToDetailResponse(Listing l) => new
    {
        id = l.Id,
        title = l.Title,
        description = l.Description,
        estimatedAmount = l.EstimatedValue.Amount,
        currency = l.EstimatedValue.Currency,
        condition = l.Condition.ToString(),
        status = l.Status.ToString(),
        brand = l.Brand,
        exchangeWish = l.ExchangeWish,
        cashTopUpAmount = l.CashTopUp?.Amount,
        location = l.Location,
        viewCount = l.ViewCount,
        favoriteCount = l.FavoriteCount,
        ownerId = l.OwnerId,
        ownerName = l.Owner?.FullName ?? string.Empty,
        categoryId = l.CategoryId,
        categoryName = l.Category?.Name ?? string.Empty,
        imageUrls = l.Images.Select(i => i.ImageUrl).ToList(),
        createdAt = l.CreatedAt,
        updatedAt = l.UpdatedAt,
        expiresAt = l.ExpiresAt,
        rejectionReason = l.RejectionReason
    };

    private static object MapToDeletedSummary(Listing l) => new
    {
        id = l.Id,
        title = l.Title,
        ownerName = l.Owner?.FullName ?? string.Empty,
        status = l.Status.ToString(),
        deleted = l.IsDeleted,
        createdAt = l.CreatedAt,
        updatedAt = l.UpdatedAt
    };
}

public record AdminListingSearchRequest(
    string? Keyword = null,
    ListingStatus? Status = null,
    string? SortBy = "newest",
    int Page = 1,
    int PageSize = 20
);

public record RejectListingRequest(string? Reason);
