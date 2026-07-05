using Swaply.Application.Administration;
using Swaply.Application.Authentication;
using Swaply.Application.ChatManagement;
using Swaply.Application.ExchangeManagement;
using Swaply.Application.ListingManagement;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.DomainServices;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Cloudinary;
using Swaply.Infrastructure.Identity;
using Swaply.Infrastructure.Payment;
using Swaply.Infrastructure.Persistence;
using Swaply.Infrastructure.RepositoryImplementation;
using Swaply.Infrastructure.SignalR;

namespace Swaply.Api.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Domain services
        services.AddScoped<IExchangeDomainService, ExchangeDomainService>();

        // Application services
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IExchangeService, ExchangeService>();
        services.AddScoped<IPremiumService, PremiumService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Persistence
        services.AddSingleton<SwaplyDbContext>(); // Shared in-memory database mock
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IExchangeRepository, ExchangeRepository>();

        // Identity
        services.AddScoped<IIdentityService, IdentityService>();

        // Payment
        services.AddScoped<IPaymentProcessor, StripePaymentProcessor>();

        // Notification (SignalR)
        services.AddScoped<INotificationService, SignalRNotificationService>();

        // Image upload (Cloudinary)
        services.AddScoped<IImageUploadService, CloudinaryImageService>();

        return services;
    }
}
