using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Robot.ED.FacebookConnector.Service.RPA.UI.Services;

namespace Robot.ED.FacebookConnector.Service.RPA.UI.Forms;

public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRpaApiService _apiService;
    private readonly RpaStateService _stateService;
    private NotifyIcon? _trayIcon;
    private Form? _floatingWindow;
    private bool _isClosing = false;

    public MainForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _apiService = serviceProvider.GetRequiredService<IRpaApiService>();
        _stateService = serviceProvider.GetRequiredService<RpaStateService>();

        InitializeComponent();
        InitializeTrayIcon();
        
        // Hide the main form - we only use the tray icon
        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;
        this.Visible = false;
    }

    private void InitializeComponent()
    {
        this.Text = "Robot.ED.FacebookConnector.Service.RPA.UI";
        this.Size = new Size(1, 1);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(-10000, -10000);
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Text = "Facebook Connector RPA",
            Visible = true,
            ContextMenuStrip = CreateContextMenu()
        };

        // Create a simple icon (you can replace with a proper icon file)
        using (var bitmap = new Bitmap(16, 16))
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.DarkBlue);
            g.FillEllipse(Brushes.White, 3, 3, 10, 10);
            _trayIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
        }

        _trayIcon.Click += TrayIcon_Click;
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();
        
        var showItem = new ToolStripMenuItem("Show Dashboard");
        showItem.Click += (s, e) => ShowFloatingWindow();
        menu.Items.Add(showItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApplication();
        menu.Items.Add(exitItem);

        return menu;
    }

    private void TrayIcon_Click(object? sender, EventArgs e)
    {
        if (e is MouseEventArgs mouseEvent && mouseEvent.Button == MouseButtons.Left)
        {
            ShowFloatingWindow();
        }
    }

    private void ShowFloatingWindow()
    {
        if (_floatingWindow != null && !_floatingWindow.IsDisposed)
        {
            _floatingWindow.Activate();
            return;
        }

        _floatingWindow = new FloatingDashboard(_serviceProvider);
        
        // Position in bottom-right corner
        var screen = Screen.PrimaryScreen;
        if (screen != null)
        {
            var workingArea = screen.WorkingArea;
            _floatingWindow.StartPosition = FormStartPosition.Manual;
            _floatingWindow.Location = new Point(
                workingArea.Right - _floatingWindow.Width - 20,
                workingArea.Bottom - _floatingWindow.Height - 20
            );
        }

        _floatingWindow.Show();
    }

    private async void ExitApplication()
    {
        var result = MessageBox.Show(
            "Are you sure you want to exit the application?",
            "Confirm Exit",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _isClosing = true;
            
            // Stop the API service
            if (_apiService.IsRunning)
            {
                await _apiService.StopAsync();
            }

            _trayIcon?.Dispose();
            _floatingWindow?.Close();
            Application.Exit();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_isClosing && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
        }
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon?.Dispose();
            _floatingWindow?.Dispose();
        }
        base.Dispose(disposing);
    }
}
