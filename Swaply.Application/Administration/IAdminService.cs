namespace Swaply.Application.Administration;

public interface IAdminService
{
    Task<bool> BanUserAsync(string userId, string reason, CancellationToken cancellationToken = default);
    Task<bool> ModerateListingAsync(Guid listingId, bool approve, string feedback, CancellationToken cancellationToken = default);
}
