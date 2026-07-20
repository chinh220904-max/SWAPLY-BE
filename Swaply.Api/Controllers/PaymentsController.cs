using Microsoft.AspNetCore.Mvc;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.Repositories;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PremiumService _premiumService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentsController(PremiumService premiumService, IPaymentGateway paymentGateway, IPaymentRepository paymentRepository)
    {
        _premiumService = premiumService;
        _paymentGateway = paymentGateway;
        _paymentRepository = paymentRepository;
    }

    [HttpPost("vnpay")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> CreateVnPayPayment(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var payment = await _premiumService.StartUpgradeAsync(userId.ToString(), 99000m, cancellationToken);
            return Ok(new { payUrl = payment.PayUrl, paymentId = payment.Id });
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

    [HttpGet("vnpay/return")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> VnPayReturn([FromQuery] Microsoft.AspNetCore.Http.IQueryCollection query, CancellationToken cancellationToken)
    {
        await _paymentGateway.HandleReturnAsync(query, cancellationToken);
        var txnRef = query["vnp_TxnRef"].ToString();
        var responseCode = query["vnp_ResponseCode"].ToString();
        return Redirect($"https://localhost:3000/payment/result?txnRef={txnRef}&responseCode={responseCode}");
    }

    [HttpPost("vnpay/ipn")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> VnPayIpn([FromQuery] Microsoft.AspNetCore.Http.IQueryCollection query, CancellationToken cancellationToken)
    {
        await _paymentGateway.HandleIpnAsync(query, cancellationToken);
        return Ok(new { message = "IPN received" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        return userId;
    }
}
