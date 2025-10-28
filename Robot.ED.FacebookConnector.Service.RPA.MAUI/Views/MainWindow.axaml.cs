using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Robot.ED.FacebookConnector.Service.RPA.MAUI.ViewModels;
using System;

namespace Robot.ED.FacebookConnector.Service.RPA.MAUI.Views;

public partial class MainWindow : Window
{
    private Border? _statusBadge;
    private Border? _executionStatusBorder;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        _statusBadge = this.FindControl<Border>("StatusBadge");
        _executionStatusBorder = this.FindControl<Border>("ExecutionStatusBorder");
        
        DataContextChanged += OnDataContextChanged;
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
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.ExecutionStatusClass))
                {
                    UpdateExecutionStatusClass(viewModel.ExecutionStatusClass);
                }
            };
            
            // Initialize classes
            UpdateStatusBadgeClass(viewModel.StatusClass);
            UpdateExecutionStatusClass(viewModel.ExecutionStatusClass);
        }
    }

    private void UpdateStatusBadgeClass(string statusClass)
    {
        if (_statusBadge != null)
        {
            _statusBadge.Classes.Clear();
            if (!string.IsNullOrEmpty(statusClass))
            {
                _statusBadge.Classes.Add(statusClass);
            }
        }
    }

    private void UpdateExecutionStatusClass(string executionStatusClass)
    {
        if (_executionStatusBorder != null)
        {
            _executionStatusBorder.Classes.Clear();
            if (!string.IsNullOrEmpty(executionStatusClass))
            {
                _executionStatusBorder.Classes.Add(executionStatusClass);
            }
        }
    }
}
