using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ListingManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Domain.ValueObjects;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly IListingRepository _listingRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public ListingsController(
        IListingService listingService,
        IListingRepository listingRepository,
        IFavoriteRepository favoriteRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository)
    {
        _listingService = listingService;
        _listingRepository = listingRepository;
        _favoriteRepository = favoriteRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }

    /// <summary>
    /// Get all active listings with search and filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetListings(
        [FromQuery] string? q,
        [FromQuery] Guid? categoryId,
        [FromQuery] ItemCondition? condition,
        [FromQuery] string? location,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string sortBy = "newest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var request = new SearchListingRequest(q, categoryId, condition, location, minPrice, maxPrice, sortBy, page, pageSize);
        var result = await _listingService.SearchListingsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get listing details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetListingById(Guid id)
    {
        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null)
            return NotFound(new { error = "Listing not found." });

        // Increment view count
        await _listingService.IncrementViewCountAsync(id);

        return Ok(MapToDetailResponse(listing));
    }

    /// <summary>
    /// Create a new listing (draft)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        try
        {
            var listing = await _listingService.CreateListingAsync(request);
            return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, MapToDetailResponse(listing));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing listing
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            if (listing.OwnerId != userId)
                return Forbid();

            var updated = await _listingService.UpdateListingAsync(id, request);
            return Ok(MapToDetailResponse(updated));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a listing
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteListing(Guid id)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            if (listing.OwnerId != userId)
                return Forbid();

            await _listingService.DeleteListingAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update listing status (publish, hide, etc.)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            if (listing.OwnerId != userId)
                return Forbid();

            var updated = await _listingService.UpdateStatusAsync(id, request.Status);
            return Ok(MapToDetailResponse(updated));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Publish a draft listing
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> PublishListing(Guid id)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            if (listing.OwnerId != userId)
                return Forbid();

            var published = await _listingService.PublishListingAsync(id);
            return Ok(MapToDetailResponse(published));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Renew an expired listing
    /// </summary>
    [HttpPost("{id:guid}/renew")]
    [Authorize]
    public async Task<IActionResult> RenewListing(Guid id)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            if (listing.OwnerId != userId)
                return Forbid();

            var renewed = await _listingService.RenewListingAsync(id);
            return Ok(MapToDetailResponse(renewed));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's listings
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyListings()
    {
        try
        {
            var userId = GetCurrentUserId();
            var listings = await _listingService.GetMyListingsAsync(userId);
            return Ok(listings.Select(MapToDetailResponse));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's listings by status
    /// </summary>
    [HttpGet("my/{status}")]
    [Authorize]
    public async Task<IActionResult> GetMyListingsByStatus(ListingStatus status)
    {
        try
        {
            var userId = GetCurrentUserId();
            var listings = await _listingService.GetMyListingsByStatusAsync(userId, status);
            return Ok(listings.Select(MapToDetailResponse));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Toggle favorite on a listing
    /// </summary>
    [HttpPost("{id:guid}/favorite")]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            var result = await _favoriteRepository.ToggleFavoriteAsync(userId, id);

            return Ok(new FavoriteResponse(id, result.IsFavorited, result.FavoriteCount));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get user's favorite listings
    /// </summary>
    [HttpGet("my/favorites")]
    [Authorize]
    public async Task<IActionResult> GetMyFavorites()
    {
        try
        {
            var userId = GetCurrentUserId();
            var favorites = await _favoriteRepository.GetByUserIdAsync(userId);
            var listings = favorites.Select(f => f.Listing).Where(l => l != null).Select(l => MapToDetailResponse(l!));
            return Ok(listings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get listings by category
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var request = new SearchListingRequest(null, categoryId, null, null, null, null, "newest", page, pageSize);
        var result = await _listingService.SearchListingsAsync(request);
        return Ok(result);
    }

    private object MapToDetailResponse(Listing listing)
    {
        return new
        {
            id = listing.Id,
            title = listing.Title,
            description = listing.Description,
            ownerId = listing.OwnerId,
            ownerName = listing.Owner?.FullName ?? "",
            ownerAvatar = listing.Owner?.AvatarUrl ?? "",
            categoryId = listing.CategoryId,
            categoryName = listing.Category?.Name ?? "",
            estimatedValue = listing.EstimatedValue.Amount,
            currency = listing.EstimatedValue.Currency,
            condition = listing.Condition,
            conditionName = listing.Condition.ToString(),
            status = listing.Status,
            brand = listing.Brand,
            exchangeWish = listing.ExchangeWish,
            cashTopUpAmount = listing.CashTopUp?.Amount,
            cashTopUpCurrency = listing.CashTopUp?.Currency,
            location = listing.Location,
            viewCount = listing.ViewCount,
            favoriteCount = listing.FavoriteCount,
            images = listing.Images.Select(img => new
            {
                id = img.Id,
                imageUrl = img.ImageUrl,
                isPrimary = img.DisplayOrder == 0,
                displayOrder = img.DisplayOrder
            }),
            createdAt = listing.CreatedAt,
            updatedAt = listing.UpdatedAt,
            expiresAt = listing.ExpiresAt
        };
    }
}
