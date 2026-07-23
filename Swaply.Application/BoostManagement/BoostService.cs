using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Application.BoostManagement;

public class BoostService : IBoostService
{
    private readonly IBoostPackageRepository _packageRepository;
    private readonly IBoostPackageGoldenHourRepository _goldenHourRepository;
    private readonly IBoostSubscriptionRepository _subscriptionRepository;
    private readonly IBoostHistoryRepository _historyRepository;
    private readonly IUserMonthlyQuotaRepository _quotaRepository;
    private readonly IListingRepository _listingRepository;

    public BoostService(
        IBoostPackageRepository packageRepository,
        IBoostPackageGoldenHourRepository goldenHourRepository,
        IBoostSubscriptionRepository subscriptionRepository,
        IBoostHistoryRepository historyRepository,
        IUserMonthlyQuotaRepository quotaRepository,
        IListingRepository listingRepository)
    {
        _packageRepository = packageRepository;
        _goldenHourRepository = goldenHourRepository;
        _subscriptionRepository = subscriptionRepository;
        _historyRepository = historyRepository;
        _quotaRepository = quotaRepository;
        _listingRepository = listingRepository;
    }

    public async Task<BoostPackageDto> CreatePackageAsync(CreateBoostPackageRequest request, CancellationToken cancellationToken = default)
    {
        var package = new BoostPackage(
            request.Name,
            request.Description,
            request.Price,
            request.DurationDays,
            request.MaxListings,
            request.Priority
        );

        await _packageRepository.AddAsync(package, cancellationToken);

        if (request.GoldenHours.Count > 0)
        {
            var goldenHours = request.GoldenHours
                .Select(gh => new BoostPackageGoldenHour(package.Id, gh.StartTime, gh.EndTime))
                .ToList();
            await _goldenHourRepository.AddRangeAsync(goldenHours, cancellationToken);
        }

        return await MapToDtoAsync(package, cancellationToken);
    }

    public async Task<BoostPackageDto> UpdatePackageAsync(Guid id, UpdateBoostPackageRequest request, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Package not found.");

        package.Update(
            request.Name,
            request.Description,
            request.Price,
            request.DurationDays,
            request.MaxListings,
            request.Priority,
            request.IsActive
        );

        await _packageRepository.UpdateAsync(package, cancellationToken);

        await _goldenHourRepository.DeleteByPackageIdAsync(id, cancellationToken);
        if (request.GoldenHours.Count > 0)
        {
            var goldenHours = request.GoldenHours
                .Select(gh => new BoostPackageGoldenHour(package.Id, gh.StartTime, gh.EndTime))
                .ToList();
            await _goldenHourRepository.AddRangeAsync(goldenHours, cancellationToken);
        }

        return await MapToDtoAsync(package, cancellationToken);
    }

    public async Task DeletePackageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Package not found.");

        package.Deactivate();
        await _packageRepository.UpdateAsync(package, cancellationToken);
    }

    public async Task<IReadOnlyList<BoostPackageDto>> GetAllPackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = await _packageRepository.GetAllAsync(cancellationToken);
        var result = new List<BoostPackageDto>();

        foreach (var package in packages)
        {
            result.Add(await MapToDtoAsync(package, cancellationToken));
        }

        return result;
    }

    public async Task<BoostPackageDto?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(id, cancellationToken);
        if (package == null)
            return null;

        return await MapToDtoAsync(package, cancellationToken);
    }

    public async Task<SubscriptionResultDto> SubscribeAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(packageId, cancellationToken)
            ?? throw new InvalidOperationException("Package not found.");

        if (!package.IsActive)
            throw new InvalidOperationException("Package is not available.");

        var subscription = new BoostSubscription(userId, package.Id, package.DurationDays, package.MaxListings);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        return new SubscriptionResultDto(
            subscription.Id,
            package.Id,
            package.Name,
            subscription.TotalQuota,
            subscription.UsedQuota,
            subscription.RemainingQuota(),
            subscription.StartedAt,
            subscription.ExpiresAt
        );
    }

    public async Task<SubscriptionResultDto?> GetCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (subscription == null)
            return null;

        var package = await _packageRepository.GetByIdAsync(subscription.BoostPackageId, cancellationToken);

        return new SubscriptionResultDto(
            subscription.Id,
            subscription.BoostPackageId,
            package?.Name ?? "Unknown",
            subscription.TotalQuota,
            subscription.UsedQuota,
            subscription.RemainingQuota(),
            subscription.StartedAt,
            subscription.ExpiresAt
        );
    }

    public async Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("No active subscription found.");

        subscription.Cancel();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
    }

    public async Task<QuotaStatusDto> GetQuotaStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var boostSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        var monthlyQuota = await _quotaRepository.GetCurrentByUserIdAsync(userId, cancellationToken);
        if (monthlyQuota == null)
        {
            monthlyQuota = UserMonthlyQuota.CreateForCurrentMonth(userId);
            await _quotaRepository.AddAsync(monthlyQuota, cancellationToken);
        }

        var hasActiveBoost = boostSubscription != null && boostSubscription.IsActive();
        var boostRemaining = boostSubscription?.RemainingQuota() ?? 0;

        return new QuotaStatusDto(
            hasActiveBoost,
            boostRemaining,
            boostSubscription?.ExpiresAt,
            monthlyQuota.RemainingQuota(),
            hasActiveBoost && boostRemaining > 0 || monthlyQuota.CanUseQuota()
        );
    }

    public async Task<bool> CanCreateListingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var status = await GetQuotaStatusAsync(userId, cancellationToken);
        return status.CanCreateListing;
    }

    public async Task UseQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var boostSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        if (boostSubscription != null && boostSubscription.IsActive())
        {
            if (boostSubscription.UseQuota())
            {
                await _subscriptionRepository.UpdateAsync(boostSubscription, cancellationToken);
                return;
            }
        }

        var monthlyQuota = await _quotaRepository.GetCurrentByUserIdAsync(userId, cancellationToken);
        if (monthlyQuota == null)
        {
            monthlyQuota = UserMonthlyQuota.CreateForCurrentMonth(userId);
            await _quotaRepository.AddAsync(monthlyQuota, cancellationToken);
        }

        if (!monthlyQuota.UseQuota())
            throw new InvalidOperationException("Monthly quota exceeded.");

        await _quotaRepository.UpdateAsync(monthlyQuota, cancellationToken);
    }

    public async Task ProcessAutoBoostAsync(CancellationToken cancellationToken = default)
    {
        var activePackages = await _packageRepository.GetActiveAsync(cancellationToken);

        foreach (var package in activePackages)
        {
            var goldenHours = await _goldenHourRepository.GetByPackageIdAsync(package.Id, cancellationToken);
            var now = TimeOnly.FromDateTime(DateTime.UtcNow);

            var activeGoldenHour = goldenHours.FirstOrDefault(gh => gh.IsActiveAt(now));
            if (activeGoldenHour == null)
                continue;

            var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(package.Id, cancellationToken);
            if (subscription == null || !subscription.IsActive())
                continue;

            var listings = await _listingRepository.GetByOwnerIdAsync(subscription.UserId, cancellationToken);
            var activeListings = listings.Where(l => l.Status == Domain.Enums.ListingStatus.Active).ToList();

            var latestBoost = await _historyRepository.GetLatestByListingIdAsync(activeListings.First().Id, cancellationToken);
            if (latestBoost != null && !latestBoost.IsExpired())
                continue;

            foreach (var listing in activeListings.Take(subscription.RemainingQuota()))
            {
                var expiresAt = DateTime.UtcNow.AddDays(1);
                var history = new BoostHistory(listing.Id, subscription.UserId, subscription.Id, package.Priority, expiresAt);
                await _historyRepository.AddAsync(history, cancellationToken);

                listing.SetBoost(subscription.Id, history.BoostedAt, expiresAt, package.Priority);
                await _listingRepository.UpdateAsync(listing, cancellationToken);
            }
        }
    }

    private async Task<BoostPackageDto> MapToDtoAsync(BoostPackage package, CancellationToken cancellationToken)
    {
        var goldenHours = await _goldenHourRepository.GetByPackageIdAsync(package.Id, cancellationToken);

        return new BoostPackageDto(
            package.Id,
            package.Name,
            package.Description,
            package.Price,
            package.DurationDays,
            package.MaxListings,
            package.Priority,
            package.IsActive,
            goldenHours.Select(gh => new GoldenHourDto(gh.StartTime, gh.EndTime)).ToList(),
            package.CreatedAt,
            package.UpdatedAt
        );
    }
}
