using System;
using Swaply.Domain.Entities;

namespace Swaply.Application.AdminManagement;

public record AdminUserListItemResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Status,
    bool IsPremium,
    DateTime CreatedAt
);

public record AdminUserDetailResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Status,
    bool IsPremium,
    DateTime CreatedAt,
    int ListingCount,
    int CompletedExchanges,
    double AverageRating,
    int ReviewCount,
    int ReportCount
);
