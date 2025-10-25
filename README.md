# Robot.ED.FacebookConnector System

A comprehensive .NET 8.0 solution for RPA (Robotic Process Automation) orchestration with Facebook integration, featuring an API orchestrator, RPA service with Selenium automation, and a real-time monitoring dashboard.

## Architecture Overview

The system consists of 4 main components:

1. **Robot.ED.FacebookConnector.Common** - Shared library with models, DTOs, validators, and database configuration
2. **Robot.ED.FacebookConnector.Service.API** - REST API orchestrator that manages RPA allocation and request queuing
3. **Robot.ED.FacebookConnector.Service.RPA** - RPA service that performs browser automation using Selenium
4. **Robot.ED.FacebookConnector.Dashboard** - Web dashboard for real-time monitoring with role-based access

## Technologies Used

- **.NET 8.0** - Core framework
- **ASP.NET Core** - Web APIs and Razor Pages
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM
- **FluentValidation** - Request validation
- **Selenium WebDriver** - Browser automation
- **ASP.NET Core Identity** - Authentication and authorization
- **Bootstrap 5** - UI framework
- **Chart.js** - Data visualization

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12 or higher
- Chrome/Chromium browser (for RPA service)
- ChromeDriver (compatible with your Chrome version)
- Entity Framework Core Tools (for database migrations): `dotnet tool install --global dotnet-ef`

## Database Configuration

All projects use the same PostgreSQL database: `robotfacebookconnector`

Default connection string:
```
Host=localhost;Port=5432;Database=robotfacebookconnector;Username=postgres;Password=postgres
```

## Quick Start

### 1. Setup Database

```bash
# Create PostgreSQL database
createdb robotfacebookconnector

# Or using psql
psql -U postgres
CREATE DATABASE robotfacebookconnector;
```

### 2. Build Solution

```bash
cd /path/to/Workspace2
dotnet build
```

### 3. Run Projects

Open 3 separate terminals:

**Terminal 1 - API Orchestrator:**
```bash
cd Robot.ED.FacebookConnector.Service.API
dotnet run
# Access: https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

**Terminal 2 - RPA Service:**
```bash
cd Robot.ED.FacebookConnector.Service.RPA
dotnet run
# Access: https://localhost:8081
# Swagger: https://localhost:8081/swagger
```

**Terminal 3 - Dashboard:**
```bash
cd Robot.ED.FacebookConnector.Dashboard
dotnet run
# Access: https://localhost:7001
```

### 4. Default Credentials

**Dashboard Login:**
- Username: `admin`
- Password: `admin@1932`
- Role: Admin

**API Token:**
- Token: `default-token` (configured in database seed)

## Entity Framework Migrations

This project uses Entity Framework Core for database management. Due to the presence of multiple DbContext classes in some projects, special care is needed when working with migrations.

### Installing EF Core Tools

First, ensure you have the EF Core tools installed:

```bash
dotnet tool install --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

### Understanding the DbContext Structure

- **API Project** (`Robot.ED.FacebookConnector.Service.API`):
  - Single DbContext: `AppDbContext`
  - Migrations location: `Migrations/`
  
- **Dashboard Project** (`Robot.ED.FacebookConnector.Dashboard`):
  - **Two DbContexts** (requires `--context` parameter):
    - `ApplicationDbContext` - For ASP.NET Identity tables (users, roles, etc.)
      - Migrations location: `Migrations/`
    - `AppDbContext` - For shared application data (queues, robots, etc.)
      - Migrations location: `Migrations/AppDb/`

### Using Helper Scripts (Recommended)

To simplify working with migrations, we provide helper scripts for both Linux/macOS and Windows:

#### Linux/macOS Scripts

```bash
# List migrations
./migrations-list.sh api                              # API project
./migrations-list.sh dashboard ApplicationDbContext   # Dashboard Identity
./migrations-list.sh dashboard AppDbContext          # Dashboard Shared Data
./migrations-list.sh dashboard all                   # Both Dashboard contexts

# Add a new migration
./migrations-add.sh api MigrationName
./migrations-add.sh dashboard MigrationName ApplicationDbContext
./migrations-add.sh dashboard MigrationName AppDbContext

# Update database
./migrations-update.sh api
./migrations-update.sh dashboard ApplicationDbContext
./migrations-update.sh dashboard AppDbContext

# Remove last migration
./migrations-remove.sh api
./migrations-remove.sh dashboard ApplicationDbContext
./migrations-remove.sh dashboard AppDbContext
```

#### Windows Scripts

```batch
REM List migrations
migrations-list.bat api
migrations-list.bat dashboard ApplicationDbContext
migrations-list.bat dashboard AppDbContext
migrations-list.bat dashboard all

REM Add a new migration
migrations-add.bat api MigrationName
migrations-add.bat dashboard MigrationName ApplicationDbContext
migrations-add.bat dashboard MigrationName AppDbContext

REM Update database
migrations-update.bat api
migrations-update.bat dashboard ApplicationDbContext
migrations-update.bat dashboard AppDbContext

REM Remove last migration
migrations-remove.bat api
migrations-remove.bat dashboard ApplicationDbContext
migrations-remove.bat dashboard AppDbContext
```

### Manual Migration Commands

If you prefer to use `dotnet ef` commands directly:

#### API Project (Single DbContext)

```bash
cd Robot.ED.FacebookConnector.Service.API

# List migrations
dotnet ef migrations list

# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

#### Dashboard Project (Multiple DbContexts)

**Important:** The Dashboard project has TWO DbContexts, so you MUST specify which one using the `--context` parameter.

**For Identity tables (users, roles, etc.):**
```bash
cd Robot.ED.FacebookConnector.Dashboard

# List migrations
dotnet ef migrations list --context ApplicationDbContext

# Add migration
dotnet ef migrations add MigrationName --context ApplicationDbContext

# Update database
dotnet ef database update --context ApplicationDbContext

# Remove last migration
dotnet ef migrations remove --context ApplicationDbContext
```

**For shared application data (queues, robots, etc.):**
```bash
cd Robot.ED.FacebookConnector.Dashboard

# List migrations
dotnet ef migrations list --context AppDbContext

# Add migration
dotnet ef migrations add MigrationName --context AppDbContext --output-dir Migrations/AppDb

# Update database
dotnet ef database update --context AppDbContext

# Remove last migration
dotnet ef migrations remove --context AppDbContext
```

### Common Issues and Solutions

**Problem:** "More than one DbContext was found. Specify which one to use."

**Solution:** This happens in the Dashboard project because it has two DbContexts. Always use the `--context` parameter or the helper scripts.

**Problem:** Migrations are created in the wrong directory.

**Solution:** For `AppDbContext` in the Dashboard project, always use `--output-dir Migrations/AppDb` to maintain consistency with existing migrations.

**Problem:** "Unable to create an object of type 'AppDbContext'"

**Solution:** Make sure you're in the correct project directory and that the connection string in `appsettings.json` is properly configured.

## Project Details

### API Orchestrator (Port 5000/5001)

**Endpoints:**
- `POST /api/rpa/allocate` - Submit new request to queue
- `POST /api/rpa/result` - Receive processing results from RPA
- `GET /api/health` - Health check

**Features:**
- Token-based authentication
- Background service for RPA allocation (60s interval)
- Automatic retry logic (3 attempts)
- RPA timeout management (5 minutes)
- Webhook forwarding to external endpoint
- Data expiration (90 days)
- Memory cleanup after processing

### RPA Service (Port 8080/8081)

**Endpoints:**
- `POST /api/rpa/process` - Process automation request
- `GET /api/health` - Health check

**Features:**
- Selenium WebDriver integration (headless Chrome)
- Facebook login automation
- Error screenshot capture
- 10-minute processing timeout
- Async result callback to orchestrator
- Data and file expiration (90 days)

### Dashboard (Port 7000/7001)

**Features:**
- Real-time statistics dashboard
- Auto-refresh (30 seconds, configurable)
- User authentication with ASP.NET Core Identity
- Role-based access control (Admin/User)
- User action logging
- Modern UI with Bootstrap 5 and Chart.js
- Metrics displayed:
  - Pending queue count
  - Processed today count
  - Success/Error counts
  - Available/Occupied robots
  - Most active robot
  - Last processing time
  - Recent queue items
  - Robot statuses

## API Usage Examples

### Submit Request to Queue

```bash
curl -X POST https://localhost:5001/api/rpa/allocate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer default-token" \
  -d '{
    "aiConfig": "config1",
    "trackId": "TRACK-001",
    "bridgeKey": "bridge123",
    "originType": "web",
    "mediaId": "media456",
    "customer": "Customer A",
    "channel": "facebook",
    "phrase": "test phrase",
    "tags": ["tag1", "tag2"],
    "data": [
      {"header": "key1", "value": "value1"}
    ]
  }'
```

## Configuration

### API Orchestrator (appsettings.json)

```json
{
  "OrchestratorSettings": {
    "AllocationIntervalSeconds": 60,
    "RobotTimeoutMinutes": 5,
    "MaxRetryAttempts": 3,
    "DataRetentionDays": 90,
    "WebhookUrl": "https://webhook.site/...",
    "WebhookBearerToken": "..."
  }
}
```

### RPA Service (appsettings.json)

```json
{
  "RpaSettings": {
    "ProcessTimeoutMinutes": 10,
    "DataRetentionDays": 90,
    "ScreenshotPath": "screenshots",
    "OrchestratorUrl": "http://localhost:5000",
    "OrchestratorToken": "default-token",
    "FacebookUsername": "...",
    "FacebookPassword": "..."
  }
}
```

### Dashboard (appsettings.json)

```json
{
  "DashboardSettings": {
    "RefreshIntervalSeconds": 30
  }
}
```

## Database Schema

- **queue** - Request queue items
- **queue_tag** - Tags associated with queue items
- **queue_data** - Additional data for queue items
- **queue_result** - Processing results
- **queue_result_message** - Result messages
- **queue_result_attachment** - Result attachments
- **robot** - RPA robot configurations
- **token** - API authentication tokens
- **user** - RPA service users
- **AspNetUsers** - Dashboard users (Identity)
- **user_action_log** - User action logs

## Troubleshooting

### Chrome/ChromeDriver Issues

If you encounter ChromeDriver errors:

1. Check your Chrome version: `google-chrome --version`
2. Download matching ChromeDriver from: https://chromedriver.chromium.org/
3. Add ChromeDriver to PATH or place in project directory

### Database Connection Issues

1. Verify PostgreSQL is running: `pg_isready`
2. Check connection string in appsettings.json
3. Ensure database exists: `psql -l | grep robotfacebookconnector`

### Token Authentication Issues

1. Verify token exists in database
2. Check Authorization header format: `Bearer <token>`
3. Ensure middleware is configured correctly

## Development

### Running Migrations

```bash
# API project
cd Robot.ED.FacebookConnector.Service.API
dotnet ef migrations add MigrationName
dotnet ef database update

# Dashboard project
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Adding New Features

1. Add models to Common library
2. Update DbContext and create migrations
3. Implement business logic in Services
4. Add controllers/pages as needed
5. Update Swagger documentation

## Testing

The system includes:
- Health check endpoints for monitoring
- Swagger UI for API testing
- Comprehensive logging
- Error handling with screenshots (RPA)

## Security Considerations

1. Change default passwords and tokens in production
2. Use environment variables for sensitive data
3. Enable HTTPS in production
4. Implement rate limiting for API endpoints
5. Regular token rotation
6. Monitor user action logs

## Performance

- Background services run asynchronously
- Memory cleanup after each processing
- Database indexes on frequently queried columns
- Connection pooling for database
- Configurable timeout and retry logic

## License

Proprietary - All rights reserved

## Support

For issues and questions, please create an issue in the repository.