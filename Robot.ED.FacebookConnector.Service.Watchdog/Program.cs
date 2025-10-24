using Robot.ED.FacebookConnector.Service.Watchdog;
using Robot.ED.FacebookConnector.Service.Watchdog.Configuration;
using Robot.ED.FacebookConnector.Service.Watchdog.Services;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/watchdog-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Robot.ED.FacebookConnector.Service.Watchdog");

    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    // Configure settings
    builder.Services.Configure<WatchdogSettings>(
        builder.Configuration.GetSection("WatchdogSettings"));
    builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection("EmailSettings"));

    // Register services
    builder.Services.AddSingleton<IProcessMonitoringService, ProcessMonitoringService>();
    builder.Services.AddSingleton<IEmailNotificationService, EmailNotificationService>();

    // Add the worker service
    builder.Services.AddHostedService<Worker>();

    // Enable Windows Service support if running on Windows
    if (OperatingSystem.IsWindows())
    {
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "Robot.ED.FacebookConnector.Watchdog";
        });
    }

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
