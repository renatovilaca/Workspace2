# Robot.ED.FacebookConnector.Service.RPA.UI

A modern Windows desktop application for managing the Facebook Connector RPA service with a beautiful dark-themed user interface.

## Overview

This application provides a Windows-native UI for the Robot.ED.FacebookConnector.Service.RPA functionality. It features:

- **System Tray Integration**: Minimizes to system tray for unobtrusive background operation
- **Floating Dashboard**: Modern, dark-themed dashboard window that appears in the bottom-right corner
- **RPA Control**: Start, pause, and stop RPA operations with easy-to-use buttons
- **Real-time Monitoring**: View current cycle execution time and process status
- **Status Indicators**: Visual feedback for success (green) and failure (red) states
- **REST API Server**: Built-in REST API server compatible with the original RPA service
- **Selenium Integration**: Full integration with Selenium WebDriver for browser automation

## Features

### User Interface
- **Dark Theme**: Modern, visually appealing dark color scheme
- **System Tray Icon**: Click to show/hide the dashboard
- **Floating Window**: Stays on top, positioned in the bottom-right corner
- **Responsive Design**: Clean, organized layout with smooth animations

### RPA Management
- **Start/Stop Control**: Begin or end RPA operations
- **Pause/Resume**: Temporarily pause incoming requests
- **Execution Timer**: Real-time display of current cycle duration
- **Status Display**: Shows Running, Paused, or Stopped state

### Status Monitoring
- **Last Execution Status**: Green background for success, red for failure
- **Execution Messages**: Detailed information about the last operation
- **API Server Status**: Indicates if the REST API server is running

### REST API
- **Process Endpoint**: `POST /api/rpa/process` - Accepts RPA processing requests
- **Health Endpoint**: `GET /api/health` - Returns health status
- **Ports**: HTTP (8080) and HTTPS (8081)

## Technology Stack

- **.NET 8** with Windows Forms
- **Blazor Hybrid** (WebView2) for modern UI components
- **Selenium WebDriver** for browser automation
- **Serilog** for logging
- **Entity Framework Core** with PostgreSQL

## Requirements

- Windows 10 or later
- .NET 8 Runtime
- PostgreSQL database (for RPA settings)
- Google Chrome (for Selenium automation)

## Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=robotfacebookconnector;Username=postgres;Password=postgres"
  },
  "RpaSettings": {
    "ProcessTimeoutMinutes": 10,
    "DataRetentionDays": 90,
    "ScreenshotPath": "screenshots",
    "OrchestratorUrl": "http://localhost:5000",
    "OrchestratorToken": "default-token",
    "FacebookUsername": "your-email",
    "FacebookPassword": "your-password"
  }
}
```

## Building

To build the application on Windows:

```bash
dotnet build Robot.ED.FacebookConnector.Service.RPA.UI.csproj
```

To publish:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Usage

1. **Launch the Application**: Run the executable
2. **System Tray**: Look for the Facebook Connector icon in the system tray
3. **Open Dashboard**: Left-click the tray icon
4. **Start RPA**: Click the "Start" button to begin accepting requests
5. **Monitor**: Watch the execution timer and status indicators
6. **Pause**: Click "Pause" to temporarily stop accepting new requests
7. **Exit**: Click "Exit Application" and confirm to close

## Architecture

The application consists of several key components:

- **MainForm**: Hidden main form that manages the system tray icon
- **FloatingDashboard**: The visible dashboard window hosting Blazor components
- **Dashboard.razor**: Blazor component providing the UI
- **RpaStateService**: Manages the application state
- **RpaApiService**: Hosts the REST API server
- **RpaProcessingService**: Handles RPA automation logic
- **ChromeDriverManager**: Manages the Selenium WebDriver instance

## Notes

- This application is designed exclusively for Windows platforms
- The floating window stays on top of other windows for easy monitoring
- The application can run in the background via the system tray
- All RPA functionality from the original service is preserved
- Logs are saved to the `logs/` directory

## See Also

- [Robot.ED.FacebookConnector.Service.RPA](../Robot.ED.FacebookConnector.Service.RPA/) - Original RPA service
- [Robot.ED.FacebookConnector.Common](../Robot.ED.FacebookConnector.Common/) - Shared components
