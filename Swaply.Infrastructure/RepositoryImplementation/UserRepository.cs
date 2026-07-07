using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Persistence;

namespace Swaply.Infrastructure.RepositoryImplementation;

public class UserRepository : IUserRepository
{
    private readonly SwaplyDbContext _context;

    public UserRepository(SwaplyDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var normalizedUsername = username.Trim();
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == normalizedUsername);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        var normalizedUsername = username.Trim();
        return await _context.Users.AnyAsync(u => u.UserName == normalizedUsername);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
    }
}
