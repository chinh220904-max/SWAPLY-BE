using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.BoostManagement;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Repositories;
using Swaply.Domain.ValueObjects;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IBoostService _boostService;
    private readonly IBoostSubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IWebHostEnvironment _env;

    public PaymentsController(
        IBoostService boostService,
        IBoostSubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IWebHostEnvironment env)
    {
        _boostService = boostService;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _env = env;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyPayments(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var payments = await _paymentRepository.GetByUserIdAsync(userId, cancellationToken);

            var result = payments.Select(p => new
            {
                p.Id,
                p.TransactionId,
                p.ProviderTransactionId,
                p.OrderInfo,
                amount = p.Amount.Amount,
                currency = p.Amount.Currency,
                p.Status,
                p.Method,
                p.CreatedAt,
                p.PaidAt,
                p.ExpiresAt
            });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetPayment(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

            if (payment == null)
                return NotFound(new { error = "Payment not found." });

            if (payment.UserId != userId)
                return Forbid();

            return Ok(new
            {
                payment.Id,
                payment.TransactionId,
                payment.ProviderTransactionId,
                payment.OrderInfo,
                amount = payment.Amount.Amount,
                currency = payment.Amount.Currency,
                payment.Status,
                payment.Method,
                payment.CreatedAt,
                payment.PaidAt,
                payment.ExpiresAt,
                payment.PayUrl
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> CancelPayment(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

            if (payment == null)
                return NotFound(new { error = "Payment not found." });

            if (payment.UserId != userId)
                return Forbid();

            if (payment.Status != PaymentStatus.Pending)
                return BadRequest(new { error = "Only pending payments can be cancelled." });

            payment.Cancel();
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            var subscription = await _subscriptionRepository.GetByIdAsync(payment.SubscriptionId, cancellationToken);
            if (subscription != null && subscription.IsActive())
            {
                subscription.Cancel();
                await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            }

            return Ok(new { message = "Payment cancelled successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tạo payment pending cho 1 boost package. Trả về payUrl là trang mock thanh toán nội bộ.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CheckoutPaymentRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (request == null || !request.PackageId.HasValue)
                return BadRequest(new { error = "PackageId is required." });

            var package = await _boostService.GetPackageByIdAsync(request.PackageId.Value, cancellationToken);
            if (package == null)
                return NotFound(new { error = "Package not found." });

            var result = await _boostService.SubscribeAsync(userId, request.PackageId.Value, cancellationToken);

            var money = new Money(package.Price, "VND");
            var payment = new Payment(
                userId,
                result.SubscriptionId,
                money,
                PaymentMethod.Mock,
                $"Subscribe to {package.Name} - {package.DurationDays} days",
                result.ExpiresAt
            );
            await _paymentRepository.AddAsync(payment, cancellationToken);

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var payUrl = await _paymentGateway.CreatePaymentAsync(payment, clientIp, cancellationToken);
            payment.SetPayUrl(payUrl);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            return Ok(new
            {
                subscriptionId = result.SubscriptionId,
                paymentId = payment.Id,
                payUrl = payment.PayUrl,
                amount = payment.Amount.Amount,
                currency = payment.Amount.Currency,
                orderInfo = payment.OrderInfo,
                expiresAt = payment.ExpiresAt
            });
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

    /// <summary>
    /// Trang HTML giả lập cổng thanh toán - chỉ dành cho dev/test.
    /// </summary>
    [HttpGet("mock-pay")]
    [AllowAnonymous]
    public async Task<IActionResult> MockPayPage([FromQuery(Name = "ref")] string? paymentRef, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(paymentRef))
            return Content("<h1>Missing ref query parameter</h1>", "text/html");

        if (!TryParseHexGuid(paymentRef, out var paymentId))
            return Content("<h1>Invalid ref</h1>", "text/html");

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Content("<h1>Payment not found</h1>", "text/html");

        var htmlPath = Path.Combine(_env.WebRootPath ?? "", "mock-pay.html");
        if (!System.IO.File.Exists(htmlPath))
            return Content("<h1>Mock page template not found</h1>", "text/html");

        var html = await System.IO.File.ReadAllTextAsync(htmlPath, cancellationToken);
        html = html
            .Replace("{{PAYMENT_ID}}", payment.Id.ToString())
            .Replace("{{ORDER_INFO}}", payment.OrderInfo)
            .Replace("{{AMOUNT}}", payment.Amount.Amount.ToString("N0"))
            .Replace("{{CURRENCY}}", payment.Amount.Currency)
            .Replace("{{STATUS}}", payment.Status.ToString())
            .Replace("{{PAYMENT_REF}}", paymentRef);

        return Content(html, "text/html");
    }

    /// <summary>API nội bộ: xác nhận thanh toán mock thành công.</summary>
    [HttpPost("mock-pay/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> MockPayConfirm([FromQuery(Name = "ref")] string? paymentRef, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(paymentRef))
            return BadRequest(new { error = "Missing ref" });
        if (!TryParseHexGuid(paymentRef, out var paymentId))
            return BadRequest(new { error = "Invalid ref" });

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return NotFound(new { error = "Payment not found" });

        if (payment.Status != PaymentStatus.Pending)
            return Ok(new { status = payment.Status.ToString(), message = "Already processed" });

        payment.MarkAsPaid(payment.Id.ToString("N"), $"MOCK-{Guid.NewGuid():N}");
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        return Ok(new { status = payment.Status.ToString() });
    }

    /// <summary>API nội bộ: huỷ thanh toán mock.</summary>
    [HttpPost("mock-pay/cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> MockPayCancel([FromQuery(Name = "ref")] string? paymentRef, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(paymentRef))
            return BadRequest(new { error = "Missing ref" });
        if (!TryParseHexGuid(paymentRef, out var paymentId))
            return BadRequest(new { error = "Invalid ref" });

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return NotFound(new { error = "Payment not found" });

        if (payment.Status != PaymentStatus.Pending)
            return Ok(new { status = payment.Status.ToString(), message = "Already processed" });

        payment.MarkAsFailed();
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        return Ok(new { status = payment.Status.ToString() });
    }

    /// <summary>Frontend redirect sau khi thanh toán.</summary>
    [HttpGet("return")]
    [AllowAnonymous]
    public IActionResult PaymentReturn([FromQuery(Name = "ref")] string? paymentRef)
    {
        var redirect = $"http://localhost:3000/payment/result?ref={paymentRef}";
        return Redirect(redirect);
    }

    [HttpPost("ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentIpn([FromQuery] Microsoft.AspNetCore.Http.IQueryCollection query, CancellationToken cancellationToken)
    {
        var success = await _paymentGateway.HandleIpnAsync(query, cancellationToken);
        return success
            ? Ok(new { code = "00", message = "Success" })
            : Ok(new { code = "99", message = "Unknown error" });
    }

    private static bool TryParseHexGuid(string? hex, out Guid guid)
    {
        guid = Guid.Empty;
        if (string.IsNullOrEmpty(hex)) return false;
        if (Guid.TryParse(hex, out guid)) return true;
        if (hex.Length == 32)
        {
            try { guid = Guid.ParseExact(hex, "N"); return true; }
            catch { return false; }
        }
        return false;
    }
}

public record CheckoutPaymentRequest(Guid? PackageId);
