namespace Swaply.Application.Authentication;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otpCode, string purpose);
}
