using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swaply.Api.DependencyInjection;
using Swaply.Api.Hubs;
using Swaply.Api.Middleware;
using Swaply.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add API Controllers and SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
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

app.UseHttpsRedirection();

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
