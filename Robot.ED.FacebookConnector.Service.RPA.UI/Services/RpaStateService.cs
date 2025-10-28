namespace Robot.ED.FacebookConnector.Service.RPA.UI.Services;

public enum RpaProcessState
{
    Stopped,
    Running,
    Paused
}

public class RpaStateService
{
    private RpaProcessState _state = RpaProcessState.Stopped;
    private DateTime? _currentCycleStartTime;
    private bool _lastExecutionSuccess = true;
    private string _lastExecutionMessage = string.Empty;

    public event EventHandler? StateChanged;

    public RpaProcessState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnStateChanged();
            }
        }
    }

    public DateTime? CurrentCycleStartTime
    {
        get => _currentCycleStartTime;
        set
        {
            _currentCycleStartTime = value;
            OnStateChanged();
        }
    }

    public bool LastExecutionSuccess
    {
        get => _lastExecutionSuccess;
        set
        {
            _lastExecutionSuccess = value;
            OnStateChanged();
        }
    }

    public string LastExecutionMessage
    {
        get => _lastExecutionMessage;
        set
        {
            _lastExecutionMessage = value;
            OnStateChanged();
        }
    }

    public TimeSpan? GetCurrentCycleElapsedTime()
    {
        if (_currentCycleStartTime.HasValue && _state == RpaProcessState.Running)
        {
            return DateTime.Now - _currentCycleStartTime.Value;
        }
        return null;
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
