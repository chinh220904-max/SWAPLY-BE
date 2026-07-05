using Swaply.Api.DependencyInjection;
using Swaply.Api.Hubs;
using Swaply.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add API Controllers and SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Configure OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

// Map HTTP Controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
