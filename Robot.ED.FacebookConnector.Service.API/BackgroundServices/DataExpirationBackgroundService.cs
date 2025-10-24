using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.API.BackgroundServices;

public class DataExpirationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OrchestratorSettings _settings;
    private readonly ILogger<DataExpirationBackgroundService> _logger;

    public DataExpirationBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<OrchestratorSettings> settings,
        ILogger<DataExpirationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Expiration Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Run once per day
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var expirationDate = DateTime.UtcNow.AddDays(-_settings.DataRetentionDays);

                // Delete old queue results
                var oldResults = await dbContext.QueueResults
                    .Where(qr => qr.ReceivedAt < expirationDate)
                    .ToListAsync(stoppingToken);

                if (oldResults.Any())
                {
                    dbContext.QueueResults.RemoveRange(oldResults);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Deleted {Count} old queue results", oldResults.Count);
                }

                // Delete old processed queues
                var oldQueues = await dbContext.Queues
                    .Where(q => q.IsProcessed && q.UpdatedAt < expirationDate)
                    .ToListAsync(stoppingToken);

                if (oldQueues.Any())
                {
                    dbContext.Queues.RemoveRange(oldQueues);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Deleted {Count} old queue items", oldQueues.Count);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in data expiration background service");
            }
        }

        _logger.LogInformation("Data Expiration Background Service stopped");
    }
}
