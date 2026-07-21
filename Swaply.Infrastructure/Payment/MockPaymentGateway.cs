using Microsoft.AspNetCore.Http;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Infrastructure.Payments;

/// <summary>
/// Mock payment gateway for testing/demo purposes.
/// Simulates instant successful payment without real payment provider.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly IPaymentRepository _paymentRepository;

    public MockPaymentGateway(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public Task<string> CreatePaymentAsync(Payment payment, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // Trả về URL trang mock thanh toán của chính API (không cần provider ngoài)
        var payUrl = $"http://localhost:5191/api/Payments/mock-pay?ref={payment.Id:N}";
        payment.SetPayUrl(payUrl);
        return Task.FromResult(payUrl);
    }

    public async Task<bool> HandleReturnAsync(IQueryCollection query, CancellationToken cancellationToken = default)
    {
        var refId = query["ref"].ToString();
        if (string.IsNullOrEmpty(refId))
            return false;

        // Parse lại từ string form (id dạng "N" - 32 hex chars không có dash)
        if (!TryParseHexGuid(refId, out var paymentId))
            return false;

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null || payment.Status != Domain.Enums.PaymentStatus.Pending)
            return false;

        payment.MarkAsPaid(paymentId.ToString("N"), $"MOCK-{Guid.NewGuid():N}");
        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        return true;
    }

    public async Task<bool> HandleIpnAsync(IQueryCollection query, CancellationToken cancellationToken = default)
    {
        // Mock: IPN đồng nghĩa với return - cùng đánh dấu paid
        return await HandleReturnAsync(query, cancellationToken);
    }

    private static bool TryParseHexGuid(string hex, out Guid guid)
    {
        // Chấp nhận cả dạng "N" (32 hex) lẫn dạng chuẩn
        if (Guid.TryParse(hex, out guid))
            return true;

        if (hex.Length == 32)
        {
            try
            {
                guid = Guid.ParseExact(hex, "N");
                return true;
            }
            catch
            {
                guid = Guid.Empty;
                return false;
            }
        }

        guid = Guid.Empty;
        return false;
    }
}
