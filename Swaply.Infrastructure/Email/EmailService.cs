using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Swaply.Application.Authentication;

namespace Swaply.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly GmailOptions _gmailOptions;

    public EmailService(ILogger<EmailService> logger, IOptions<GmailOptions> gmailOptions)
    {
        _logger = logger;
        _gmailOptions = gmailOptions.Value;
    }

    public async Task SendOtpAsync(string toEmail, string otpCode, string purpose)
    {
        var subject = purpose switch
        {
            "EmailVerification" => "Mã xác thực email - Swaply",
            "PasswordReset" => "Mã đặt lại mật khẩu - Swaply",
            _ => "Mã OTP - Swaply"
        };

        var body = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <h2 style='color: #333;'>Xin chào!</h2>
    <p>Mã xác thực của bạn là:</p>
    <div style='background: #f5f5f5; padding: 20px; text-align: center; font-size: 32px; letter-spacing: 8px; font-weight: bold; margin: 20px 0;'>
        {otpCode}
    </div>
    <p>Mã này có hiệu lực trong <strong>5 phút</strong>.</p>
    <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
    <p style='color: #666; font-size: 12px;'>Swaply - Nền tảng trao đổi đồ cũ</p>
</div>";

        // Log email info
        _logger.LogInformation("========== SENDING EMAIL ==========");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Purpose: {Purpose}", purpose);

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_gmailOptions.SenderName, _gmailOptions.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(
                _gmailOptions.SmtpHost,
                _gmailOptions.SmtpPort,
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(_gmailOptions.SenderEmail, _gmailOptions.SenderPassword);
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("✅ Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
