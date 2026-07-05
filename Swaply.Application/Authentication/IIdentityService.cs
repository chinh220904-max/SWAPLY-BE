namespace Swaply.Application.Authentication;

public interface IIdentityService
{
    Task<(bool Success, string Token, string Error)> LoginAsync(string email, string password);
    Task<(bool Success, string UserId, string Error)> RegisterAsync(string email, string username, string password);
}
