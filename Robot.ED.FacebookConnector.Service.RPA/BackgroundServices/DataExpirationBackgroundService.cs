using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.RPA.BackgroundServices;

public class DataExpirationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RpaSettings _settings;
    private readonly ILogger<DataExpirationBackgroundService> _logger;

    public DataExpirationBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<RpaSettings> settings,
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

                // Delete old screenshots
                var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.ScreenshotPath);
                if (Directory.Exists(screenshotPath))
                {
                    var expirationDate = DateTime.UtcNow.AddDays(-_settings.DataRetentionDays);
                    var files = Directory.GetFiles(screenshotPath, "*.png")
                        .Where(f => File.GetCreationTimeUtc(f) < expirationDate)
                        .ToList();

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    if (files.Any())
                    {
                        _logger.LogInformation("Deleted {Count} old screenshot files", files.Count);
                    }
                }

                // Delete old log files if any
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (Directory.Exists(logPath))
                {
                    var expirationDate = DateTime.UtcNow.AddDays(-_settings.DataRetentionDays);
                    var files = Directory.GetFiles(logPath, "*.log")
                        .Where(f => File.GetCreationTimeUtc(f) < expirationDate)
                        .ToList();

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    if (files.Any())
                    {
                        _logger.LogInformation("Deleted {Count} old log files", files.Count);
                    }
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
