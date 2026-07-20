using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Options;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.Payments;

public class VNPayPaymentGateway : IPaymentGateway
{
    private readonly VNPayOptions _options;
    private readonly SwaplyDbContext _dbContext;

    public VNPayPaymentGateway(IOptions<VNPayOptions> options, SwaplyDbContext dbContext)
    {
        _options = options.Value;
        _dbContext = dbContext;
    }

    public Task<string> CreatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        payment.SetIpAddress("127.0.0.1");
        payment.SetPayUrl(BuildPayUrl(payment.Id.ToString("N"), ConvertAmount(payment.Amount.Amount), payment.OrderInfo, payment.Id.ToString("N")));

        return Task.FromResult(payment.PayUrl);
    }

    public Task<bool> HandleReturnAsync(IQueryCollection query, CancellationToken cancellationToken = default)
    {
        var queryParams = query.Where(kv => !string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        var hashInput = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var expectedHash = BuildSecureHash(hashInput);

        var paymentId = Guid.Parse(query["vnp_TxnRef"].ToString()!);
        var providerTransactionId = query["vnp_TransactionNo"].ToString()!;
        var responseCode = query["vnp_ResponseCode"].ToString()!;

        var payment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment == null)
        {
            return Task.FromResult(false);
        }

        payment.SetReturnQuery(hashInput);

        if (expectedHash == query["vnp_SecureHash"].ToString() && responseCode == "00")
        {
            payment.MarkAsPaid(payment.Id.ToString("N"), providerTransactionId);
        }
        else
        {
            payment.MarkAsFailed();
        }

        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    public Task<bool> HandleIpnAsync(IQueryCollection query, CancellationToken cancellationToken = default)
    {
        var queryParams = query.Where(kv => !string.Equals(kv.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        var hashInput = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var expectedHash = BuildSecureHash(hashInput);

        var paymentId = Guid.Parse(query["vnp_TxnRef"].ToString()!);
        var providerTransactionId = query["vnp_TransactionNo"].ToString()!;
        var responseCode = query["vnp_ResponseCode"].ToString()!;

        var payment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment == null)
        {
            return Task.FromResult(false);
        }

        payment.SetIpnQuery(hashInput);

        if (expectedHash == query["vnp_SecureHash"].ToString() && responseCode == "00")
        {
            payment.MarkAsPaid(payment.Id.ToString("N"), providerTransactionId);
        }
        else
        {
            payment.MarkAsFailed();
        }

        _dbContext.SaveChanges();
        return Task.FromResult(true);
    }

    private string BuildPayUrl(string txnRef, string amount, string orderInfo, string tmnOrderInfo)
    {
        var query = new Dictionary<string, string>
        {
            ["vnp_Version"] = _options.Version,
            ["vnp_Command"] = _options.Command,
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = amount,
            ["vnp_CurrCode"] = _options.Currency,
            ["vnp_TxnRef"] = txnRef,
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = _options.Locale,
            ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_IpnUrl"] = _options.IpnUrl,
            ["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
        };

        var ordered = query.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase);
        var hashInput = string.Join("&", ordered.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var secureHash = BuildSecureHash(hashInput);

        var url = new StringBuilder(_options.PaymentUrl);
        url.Append("?");
        url.Append(string.Join("&", ordered.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}")));
        url.Append("&vnp_SecureHash=");
        url.Append(secureHash);

        return url.ToString();
    }

    private string ConvertAmount(decimal amount)
    {
        return Convert.ToInt64(amount * 100).ToString();
    }

    private string BuildSecureHash(string input)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_options.HashSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
