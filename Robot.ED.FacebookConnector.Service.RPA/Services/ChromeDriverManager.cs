using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.RPA.Services;

public class ChromeDriverManager : IChromeDriverManager, IDisposable
{
    private readonly RpaSettings _settings;
    private readonly ILogger<ChromeDriverManager> _logger;
    private IWebDriver? _driver;
    private readonly SemaphoreSlim _driverLock = new(1, 1);
    private bool _disposed;

    public ChromeDriverManager(
        IOptions<RpaSettings> settings,
        ILogger<ChromeDriverManager> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public IWebDriver? GetDriver()
    {
        return _driver;
    }

    public async Task InitializeDriverAsync()
    {
        await _driverLock.WaitAsync();
        try
        {
            if (_driver != null)
            {
                _logger.LogInformation("ChromeDriver already initialized");
                return;
            }

            _logger.LogInformation("Initializing ChromeDriver...");

            // Configure Chrome options - NO HEADLESS MODE
            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--start-maximized");

            // Create driver
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(_settings.ProcessTimeoutMinutes);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            _logger.LogInformation("ChromeDriver initialized successfully");

            // Open Facebook website after initialization
            await OpenFacebookWebsiteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ChromeDriver");
            DisposeDriverInternal();
            throw;
        }
        finally
        {
            _driverLock.Release();
        }
    }

    public async Task<IWebDriver> EnsureDriverAsync()
    {
        if (_driver != null)
        {
            try
            {
                // Test if driver is still alive by checking the window handle
                var _ = _driver.CurrentWindowHandle;
                return _driver;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Driver is not responsive, reinitializing...");
                DisposeDriverInternal();
            }
        }

        // Driver is not available, initialize it
        await InitializeDriverAsync();
        
        if (_driver == null)
        {
            throw new InvalidOperationException("Failed to initialize ChromeDriver");
        }

        return _driver;
    }

    public void DisposeDriver()
    {
        _driverLock.Wait();
        try
        {
            DisposeDriverInternal();
        }
        finally
        {
            _driverLock.Release();
        }
    }

    private void DisposeDriverInternal()
    {
        if (_driver != null)
        {
            try
            {
                _logger.LogInformation("Disposing ChromeDriver");
                _driver.Quit();
                _driver.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing ChromeDriver");
            }
            finally
            {
                _driver = null;
            }
        }
    }

    public async Task OpenFacebookWebsiteAsync()
    {
        try
        {
            if (_driver == null)
            {
                _logger.LogWarning("Cannot open Facebook website - driver is null");
                return;
            }

            _logger.LogInformation("Opening Facebook website - navigating to https://www.facebook.com");
            _driver.Navigate().GoToUrl("https://www.facebook.com");

            // Wait for page to load
            await Task.Delay(3000);

            _logger.LogInformation("Facebook website opened successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Facebook website");
            // Don't throw - failure to open Facebook shouldn't prevent driver initialization
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeDriver();
        _driverLock.Dispose();
        _disposed = true;
    }
}
