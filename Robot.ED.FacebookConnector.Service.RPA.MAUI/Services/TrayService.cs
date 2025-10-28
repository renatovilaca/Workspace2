using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using System;

namespace Robot.ED.FacebookConnector.Service.RPA.MAUI.Services;

public class TrayService
{
    private Window? _mainWindow;
    private TrayIcon? _trayIcon;

    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;

        // Create tray icon
        _trayIcon = new TrayIcon
        {
            ToolTipText = "RPA Facebook Connector",
            IsVisible = true
        };

        // Create tray menu
        var menu = new NativeMenu();
        
        var showItem = new NativeMenuItem { Header = "Mostrar" };
        showItem.Click += (s, e) => ShowWindow();
        
        var hideItem = new NativeMenuItem { Header = "Ocultar" };
        hideItem.Click += (s, e) => HideWindow();
        
        var separator = new NativeMenuItemSeparator();
        
        var exitItem = new NativeMenuItem { Header = "Sair" };
        exitItem.Click += (s, e) => ExitApplication();

        menu.Items.Add(showItem);
        menu.Items.Add(hideItem);
        menu.Items.Add(separator);
        menu.Items.Add(exitItem);

        _trayIcon.Menu = menu;
        _trayIcon.Clicked += (s, e) => ShowWindow();

        // Position window at bottom-right on first show
        if (_mainWindow != null)
        {
            _mainWindow.Opened += (s, e) => PositionWindowBottomRight();
        }
    }

    public void ShowWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();
            PositionWindowBottomRight();
        }
    }

    public void HideWindow()
    {
        _mainWindow?.Hide();
    }

    private void PositionWindowBottomRight()
    {
        if (_mainWindow?.Screens?.Primary != null)
        {
            var screen = _mainWindow.Screens.Primary;
            var workingArea = screen.WorkingArea;
            
            var windowWidth = _mainWindow.Width;
            var windowHeight = _mainWindow.Height;
            
            // Position at bottom-right with 20px margin
            var x = workingArea.Right - windowWidth - 20;
            var y = workingArea.Bottom - windowHeight - 20;
            
            _mainWindow.Position = new Avalonia.PixelPoint((int)x, (int)y);
        }
    }

    private void ExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }

    public void Cleanup()
    {
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
        }
    }
}
