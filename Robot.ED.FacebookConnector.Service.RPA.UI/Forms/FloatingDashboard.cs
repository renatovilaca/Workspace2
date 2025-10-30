using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace Robot.ED.FacebookConnector.Service.RPA.UI.Forms;

public class FloatingDashboard : Form
{
    private readonly BlazorWebView _blazorWebView;

    public FloatingDashboard(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _blazorWebView = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = serviceProvider
        };

#if DEBUG
        _blazorWebView.BlazorWebViewInitialized += (sender, args) =>
        {
            args.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        };
#endif

        _blazorWebView.RootComponents.Add<Components.Dashboard>("#app");
        
        this.Controls.Add(_blazorWebView);
    }

    private void InitializeComponent()
    {
        this.Text = "Facebook Connector RPA Dashboard";
        this.Size = new Size(400, 600);
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.ShowInTaskbar = true;
        this.TopMost = true;
        this.BackColor = Color.FromArgb(30, 30, 30);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _blazorWebView?.Dispose();
        }
        base.Dispose(disposing);
    }
}
