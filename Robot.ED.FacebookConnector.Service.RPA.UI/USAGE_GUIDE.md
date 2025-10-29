# Robot.ED.FacebookConnector.Service.RPA.UI - Usage Guide

## Getting Started

### Prerequisites
Before running the application, ensure you have:
- Windows 10 or later
- .NET 8 Runtime installed
- PostgreSQL database running (for RPA settings)
- Google Chrome browser installed (for Selenium automation)

### First Run

1. **Configure Database Connection**
   - Open `appsettings.json`
   - Update the `ConnectionStrings:DefaultConnection` with your PostgreSQL details

2. **Configure RPA Settings**
   - Update `RpaSettings` section in `appsettings.json`:
     ```json
     {
       "RpaSettings": {
         "FacebookUsername": "your-facebook-email",
         "FacebookPassword": "your-facebook-password",
         "OrchestratorUrl": "http://your-orchestrator:5000",
         "OrchestratorToken": "your-token"
       }
     }
     ```

3. **Launch the Application**
   - Run the executable: `Robot.ED.FacebookConnector.Service.RPA.UI.exe`
   - Look for the icon in the system tray (notification area)

## Using the Application

### System Tray Icon

The application runs in the background with a system tray icon.

**Left-Click**: Opens or brings focus to the dashboard window
**Right-Click**: Shows context menu
- "Show Dashboard" - Opens the dashboard
- "Exit" - Closes the application

### Dashboard Window

The dashboard is a floating window that appears in the bottom-right corner of your screen.

#### Starting the RPA Service

1. Click the system tray icon to open the dashboard
2. Click the **"▶ Start"** button (green)
3. The API server will start on ports 8080 and 8081
4. Status badge changes to "Stopped" (ready to accept requests)

The application is now ready to receive RPA requests via the REST API.

#### Processing Requests

When a request is received:
1. Status automatically changes to **"Running"** (green badge with glow)
2. The **Current Cycle Time** timer begins counting
3. The Chrome browser will open and perform the automation
4. After completion:
   - Timer stops
   - Status returns to "Stopped"
   - **Last Execution** shows either:
     - **Success** (green) - "Execution completed successfully"
     - **Failure** (red) - Error details displayed

#### Pausing the Service

To temporarily stop accepting new requests:
1. Click the **"⏸ Pause"** button (orange) while running
2. Status changes to **"Paused"** (orange badge)
3. Current requests continue to completion
4. New requests are rejected with HTTP 503

From the paused state, you can:
- Click **"▶ Resume"** (green) to continue accepting requests
- Click **"⏹ Stop"** (red) to completely stop the API server

#### Stopping the Service

To completely stop the RPA service:
1. From the Paused state, click **"⏹ Stop"** (red)
2. Or close and restart by clicking Start again
3. API server shuts down
4. Status returns to "Stopped"

#### Exiting the Application

1. Click the **"✕ Exit Application"** button (red) at the bottom
2. Confirm your choice in the dialog box
3. Or right-click the system tray icon and select "Exit"

The application will:
- Stop the API server if running
- Close the Chrome browser if open
- Clean up all resources
- Remove the system tray icon
- Exit completely

## Monitoring

### Current Cycle Time
- Shows elapsed time for the current RPA cycle
- Format: `mm:ss` (e.g., "02:15" for 2 minutes 15 seconds)
- Updates every second during execution
- Shows `--:--` when no cycle is running

### Last Execution Status

**Success Indicator** (Green)
- Appears after successful automation
- Message: "Execution completed successfully"
- Background: Bright green gradient with glow

**Failure Indicator** (Red)
- Appears after failed automation
- Message: Shows the specific error that occurred
- Background: Red gradient with glow

### API Server Status
- **● Running (Port 8080/8081)** - Server is accepting requests
- **● Stopped** - Server is not running

### Process Status Badge

Located at the top of the dashboard:

- **Running** (Green badge)
  - RPA is actively processing a request
  - Timer is counting
  
- **Paused** (Orange badge)
  - Service is paused
  - New requests are rejected
  - Can resume or stop
  
- **Stopped** (Gray badge)
  - Service is ready but not processing
  - Can start receiving requests

## REST API Usage

### Sending Requests to the RPA Service

The application exposes a REST API compatible with the original RPA service:

**Endpoint**: `POST http://localhost:8080/api/rpa/process`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "queueId": 123,
  "trackId": "TRACK-001",
  "bridgeKey": "bridge-key",
  "originType": "facebook",
  "mediaId": "media-001",
  "customer": "customer-name",
  "channel": "facebook-channel",
  "phrase": "search-phrase",
  "tags": ["tag1", "tag2"],
  "data": []
}
```

**Response**:
- **202 Accepted** - Request accepted for processing
- **503 Service Unavailable** - RPA is paused
- **500 Internal Server Error** - Error accepting request

### Health Check

**Endpoint**: `GET http://localhost:8080/api/health`

**Response**:
```json
{
  "status": "healthy"
}
```

## Window Management

### Dashboard Window
- **Size**: 400 x 600 pixels (fixed)
- **Position**: Bottom-right corner with 20px margins
- **Behavior**: 
  - Always stays on top of other windows
  - Can be moved by dragging
  - Cannot be resized
  - Closes when clicking close button (minimizes to tray)

### Hiding the Dashboard
- Click the close (X) button on the window
- The window hides but the application continues running in the system tray
- Click the tray icon again to show the dashboard

## Troubleshooting

### Dashboard Won't Open
- Check if the application is running (look for system tray icon)
- Try right-clicking the tray icon and selecting "Show Dashboard"
- If still not visible, exit and restart the application

### API Server Won't Start
- Check if ports 8080/8081 are available
- Verify firewall settings
- Check logs in the `logs/` directory for errors

### RPA Processing Fails
- Ensure Google Chrome is installed
- Check Facebook credentials in `appsettings.json`
- Review error message in "Last Execution" indicator
- Check `screenshots/` folder for error screenshots
- Review logs for detailed error information

### Database Connection Errors
- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Ensure the database exists
- Verify credentials

### Chrome Browser Issues
- Ensure Chrome is installed and up to date
- ChromeDriver will be automatically managed by Selenium
- Check system resources (memory/CPU)

## Logs

Application logs are saved to:
- **Location**: `logs/rpa-ui-YYYYMMDD.log`
- **Format**: Timestamped entries with level indicators
- **Rotation**: Daily (new file each day)
- **Retention**: 30 days
- **Levels**: Information, Warning, Error

Example log entry:
```
2024-01-15 10:30:45.123 +00:00 [INF] Starting RPA service...
```

## Best Practices

### Daily Operation
1. Start the application at system startup (add to Windows Startup folder)
2. Keep the dashboard minimized to system tray
3. Check periodically for status and any errors
4. Monitor execution success/failure rates

### When Processing Requests
- Watch the timer to ensure cycles complete in reasonable time
- Check the Last Execution status after each cycle
- If failures occur frequently, investigate error messages

### Maintenance
- Review logs periodically
- Clear old screenshots from `screenshots/` folder
- Update Facebook credentials if they change
- Restart the application weekly for optimal performance

### Performance
- The application uses minimal resources when idle
- Chrome browser appears only during processing
- Keep the system tray icon always visible for quick access

## Keyboard Shortcuts

Currently, the application does not implement keyboard shortcuts. All interactions are through mouse clicks.

## Tips

1. **Quick Access**: Pin the system tray icon to always visible
2. **Monitoring**: Keep dashboard open on a second monitor for real-time monitoring
3. **Testing**: Use the health endpoint to verify the API server is running
4. **Screenshots**: Check the `screenshots/` folder after failures for visual debugging
5. **Logs**: Use logs to diagnose issues when error messages aren't clear

## Security Notes

- Store credentials securely in `appsettings.json`
- The API does not currently require authentication
- The application should run with normal user privileges
- Database credentials should be protected

## Getting Help

If you encounter issues:
1. Check the logs in `logs/` directory
2. Review screenshots in `screenshots/` folder for visual errors
3. Verify configuration in `appsettings.json`
4. Ensure all prerequisites are met
5. Check the GitHub repository for updates

## Next Steps

After getting familiar with the basic usage:
- Integrate with your orchestrator system
- Set up automated request sending
- Configure monitoring and alerting based on logs
- Customize the configuration for your environment
