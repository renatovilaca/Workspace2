using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Robot.ED.FacebookConnector.Service.RPA.MAUI.ViewModels;
using Robot.ED.FacebookConnector.Service.RPA.MAUI.Views;
using Robot.ED.FacebookConnector.Service.RPA.MAUI.Services;

namespace Robot.ED.FacebookConnector.Service.RPA.MAUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var rpaStateService = new RpaStateService();
            var trayService = new TrayService();
            
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(rpaStateService, trayService),
            };

            desktop.MainWindow = mainWindow;
            trayService.Initialize(mainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
