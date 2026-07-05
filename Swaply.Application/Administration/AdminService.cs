using Swaply.Domain.Repositories;

namespace Swaply.Application.Administration;

public class AdminService : IAdminService
{
    private readonly IListingRepository _listingRepository;

    public AdminService(IListingRepository listingRepository)
    {
        _listingRepository = listingRepository;
    }

    public Task<bool> BanUserAsync(string userId, string reason, CancellationToken cancellationToken = default)
    {
        // Simulated ban action
        Console.WriteLine($"[Admin] User {userId} has been banned. Reason: {reason}");
        return Task.FromResult(true);
    }

    public async Task<bool> ModerateListingAsync(Guid listingId, bool approve, string feedback, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null) return false;

        if (!approve)
        {
            listing.Deactivate();
            await _listingRepository.UpdateAsync(listing, cancellationToken);
            Console.WriteLine($"[Admin] Listing {listingId} rejected. Feedback: {feedback}");
        }
        else
        {
            Console.WriteLine($"[Admin] Listing {listingId} approved.");
        }

        return true;
    }
}
