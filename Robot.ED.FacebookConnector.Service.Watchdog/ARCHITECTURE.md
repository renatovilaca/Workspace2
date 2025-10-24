# Watchdog Service Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Watchdog Service Architecture                 │
└─────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────┐
│                          Worker Service (Host)                         │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │                   Background Worker                              │  │
│  │  - Runs continuously with configurable interval                 │  │
│  │  - Coordinates monitoring and notifications                     │  │
│  │  - Tracks application states                                    │  │
│  └──────────────────┬─────────────────────┬────────────────────────┘  │
│                     │                     │                            │
│                     ▼                     ▼                            │
│  ┌──────────────────────────┐  ┌─────────────────────────────────┐   │
│  │ ProcessMonitoringService │  │ EmailNotificationService        │   │
│  │                          │  │                                 │   │
│  │ - Check if running       │  │ - Send stop notifications       │   │
│  │ - Start processes        │  │ - Send restart notifications    │   │
│  │ - Handle errors          │  │ - AWS SES integration           │   │
│  └────────┬─────────────────┘  └────────┬────────────────────────┘   │
│           │                              │                             │
│           ▼                              ▼                             │
│  ┌──────────────────────────┐  ┌─────────────────────────────────┐   │
│  │   System.Diagnostics     │  │         AWS SES                 │   │
│  │   Process Management     │  │   Simple Email Service          │   │
│  └──────────────────────────┘  └─────────────────────────────────┘   │
└───────────────────────────────────────────────────────────────────────┘
                     │                              │
                     ▼                              ▼
        ┌─────────────────────────┐   ┌───────────────────────────┐
        │ Monitored Applications  │   │  Email Recipients         │
        │                         │   │                           │
        │ - API Service           │   │ - admin@example.com       │
        │ - RPA Service           │   │ - team@example.com        │
        │ - Custom Apps           │   │ - ops@example.com         │
        └─────────────────────────┘   └───────────────────────────┘
```

## Component Interaction Flow

```
┌─────────┐
│ START   │
└────┬────┘
     │
     ▼
┌─────────────────────────────────────┐
│ 1. Initialize Services              │
│    - Load Configuration             │
│    - Setup Serilog                  │
│    - Initialize AWS SES Client      │
│    - Register DI Services           │
└────┬────────────────────────────────┘
     │
     ▼
┌─────────────────────────────────────┐
│ 2. Start Background Worker          │
│    - Begin monitoring loop          │
│    - Log monitored applications     │
└────┬────────────────────────────────┘
     │
     │  ┌────────────────────────────┐
     └──► 3. For Each Application    │
        └────┬───────────────────────┘
             │
             ▼
        ┌──────────────────────────────┐
        │ 4. Check if Running          │
        │    - Get process by name     │
        └────┬─────────────┬───────────┘
             │             │
       YES   │             │   NO
             │             │
             ▼             ▼
        ┌─────────┐   ┌──────────────────────┐
        │ Update  │   │ 5. Send Stop Email   │
        │ State   │   │    (if was running)  │
        └─────────┘   └────┬─────────────────┘
                           │
                           ▼
                      ┌──────────────────────┐
                      │ 6. Start Process     │
                      │    - Use configured  │
                      │      executable path │
                      │    - Set working dir │
                      │    - Pass arguments  │
                      └────┬─────────────────┘
                           │
                      ┌────┴────┐
                      │         │
                 SUCCESS    FAILED
                      │         │
                      ▼         ▼
            ┌──────────────┐  ┌────────────┐
            │ 7. Send      │  │ Log Error  │
            │    Restart   │  │            │
            │    Email     │  └────────────┘
            └──────────────┘
                      │
                      ▼
        ┌────────────────────────────────┐
        │ 8. Wait for Check Interval     │
        │    (e.g., 60 seconds)          │
        └────┬───────────────────────────┘
             │
             │
             └──────────────┐
                           │
                           ▼
                    ┌──────────────┐
                    │ Loop Back to │
                    │ Step 3       │
                    └──────────────┘
```

## Data Flow Diagram

```
Configuration (appsettings.json)
         │
         ├──► WatchdogSettings ──────────┐
         │    - CheckIntervalSeconds      │
         │    - Applications[]            │
         │                                │
         └──► EmailSettings ──────────────┤
              - NotificationEnabled       │
              - SenderEmail              │
              - Recipients[]             │
              - AwsRegion                │
              - AwsAccessKeyId           │
              - AwsSecretAccessKey       │
                                         │
                                         ▼
                              ┌──────────────────────┐
                              │   Worker Service     │
                              │  (Dependency Inject) │
                              └──────────────────────┘
                                         │
                    ┌────────────────────┼─────────────────────┐
                    │                    │                     │
                    ▼                    ▼                     ▼
         ┌────────────────────┐  ┌──────────────┐  ┌───────────────────┐
         │ Process Monitoring │  │   Worker     │  │ Email Notification│
         │     Service        │  │   Logic      │  │     Service       │
         └────────────────────┘  └──────────────┘  └───────────────────┘
                    │                    │                     │
                    ▼                    ▼                     ▼
         ┌────────────────────┐  ┌──────────────┐  ┌───────────────────┐
         │  Process.Start()   │  │  State       │  │  SES.SendEmail()  │
         │  Process.GetBy...  │  │  Tracking    │  │                   │
         └────────────────────┘  └──────────────┘  └───────────────────┘
                    │                    │                     │
                    ▼                    ▼                     ▼
         ┌────────────────────┐  ┌──────────────┐  ┌───────────────────┐
         │  Windows Process   │  │  In-Memory   │  │   AWS SES API     │
         │     Manager        │  │  Dictionary  │  │                   │
         └────────────────────┘  └──────────────┘  └───────────────────┘
                    │                                        │
                    ▼                                        ▼
         ┌────────────────────┐              ┌──────────────────────────┐
         │ Monitored Apps     │              │  Email Recipients        │
         │ - API Service      │              │  - Inbox                 │
         │ - RPA Service      │              └──────────────────────────┘
         └────────────────────┘
                    │
                    ▼
                  Serilog
                    │
                    ▼
         ┌────────────────────┐
         │ Log Files          │
         │ logs/watchdog-     │
         │   YYYYMMDD.log     │
         └────────────────────┘
```

## Class Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                      Configuration Models                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────┐  ┌─────────────────────────┐ │
│  │   WatchdogSettings           │  │   EmailSettings         │ │
│  ├──────────────────────────────┤  ├─────────────────────────┤ │
│  │ + CheckIntervalSeconds: int  │  │ + NotificationEnabled   │ │
│  │ + Applications: List<App>    │  │ + SenderEmail: string   │ │
│  └──────────────────────────────┘  │ + Recipients: List<str> │ │
│                                     │ + AwsRegion: string     │ │
│  ┌──────────────────────────────┐  │ + AwsAccessKeyId        │ │
│  │  MonitoredApplication        │  │ + AwsSecretAccessKey    │ │
│  ├──────────────────────────────┤  └─────────────────────────┘ │
│  │ + Name: string               │                              │
│  │ + ProcessName: string        │                              │
│  │ + ExecutablePath: string     │                              │
│  │ + Arguments: string?         │                              │
│  │ + WorkingDirectory: string?  │                              │
│  └──────────────────────────────┘                              │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                           Services                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  IProcessMonitoringService                               │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ + IsProcessRunning(app): bool                            │  │
│  │ + StartProcess(app): Process?                            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                          ▲                                      │
│                          │ implements                           │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  ProcessMonitoringService                                │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ - _logger: ILogger                                       │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ + IsProcessRunning(app): bool                            │  │
│  │ + StartProcess(app): Process?                            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  IEmailNotificationService                               │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ + SendApplicationStoppedNotificationAsync(name)          │  │
│  │ + SendApplicationRestartedNotificationAsync(name)        │  │
│  └──────────────────────────────────────────────────────────┘  │
│                          ▲                                      │
│                          │ implements                           │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  EmailNotificationService                                │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ - _emailSettings: EmailSettings                          │  │
│  │ - _logger: ILogger                                       │  │
│  │ - _sesClient: IAmazonSimpleEmailService?                 │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ + SendApplicationStoppedNotificationAsync(name)          │  │
│  │ + SendApplicationRestartedNotificationAsync(name)        │  │
│  │ - SendEmailAsync(subject, htmlBody)                      │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      Background Worker                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Worker : BackgroundService                              │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ - _logger: ILogger                                       │  │
│  │ - _watchdogSettings: WatchdogSettings                    │  │
│  │ - _processMonitoringService: IProcessMonitoringService   │  │
│  │ - _emailNotificationService: IEmailNotificationService   │  │
│  │ - _applicationStates: Dictionary<string, bool>           │  │
│  ├──────────────────────────────────────────────────────────┤  │
│  │ + ExecuteAsync(CancellationToken): Task                  │  │
│  │ - CheckAndRestartApplicationsAsync(): Task               │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Windows Server / VM                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Windows Service Control Manager               │ │
│  │                                                            │ │
│  │  ┌──────────────────────────────────────────────────────┐ │ │
│  │  │  Robot.ED.FacebookConnector.Watchdog Service         │ │ │
│  │  │                                                      │ │ │
│  │  │  Service Name: Robot.ED.FacebookConnector.Watchdog   │ │ │
│  │  │  Status: Running                                     │ │ │
│  │  │  Startup Type: Automatic                             │ │ │
│  │  │  Recovery: Restart on failure                        │ │ │
│  │  └──────────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Application Files                             │ │
│  │                                                            │ │
│  │  C:\Services\Watchdog\                                     │ │
│  │  ├── Robot.ED.FacebookConnector.Service.Watchdog.exe      │ │
│  │  ├── appsettings.json                                      │ │
│  │  ├── appsettings.Development.json                          │ │
│  │  ├── *.dll (dependencies)                                  │ │
│  │  └── logs\                                                 │ │
│  │      └── watchdog-YYYYMMDD.log                             │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Monitored Applications                        │ │
│  │                                                            │ │
│  │  C:\Services\API\                                          │ │
│  │  └── Robot.ED.FacebookConnector.Service.API.exe            │ │
│  │                                                            │ │
│  │  C:\Services\RPA\                                          │ │
│  │  └── Robot.ED.FacebookConnector.Service.RPA.exe            │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTPS (443)
                              ▼
                    ┌─────────────────────┐
                    │     AWS Cloud       │
                    │                     │
                    │  ┌───────────────┐  │
                    │  │   AWS SES     │  │
                    │  │ (us-east-1)   │  │
                    │  └───────────────┘  │
                    └─────────────────────┘
                              │
                              │ Email Delivery
                              ▼
                    ┌─────────────────────┐
                    │  Email Recipients   │
                    │                     │
                    │  □ admin@example    │
                    │  □ team@example     │
                    │  □ ops@example      │
                    └─────────────────────┘
```

## State Machine Diagram

```
Application State Management:

   ┌─────────────────┐
   │   UNKNOWN       │  ← Initial state for each application
   └────────┬────────┘
            │ Service starts
            ▼
   ┌─────────────────┐
   │   CHECKING      │
   └────────┬────────┘
            │
     ┌──────┴──────┐
     │             │
     ▼             ▼
┌─────────┐   ┌──────────┐
│ RUNNING │   │ STOPPED  │
└────┬────┘   └────┬─────┘
     │             │
     │             │ Send stop email (if was running)
     │             ▼
     │        ┌──────────┐
     │        │RESTARTING│
     │        └────┬─────┘
     │             │
     │        ┌────┴────┐
     │        │         │
     │   SUCCESS    FAILED
     │        │         │
     │        │ Send    │ Log error
     │        │restart  │
     │        │email    │
     │        ▼         ▼
     └───► ┌─────────────────┐
           │   Next Check    │
           │   (wait N secs) │
           └────────┬────────┘
                    │
                    └──► Back to CHECKING
```

## Security Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Security Layers                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Layer 1: Windows Service Permissions                      │ │
│  │  - Service runs under specific account                     │ │
│  │  - Account has minimum required permissions                │ │
│  │  - Can start configured processes only                     │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Layer 2: Configuration File Security                      │ │
│  │  - appsettings.json protected by file system permissions   │ │
│  │  - Sensitive data can use User Secrets or Key Vault        │ │
│  │  - .gitignore prevents credential commits                  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Layer 3: AWS Credential Management                        │ │
│  │  - Prefer IAM roles over access keys                       │ │
│  │  - Support for AWS credential chain                        │ │
│  │  - Credentials encrypted in memory                         │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Layer 4: Network Security                                 │ │
│  │  - HTTPS only communication with AWS SES                   │ │
│  │  - No inbound network ports required                       │ │
│  │  - Outbound to AWS SES API only                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Layer 5: Logging Security                                 │ │
│  │  - No credentials logged                                   │ │
│  │  - Structured logging with sanitization                    │ │
│  │  - Log files protected by file system permissions          │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Performance Characteristics

```
Resource Usage Profile:
┌────────────────────────────────────────┐
│ Metric           │ Typical    │ Peak   │
├──────────────────┼────────────┼────────┤
│ CPU Usage        │ < 1%       │ 2-5%   │
│ Memory (RAM)     │ 30-50 MB   │ 80 MB  │
│ Disk I/O         │ Minimal    │ Low    │
│ Network          │ < 1 KB/min │ 50 KB  │
└────────────────────────────────────────┘

Timing Characteristics:
┌────────────────────────────────────────────────┐
│ Operation              │ Typical Duration     │
├────────────────────────┼──────────────────────┤
│ Process Check          │ < 10 ms              │
│ Process Start          │ 100-500 ms           │
│ Email Send (AWS SES)   │ 200-1000 ms          │
│ Check Cycle (default)  │ 60 seconds (config)  │
│ Service Startup        │ 1-2 seconds          │
└────────────────────────────────────────────────┘

Scalability:
┌────────────────────────────────────────┐
│ Monitored Apps  │ Check Time  │ Memory │
├─────────────────┼─────────────┼────────┤
│ 2 apps          │ < 20 ms     │ 40 MB  │
│ 5 apps          │ < 50 ms     │ 50 MB  │
│ 10 apps         │ < 100 ms    │ 70 MB  │
│ 20 apps         │ < 200 ms    │ 100 MB │
└────────────────────────────────────────┘
```

## High Availability Design

```
┌─────────────────────────────────────────────────────────────────┐
│                    Reliability Features                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Windows Service Recovery                                    │
│     ┌─────────────────────────────────────────────────────┐    │
│     │ First failure:  Restart service after 60 seconds    │    │
│     │ Second failure: Restart service after 60 seconds    │    │
│     │ Third failure:  Restart service after 60 seconds    │    │
│     └─────────────────────────────────────────────────────┘    │
│                                                                  │
│  2. Error Handling                                              │
│     ┌─────────────────────────────────────────────────────┐    │
│     │ - Try/catch blocks around all operations            │    │
│     │ - Graceful degradation on email failures            │    │
│     │ - Continues monitoring even if some apps fail       │    │
│     │ - Structured logging of all errors                  │    │
│     └─────────────────────────────────────────────────────┘    │
│                                                                  │
│  3. Monitoring & Logging                                        │
│     ┌─────────────────────────────────────────────────────┐    │
│     │ - Daily rotating logs (30-day retention)            │    │
│     │ - All operations logged with timestamps             │    │
│     │ - Exception details captured                        │    │
│     │ - Application state tracking                        │    │
│     └─────────────────────────────────────────────────────┘    │
│                                                                  │
│  4. Graceful Shutdown                                           │
│     ┌─────────────────────────────────────────────────────┐    │
│     │ - Cancellation token support                        │    │
│     │ - Cleanup on service stop                           │    │
│     │ - Serilog flush on exit                             │    │
│     └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## Integration Points

```
External Dependencies:
┌────────────────────────────────────────────────────────────────┐
│                                                                 │
│  AWS SES API                                                    │
│  ├── Endpoint: email.{region}.amazonaws.com                    │
│  ├── Protocol: HTTPS (443)                                     │
│  ├── Authentication: AWS Signature V4                          │
│  └── Operations:                                               │
│      ├── SendEmail                                             │
│      └── SendRawEmail                                          │
│                                                                 │
│  Windows Process API                                           │
│  ├── Process.GetProcessesByName()                              │
│  ├── Process.Start()                                           │
│  └── Process.Id, Process.HasExited                             │
│                                                                 │
│  .NET Configuration System                                     │
│  ├── appsettings.json                                          │
│  ├── appsettings.{Environment}.json                            │
│  ├── Environment Variables                                     │
│  └── User Secrets (Development)                                │
│                                                                 │
│  Serilog Sinks                                                 │
│  ├── Console Sink (Development/Debugging)                      │
│  └── File Sink (Production Logging)                            │
│      ├── Rolling Interval: Daily                               │
│      └── Retention: 30 days                                    │
└────────────────────────────────────────────────────────────────┘
```

## Future Enhancement Possibilities

```
┌─────────────────────────────────────────────────────────────────┐
│                    Potential Extensions                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  □ Health Check Endpoints                                       │
│    - HTTP endpoint for health status                            │
│    - Integration with monitoring tools                          │
│                                                                  │
│  □ Metrics & Monitoring                                         │
│    - Prometheus metrics export                                  │
│    - Application Insights integration                           │
│    - Performance counters                                       │
│                                                                  │
│  □ Advanced Notifications                                       │
│    - Slack integration                                          │
│    - Teams webhooks                                             │
│    - SMS via SNS                                                │
│    - Custom webhook support                                     │
│                                                                  │
│  □ Enhanced Process Management                                  │
│    - CPU/Memory usage monitoring                                │
│    - Process hang detection                                     │
│    - Graceful shutdown attempts                                 │
│    - Dependency checking                                        │
│                                                                  │
│  □ Configuration Management                                     │
│    - Hot reload of configuration                                │
│    - Azure Key Vault integration                                │
│    - Central configuration server                               │
│                                                                  │
│  □ Advanced Features                                            │
│    - Application performance trending                           │
│    - Predictive failure analysis                                │
│    - Automatic scaling triggers                                 │
│    - Multi-server coordination                                  │
└─────────────────────────────────────────────────────────────────┘
```
