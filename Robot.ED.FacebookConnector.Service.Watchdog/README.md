# Robot.ED.FacebookConnector.Service.Watchdog

A Windows background service that monitors and automatically restarts the Robot.ED.FacebookConnector applications (API and RPA services).

## Features

- **Automatic Process Monitoring**: Continuously monitors configured applications
- **Auto-Restart**: Automatically restarts stopped applications
- **Email Notifications**: Sends alerts via AWS SES when applications stop or restart
- **Configurable**: Fully configurable via `appsettings.json`
- **Structured Logging**: Serilog with daily rotating logs (30-day retention)
- **Windows Service**: Runs as a Windows background service

## Quick Start

### 1. Configure Application Paths

Edit `appsettings.json` and set the correct paths for monitored applications:

```json
{
  "WatchdogSettings": {
    "CheckIntervalSeconds": 60,
    "Applications": [
      {
        "Name": "Robot.ED.FacebookConnector.Service.API",
        "ProcessName": "Robot.ED.FacebookConnector.Service.API",
        "ExecutablePath": "C:\\Path\\To\\Robot.ED.FacebookConnector.Service.API.exe",
        "WorkingDirectory": "C:\\Path\\To\\API\\Directory"
      },
      {
        "Name": "Robot.ED.FacebookConnector.Service.RPA",
        "ProcessName": "Robot.ED.FacebookConnector.Service.RPA",
        "ExecutablePath": "C:\\Path\\To\\Robot.ED.FacebookConnector.Service.RPA.exe",
        "WorkingDirectory": "C:\\Path\\To\\RPA\\Directory"
      }
    ]
  }
}
```

### 2. Configure AWS SES for Email Notifications

#### Option A: Using AWS Credentials
```json
{
  "EmailSettings": {
    "NotificationEnabled": true,
    "SenderEmail": "watchdog@yourdomain.com",
    "Recipients": [
      "admin@yourdomain.com"
    ],
    "AwsRegion": "us-east-1",
    "AwsAccessKeyId": "YOUR_ACCESS_KEY_ID",
    "AwsSecretAccessKey": "YOUR_SECRET_ACCESS_KEY"
  }
}
```

#### Option B: Using IAM Role (Recommended for EC2/ECS)
```json
{
  "EmailSettings": {
    "NotificationEnabled": true,
    "SenderEmail": "watchdog@yourdomain.com",
    "Recipients": [
      "admin@yourdomain.com"
    ],
    "AwsRegion": "us-east-1"
  }
}
```

### 3. Build the Project

```powershell
dotnet build -c Release
```

### 4. Install as Windows Service

```powershell
# Publish the application
dotnet publish -c Release -o C:\Services\Watchdog

# Install as Windows Service
sc.exe create "Robot.ED.FacebookConnector.Watchdog" binPath="C:\Services\Watchdog\Robot.ED.FacebookConnector.Service.Watchdog.exe"

# Set to start automatically
sc.exe config "Robot.ED.FacebookConnector.Watchdog" start=auto

# Start the service
sc.exe start "Robot.ED.FacebookConnector.Watchdog"
```

### 5. Verify Service is Running

```powershell
# Check service status
sc.exe query "Robot.ED.FacebookConnector.Watchdog"

# View logs
type C:\Services\Watchdog\logs\watchdog-*.log
```

## Configuration Reference

### WatchdogSettings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| CheckIntervalSeconds | int | 60 | How often to check if applications are running |
| Applications | array | [] | List of applications to monitor |

### MonitoredApplication

| Setting | Type | Required | Description |
|---------|------|----------|-------------|
| Name | string | Yes | Friendly name for the application |
| ProcessName | string | Yes | Process name (without .exe extension) |
| ExecutablePath | string | Yes | Full path to the executable |
| Arguments | string | No | Command line arguments |
| WorkingDirectory | string | No | Working directory for the process |

### EmailSettings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| NotificationEnabled | bool | true | Enable/disable email notifications |
| SenderEmail | string | - | Email address to send from (must be verified in AWS SES) |
| Recipients | array | [] | List of email recipients |
| AwsRegion | string | us-east-1 | AWS region for SES |
| AwsAccessKeyId | string | - | AWS access key (optional if using IAM role) |
| AwsSecretAccessKey | string | - | AWS secret key (optional if using IAM role) |

## AWS SES Setup

### Prerequisites
1. Verify your sender email address in AWS SES
2. If using SES Sandbox, verify recipient email addresses
3. Request production access for unrestricted sending

### Verification Steps
```bash
# Using AWS CLI to verify email
aws ses verify-email-identity --email-address watchdog@yourdomain.com
```

## Monitoring and Logs

### Log Files
- **Location**: `logs/watchdog-YYYYMMDD.log`
- **Rotation**: Daily
- **Retention**: 30 days
- **Format**: `{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message}`

### Log Example
```
2025-10-24 14:37:45.123 +00:00 [INF] Starting Robot.ED.FacebookConnector.Service.Watchdog
2025-10-24 14:37:45.456 +00:00 [INF] Watchdog service started. Monitoring 2 application(s)
2025-10-24 14:38:45.789 +00:00 [WRN] Application Robot.ED.FacebookConnector.Service.API is not running
2025-10-24 14:38:46.012 +00:00 [INF] Starting process Robot.ED.FacebookConnector.Service.API
2025-10-24 14:38:46.345 +00:00 [INF] Successfully started process (PID: 12345)
2025-10-24 14:38:46.678 +00:00 [INF] Application Robot.ED.FacebookConnector.Service.API restarted successfully
```

## Email Notifications

### Application Stopped Email
```
Subject: ⚠️ Application Stopped: Robot.ED.FacebookConnector.Service.API

Application: Robot.ED.FacebookConnector.Service.API
Status: STOPPED
Time: 2025-10-24 14:38:45 UTC

The watchdog service detected that the application has stopped running 
and will attempt to restart it.
```

### Application Restarted Email
```
Subject: ✅ Application Restarted: Robot.ED.FacebookConnector.Service.API

Application: Robot.ED.FacebookConnector.Service.API
Status: RESTARTED
Time: 2025-10-24 14:38:46 UTC

The watchdog service successfully restarted the application.
```

## Troubleshooting

### Service Won't Start
1. Check Event Viewer → Windows Logs → Application
2. Verify .NET 8.0 Runtime is installed
3. Check service account permissions
4. Review watchdog logs

### Applications Not Restarting
1. Verify executable paths in configuration
2. Check ProcessName matches the actual process name
3. Ensure working directories exist
4. Verify service account has permission to start processes
5. Review watchdog logs for errors

### Email Notifications Not Working
1. Verify sender email is verified in AWS SES
2. Check recipient emails (if in SES sandbox)
3. Verify AWS credentials or IAM role permissions
4. Check AWS region setting
5. Review logs for AWS errors

### Check Service Status
```powershell
# Get service status
Get-Service "Robot.ED.FacebookConnector.Watchdog"

# View service logs in Event Viewer
Get-EventLog -LogName Application -Source "Robot.ED.FacebookConnector.Watchdog" -Newest 50
```

## Uninstall

```powershell
# Stop the service
sc.exe stop "Robot.ED.FacebookConnector.Watchdog"

# Delete the service
sc.exe delete "Robot.ED.FacebookConnector.Watchdog"

# Remove files (optional)
Remove-Item -Path "C:\Services\Watchdog" -Recurse -Force
```

## Development

### Running Locally (Non-Service Mode)
```powershell
cd Robot.ED.FacebookConnector.Service.Watchdog
dotnet run
```

### Building
```powershell
dotnet build
```

### Publishing
```powershell
dotnet publish -c Release -o ./publish
```

## Architecture

```
Worker (BackgroundService)
  ├── ProcessMonitoringService
  │   └── Monitors process execution
  ├── EmailNotificationService
  │   └── Sends notifications via AWS SES
  └── Configuration
      ├── WatchdogSettings
      └── EmailSettings
```

## Security Best Practices

1. **Use IAM Roles**: Prefer IAM roles over hardcoded AWS credentials
2. **Limit Permissions**: Grant only necessary permissions to service account
3. **Secure Configuration**: Protect appsettings.json with appropriate file permissions
4. **Monitor Logs**: Regularly review logs for suspicious activity
5. **Update Regularly**: Keep dependencies and runtime updated

## License

This is part of the Robot.ED.FacebookConnector project.

## Support

For issues or questions, please check:
- IMPLEMENTATION_NOTES.md for detailed implementation documentation
- Project logs in the `logs/` directory
- Event Viewer for Windows Service events
