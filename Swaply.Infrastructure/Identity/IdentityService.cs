using Swaply.Application.Authentication;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public IdentityService(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IEmailService emailService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string Error)> RequestOtpAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return (false, "Invalid email address.");

        if (await _userRepository.ExistsByEmailAsync(email))
            return (false, "Email already registered.");

        // Generate OTP
        var otpCode = GenerateOtpCode();

        // Invalidate previous OTPs for this email
        await _otpRepository.InvalidatePreviousOtpsAsync(email, OtpType.EmailVerification);

        // Save OTP to database
        var otp = new OtpCode(email, otpCode, OtpType.EmailVerification);
        await _otpRepository.AddAsync(otp);

        // Send OTP via email
        await _emailService.SendOtpAsync(email, otpCode, "EmailVerification");

        return (true, "OTP sent to your email.");
    }

    public async Task<(bool Success, string Token, string Error)> RegisterAsync(
        string email, string username, string password, string? phoneNumber, string? fullName, string? role, string otpCode)
    {
        // Validate OTP
        var validOtp = await _otpRepository.GetValidOtpAsync(email, otpCode, OtpType.EmailVerification);
        if (validOtp == null)
            return (false, string.Empty, "Invalid or expired OTP code.");

        // Validate inputs
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return (false, string.Empty, "Invalid email address.");

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, string.Empty, "Username must be at least 3 characters.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, string.Empty, "Password must be at least 6 characters.");

        if (await _userRepository.ExistsByEmailAsync(email))
            return (false, string.Empty, "Email already registered.");

        if (await _userRepository.ExistsByUsernameAsync(username))
            return (false, string.Empty, "Username already taken.");

        // Hash password
        var passwordHash = HashPassword(password);

        // Get role - use provided role or default to "Member"
        Guid roleId;
        if (!string.IsNullOrWhiteSpace(role))
        {
            var specifiedRole = await _userRepository.GetRoleByNameAsync(role);
            if (specifiedRole == null)
                return (false, string.Empty, $"Role '{role}' does not exist.");
            roleId = specifiedRole.Id;
        }
        else
        {
            var memberRole = await _userRepository.GetRoleByNameAsync("Member");
            roleId = memberRole?.Id ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        }

        // Create user with full name
        var user = new User(email, username, passwordHash, phoneNumber, roleId, fullName);
        await _userRepository.AddAsync(user);

        // Mark OTP as used
        validOtp.MarkAsUsed();

        // Generate token for immediate login
        var roleName = role ?? "Member";
        var token = _tokenService.GenerateToken(user.Id, user.Email, roleName);

        return (true, token, string.Empty);
    }

    public async Task<(bool Success, string Token, string Error)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, string.Empty, "Username is required.");

        if (string.IsNullOrWhiteSpace(password))
            return (false, string.Empty, "Password is required.");

        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null)
            return (false, string.Empty, "Invalid username or password.");

        if (user.IsBanned)
            return (false, string.Empty, "Your account has been banned.");

        // Verify password
        if (!VerifyPassword(password, user.PasswordHash))
            return (false, string.Empty, "Invalid username or password.");

        // Get role
        var role = user.Role?.Name ?? "User";

        // Generate JWT token
        var token = _tokenService.GenerateToken(user.Id, user.Email, role);

        return (true, token, string.Empty);
    }

    public async Task<(bool Success, string Error)> SendPasswordResetOtpAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return (false, "If the email exists, a reset code has been sent.");

        var otpCode = GenerateOtpCode();

        await _otpRepository.InvalidatePreviousOtpsAsync(email, OtpType.PasswordReset);

        var otp = new OtpCode(email, otpCode, OtpType.PasswordReset);
        await _otpRepository.AddAsync(otp);

        await _emailService.SendOtpAsync(email, otpCode, "PasswordReset");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ResetPasswordAsync(string email, string otpCode, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "Password must be at least 6 characters.");

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return (false, "User not found.");

        var validOtp = await _otpRepository.GetValidOtpAsync(email, otpCode, OtpType.PasswordReset);
        if (validOtp == null)
            return (false, "Invalid or expired OTP code.");

        user.UpdatePassword(HashPassword(newPassword));
        await _userRepository.UpdateAsync(user);

        validOtp.MarkAsUsed();

        return (true, string.Empty);
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static string HashPassword(string password)
    {
        // Simple hash for demo - in production use BCrypt
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hash;
    }
}
