using Microsoft.AspNetCore.Mvc;
using Swaply.Application.Authentication;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// Step 1: Request OTP - sends OTP to email for registration
    /// </summary>
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpModel model)
    {
        var (success, error) = await _identityService.RequestOtpAsync(model.Email);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "OTP sent to your email. Please verify to complete registration." });
    }

    /// <summary>
    /// Step 2: Register with full info + OTP verification
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (model.Password != model.ConfirmPassword)
            return BadRequest(new { error = "Password and confirm password do not match." });

        var (success, token, error) = await _identityService.RegisterAsync(
            model.Email, model.Username, model.Password, model.PhoneNumber, model.FullName, model.Role, model.OtpCode);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { token, message = "Registration successful!" });
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var (success, token, error) = await _identityService.LoginAsync(model.Username, model.Password);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { token });
    }

    /// <summary>
    /// Request password reset - sends OTP to email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        var (success, error) = await _identityService.SendPasswordResetOtpAsync(model.Email);

        // Always return success for security (don't reveal if email exists)
        return Ok(new { message = "If the email exists, a reset code has been sent." });
    }

    /// <summary>
    /// Reset password with OTP
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var (success, error) = await _identityService.ResetPasswordAsync(
            model.Email, model.OtpCode, model.NewPassword);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Password reset successful!" });
    }
}

public record RequestOtpModel(string Email);
public record RegisterModel(string Email, string Username, string Password, string ConfirmPassword, string? PhoneNumber, string? FullName, string? Role, string OtpCode);
public record LoginModel(string Username, string Password);
public record ForgotPasswordModel(string Email);
public record ResetPasswordModel(string Email, string OtpCode, string NewPassword);
