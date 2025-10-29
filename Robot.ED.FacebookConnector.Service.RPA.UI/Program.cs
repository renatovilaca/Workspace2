using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Service.RPA.UI.Forms;
using Robot.ED.FacebookConnector.Service.RPA.UI.Services;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Robot.ED.FacebookConnector.Service.RPA.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/rpa-ui-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            Log.Information("Starting Robot.ED.FacebookConnector.Service.RPA.UI");

            ApplicationConfiguration.Initialize();

            var host = CreateHostBuilder().Build();

            // Start the host in the background
            _ = host.RunAsync();

            var mainForm = host.Services.GetRequiredService<MainForm>();
            Application.Run(mainForm);

            Log.Information("Application shutting down");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                    optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configure settings
                services.Configure<RpaSettings>(
                    context.Configuration.GetSection("RpaSettings"));

                // Configure DbContext
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(
                        context.Configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Service.RPA.UI")));

                // Register services
                services.AddHttpClient();
                services.AddSingleton<IChromeDriverManager, ChromeDriverManager>();
                services.AddSingleton<IRpaProcessingService, RpaProcessingService>();
                services.AddSingleton<IRpaApiService, RpaApiService>();
                services.AddSingleton<RpaStateService>();

                // Register forms
                services.AddTransient<MainForm>();

                // Add Blazor services
                services.AddWindowsFormsBlazorWebView();
                
                // Add logging
                services.AddLogging();
            });
    }
}
