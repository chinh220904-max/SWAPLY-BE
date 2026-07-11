using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ExchangeManagement;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExchangesController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangesController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    private Guid GetRequesterId()
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRequest request)
    {
        var proposerId = GetRequesterId();
        if (proposerId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchange = await _exchangeService.CreateExchangeAsync(request, proposerId);
        return CreatedAtAction(nameof(GetById), new { id = exchange.Id }, exchange);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchange = await _exchangeService.GetExchangeByIdAsync(id, requesterId);
        if (exchange == null) return NotFound();
        return Ok(exchange);
    }

    [HttpGet("my")]
    public async Task<IActionResult> My()
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchanges = await _exchangeService.GetMyExchangesAsync(requesterId);
        return Ok(exchanges);
    }

    [HttpGet("incoming")]
    public async Task<IActionResult> Incoming()
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchanges = await _exchangeService.GetIncomingExchangesAsync(requesterId);
        return Ok(exchanges);
    }

    [HttpPut("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchange = await _exchangeService.AcceptExchangeAsync(id, requesterId);
        return Ok(exchange);
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchange = await _exchangeService.RejectExchangeAsync(id, requesterId);
        return Ok(exchange);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchange = await _exchangeService.CancelExchangeAsync(id, requesterId);
        return Ok(exchange);
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var requesterId = GetRequesterId();
        if (requesterId == Guid.Empty)
            return Unauthorized(new { error = "Invalid or missing user id in token." });

        var exchange = await _exchangeService.CompleteExchangeAsync(id, requesterId);
        return Ok(exchange);
    }
}
