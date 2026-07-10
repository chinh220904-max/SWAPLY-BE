using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ListingManagement;
using Swaply.Domain.Repositories;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IImageUploadService _imageUploadService;

    public ProfileController(IUserRepository userRepository, IImageUploadService imageUploadService)
    {
        _userRepository = userRepository;
        _imageUploadService = imageUploadService;
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
    /// Get current user's profile
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found." });

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                fullName = user.FullName,
                avatarUrl = user.AvatarUrl,
                phoneNumber = user.PhoneNumber,
                role = user.Role?.Name,
                createdAt = user.CreatedAt,
                lastLoginAt = user.LastLoginAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update current user's profile (fullName, phoneNumber)
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found." });

            user.UpdateProfile(model.FullName, model.PhoneNumber);
            await _userRepository.UpdateAsync(user);

            return Ok(new
            {
                message = "Profile updated successfully.",
                fullName = user.FullName,
                phoneNumber = user.PhoneNumber
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Upload avatar image
    /// </summary>
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { error = "Only JPG, PNG, and WEBP images are allowed." });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "File size must be less than 5MB." });

            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found." });

            // Upload to Cloudinary
            using var stream = file.OpenReadStream();
            var avatarUrl = await _imageUploadService.UploadImageAsync(stream, file.FileName);

            // Update user's avatar
            user.SetAvatar(avatarUrl);
            await _userRepository.UpdateAsync(user);

            return Ok(new
            {
                message = "Avatar uploaded successfully.",
                avatarUrl = avatarUrl
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}

public record UpdateProfileModel(string? FullName, string? PhoneNumber);
