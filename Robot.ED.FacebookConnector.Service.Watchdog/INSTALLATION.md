# Watchdog Service - Quick Installation Guide

This guide provides step-by-step instructions to install and configure the Robot.ED.FacebookConnector.Service.Watchdog on Windows.

## Prerequisites

- Windows Server 2016 or later / Windows 10 or later
- .NET 8.0 Runtime or SDK installed
- Administrator privileges
- AWS SES account (for email notifications)

## Step 1: Build the Service

```powershell
# Navigate to the solution directory
cd C:\Path\To\Workspace2

# Build in Release mode
dotnet build -c Release

# Publish the watchdog service
dotnet publish Robot.ED.FacebookConnector.Service.Watchdog -c Release -o C:\Services\Watchdog
```

## Step 2: Configure the Service

Edit `C:\Services\Watchdog\appsettings.json`:

### 2.1 Configure Monitored Applications

Find the actual paths to your API and RPA executables:

```json
{
  "WatchdogSettings": {
    "CheckIntervalSeconds": 60,
    "Applications": [
      {
        "Name": "Robot.ED.FacebookConnector.Service.API",
        "ProcessName": "Robot.ED.FacebookConnector.Service.API",
        "ExecutablePath": "C:\\Services\\API\\Robot.ED.FacebookConnector.Service.API.exe",
        "Arguments": "",
        "WorkingDirectory": "C:\\Services\\API"
      },
      {
        "Name": "Robot.ED.FacebookConnector.Service.RPA",
        "ProcessName": "Robot.ED.FacebookConnector.Service.RPA",
        "ExecutablePath": "C:\\Services\\RPA\\Robot.ED.FacebookConnector.Service.RPA.exe",
        "Arguments": "",
        "WorkingDirectory": "C:\\Services\\RPA"
      }
    ]
  }
}
```

**Important Notes:**
- `ProcessName` should match the executable name WITHOUT the `.exe` extension
- `ExecutablePath` must be the full path to the .exe file
- `WorkingDirectory` should be the directory containing the application's files
- Use double backslashes `\\` in JSON paths or single forward slashes `/`

### 2.2 Configure AWS SES for Email Notifications

#### Option A: Using AWS Credentials (Development/Testing)

```json
{
  "EmailSettings": {
    "NotificationEnabled": true,
    "SenderEmail": "watchdog@yourdomain.com",
    "Recipients": [
      "admin@yourdomain.com",
      "team@yourdomain.com"
    ],
    "AwsRegion": "us-east-1",
    "AwsAccessKeyId": "AKIAIOSFODNN7EXAMPLE",
    "AwsSecretAccessKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
  }
}
```

#### Option B: Using IAM Role (Production - Recommended)

If running on EC2 or with an IAM role attached:

```json
{
  "EmailSettings": {
    "NotificationEnabled": true,
    "SenderEmail": "watchdog@yourdomain.com",
    "Recipients": [
      "admin@yourdomain.com"
    ],
    "AwsRegion": "us-east-1",
    "AwsAccessKeyId": "",
    "AwsSecretAccessKey": ""
  }
}
```

**Important:** 
- Verify the sender email in AWS SES Console
- If in SES Sandbox mode, verify recipient emails too
- Request production access for unrestricted sending

### 2.3 Disable Email Notifications (Optional)

If you don't want email notifications:

```json
{
  "EmailSettings": {
    "NotificationEnabled": false
  }
}
```

## Step 3: Verify AWS SES Configuration

### Verify Sender Email
```powershell
# Using AWS CLI
aws ses verify-email-identity --email-address watchdog@yourdomain.com --region us-east-1

# Check verification status
aws ses get-identity-verification-attributes --identities watchdog@yourdomain.com --region us-east-1
```

### Verify Recipient Emails (if in Sandbox)
```powershell
aws ses verify-email-identity --email-address admin@yourdomain.com --region us-east-1
```

### Request Production Access
If you need to send to unverified addresses:
1. Go to AWS SES Console
2. Navigate to "Account Dashboard"
3. Click "Request production access"
4. Fill out the request form

## Step 4: Install as Windows Service

Open PowerShell as Administrator:

```powershell
# Create the service
sc.exe create "Robot.ED.FacebookConnector.Watchdog" `
    binPath= "C:\Services\Watchdog\Robot.ED.FacebookConnector.Service.Watchdog.exe" `
    DisplayName= "Robot ED Facebook Connector Watchdog" `
    Description= "Monitors and restarts Robot.ED.FacebookConnector applications"

# Configure service to start automatically
sc.exe config "Robot.ED.FacebookConnector.Watchdog" start= auto

# Configure service recovery options (optional but recommended)
sc.exe failure "Robot.ED.FacebookConnector.Watchdog" reset= 86400 actions= restart/60000/restart/60000/restart/60000
```

## Step 5: Start the Service

```powershell
# Start the service
sc.exe start "Robot.ED.FacebookConnector.Watchdog"

# Verify service is running
sc.exe query "Robot.ED.FacebookConnector.Watchdog"

# Or use Get-Service
Get-Service "Robot.ED.FacebookConnector.Watchdog"
```

## Step 6: Verify Installation

### Check Service Status
```powershell
Get-Service "Robot.ED.FacebookConnector.Watchdog" | Format-List
```

Expected output:
```
Name                : Robot.ED.FacebookConnector.Watchdog
DisplayName         : Robot ED Facebook Connector Watchdog
Status              : Running
DependentServices   : {}
ServicesDependedOn  : {}
CanPauseAndContinue : False
CanShutdown         : True
CanStop             : True
ServiceType         : Win32OwnProcess
```

### Check Logs
```powershell
# View the latest log file
Get-Content "C:\Services\Watchdog\logs\watchdog-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50

# Or use Notepad
notepad "C:\Services\Watchdog\logs\watchdog-$(Get-Date -Format 'yyyyMMdd').log"
```

Look for these log entries:
```
[INF] Starting Robot.ED.FacebookConnector.Service.Watchdog
[INF] Email notification service initialized with AWS SES in region us-east-1
[INF] Watchdog service started. Monitoring 2 application(s)
[INF] Monitoring: Robot.ED.FacebookConnector.Service.API (Process: Robot.ED.FacebookConnector.Service.API)
[INF] Monitoring: Robot.ED.FacebookConnector.Service.RPA (Process: Robot.ED.FacebookConnector.Service.RPA)
```

### Test Email Notifications

Stop one of the monitored applications manually:

```powershell
# Stop the API service (if running as a service)
Stop-Service "Robot.ED.FacebookConnector.Service.API"

# Or kill the process
Stop-Process -Name "Robot.ED.FacebookConnector.Service.API" -Force
```

Wait for the check interval (default 60 seconds), then:
1. Check the watchdog log - you should see restart messages
2. Check your email - you should receive two emails (stopped and restarted)

## Step 7: Configure Windows Event Viewer (Optional)

To view service events in Event Viewer:

```powershell
# Open Event Viewer
eventvwr.msc

# Navigate to: Windows Logs > Application
# Filter by source: Robot.ED.FacebookConnector.Watchdog
```

## Troubleshooting

### Service Won't Start

1. **Check Event Viewer:**
   ```powershell
   Get-EventLog -LogName Application -Source "Robot.ED.FacebookConnector.Watchdog" -Newest 10
   ```

2. **Verify .NET Runtime:**
   ```powershell
   dotnet --list-runtimes
   ```
   Should show: `Microsoft.NETCore.App 8.0.x`

3. **Check File Permissions:**
   ```powershell
   icacls "C:\Services\Watchdog"
   ```
   Ensure NETWORK SERVICE or the service account has Read & Execute permissions

### Applications Not Restarting

1. **Verify Process Names:**
   ```powershell
   Get-Process | Where-Object {$_.Name -like "*FacebookConnector*"}
   ```

2. **Check Executable Paths:**
   ```powershell
   Test-Path "C:\Services\API\Robot.ED.FacebookConnector.Service.API.exe"
   Test-Path "C:\Services\RPA\Robot.ED.FacebookConnector.Service.RPA.exe"
   ```

3. **Review Logs:**
   Look for "Error starting process" messages in the log file

### Email Notifications Not Working

1. **Check AWS Credentials:**
   ```powershell
   # If using credentials, test them
   aws ses get-send-quota --region us-east-1
   ```

2. **Verify Email Addresses:**
   - Sender must be verified in AWS SES
   - Recipients must be verified if in Sandbox mode

3. **Check Logs:**
   Look for AWS-related errors in the watchdog log

### High CPU Usage

If the watchdog is using too much CPU:

1. **Increase Check Interval:**
   ```json
   {
     "WatchdogSettings": {
       "CheckIntervalSeconds": 300
     }
   }
   ```

2. **Restart the service** after changing configuration

## Uninstall

```powershell
# Stop the service
sc.exe stop "Robot.ED.FacebookConnector.Watchdog"

# Delete the service
sc.exe delete "Robot.ED.FacebookConnector.Watchdog"

# Remove files (optional)
Remove-Item -Path "C:\Services\Watchdog" -Recurse -Force
```

## Updating the Service

When you need to update the watchdog:

```powershell
# Stop the service
sc.exe stop "Robot.ED.FacebookConnector.Watchdog"

# Build and publish new version
cd C:\Path\To\Workspace2
dotnet publish Robot.ED.FacebookConnector.Service.Watchdog -c Release -o C:\Services\Watchdog

# Start the service
sc.exe start "Robot.ED.FacebookConnector.Watchdog"
```

**Note:** Configuration files (appsettings.json) will be overwritten. Back them up first!

## Best Practices

1. **Backup Configuration:** Always backup `appsettings.json` before updating
2. **Monitor Logs:** Check logs regularly for issues
3. **Use IAM Roles:** In production, prefer IAM roles over hardcoded credentials
4. **Set Appropriate Intervals:** Balance between responsiveness and resource usage
5. **Test Before Production:** Test email notifications in a non-production environment first
6. **Secure Credentials:** Use Windows DPAPI or Azure Key Vault for sensitive data

## Support

For issues or questions:
- Check `IMPLEMENTATION_NOTES.md` for technical details
- Review logs in `C:\Services\Watchdog\logs\`
- Check Windows Event Viewer
- Consult the README.md in the project directory

## Additional Resources

- [AWS SES Documentation](https://docs.aws.amazon.com/ses/)
- [.NET Generic Host Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [Serilog Documentation](https://serilog.net/)
