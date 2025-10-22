using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Service.API.Services;

namespace Robot.ED.FacebookConnector.Service.API.BackgroundServices;

public class RpaAllocationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OrchestratorSettings _settings;
    private readonly ILogger<RpaAllocationBackgroundService> _logger;

    public RpaAllocationBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<OrchestratorSettings> settings,
        ILogger<RpaAllocationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RPA Allocation Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var allocationService = scope.ServiceProvider.GetRequiredService<IRpaAllocationService>();
                
                await allocationService.AllocateRpaAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RPA allocation background service");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.AllocationIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("RPA Allocation Background Service stopped");
    }
}
