using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public CategoriesController(ICategoryRepository categoryRepository, IUserRepository userRepository)
    {
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            description = c.Description
        }));
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { error = "Category not found." });

        return Ok(new
        {
            id = category.Id,
            name = category.Name,
            description = category.Description
        });
    }

    /// <summary>
    /// Create a new category (admin only)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        if (await _categoryRepository.GetByNameAsync(request.Name) != null)
            return BadRequest(new { error = "Category name already exists." });

        var category = new Category(Guid.NewGuid(), request.Name.Trim(), request.Description?.Trim() ?? string.Empty);
        await _categoryRepository.AddAsync(category);

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, new
        {
            id = category.Id,
            name = category.Name,
            description = category.Description
        });
    }

    /// <summary>
    /// Update an existing category (admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { error = "Category not found." });

        var existingWithSameName = await _categoryRepository.GetByNameAsync(request.Name);
        if (existingWithSameName != null && existingWithSameName.Id != id)
            return BadRequest(new { error = "Category name already exists." });

        category.Update(request.Name.Trim(), request.Description?.Trim() ?? string.Empty);
        await _categoryRepository.UpdateAsync(category);

        return Ok(new
        {
            id = category.Id,
            name = category.Name,
            description = category.Description
        });
    }

    /// <summary>
    /// Delete a category (admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { error = "Category not found." });

        await _categoryRepository.DeleteAsync(category);
        return NoContent();
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
}

public record CreateCategoryRequest(string Name, string? Description);
public record UpdateCategoryRequest(string Name, string? Description);
