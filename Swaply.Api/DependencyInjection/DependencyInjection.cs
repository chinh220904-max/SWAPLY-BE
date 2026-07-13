using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swaply.Api.Services;
using Swaply.Application.Administration;
using Swaply.Application.AdminManagement;
using Swaply.Application.AdminManagement.FluentValidation;
using Swaply.Application.Authentication;
using Swaply.Application.ChatManagement;
using Swaply.Application.ConversationManagement;
using Swaply.Application.ExchangeManagement;
using Swaply.Application.ListingManagement;
using Swaply.Application.NotificationManagement;
using Swaply.Application.PremiumManagement;
using Swaply.Application.ReportManagement;
using Swaply.Application.ReviewManagement;
using FluentValidation;
using Swaply.Domain.DomainServices;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Cloudinary;
using Swaply.Infrastructure.Email;
using Swaply.Infrastructure.Identity;
using Swaply.Infrastructure.Payment;
using Swaply.Infrastructure.Persistence;
using Swaply.Infrastructure.RepositoryImplementation;

namespace Swaply.Api.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IExchangeDomainService, ExchangeDomainService>();

        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IExchangeService, ExchangeService>();
        services.AddScoped<IPremiumService, PremiumService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<Swaply.Application.NotificationManagement.INotificationService, Swaply.Application.NotificationManagement.NotificationService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IValidator<LockUserRequest>, LockUserValidator>();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=.\\SQLEXPRESS;Database=SwaplyDb;Trusted_Connection=True;TrustServerCertificate=True";
        // Persistence — real EF Core DbContext
        services.AddDbContext<SwaplyDbContext>(options =>
        {
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Swaply.Api"));
        });

        // Repositories
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IListingImageRepository, ListingImageRepository>();
        services.AddScoped<IExchangeRepository, ExchangeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        // Identity Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var jwtSettings = new JwtSettings
            {
                SecretKey = configuration["Jwt:SecretKey"] ?? "SwaplySuperSecretKey12345678901234567890",
                Issuer = configuration["Jwt:Issuer"] ?? "Swaply",
                Audience = configuration["Jwt:Audience"] ?? "Swaply",
                ExpirationDays = int.Parse(configuration["Jwt:ExpirationDays"] ?? "7")
            };
            return new TokenService(jwtSettings);
        });

        // Email Service
        services.Configure<GmailOptions>(configuration.GetSection(GmailOptions.SectionName));
        services.AddScoped<IEmailService, EmailService>();

        // Payment
        services.AddScoped<IPaymentProcessor, StripePaymentProcessor>();

        // Notification (SignalR) - implements ChatManagement.INotificationService and NotificationManagement.IRealTimeNotificationService
        services.AddScoped<global::Swaply.Application.ChatManagement.INotificationService, Swaply.Api.Services.SignalRNotificationService>();
        services.AddScoped<Swaply.Application.NotificationManagement.IRealTimeNotificationService, Swaply.Api.Services.SignalRNotificationService>();

        // Image upload (Cloudinary)
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.AddScoped<IImageUploadService, CloudinaryImageService>();

        return services;
    }
}
