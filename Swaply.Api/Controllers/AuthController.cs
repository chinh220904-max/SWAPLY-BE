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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var (success, userId, error) = await _identityService.RegisterAsync(model.Email, model.Username, model.Password);
        if (!success)
        {
            return BadRequest(new { error });
        }
        return Ok(new { userId, message = "Registration successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var (success, token, error) = await _identityService.LoginAsync(model.Email, model.Password);
        if (!success)
        {
            return BadRequest(new { error });
        }
        return Ok(new { token });
    }
}

public record RegisterModel(string Email, string Username, string Password);
public record LoginModel(string Email, string Password);
