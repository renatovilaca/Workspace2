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
