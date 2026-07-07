namespace Swaply.Application.Authentication;

public interface IIdentityService
{
    // Step 1: Request OTP for registration - only sends OTP
    Task<(bool Success, string Error)> RequestOtpAsync(string email);

    // Step 2: Register with full info + OTP verification
    Task<(bool Success, string Token, string Error)> RegisterAsync(
        string email, string username, string password, string? phoneNumber, string? fullName, string? role, string otpCode);

    // Login - returns JWT token
    Task<(bool Success, string Token, string Error)> LoginAsync(string username, string password);

    // Password reset
    Task<(bool Success, string Error)> SendPasswordResetOtpAsync(string email);
    Task<(bool Success, string Error)> ResetPasswordAsync(string email, string otpCode, string newPassword);
}
