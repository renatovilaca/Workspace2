using OpenQA.Selenium;

namespace Robot.ED.FacebookConnector.Service.RPA.Services;

public interface IChromeDriverManager
{
    /// <summary>
    /// Gets the current ChromeDriver instance. Returns null if not initialized.
    /// </summary>
    IWebDriver? GetDriver();

    /// <summary>
    /// Initializes the ChromeDriver with the specified options.
    /// </summary>
    Task InitializeDriverAsync();

    /// <summary>
    /// Ensures the driver is available, reinitializing if necessary.
    /// </summary>
    Task<IWebDriver> EnsureDriverAsync();

    /// <summary>
    /// Disposes the current driver instance.
    /// </summary>
    void DisposeDriver();

    /// <summary>
    /// Opens the Facebook website (https://www.facebook.com) in the browser.
    /// This method is called automatically during initialization and can be called manually if needed.
    /// No parameters required, no return value, and does not send data to orchestrator.
    /// </summary>
    Task OpenFacebookWebsiteAsync();
}
