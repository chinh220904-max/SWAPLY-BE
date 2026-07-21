using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.BoostManagement;
using Swaply.Domain.Repositories;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/boost")]
[Authorize]
public class UserBoostController : ControllerBase
{
    private readonly IBoostService _boostService;
    private readonly IBoostPackageRepository _packageRepository;

    public UserBoostController(IBoostService boostService, IBoostPackageRepository packageRepository)
    {
        _boostService = boostService;
        _packageRepository = packageRepository;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }

    [HttpGet("quota")]
    public async Task<IActionResult> GetQuotaStatus(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _boostService.GetQuotaStatusAsync(userId, cancellationToken);
            return Ok(status);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpGet("packages")]
    public async Task<IActionResult> GetAvailablePackages(CancellationToken cancellationToken)
    {
        var packages = await _boostService.GetAllPackagesAsync(cancellationToken);
        return Ok(packages);
    }

    [HttpPost("subscribe/{packageId:guid}")]
    public async Task<IActionResult> Subscribe(Guid packageId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _boostService.SubscribeAsync(userId, packageId, cancellationToken);
            return Ok(result);
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

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscription(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var subscription = await _boostService.GetCurrentSubscriptionAsync(userId, cancellationToken);

            if (subscription == null)
                return NotFound(new { message = "No active subscription." });

            return Ok(subscription);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpDelete("cancel")]
    public async Task<IActionResult> CancelSubscription(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _boostService.CancelSubscriptionAsync(userId, cancellationToken);
            return Ok(new { message = "Subscription cancelled successfully." });
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
}
