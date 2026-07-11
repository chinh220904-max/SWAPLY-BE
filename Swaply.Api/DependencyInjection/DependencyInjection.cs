using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swaply.Application.Administration;
using Swaply.Application.Authentication;
using Swaply.Application.ChatManagement;
using Swaply.Application.ConversationManagement;
using Swaply.Application.ExchangeManagement;
using Swaply.Application.ListingManagement;
using Swaply.Application.PremiumManagement;
using Swaply.Domain.DomainServices;
using Swaply.Domain.Repositories;
using Swaply.Infrastructure.Cloudinary;
using Swaply.Infrastructure.Email;
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
        services.AddScoped<IExchangeDomainService, ExchangeDomainService>();

        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IExchangeService, ExchangeService>();
        services.AddScoped<IPremiumService, PremiumService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IConversationService, ConversationService>();

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
        services.AddScoped<IExchangeRepository, ExchangeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

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

        // Notification (SignalR)
        services.AddScoped<INotificationService, SignalRNotificationService>();

        // Image upload (Cloudinary)
        services.AddScoped<IImageUploadService, CloudinaryImageService>();

        return services;
    }
}
