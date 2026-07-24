using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swaply.Api.BackgroundServices;
using Swaply.Api.DependencyInjection;
using Swaply.Api.Hubs;
using Swaply.Api.Middleware;
using Swaply.Application.BoostManagement;
using Swaply.Infrastructure.Persistence;

// Clear default claim type mappings to preserve original claim types
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Add API Controllers and SignalR
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

// ==========================================
// CẤU HÌNH CORS CHO PHÉP FRONTEND TRUY CẬP
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Cho phép cả cổng 3000 và 5173 truy cập
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Cần thiết cho SignalR
    });
});
// ==========================================

// Register Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register Boost Management
builder.Services.AddScoped<IBoostService, BoostService>();

// Register Background Services
builder.Services.AddHostedService<AutoBoostBackgroundService>();

// Configure JWT Authentication
var jwtSettings = new Swaply.Infrastructure.Identity.JwtSettings
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? "SwaplySuperSecretKey12345678901234567890",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "Swaply",
    Audience = builder.Configuration["Jwt:Audience"] ?? "Swaply",
    ExpirationDays = int.Parse(builder.Configuration["Jwt:ExpirationDays"] ?? "7")
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;  // Save token for debugging
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    // Log authentication events for debugging
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("OnMessageReceived - Token exists: {Token}",
context.Request.Headers["Authorization"].FirstOrDefault()?.Substring(0, 50));
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT Authentication failed. Type: {Type}, Message: {Message}",
                context.Exception.GetType().Name, context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT Token validated. User: {User}, Claims: {Claims}",
                context.Principal?.Identity?.Name,
                context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").Aggregate((a, b) => $"{a}, {b}"));
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge. Error: {Error}, Desc: {Desc}, AuthScheme: {Scheme}",
                context.Error, context.ErrorDescription, context.AuthenticateFailure?.GetType().Name);
            return Task.CompletedTask;
        }
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT AUTH FAILED: {Error}", context.Exception.GetType().Name);
            logger.LogError("JWT AUTH FAILED Message: {Message}", context.Exception.Message);
            logger.LogError("JWT AUTH FAILED Inner: {Inner}", context.Exception.InnerException?.Message ?? "none");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT CHALLENGE: Error={Error}, ErrorDescription={Desc}", context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// Configure OpenAPI/Swagger with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Swaply API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
Console.WriteLine(app.Environment.EnvironmentName);

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SwaplyDbContext>();
                                    await context.Database.MigrateAsync();

    // Seed default roles if not exist
    if (!await context.Roles.AnyAsync())
    {
        var roles = new[]
        {
            new Swaply.Domain.Entities.Role(Guid.NewGuid(), "Admin", "Full system access"),
            new Swaply.Domain.Entities.Role(Guid.NewGuid(), "Member", "Standard user access"),
        };
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
        Console.WriteLine("Default roles seeded.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// === KÍCH HOẠT CORS POLICY ===
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();