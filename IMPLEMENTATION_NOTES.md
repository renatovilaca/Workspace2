# Implementation Notes

This document describes the enhancements made to the Robot.ED.FacebookConnector.Service.API project.

## 1. Token-Based Authorization for RPA Communication

### Overview
Added support for robot-specific authorization tokens to secure communication between the orchestrator and RPA services.

### Changes Made

#### Database Schema
- **File Modified**: `Robot.ED.FacebookConnector.Common/Models/Robot.cs`
- **Change**: Added nullable `Token` field (string?) to the Robot model
- **Migration**: Created Entity Framework migration `20251024012314_AddTokenToRobot.cs`
- **Database Update**: Adds `Token` column to the `robot` table as nullable text field

#### Service Layer
- **File Modified**: `Robot.ED.FacebookConnector.Service.API/Services/RpaAllocationService.cs`
- **Changes**:
  - Modified `AllocateRpaAsync()` to pass the robot's token to `SendProcessRequestToRpaAsync()`
  - Updated `SendProcessRequestToRpaAsync()` method signature to accept token parameter: `SendProcessRequestToRpaAsync(string rpaUrl, Common.Models.Queue queue, string? token)`
  - Added logic to set `Authorization: Bearer {token}` header when token is available
  - Gracefully handles robots without tokens (backward compatible)

### Usage Example

```sql
-- Configure a robot with an authorization token
UPDATE robot SET "Token" = 'your-bearer-token-here' WHERE id = 1;
```

The orchestrator will now send authenticated requests to RPA services, with the token retrieved from the robot record.

### Backward Compatibility
- Robots without tokens will continue to work normally
- The token field is nullable, so existing robots are unaffected
- The authorization header is only added when a token is present

## 2. Configurable Kestrel Server Ports

### Overview
Made HTTP and HTTPS ports configurable through application settings instead of hardcoded values.

### Changes Made

#### Configuration Files
- **File Modified**: `appsettings.json`
- **Added Section**:
```json
{
  "Kestrel": {
    "HttpPort": 5000,
    "HttpsPort": 5001
  }
}
```

#### Application Startup
- **File Modified**: `Program.cs`
- **Change**: Updated Kestrel configuration to read ports from configuration
- **Default Values**: HTTP on port 5000, HTTPS on port 5001
- **Flexibility**: Can be overridden via:
  - Environment-specific configuration files (e.g., `appsettings.Development.json`)
  - Environment variables (e.g., `Kestrel__HttpPort=8080`)

### Configuration Example

```json
{
  "Kestrel": {
    "HttpPort": 8080,
    "HttpsPort": 8443
  }
}
```

## 3. Serilog Integration with Daily Rotating Logs

### Overview
Implemented structured logging with Serilog to improve observability and troubleshooting.

### Changes Made

#### Packages Added
- **Serilog.AspNetCore**: Version 9.0.0
- **Serilog.Sinks.File**: Version 7.0.0

#### Features
- **Daily Rotating Logs**: Files created in `logs/api-{date}.log` format
- **30-Day Retention**: Automatic cleanup of logs older than 30 days
- **Console Output**: For development/debugging
- **Custom Output Template**: With timestamps, log levels, and exception details
- **Full Integration**: With ASP.NET Core logging pipeline

#### Configuration
- **File Modified**: `Program.cs`
- **Log Configuration**:
  - Minimum level: Information
  - Microsoft.AspNetCore: Warning
  - Microsoft.EntityFrameworkCore: Warning
  - Enriched with log context
  - Dual output: Console and File

#### Output Template
```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

### Log Output Example

```
2025-10-23 23:30:45.123 +00:00 [INF] Starting Robot.ED.FacebookConnector.Service.API
2025-10-23 23:30:46.456 +00:00 [INF] Allocated Robot 1 to Queue 42, Attempt 1
```

### Log Files Location
- **Directory**: `logs/` (in the application root)
- **File Pattern**: `api-YYYYMMDD.log`
- **Example**: `logs/api-20251024.log`

## 4. Additional Improvements

### .gitignore Updates
- **File Modified**: `.gitignore`
- **Added**: 
  - `logs/` directory exclusion
  - `*.log` file pattern exclusion
- **Purpose**: Prevent log files from being committed to version control

### Documentation
- **File Created**: `IMPLEMENTATION_NOTES.md` (this file)
- **Purpose**: Comprehensive documentation of all changes

## Backward Compatibility

All changes are fully backward compatible:

1. **Token Field**: Nullable, so existing robots without tokens continue to work
2. **Kestrel Ports**: Default values match previous hardcoded values (5000/5001)
3. **Logging**: Replaces standard ASP.NET Core logging without breaking existing log calls
4. **Database**: Migration is additive only (no breaking changes)

## Testing Recommendations

### Token Authorization
1. Create a robot without a token - verify it works normally
2. Add a token to a robot - verify the Authorization header is sent
3. Test with RPA service that validates the token

### Configurable Ports
1. Run with default configuration - verify ports 5000/5001
2. Override via appsettings.json - verify custom ports
3. Override via environment variables - verify environment takes precedence

### Serilog
1. Start the application - verify `logs/` directory is created
2. Generate log entries - verify they appear in both console and file
3. Check log file format - verify timestamp and level formatting
4. Wait for day rollover - verify new log file is created
5. Check retention - verify old logs are cleaned up after 30 days

## Migration Instructions

### Database Migration

To apply the database migration:

```bash
cd Robot.ED.FacebookConnector.Service.API
dotnet ef database update
```

Or the application will automatically apply migrations on startup.

### Configuration Override Examples

#### Via appsettings.Development.json
```json
{
  "Kestrel": {
    "HttpPort": 8080,
    "HttpsPort": 8443
  }
}
```

#### Via Environment Variables
```bash
export Kestrel__HttpPort=8080
export Kestrel__HttpsPort=8443
```

#### Via Docker Compose
```yaml
environment:
  - Kestrel__HttpPort=8080
  - Kestrel__HttpsPort=8443
```

## Support

For issues or questions about these implementations, please refer to:
- The commit history for this feature
- The Entity Framework migration files
- The Serilog documentation: https://serilog.net/

---

# Robot.ED.FacebookConnector.Service.RPA Enhancements

This section describes the enhancements made to the Robot.ED.FacebookConnector.Service.RPA project.

## 1. Configurable Kestrel Server Ports

### Overview
Made HTTP and HTTPS ports configurable through application settings instead of hardcoded values.

### Changes Made

#### Configuration Files
- **File Modified**: `Robot.ED.FacebookConnector.Service.RPA/appsettings.json`
- **Added Section**:
```json
{
  "Kestrel": {
    "HttpPort": 8080,
    "HttpsPort": 8081
  }
}
```

#### Application Startup
- **File Modified**: `Robot.ED.FacebookConnector.Service.RPA/Program.cs`
- **Change**: Updated Kestrel configuration to read ports from configuration
- **Default Values**: HTTP on port 8080, HTTPS on port 8081
- **Flexibility**: Can be overridden via:
  - Environment-specific configuration files (e.g., `appsettings.Development.json`)
  - Environment variables (e.g., `Kestrel__HttpPort=8080`)

### Configuration Example

```json
{
  "Kestrel": {
    "HttpPort": 8080,
    "HttpsPort": 8443
  }
}
```

### Code Changes

```csharp
// Configure Kestrel
var httpPort = builder.Configuration.GetValue<int>("Kestrel:HttpPort", 8080);
var httpsPort = builder.Configuration.GetValue<int>("Kestrel:HttpsPort", 8081);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(httpPort);
    serverOptions.ListenAnyIP(httpsPort, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});
```

## 2. Serilog Integration with Daily Rotating Logs

### Overview
Implemented structured logging with Serilog to improve observability and troubleshooting.

### Changes Made

#### Packages Added
- **File Modified**: `Robot.ED.FacebookConnector.Service.RPA/Robot.ED.FacebookConnector.Service.RPA.csproj`
- **Serilog.AspNetCore**: Version 9.0.0
- **Serilog.Sinks.File**: Version 7.0.0

#### Features
- **Daily Rotating Logs**: Files created in `logs/rpa-api-{date}.log` format
- **30-Day Retention**: Automatic cleanup of logs older than 30 days
- **Console Output**: For development/debugging
- **Custom Output Template**: With timestamps, log levels, and exception details
- **Full Integration**: With ASP.NET Core logging pipeline

#### Configuration
- **File Modified**: `Robot.ED.FacebookConnector.Service.RPA/Program.cs`
- **Log Configuration**:
  - Minimum level: Information
  - Microsoft.AspNetCore: Warning
  - Microsoft.EntityFrameworkCore: Warning
  - Enriched with log context
  - Dual output: Console and File

#### Serilog Setup Code

```csharp
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/rpa-api-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Robot.ED.FacebookConnector.Service.RPA");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Add Serilog
    builder.Host.UseSerilog();
    
    // ... rest of application configuration
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

#### Output Template
```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

### Log Output Example

```
2025-10-23 23:30:45.123 +00:00 [INF] Starting Robot.ED.FacebookConnector.Service.RPA
2025-10-23 23:30:46.456 +00:00 [INF] Allocated Robot 1 to Queue 42, Attempt 1
```

### Log Files Location
- **Directory**: `logs/` (in the application root)
- **File Pattern**: `rpa-api-YYYYMMDD.log`
- **Example**: `logs/rpa-api-20251024.log`

## 3. Additional Improvements

### .gitignore Updates
- **File**: `.gitignore`
- **Status**: Already contains proper exclusions
  - `logs/` directory exclusion
  - `*.log` file pattern exclusion
- **Purpose**: Prevent log files from being committed to version control

### Documentation
- **File Updated**: `IMPLEMENTATION_NOTES.md` (this file)
- **Purpose**: Comprehensive documentation of all changes to both API and RPA services

## Backward Compatibility

All changes are fully backward compatible:

1. **Kestrel Ports**: Default values (8080/8081) are used if not configured
2. **Logging**: Replaces standard ASP.NET Core logging without breaking existing log calls
3. **Configuration**: All settings are optional with sensible defaults

## Testing Recommendations

### Configurable Ports
1. Run with default configuration - verify ports 8080/8081
2. Override via appsettings.json - verify custom ports
3. Override via environment variables - verify environment takes precedence

### Serilog
1. Start the application - verify `logs/` directory is created
2. Generate log entries - verify they appear in both console and file
3. Check log file format - verify timestamp and level formatting
4. Wait for day rollover - verify new log file is created (format: `rpa-api-YYYYMMDD.log`)
5. Check retention - verify old logs are cleaned up after 30 days

## Configuration Override Examples

### Via appsettings.Development.json
```json
{
  "Kestrel": {
    "HttpPort": 8080,
    "HttpsPort": 8443
  }
}
```

### Via Environment Variables
```bash
export Kestrel__HttpPort=8080
export Kestrel__HttpsPort=8443
```

### Via Docker Compose
```yaml
environment:
  - Kestrel__HttpPort=8080
  - Kestrel__HttpsPort=8443
```

## Comparison: API vs RPA Service Configurations

| Feature | API Service | RPA Service |
|---------|-------------|-------------|
| HTTP Port (default) | 5000 | 8080 |
| HTTPS Port (default) | 5001 | 8081 |
| Log File Pattern | `logs/api-{date}.log` | `logs/rpa-api-{date}.log` |
| Log Retention | 30 days | 30 days |
| Serilog Version | 9.0.0 | 9.0.0 |
| Configuration | Via appsettings.json | Via appsettings.json |

---

# Robot.ED.FacebookConnector.Service.Watchdog

This section describes the implementation of the Watchdog Service for monitoring and restarting applications.

## Overview

The Watchdog Service is a Windows background service that monitors the execution of critical applications (Robot.ED.FacebookConnector.Service.API and Robot.ED.FacebookConnector.Service.RPA). When an application stops, the watchdog automatically restarts it and sends email notifications via AWS SES.

## Features

### 1. Process Monitoring
- Configurable check interval (default: 60 seconds)
- Monitors multiple applications simultaneously
- Detects when processes are not running
- Automatically restarts stopped applications

### 2. Email Notifications via AWS SES
- Sends notifications when applications stop
- Sends notifications when applications are restarted
- Configurable recipient list
- Email notifications can be enabled/disabled
- Supports AWS credentials or IAM roles

### 3. Serilog Integration with Daily Rotating Logs
- Daily rotating log files in `logs/watchdog-{date}.log` format
- 30-day retention policy for automatic cleanup
- Console output for development/debugging
- Custom output template with timestamps, log levels, and exception details
- Full integration with .NET logging pipeline

### 4. Configurable Settings
All settings are configured via `appsettings.json`:
- Check interval
- List of monitored applications
- Application executable paths and arguments
- Email notification settings
- AWS SES configuration

## Changes Made

### Project Created
- **Project**: `Robot.ED.FacebookConnector.Service.Watchdog`
- **Type**: .NET 8.0 Worker Service
- **Platform**: Windows Service compatible

### Packages Added
- **Microsoft.Extensions.Hosting.WindowsServices**: Version 8.0.* - Windows Service support
- **AWSSDK.SimpleEmail**: Version 3.7.* - AWS SES email notifications
- **Serilog.AspNetCore**: Version 9.0.0 - Structured logging
- **Serilog.Sinks.File**: Version 7.0.0 - File logging with rotation

### Files Created

#### Configuration Models
1. **WatchdogSettings.cs**
   - `CheckIntervalSeconds`: Interval between checks (default: 60)
   - `Applications`: List of applications to monitor

2. **EmailSettings.cs**
   - `NotificationEnabled`: Enable/disable notifications
   - `SenderEmail`: Email address for sending notifications
   - `Recipients`: List of email recipients
   - `AwsRegion`: AWS region for SES
   - `AwsAccessKeyId`: AWS access key (optional, can use IAM role)
   - `AwsSecretAccessKey`: AWS secret key (optional)

#### Services
1. **ProcessMonitoringService.cs**
   - `IsProcessRunning()`: Check if a process is running
   - `StartProcess()`: Start a stopped process

2. **EmailNotificationService.cs**
   - `SendApplicationStoppedNotificationAsync()`: Send stopped notification
   - `SendApplicationRestartedNotificationAsync()`: Send restarted notification
   - HTML email formatting
   - AWS SES integration

#### Worker Service
**Worker.cs**
- Main background service logic
- Periodic monitoring loop
- State tracking for applications
- Coordinates process monitoring and notifications

#### Application Entry Point
**Program.cs**
- Serilog configuration
- Service registration
- Windows Service support
- Configuration binding

### Configuration File

**appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WatchdogSettings": {
    "CheckIntervalSeconds": 60,
    "Applications": [
      {
        "Name": "Robot.ED.FacebookConnector.Service.API",
        "ProcessName": "Robot.ED.FacebookConnector.Service.API",
        "ExecutablePath": "C:\\Path\\To\\Robot.ED.FacebookConnector.Service.API.exe",
        "Arguments": "",
        "WorkingDirectory": "C:\\Path\\To\\API\\Directory"
      },
      {
        "Name": "Robot.ED.FacebookConnector.Service.RPA",
        "ProcessName": "Robot.ED.FacebookConnector.Service.RPA",
        "ExecutablePath": "C:\\Path\\To\\Robot.ED.FacebookConnector.Service.RPA.exe",
        "Arguments": "",
        "WorkingDirectory": "C:\\Path\\To\\RPA\\Directory"
      }
    ]
  },
  "EmailSettings": {
    "NotificationEnabled": true,
    "SenderEmail": "watchdog@example.com",
    "Recipients": [
      "admin@example.com",
      "team@example.com"
    ],
    "AwsRegion": "us-east-1",
    "AwsAccessKeyId": "",
    "AwsSecretAccessKey": ""
  }
}
```

## Log Output Example

```
2025-10-24 14:37:45.123 +00:00 [INF] Starting Robot.ED.FacebookConnector.Service.Watchdog
2025-10-24 14:37:45.456 +00:00 [INF] Watchdog service started. Monitoring 2 application(s)
2025-10-24 14:37:45.457 +00:00 [INF] Monitoring: Robot.ED.FacebookConnector.Service.API (Process: Robot.ED.FacebookConnector.Service.API)
2025-10-24 14:37:45.458 +00:00 [INF] Monitoring: Robot.ED.FacebookConnector.Service.RPA (Process: Robot.ED.FacebookConnector.Service.RPA)
2025-10-24 14:38:45.789 +00:00 [WRN] Application Robot.ED.FacebookConnector.Service.API is not running
2025-10-24 14:38:46.012 +00:00 [INF] Starting process Robot.ED.FacebookConnector.Service.API from C:\Path\To\Robot.ED.FacebookConnector.Service.API.exe
2025-10-24 14:38:46.345 +00:00 [INF] Successfully started process Robot.ED.FacebookConnector.Service.API (PID: 12345)
2025-10-24 14:38:46.678 +00:00 [INF] Application Robot.ED.FacebookConnector.Service.API restarted successfully
2025-10-24 14:38:46.901 +00:00 [INF] Email sent successfully. MessageId: 01000192f8c5e1d8-a1b2c3d4-e5f6-7890-abcd-ef1234567890-000000
```

## Email Notification Examples

### Application Stopped Email
- **Subject**: ⚠️ Application Stopped: Robot.ED.FacebookConnector.Service.API
- **Body**: HTML formatted with application name, status (STOPPED in red), and timestamp

### Application Restarted Email
- **Subject**: ✅ Application Restarted: Robot.ED.FacebookConnector.Service.API
- **Body**: HTML formatted with application name, status (RESTARTED in green), and timestamp

## Installation and Configuration

### 1. Configure Application Paths
Edit `appsettings.json` and update the executable paths for the monitored applications:
```json
"ExecutablePath": "C:\\Path\\To\\Your\\Application.exe",
"WorkingDirectory": "C:\\Path\\To\\Application\\Directory"
```

### 2. Configure AWS SES
You have two options:

#### Option A: Use AWS Credentials
```json
"EmailSettings": {
  "AwsAccessKeyId": "YOUR_ACCESS_KEY",
  "AwsSecretAccessKey": "YOUR_SECRET_KEY",
  "AwsRegion": "us-east-1"
}
```

#### Option B: Use IAM Role (Recommended for EC2/ECS)
```json
"EmailSettings": {
  "AwsRegion": "us-east-1"
}
```
Leave credentials empty to use the default AWS credential chain.

### 3. Configure Recipients
```json
"Recipients": [
  "admin@example.com",
  "team@example.com"
]
```

### 4. Install as Windows Service
```powershell
# Build the project in Release mode
dotnet publish -c Release

# Create the service using sc.exe
sc.exe create "Robot.ED.FacebookConnector.Watchdog" binPath="C:\Path\To\Robot.ED.FacebookConnector.Service.Watchdog.exe"

# Start the service
sc.exe start "Robot.ED.FacebookConnector.Watchdog"
```

### 5. Configure Service Startup
```powershell
# Set service to start automatically
sc.exe config "Robot.ED.FacebookConnector.Watchdog" start=auto
```

## Testing Recommendations

### Process Monitoring
1. Start the watchdog service
2. Verify it detects running applications
3. Stop a monitored application manually
4. Verify the watchdog restarts it within the check interval
5. Check logs for monitoring activity

### Email Notifications
1. Configure a test email address
2. Stop a monitored application
3. Verify you receive the "Application Stopped" email
4. Verify you receive the "Application Restarted" email
5. Test with notifications disabled (`NotificationEnabled: false`)

### AWS SES Configuration
1. Verify sender email is verified in AWS SES
2. If in sandbox mode, verify recipient emails are verified
3. Test with both credentials and IAM role configurations
4. Monitor AWS SES sending statistics

### Logging
1. Start the service and verify `logs/` directory is created
2. Check log file format and content
3. Verify daily log rotation
4. Verify 30-day retention cleanup

## Troubleshooting

### Common Issues

**Watchdog doesn't restart applications:**
- Check executable paths in configuration
- Verify working directories exist
- Check process names match exactly (case-sensitive)
- Review logs for error messages

**Email notifications not sent:**
- Verify AWS SES credentials
- Check sender email is verified in AWS SES
- Verify recipient emails (if in SES sandbox)
- Check AWS region configuration
- Review logs for AWS errors

**Service won't start:**
- Check .NET 8.0 Runtime is installed
- Verify service account has necessary permissions
- Check Event Viewer for service startup errors
- Review watchdog logs

## Architecture

### Service Flow
1. Worker service starts and loads configuration
2. Initializes process monitoring and email services
3. Enters monitoring loop with configurable interval
4. For each monitored application:
   - Checks if process is running
   - If not running:
     - Sends "stopped" notification (if previously running)
     - Attempts to restart process
     - Sends "restarted" notification on success
   - Updates application state
5. Waits for next check interval
6. Repeats until service is stopped

### Dependencies
```
Worker (BackgroundService)
  ├── ProcessMonitoringService
  │   └── System.Diagnostics.Process
  ├── EmailNotificationService
  │   └── AWS.SimpleEmail
  └── WatchdogSettings / EmailSettings
```

## Security Considerations

1. **AWS Credentials**: Use IAM roles instead of hardcoded credentials when possible
2. **File Permissions**: Ensure proper permissions on configuration files
3. **Process Permissions**: Service account must have rights to start monitored processes
4. **Email Content**: Notifications contain application names but no sensitive data

## Backward Compatibility

This is a new service and does not affect existing services. It can be:
- Installed independently
- Configured to monitor any number of applications
- Disabled without affecting monitored applications

## Log Files Location
- **Directory**: `logs/` (in the application root)
- **File Pattern**: `watchdog-YYYYMMDD.log`
- **Example**: `logs/watchdog-20251024.log`
- **Retention**: 30 days

## Comparison: All Services

| Feature | API Service | RPA Service | Watchdog Service |
|---------|-------------|-------------|------------------|
| Type | Web API | Web API | Worker Service |
| HTTP Port (default) | 5000 | 8080 | N/A |
| HTTPS Port (default) | 5001 | 8081 | N/A |
| Log File Pattern | `logs/api-{date}.log` | `logs/rpa-api-{date}.log` | `logs/watchdog-{date}.log` |
| Log Retention | 30 days | 30 days | 30 days |
| Serilog Version | 9.0.0 | 9.0.0 | 9.0.0 |
| Purpose | Orchestrator | RPA Execution | Process Monitoring |
| Windows Service | No | No | Yes |

