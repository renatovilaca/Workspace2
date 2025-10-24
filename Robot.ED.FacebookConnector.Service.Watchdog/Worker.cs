using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Service.Watchdog.Configuration;
using Robot.ED.FacebookConnector.Service.Watchdog.Services;

namespace Robot.ED.FacebookConnector.Service.Watchdog;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly WatchdogSettings _watchdogSettings;
    private readonly IProcessMonitoringService _processMonitoringService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly Dictionary<string, bool> _applicationStates = new();

    public Worker(
        ILogger<Worker> logger,
        IOptions<WatchdogSettings> watchdogSettings,
        IProcessMonitoringService processMonitoringService,
        IEmailNotificationService emailNotificationService)
    {
        _logger = logger;
        _watchdogSettings = watchdogSettings.Value;
        _processMonitoringService = processMonitoringService;
        _emailNotificationService = emailNotificationService;

        // Initialize application states
        foreach (var app in _watchdogSettings.Applications)
        {
            _applicationStates[app.Name] = false;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Watchdog service started. Monitoring {Count} application(s)", 
            _watchdogSettings.Applications.Count);

        // Log monitored applications
        foreach (var app in _watchdogSettings.Applications)
        {
            _logger.LogInformation("Monitoring: {ApplicationName} (Process: {ProcessName})", 
                app.Name, app.ProcessName);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRestartApplicationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during watchdog check cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_watchdogSettings.CheckIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Watchdog service stopping");
    }

    private async Task CheckAndRestartApplicationsAsync(CancellationToken cancellationToken)
    {
        foreach (var application in _watchdogSettings.Applications)
        {
            try
            {
                var isRunning = _processMonitoringService.IsProcessRunning(application);
                var wasRunning = _applicationStates[application.Name];

                if (!isRunning)
                {
                    _logger.LogWarning("Application {ApplicationName} is not running", application.Name);

                    // Only send notification if it was previously running (to avoid spam on first start)
                    if (wasRunning)
                    {
                        await _emailNotificationService.SendApplicationStoppedNotificationAsync(
                            application.Name, cancellationToken);
                    }

                    // Attempt to start the application
                    var process = _processMonitoringService.StartProcess(application);

                    if (process != null)
                    {
                        _logger.LogInformation("Application {ApplicationName} restarted successfully", 
                            application.Name);
                        
                        await _emailNotificationService.SendApplicationRestartedNotificationAsync(
                            application.Name, cancellationToken);
                        
                        _applicationStates[application.Name] = true;
                    }
                    else
                    {
                        _logger.LogError("Failed to restart application {ApplicationName}", 
                            application.Name);
                        _applicationStates[application.Name] = false;
                    }
                }
                else
                {
                    // Update state to running
                    _applicationStates[application.Name] = true;
                    _logger.LogDebug("Application {ApplicationName} is running normally", application.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking/restarting application {ApplicationName}", 
                    application.Name);
            }
        }
    }
}
