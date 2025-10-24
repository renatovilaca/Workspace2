using System.Diagnostics;
using Robot.ED.FacebookConnector.Service.Watchdog.Configuration;

namespace Robot.ED.FacebookConnector.Service.Watchdog.Services;

public interface IProcessMonitoringService
{
    bool IsProcessRunning(MonitoredApplication application);
    Process? StartProcess(MonitoredApplication application);
}

public class ProcessMonitoringService : IProcessMonitoringService
{
    private readonly ILogger<ProcessMonitoringService> _logger;

    public ProcessMonitoringService(ILogger<ProcessMonitoringService> logger)
    {
        _logger = logger;
    }

    public bool IsProcessRunning(MonitoredApplication application)
    {
        try
        {
            var processes = Process.GetProcessesByName(application.ProcessName);
            var isRunning = processes.Length > 0;
            
            if (isRunning)
            {
                _logger.LogDebug("Process {ProcessName} is running (PID: {ProcessIds})", 
                    application.ProcessName, 
                    string.Join(", ", processes.Select(p => p.Id)));
            }
            
            return isRunning;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if process {ProcessName} is running", application.ProcessName);
            return false;
        }
    }

    public Process? StartProcess(MonitoredApplication application)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = application.ExecutablePath,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            if (!string.IsNullOrEmpty(application.Arguments))
            {
                startInfo.Arguments = application.Arguments;
            }

            if (!string.IsNullOrEmpty(application.WorkingDirectory))
            {
                startInfo.WorkingDirectory = application.WorkingDirectory;
            }

            _logger.LogInformation("Starting process {ProcessName} from {ExecutablePath}", 
                application.ProcessName, 
                application.ExecutablePath);

            var process = Process.Start(startInfo);
            
            if (process != null)
            {
                _logger.LogInformation("Successfully started process {ProcessName} (PID: {ProcessId})", 
                    application.ProcessName, 
                    process.Id);
                return process;
            }
            else
            {
                _logger.LogError("Failed to start process {ProcessName} - Process.Start returned null", 
                    application.ProcessName);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process {ProcessName} from {ExecutablePath}", 
                application.ProcessName, 
                application.ExecutablePath);
            return null;
        }
    }
}
