using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.AdminManagement;
using Swaply.Application.AdminManagement.FluentValidation;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using FluentValidation;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class AdminUsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAdminUserService _adminUserService;
    private readonly IValidator<LockUserRequest> _lockValidator;

    public AdminUsersController(
        IUserRepository userRepository,
        IAdminUserService adminUserService,
        IValidator<LockUserRequest> lockValidator)
    {
        _userRepository = userRepository;
        _adminUserService = adminUserService;
        _lockValidator = lockValidator;
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _adminUserService.SearchUsersAsync(keyword, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserDetail(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var result = await _adminUserService.GetUserDetailAsync(id);
        if (result == null)
            return NotFound(new { error = "User not found." });

        return Ok(result);
    }

    [HttpPut("{id:guid}/lock")]
    public async Task<IActionResult> LockUser(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        var request = new LockUserRequest { Id = id };
        var validation = await _lockValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { error = validation.Errors.FirstOrDefault()?.ErrorMessage });

        try
        {
            await _adminUserService.LockUserAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await IsAdminAsync(userId))
            return Forbid();

        try
        {
            await _adminUserService.UnlockUserAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
