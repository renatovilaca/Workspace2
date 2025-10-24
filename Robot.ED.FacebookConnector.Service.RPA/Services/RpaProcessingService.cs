using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.RPA.Services;

public class RpaProcessingService : IRpaProcessingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RpaSettings _settings;
    private readonly ILogger<RpaProcessingService> _logger;

    public RpaProcessingService(
        IHttpClientFactory httpClientFactory,
        IOptions<RpaSettings> settings,
        ILogger<RpaProcessingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ProcessAsync(ProcessRequestDto request)
    {
        var result = new RpaResultDto
        {
            TrackId = request.TrackId,
            Type = request.OriginType,
            MediaId = request.MediaId,
            Channel = request.Channel,
            Tag = request.Tags.FirstOrDefault(),
            HasError = false,
            Messages = new List<string>()
        };

        IWebDriver? driver = null;
        var screenshotTaken = false;

        try
        {
            _logger.LogInformation("Starting RPA processing for QueueId: {QueueId}", request.QueueId);

            // Ensure screenshots directory exists
            var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.ScreenshotPath);
            Directory.CreateDirectory(screenshotPath);

            // Configure Chrome options
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");

            // Create driver
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(_settings.ProcessTimeoutMinutes);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            _logger.LogInformation("Navigating to Facebook");
            driver.Navigate().GoToUrl("https://www.facebook.com");

            // Wait for page to load
            await Task.Delay(3000);

            // Find email field
            var emailField = driver.FindElement(By.Id("email"));
            emailField.SendKeys(_settings.FacebookUsername);

            // Find password field
            var passwordField = driver.FindElement(By.Id("pass"));
            passwordField.SendKeys(_settings.FacebookPassword);

            _logger.LogInformation("Credentials entered, attempting login");

            // Find and click login button
            var loginButton = driver.FindElement(By.Name("login"));
            loginButton.Click();

            // Wait for login to process
            await Task.Delay(5000);

            _logger.LogInformation("Login completed successfully");

            // Close browser
            driver.Quit();
            driver = null;

            result.HasError = false;
            result.Messages.Add("Facebook login automation completed successfully");
            
            _logger.LogInformation("RPA processing completed successfully for QueueId: {QueueId}", request.QueueId);
        }
        catch (NoSuchElementException ex)
        {
            _logger.LogError(ex, "Element not found during Facebook automation");
            result.HasError = true;
            result.ErrorMessage = $"Element not found: {ex.Message}";
            
            // Take screenshot
            if (driver != null)
            {
                await TakeScreenshotAsync(driver, request.QueueId, "element-not-found");
                screenshotTaken = true;
            }
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogError(ex, "Timeout during Facebook automation");
            result.HasError = true;
            result.ErrorMessage = $"Timeout: {ex.Message}";
            
            // Take screenshot
            if (driver != null)
            {
                await TakeScreenshotAsync(driver, request.QueueId, "timeout");
                screenshotTaken = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RPA processing");
            result.HasError = true;
            result.ErrorMessage = ex.Message;
            
            // Take screenshot
            if (driver != null && !screenshotTaken)
            {
                await TakeScreenshotAsync(driver, request.QueueId, "error");
            }
        }
        finally
        {
            // Ensure driver is closed
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                    driver.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing WebDriver");
                }
            }

            // Send result back to orchestrator
            await SendResultToOrchestratorAsync(result);

            // Memory cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private async Task TakeScreenshotAsync(IWebDriver driver, int queueId, string reason)
    {
        try
        {
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            var screenshotPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                _settings.ScreenshotPath,
                $"error-queue-{queueId}-{reason}-{DateTime.UtcNow:yyyyMMddHHmmss}.png"
            );

            screenshot.SaveAsFile(screenshotPath);
            _logger.LogInformation("Screenshot saved: {Path}", screenshotPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error taking screenshot");
        }
    }

    private async Task SendResultToOrchestratorAsync(RpaResultDto result)
    {
        try
        {
            var json = JsonSerializer.Serialize(result);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OrchestratorToken}");

            var url = $"{_settings.OrchestratorUrl.TrimEnd('/')}/api/rpa/result";
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Result sent successfully to orchestrator for TrackId: {TrackId}", result.TrackId);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send result to orchestrator. Status: {StatusCode}, Body: {Body}", 
                    response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending result to orchestrator");
        }
    }
}
