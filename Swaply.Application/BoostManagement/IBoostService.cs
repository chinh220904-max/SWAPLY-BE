namespace Swaply.Application.BoostManagement;

public interface IBoostService
{
    Task<BoostPackageDto> CreatePackageAsync(CreateBoostPackageRequest request, CancellationToken cancellationToken = default);
    Task<BoostPackageDto> UpdatePackageAsync(Guid id, UpdateBoostPackageRequest request, CancellationToken cancellationToken = default);
    Task DeletePackageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BoostPackageDto>> GetAllPackagesAsync(CancellationToken cancellationToken = default);
    Task<BoostPackageDto?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SubscriptionResultDto> SubscribeAsync(Guid userId, Guid packageId, CancellationToken cancellationToken = default);
    Task<SubscriptionResultDto?> GetCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<QuotaStatusDto> GetQuotaStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanCreateListingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UseQuotaAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ProcessAutoBoostAsync(CancellationToken cancellationToken = default);
}

public record CreateBoostPackageRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    int MaxListings,
    int Priority,
    List<GoldenHourDto> GoldenHours
);

public record UpdateBoostPackageRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    int MaxListings,
    int Priority,
    bool IsActive,
    List<GoldenHourDto> GoldenHours
);

public record GoldenHourDto(TimeOnly StartTime, TimeOnly EndTime);

public record BoostPackageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    int MaxListings,
    int Priority,
    bool IsActive,
    List<GoldenHourDto> GoldenHours,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SubscriptionResultDto(
    Guid SubscriptionId,
    Guid PackageId,
    string PackageName,
    int TotalQuota,
    int UsedQuota,
    int RemainingQuota,
    DateTime StartedAt,
    DateTime ExpiresAt
);

public record QuotaStatusDto(
    bool HasActiveBoost,
    int BoostRemainingQuota,
    DateTime? BoostExpiresAt,
    int MonthlyRemainingQuota,
    bool CanCreateListing
);
