using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Robot.ED.FacebookConnector.Service.RPA.MAUI.ViewModels;
using System;
using Av = Avalonia;

namespace Robot.ED.FacebookConnector.Service.RPA.MAUI.Views;

public partial class MainWindow : Window
{
    private Border? _statusBadge;
    private Border? _executionStatusBorder;
    private TextBlock? _statusTextBlock;
    private TextBlock? _statusMessageBlock;
    private TextBlock? _timerBlock;
    private Button? _startButton;
    private Button? _pauseButton;
    private Button? _stopButton;

    public MainWindow()
    {
        Title = "RPA Facebook Connector";
        Width = 400;
        Height = 550;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Background = new SolidColorBrush(Color.Parse("#1e1e1e"));

        Content = CreateContent();
        
        DataContextChanged += OnDataContextChanged;
    }

    private Control CreateContent()
    {
        // Main Grid
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*")
        };

        // Header
        var header = CreateHeader();
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        // Content area
        var scrollViewer = new ScrollViewer
        {
            Content = CreateMainContent()
        };
        Grid.SetRow(scrollViewer, 1);
        grid.Children.Add(scrollViewer);

        return grid;
    }

    private Control CreateHeader()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#252526")),
            BorderBrush = new SolidColorBrush(Color.Parse("#007acc")),
            BorderThickness = new Av.Thickness(0, 0, 0, 2),
            Padding = new Av.Thickness(20)
        };

        var stack = new StackPanel { Spacing = 10 };

        var titleText = new TextBlock
        {
            Text = "RPA Facebook Connector",
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White
        };
        stack.Children.Add(titleText);

        _statusBadge = new Border
        {
            CornerRadius = new Av.CornerRadius(12),
            Padding = new Av.Thickness(16, 6),
            Background = new SolidColorBrush(Color.Parse("#858585"))
        };

        _statusTextBlock = new TextBlock
        {
            Text = "PARADO",
            FontSize = 12,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Black
        };
        _statusBadge.Child = _statusTextBlock;
        stack.Children.Add(_statusBadge);

        border.Child = stack;
        return border;
    }

    private Control CreateMainContent()
    {
        var stack = new StackPanel
        {
            Margin = new Av.Thickness(20),
            Spacing = 20
        };

        // Status Section
        stack.Children.Add(CreateStatusSection());

        // Timer Section
        stack.Children.Add(CreateTimerSection());

        // Control Buttons
        stack.Children.Add(CreateControlButtons());

        // Exit Button
        stack.Children.Add(CreateExitSection());

        return stack;
    }

    private Control CreateStatusSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#252526")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3f3f46")),
            BorderThickness = new Av.Thickness(1),
            CornerRadius = new Av.CornerRadius(8),
            Padding = new Av.Thickness(16)
        };

        var stack = new StackPanel { Spacing = 8 };

        var label = new TextBlock
        {
            Text = "STATUS DA EXECUÇÃO",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#858585"))
        };
        stack.Children.Add(label);

        _executionStatusBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#2d2d30")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3f3f46")),
            BorderThickness = new Av.Thickness(1),
            CornerRadius = new Av.CornerRadius(6),
            Padding = new Av.Thickness(12)
        };

        _statusMessageBlock = new TextBlock
        {
            Text = "Pronto para iniciar",
            FontSize = 14,
            FontWeight = FontWeight.Medium,
            Foreground = Brushes.White
        };
        _executionStatusBorder.Child = _statusMessageBlock;
        stack.Children.Add(_executionStatusBorder);

        border.Child = stack;
        return border;
    }

    private Control CreateTimerSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#252526")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3f3f46")),
            BorderThickness = new Av.Thickness(1),
            CornerRadius = new Av.CornerRadius(8),
            Padding = new Av.Thickness(16)
        };

        var stack = new StackPanel { Spacing = 8 };

        var label = new TextBlock
        {
            Text = "TEMPO DE EXECUÇÃO DO CICLO",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#858585"))
        };
        stack.Children.Add(label);

        var timerBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#2d2d30")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3f3f46")),
            BorderThickness = new Av.Thickness(1),
            CornerRadius = new Av.CornerRadius(6),
            Padding = new Av.Thickness(16)
        };

        _timerBlock = new TextBlock
        {
            Text = "00:00:00.000",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            FontFamily = new FontFamily("Courier New"),
            Foreground = new SolidColorBrush(Color.Parse("#007acc")),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        timerBorder.Child = _timerBlock;
        stack.Children.Add(timerBorder);

        border.Child = stack;
        return border;
    }

    private Control CreateControlButtons()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,12,*,12,*")
        };

        // Start Button
        _startButton = CreateGradientButton("▶ Iniciar", "#4ec9b0", "#3da58a");
        _startButton.Click += (s, e) => (DataContext as MainWindowViewModel)?.StartCommand.Execute(null);
        Grid.SetColumn(_startButton, 0);
        grid.Children.Add(_startButton);

        // Pause Button
        _pauseButton = CreateGradientButton("⏸ Pausar", "#f0ad4e", "#d89a3e");
        _pauseButton.Click += (s, e) => (DataContext as MainWindowViewModel)?.PauseCommand.Execute(null);
        Grid.SetColumn(_pauseButton, 2);
        grid.Children.Add(_pauseButton);

        // Stop Button
        _stopButton = CreateGradientButton("⏹ Parar", "#f48771", "#e06751");
        _stopButton.Click += (s, e) => (DataContext as MainWindowViewModel)?.StopCommand.Execute(null);
        Grid.SetColumn(_stopButton, 4);
        grid.Children.Add(_stopButton);

        return grid;
    }

    private Button CreateGradientButton(string text, string color1, string color2)
    {
        var button = new Button
        {
            Content = text,
            Foreground = text.Contains("Parar") || text.Contains("⏹") ? Brushes.White : Brushes.Black,
            BorderThickness = new Av.Thickness(0),
            CornerRadius = new Av.CornerRadius(6),
            Padding = new Av.Thickness(14, 10),
            FontWeight = FontWeight.SemiBold,
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = new LinearGradientBrush
            {
                StartPoint = new Av.RelativePoint(0, 0, Av.RelativeUnit.Relative),
                EndPoint = new Av.RelativePoint(1, 1, Av.RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop { Color = Color.Parse(color1), Offset = 0 },
                    new GradientStop { Color = Color.Parse(color2), Offset = 1 }
                }
            }
        };
        return button;
    }

    private Control CreateExitSection()
    {
        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#3f3f46")),
            BorderThickness = new Av.Thickness(0, 1, 0, 0),
            Padding = new Av.Thickness(0, 20, 0, 0)
        };

        var exitButton = CreateGradientButton("✕ Encerrar Aplicação", "#c72e2e", "#a71e1e");
        exitButton.Foreground = Brushes.White;
        exitButton.Click += (s, e) => { ((DataContext as MainWindowViewModel)!.ExitCommand as System.Windows.Input.ICommand)?.Execute(null); };
        exitButton.HorizontalAlignment = HorizontalAlignment.Stretch;

        border.Child = exitButton;
        return border;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.StatusClass))
                {
                    UpdateStatusBadgeClass(viewModel.StatusClass);
                    UpdateStatusText(viewModel.StatusText);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.StatusText))
                {
                    UpdateStatusText(viewModel.StatusText);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.ExecutionStatusClass))
                {
                    UpdateExecutionStatusClass(viewModel.ExecutionStatusClass);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.StatusMessage))
                {
                    UpdateStatusMessage(viewModel.StatusMessage);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.ElapsedTime))
                {
                    UpdateTimerDisplay(viewModel.ElapsedTime);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.CanStart))
                {
                    UpdateButtonStates(viewModel);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.CanPause))
                {
                    UpdateButtonStates(viewModel);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.CanStop))
                {
                    UpdateButtonStates(viewModel);
                }
            };
            
            // Initialize
            UpdateStatusBadgeClass(viewModel.StatusClass);
            UpdateStatusText(viewModel.StatusText);
            UpdateExecutionStatusClass(viewModel.ExecutionStatusClass);
            UpdateStatusMessage(viewModel.StatusMessage);
            UpdateTimerDisplay(viewModel.ElapsedTime);
            UpdateButtonStates(viewModel);
        }
    }

    private void UpdateStatusBadgeClass(string statusClass)
    {
        if (_statusBadge != null)
        {
            _statusBadge.Background = statusClass switch
            {
                "Running" => new SolidColorBrush(Color.Parse("#4ec9b0")),
                "Paused" => new SolidColorBrush(Color.Parse("#f0ad4e")),
                _ => new SolidColorBrush(Color.Parse("#858585"))
            };
        }
    }

    private void UpdateStatusText(string text)
    {
        if (_statusTextBlock != null)
        {
            _statusTextBlock.Text = text;
        }
    }

    private void UpdateExecutionStatusClass(string executionStatusClass)
    {
        if (_executionStatusBorder != null)
        {
            _executionStatusBorder.Background = executionStatusClass switch
            {
                "Success" => new SolidColorBrush(Color.Parse("#0e6f0e")),
                "Error" => new SolidColorBrush(Color.Parse("#c72e2e")),
                _ => new SolidColorBrush(Color.Parse("#2d2d30"))
            };

            _executionStatusBorder.BorderBrush = executionStatusClass switch
            {
                "Success" => new SolidColorBrush(Color.Parse("#4ec9b0")),
                "Error" => new SolidColorBrush(Color.Parse("#f48771")),
                _ => new SolidColorBrush(Color.Parse("#3f3f46"))
            };
        }
    }

    private void UpdateStatusMessage(string message)
    {
        if (_statusMessageBlock != null)
        {
            _statusMessageBlock.Text = message;
        }
    }

    private void UpdateTimerDisplay(string time)
    {
        if (_timerBlock != null)
        {
            _timerBlock.Text = time;
        }
    }

    private void UpdateButtonStates(MainWindowViewModel viewModel)
    {
        if (_startButton != null)
            _startButton.IsEnabled = viewModel.CanStart;
        
        if (_pauseButton != null)
            _pauseButton.IsEnabled = viewModel.CanPause;
        
        if (_stopButton != null)
            _stopButton.IsEnabled = viewModel.CanStop;
    }
}
