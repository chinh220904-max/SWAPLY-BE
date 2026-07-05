using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ExchangeManagement;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangesController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangesController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [HttpPost("propose")]
    public async Task<IActionResult> ProposeExchange([FromBody] ProposeExchangeModel model)
    {
        var exchange = await _exchangeService.ProposeExchangeAsync(model.ProposerListingId, model.ReceiverListingId);
        return Ok(exchange);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> AcceptExchange(Guid id)
    {
        var result = await _exchangeService.AcceptExchangeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Exchange accepted and completed successfully." });
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectExchange(Guid id)
    {
        var result = await _exchangeService.RejectExchangeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Exchange rejected." });
    }
}

public record ProposeExchangeModel(Guid ProposerListingId, Guid ReceiverListingId);
