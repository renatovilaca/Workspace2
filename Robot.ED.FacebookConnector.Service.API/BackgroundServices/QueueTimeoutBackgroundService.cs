using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.API.BackgroundServices;

public class QueueTimeoutBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OrchestratorSettings _settings;
    private readonly ILogger<QueueTimeoutBackgroundService> _logger;

    public QueueTimeoutBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<OrchestratorSettings> settings,
        ILogger<QueueTimeoutBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Timeout Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var timeoutThreshold = DateTime.UtcNow.AddMinutes(-_settings.QueueTimeoutMinutes);
                
                var timedOutQueues = await dbContext.Queues
                    .Where(q => !q.IsProcessed 
                        && q.AllocatedRobotId != null 
                        && q.UpdatedAt != null 
                        && q.UpdatedAt < timeoutThreshold)
                    .ToListAsync(stoppingToken);

                if (timedOutQueues.Any())
                {
                    foreach (var queue in timedOutQueues)
                    {
                        _logger.LogWarning(
                            "Queue {QueueId} timed out after {Minutes} minutes with robot {RobotId}. Deallocating robot.",
                            queue.Id, _settings.QueueTimeoutMinutes, queue.AllocatedRobotId);
                        
                        queue.AllocatedRobotId = null;
                        queue.UpdatedAt = DateTime.UtcNow;
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Deallocated {Count} timed-out queue items", timedOutQueues.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in queue timeout background service");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.QueueTimeoutCheckIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Queue Timeout Background Service stopped");
    }
}
