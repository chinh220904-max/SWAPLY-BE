using Swaply.Api.DependencyInjection;
using Swaply.Api.Hubs;
using Swaply.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add API Controllers and SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Register Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
Console.WriteLine(app.Environment.EnvironmentName);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
