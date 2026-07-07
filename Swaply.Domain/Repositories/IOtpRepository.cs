using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface IOtpRepository
{
    Task<OtpCode?> GetByEmailAsync(string email, OtpType type);
    Task<OtpCode?> GetValidOtpAsync(string email, string code, OtpType type);
    Task AddAsync(OtpCode otpCode);
    Task InvalidatePreviousOtpsAsync(string email, OtpType type);
}
