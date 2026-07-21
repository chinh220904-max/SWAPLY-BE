using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.BoostManagement;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/admin/boost-packages")]
[Authorize(Roles = "Admin")]
public class AdminBoostPackagesController : ControllerBase
{
    private readonly IBoostService _boostService;

    public AdminBoostPackagesController(IBoostService boostService)
    {
        _boostService = boostService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPackages(CancellationToken cancellationToken)
    {
        var packages = await _boostService.GetAllPackagesAsync(cancellationToken);
        return Ok(packages);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPackageById(Guid id, CancellationToken cancellationToken)
    {
        var package = await _boostService.GetPackageByIdAsync(id, cancellationToken);
        if (package == null)
            return NotFound(new { error = "Package not found." });

        return Ok(package);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePackage([FromBody] CreateBoostPackageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var package = await _boostService.CreatePackageAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPackageById), new { id = package.Id }, package);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdateBoostPackageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var package = await _boostService.UpdatePackageAsync(id, request, cancellationToken);
            return Ok(package);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePackage(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _boostService.DeletePackageAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
