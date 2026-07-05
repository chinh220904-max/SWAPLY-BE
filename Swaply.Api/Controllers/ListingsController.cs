using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ListingManagement;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveListings()
    {
        var listings = await _listingService.GetActiveListingsAsync();
        return Ok(listings);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetListingById(Guid id)
    {
        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null)
        {
            return NotFound();
        }
        return Ok(listing);
    }

    [HttpPost]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        var listing = await _listingService.CreateListingAsync(request);
        return CreatedAtAction(nameof(GetListingById), new { id = listing.Id }, listing);
    }
}
