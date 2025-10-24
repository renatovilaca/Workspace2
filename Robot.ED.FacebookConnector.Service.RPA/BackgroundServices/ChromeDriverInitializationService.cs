using Robot.ED.FacebookConnector.Service.RPA.Services;

namespace Robot.ED.FacebookConnector.Service.RPA.BackgroundServices;

public class ChromeDriverInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChromeDriverInitializationService> _logger;

    public ChromeDriverInitializationService(
        IServiceProvider serviceProvider,
        ILogger<ChromeDriverInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChromeDriver Initialization Service started");

        // Wait a bit for the application to fully start
        await Task.Delay(2000, stoppingToken);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var chromeDriverManager = scope.ServiceProvider.GetRequiredService<IChromeDriverManager>();

            _logger.LogInformation("Initializing ChromeDriver on application boot...");
            await chromeDriverManager.InitializeDriverAsync();
            _logger.LogInformation("ChromeDriver initialized successfully on boot");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ChromeDriver on boot");
        }

        // This service only runs once at startup
        _logger.LogInformation("ChromeDriver Initialization Service completed");
    }
}
