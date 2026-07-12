using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.NotificationManagement;
using System.Security.Claims;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserNotifications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUnreadNotificationsAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(count);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        try
        {
            var notification = await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);
            return Ok(notification);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return NoContent();
    }
}
