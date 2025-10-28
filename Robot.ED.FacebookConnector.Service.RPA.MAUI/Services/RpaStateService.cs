using System;
using System.Timers;

namespace Robot.ED.FacebookConnector.Service.RPA.MAUI.Services;

public enum RpaProcessState
{
    Stopped,
    Running,
    Paused
}

public class RpaStateService
{
    private System.Timers.Timer? _executionTimer;
    private DateTime _cycleStartTime;
    
    public event EventHandler? StateChanged;

    public RpaProcessState State { get; private set; } = RpaProcessState.Stopped;
    public bool LastExecutionSuccess { get; private set; } = true;
    public TimeSpan CurrentCycleElapsedTime { get; private set; } = TimeSpan.Zero;
    public string StatusMessage { get; private set; } = "Pronto para iniciar";

    public void Start()
    {
        if (State == RpaProcessState.Running)
            return;

        State = RpaProcessState.Running;
        _cycleStartTime = DateTime.Now;
        StatusMessage = "Processando...";

        // Start timer to update elapsed time
        _executionTimer?.Stop();
        _executionTimer = new System.Timers.Timer(100); // Update every 100ms
        _executionTimer.Elapsed += UpdateElapsedTime;
        _executionTimer.Start();

        OnStateChanged();
    }

    public void Pause()
    {
        if (State != RpaProcessState.Running)
            return;

        State = RpaProcessState.Paused;
        StatusMessage = "Pausado";
        _executionTimer?.Stop();

        OnStateChanged();
    }

    public void Stop()
    {
        State = RpaProcessState.Stopped;
        StatusMessage = "Parado";
        CurrentCycleElapsedTime = TimeSpan.Zero;
        _executionTimer?.Stop();

        OnStateChanged();
    }

    public void SetExecutionResult(bool success, string message = "")
    {
        LastExecutionSuccess = success;
        if (!string.IsNullOrEmpty(message))
        {
            StatusMessage = message;
        }
        else
        {
            StatusMessage = success ? "Última execução: Sucesso" : "Última execução: Falha";
        }
        
        OnStateChanged();
    }

    private void UpdateElapsedTime(object? sender, ElapsedEventArgs e)
    {
        if (State == RpaProcessState.Running)
        {
            CurrentCycleElapsedTime = DateTime.Now - _cycleStartTime;
            OnStateChanged();
        }
    }

    public void ResetCycleTimer()
    {
        _cycleStartTime = DateTime.Now;
        CurrentCycleElapsedTime = TimeSpan.Zero;
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _executionTimer?.Stop();
        _executionTimer?.Dispose();
    }
}
