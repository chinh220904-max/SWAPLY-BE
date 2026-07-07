namespace Swaply.Application.Authentication;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string role);
}
