using Swaply.Application.BoostManagement;

namespace Swaply.Api.BackgroundServices;

public class AutoBoostBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoBoostBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public AutoBoostBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AutoBoostBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoBoost Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBoostCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoBoost cycle");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("AutoBoost Background Service stopped");
    }

    private async Task ProcessBoostCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var boostService = scope.ServiceProvider.GetRequiredService<IBoostService>();

        await boostService.ProcessAutoBoostAsync(cancellationToken);
    }
}
