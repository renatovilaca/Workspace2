namespace Robot.ED.FacebookConnector.Common.Configuration;

public class RpaSettings
{
    public int ProcessTimeoutMinutes { get; set; } = 10;
    public int DataRetentionDays { get; set; } = 90;
    public string ScreenshotPath { get; set; } = "screenshots";
    public string OrchestratorUrl { get; set; } = "http://localhost:5000";
    public string OrchestratorToken { get; set; } = string.Empty;
    public string FacebookUsername { get; set; } = "dummymarvel@gmail.com";
    public string FacebookPassword { get; set; } = "Deadpool@1932";
}
