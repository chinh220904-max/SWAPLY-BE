using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ReviewManagement;
using System.Security.Claims;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");
        return userId;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var review = await _reviewService.CreateReviewAsync(userId, request);
            return CreatedAtAction(nameof(GetUserReviews), new { userId = request.RevieweeId }, review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpGet("~/api/users/{userId:guid}/reviews")]
    public async Task<IActionResult> GetUserReviews(Guid userId)
    {
        var reviews = await _reviewService.GetUserReviewsAsync(userId);
        return Ok(reviews);
    }

    [HttpGet("~/api/users/{userId:guid}/rating")]
    public async Task<IActionResult> GetUserRating(Guid userId)
    {
        var rating = await _reviewService.GetUserRatingAsync(userId);
        return Ok(rating);
    }
}
