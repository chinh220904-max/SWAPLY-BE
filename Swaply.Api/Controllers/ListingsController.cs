using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.BoostManagement;
using Swaply.Application.ListingManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
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
    private readonly IImageUploadService _imageUploadService;
    private readonly IBoostService _boostService;

    public ListingsController(
        IListingService listingService,
        IListingRepository listingRepository,
        IFavoriteRepository favoriteRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        IImageUploadService imageUploadService,
        IBoostService boostService)
    {
        _listingService = listingService;
        _listingRepository = listingRepository;
        _favoriteRepository = favoriteRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _imageUploadService = imageUploadService;
        _boostService = boostService;
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
    /// Create a new listing (multipart/form-data with image upload)
    /// </summary>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateListing(
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] Guid categoryId,
        [FromForm] decimal estimatedAmount,
        [FromForm] string currency = "VND",
        [FromForm] ItemCondition condition = ItemCondition.Good,
        [FromForm] string brand = "",
        [FromForm] string exchangeWish = "",
        [FromForm] decimal? cashTopUpAmount = null,
        [FromForm] string location = "",
        [FromForm] List<IFormFile>? images = null)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Check quota before creating listing
            var canCreate = await _boostService.CanCreateListingAsync(userId);
            if (!canCreate)
            {
                var quotaStatus = await _boostService.GetQuotaStatusAsync(userId);
                return BadRequest(new
                {
                    error = "Quota exceeded. Please purchase a boost package or wait until next month.",
                    quotaStatus
                });
            }

            // Upload images if provided
            var imageUrls = new List<string>();
            if (images != null && images.Any())
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var url = await _imageUploadService.UploadFromFormFileAsync(image);
                        imageUrls.Add(url);
                    }
                }
            }

            var request = new CreateListingRequest(
                title,
                description,
                userId,
                categoryId,
                estimatedAmount,
                currency,
                condition,
                brand,
                exchangeWish,
                cashTopUpAmount,
                location,
                imageUrls.Any() ? imageUrls : null
            );

            var listing = await _listingService.CreateListingAsync(request);

            // Use quota after successful creation
            await _boostService.UseQuotaAsync(userId);

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
            var userId = GetCurrentUserId();

            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            if (listing.OwnerId != userId)
                return Forbid();

            if (listing.IsDeleted)
                return BadRequest(new { error = "Listing is deleted." });

            var updated = await _listingService.UpdateListingAsync(id, request);
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

            if (listing.IsDeleted)
                return NoContent();

            await _listingService.SoftDeleteListingAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit draft listing for admin review
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize]
    public async Task<IActionResult> SubmitForReview(Guid id)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return NotFound(new { error = "Listing not found." });

            var userId = GetCurrentUserId();
            if (listing.OwnerId != userId)
                return Forbid();

            var submitted = await _listingService.SubmitForReviewAsync(id);
            return Ok(MapToDetailResponse(submitted));
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
    /// Renew an expired listing (resubmit for admin review)
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

            if (listing.IsDeleted)
                return BadRequest(new { error = "Listing is deleted." });

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
            expiresAt = listing.ExpiresAt,
            rejectionReason = listing.RejectionReason,
            boostInfo = listing.BoostedAt.HasValue ? new
            {
                boostedAt = listing.BoostedAt,
                boostExpiresAt = listing.BoostExpiresAt,
                priority = listing.BoostPriority
            } : null
        };
    }
}
