using Swaply.Application.Authentication;

namespace Swaply.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    public Task<(bool Success, string Token, string Error)> LoginAsync(string email, string password)
    {
        if (email == "admin@swaply.com" && password == "Admin@123")
        {
            return Task.FromResult((true, "mock-jwt-token-for-admin", string.Empty));
        }

        if (email.Contains("@") && password.Length >= 6)
        {
            return Task.FromResult((true, "mock-jwt-token-for-user", string.Empty));
        }

        return Task.FromResult((false, string.Empty, "Invalid credentials. Password must be at least 6 characters."));
    }

    public Task<(bool Success, string UserId, string Error)> RegisterAsync(string email, string username, string password)
    {
        if (!email.Contains("@"))
        {
            return Task.FromResult((false, string.Empty, "Invalid email address."));
        }

        if (password.Length < 6)
        {
            return Task.FromResult((false, string.Empty, "Password must be at least 6 characters."));
        }

        return Task.FromResult((true, Guid.NewGuid().ToString(), string.Empty));
    }
}
