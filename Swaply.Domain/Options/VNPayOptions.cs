namespace Swaply.Domain.Options;

public class VNPayOptions
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string Currency { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
}
