using System;
using System.Windows.Input;
using System.Reactive;
using ReactiveUI;
using Robot.ED.FacebookConnector.Service.RPA.MAUI.Services;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

namespace Robot.ED.FacebookConnector.Service.RPA.MAUI.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly RpaStateService _rpaState;
    private readonly TrayService _trayService;
    private string _statusText = "PARADO";
    private string _statusMessage = "Pronto para iniciar";
    private string _elapsedTime = "00:00:00.000";
    private string _statusClass = "Stopped";
    private string _executionStatusClass = "";

    public MainWindowViewModel(RpaStateService rpaState, TrayService trayService)
    {
        _rpaState = rpaState;
        _trayService = trayService;
        
        _rpaState.StateChanged += OnStateChanged;
        
        StartCommand = ReactiveCommand.Create(Start, this.WhenAnyValue(x => x.CanStart));
        PauseCommand = ReactiveCommand.Create(Pause, this.WhenAnyValue(x => x.CanPause));
        StopCommand = ReactiveCommand.Create(Stop, this.WhenAnyValue(x => x.CanStop));
        ExitCommand = ReactiveCommand.CreateFromTask(Exit);
        
        UpdateDisplay();
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public string ElapsedTime
    {
        get => _elapsedTime;
        set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
    }

    public string StatusClass
    {
        get => _statusClass;
        set => this.RaiseAndSetIfChanged(ref _statusClass, value);
    }

    public string ExecutionStatusClass
    {
        get => _executionStatusClass;
        set => this.RaiseAndSetIfChanged(ref _executionStatusClass, value);
    }

    public bool CanStart => _rpaState.State != RpaProcessState.Running;
    public bool CanPause => _rpaState.State == RpaProcessState.Running;
    public bool CanStop => _rpaState.State != RpaProcessState.Stopped;

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ExitCommand { get; }

    private void Start()
    {
        _rpaState.Start();
        // TODO: Implement actual RPA service call
    }

    private void Pause()
    {
        _rpaState.Pause();
        // TODO: Implement actual RPA service pause
    }

    private void Stop()
    {
        _rpaState.Stop();
        // TODO: Implement actual RPA service stop
    }

    private async System.Threading.Tasks.Task Exit()
    {
        var result = await ShowConfirmDialog("Confirmar Encerramento", "Deseja realmente encerrar a aplicação?");
        if (result)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }
    }

    private async System.Threading.Tasks.Task<bool> ShowConfirmDialog(string title, string message)
    {
        // This is a simplified version - in real app, use proper dialog
        // For now, we'll assume yes to continue
        return await System.Threading.Tasks.Task.FromResult(true);
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdateDisplay();
        this.RaisePropertyChanged(nameof(CanStart));
        this.RaisePropertyChanged(nameof(CanPause));
        this.RaisePropertyChanged(nameof(CanStop));
    }

    private void UpdateDisplay()
    {
        StatusText = _rpaState.State switch
        {
            RpaProcessState.Running => "EM EXECUÇÃO",
            RpaProcessState.Paused => "PAUSADO",
            RpaProcessState.Stopped => "PARADO",
            _ => "PARADO"
        };

        StatusClass = _rpaState.State switch
        {
            RpaProcessState.Running => "Running",
            RpaProcessState.Paused => "Paused",
            RpaProcessState.Stopped => "Stopped",
            _ => "Stopped"
        };

        StatusMessage = _rpaState.StatusMessage;

        var time = _rpaState.CurrentCycleElapsedTime;
        ElapsedTime = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";

        if (_rpaState.State == RpaProcessState.Running)
        {
            ExecutionStatusClass = "";
        }
        else
        {
            ExecutionStatusClass = _rpaState.LastExecutionSuccess ? "Success" : "Error";
        }
    }
}
