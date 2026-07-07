using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;
using EntitiesOtpType = Swaply.Domain.Entities.OtpType;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class OtpRepository : IOtpRepository
{
    private readonly SwaplyDbContext _context;

    public OtpRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<OtpCode?> GetByEmailAsync(string email, EntitiesOtpType type)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _context.OtpCodes
            .Where(o => o.Email == normalizedEmail && o.Type == type && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<OtpCode?> GetValidOtpAsync(string email, string code, EntitiesOtpType type)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        var otp = await _context.OtpCodes
            .Where(o => o.Email == normalizedEmail && o.Code == code && o.Type == type && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        return otp?.IsValid == true ? otp : null;
    }

    public async Task AddAsync(OtpCode otpCode)
    {
        await _context.OtpCodes.AddAsync(otpCode);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidatePreviousOtpsAsync(string email, EntitiesOtpType type)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        var previousOtps = await _context.OtpCodes
            .Where(o => o.Email == normalizedEmail && o.Type == type && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in previousOtps)
        {
            otp.MarkAsUsed();
        }

        await _context.SaveChangesAsync();
    }
}
