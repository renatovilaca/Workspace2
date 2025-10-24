namespace Robot.ED.FacebookConnector.Service.Watchdog.Configuration;

public class WatchdogSettings
{
    public int CheckIntervalSeconds { get; set; } = 60;
    public List<MonitoredApplication> Applications { get; set; } = new();
}

public class MonitoredApplication
{
    public string Name { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
}
