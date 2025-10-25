# Entity Framework Migrations Guide

This guide explains how to work with Entity Framework Core migrations in the Robot.ED.FacebookConnector project.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Quick Start](#quick-start)
- [Helper Scripts](#helper-scripts)
- [Manual Commands](#manual-commands)
- [Common Scenarios](#common-scenarios)
- [Troubleshooting](#troubleshooting)

## Overview

This project uses Entity Framework Core for database management with PostgreSQL. The solution has a unique structure where:

- The **API project** has a single DbContext
- The **Dashboard project** has TWO DbContexts (requiring special handling)

Understanding this structure is crucial for successfully managing migrations.

## Prerequisites

1. **.NET 8.0 SDK** - Make sure it's installed
2. **Entity Framework Core Tools** - Install globally:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. Verify installation:
   ```bash
   dotnet ef --version
   ```
   You should see version 8.0 or higher.

## Project Structure

### API Project (`Robot.ED.FacebookConnector.Service.API`)

- **DbContext:** `AppDbContext`
- **Migrations Location:** `Migrations/`
- **Complexity:** Simple (single DbContext)

### Dashboard Project (`Robot.ED.FacebookConnector.Dashboard`)

- **DbContext 1:** `ApplicationDbContext`
  - **Purpose:** ASP.NET Identity (users, roles, claims, etc.)
  - **Migrations Location:** `Migrations/`
  
- **DbContext 2:** `AppDbContext`
  - **Purpose:** Shared application data (queues, robots, tokens, etc.)
  - **Migrations Location:** `Migrations/AppDb/`

⚠️ **Important:** The Dashboard project requires the `--context` parameter for ALL EF Core commands because it has multiple DbContexts.

## Quick Start

### For API Project

```bash
# Navigate to the project
cd Robot.ED.FacebookConnector.Service.API

# List migrations
dotnet ef migrations list

# Add a new migration
dotnet ef migrations add YourMigrationName

# Update database
dotnet ef database update
```

### For Dashboard Project

You must specify which DbContext you're working with:

**For Identity tables:**
```bash
cd Robot.ED.FacebookConnector.Dashboard

# List migrations
dotnet ef migrations list --context ApplicationDbContext

# Add migration
dotnet ef migrations add YourMigrationName --context ApplicationDbContext

# Update database
dotnet ef database update --context ApplicationDbContext
```

**For shared data tables:**
```bash
cd Robot.ED.FacebookConnector.Dashboard

# List migrations
dotnet ef migrations list --context AppDbContext

# Add migration (note the output directory!)
dotnet ef migrations add YourMigrationName --context AppDbContext --output-dir Migrations/AppDb

# Update database
dotnet ef database update --context AppDbContext
```

## Helper Scripts

To simplify migration management, we provide helper scripts for both platforms.

### Linux/macOS Scripts

All scripts are in the repository root and are executable (`.sh` extension).

#### Listing Migrations

```bash
# API project
./migrations-list.sh api

# Dashboard - Identity context
./migrations-list.sh dashboard ApplicationDbContext

# Dashboard - Shared data context
./migrations-list.sh dashboard AppDbContext

# Dashboard - Both contexts
./migrations-list.sh dashboard all
```

#### Adding Migrations

```bash
# API project
./migrations-add.sh api MigrationName

# Dashboard - Identity context
./migrations-add.sh dashboard MigrationName ApplicationDbContext

# Dashboard - Shared data context
./migrations-add.sh dashboard MigrationName AppDbContext
```

#### Updating Database

```bash
# API project
./migrations-update.sh api

# Dashboard - Identity context
./migrations-update.sh dashboard ApplicationDbContext

# Dashboard - Shared data context
./migrations-update.sh dashboard AppDbContext
```

#### Removing Last Migration

```bash
# API project
./migrations-remove.sh api

# Dashboard - Identity context
./migrations-remove.sh dashboard ApplicationDbContext

# Dashboard - Shared data context
./migrations-remove.sh dashboard AppDbContext
```

### Windows Scripts

All scripts are in the repository root with `.bat` extension.

Replace `.sh` with `.bat` in the above commands. For example:

```batch
REM API project
migrations-list.bat api

REM Dashboard - Both contexts
migrations-list.bat dashboard all

REM Add migration to API
migrations-add.bat api MigrationName

REM Add migration to Dashboard Identity
migrations-add.bat dashboard MigrationName ApplicationDbContext
```

## Manual Commands

### Complete Command Reference

#### List Migrations

```bash
# API
cd Robot.ED.FacebookConnector.Service.API
dotnet ef migrations list

# Dashboard Identity
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations list --context ApplicationDbContext

# Dashboard Shared Data
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations list --context AppDbContext
```

#### Add Migration

```bash
# API
cd Robot.ED.FacebookConnector.Service.API
dotnet ef migrations add MigrationName

# Dashboard Identity
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations add MigrationName --context ApplicationDbContext

# Dashboard Shared Data (with output directory)
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations add MigrationName --context AppDbContext --output-dir Migrations/AppDb
```

#### Update Database

```bash
# API
cd Robot.ED.FacebookConnector.Service.API
dotnet ef database update

# Dashboard Identity
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef database update --context ApplicationDbContext

# Dashboard Shared Data
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef database update --context AppDbContext
```

#### Update to Specific Migration

```bash
# API
cd Robot.ED.FacebookConnector.Service.API
dotnet ef database update MigrationName

# Dashboard Identity
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef database update MigrationName --context ApplicationDbContext

# Dashboard Shared Data
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef database update MigrationName --context AppDbContext
```

#### Remove Last Migration

```bash
# API
cd Robot.ED.FacebookConnector.Service.API
dotnet ef migrations remove

# Dashboard Identity
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations remove --context ApplicationDbContext

# Dashboard Shared Data
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations remove --context AppDbContext
```

#### Generate SQL Script

```bash
# API
cd Robot.ED.FacebookConnector.Service.API
dotnet ef migrations script

# Dashboard Identity
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations script --context ApplicationDbContext

# Dashboard Shared Data
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations script --context AppDbContext
```

## Common Scenarios

### Scenario 1: Adding a New Property to an Existing Model

Let's say you want to add a `Description` property to the `Robot` model.

1. **Modify the model** in `Robot.ED.FacebookConnector.Common/Models/Robot.cs`
2. **Add migration:**
   - If working with API: `./migrations-add.sh api AddDescriptionToRobot`
   - If working with Dashboard: `./migrations-add.sh dashboard AddDescriptionToRobot AppDbContext`
3. **Review the generated migration** in the Migrations folder
4. **Update database:**
   - If working with API: `./migrations-update.sh api`
   - If working with Dashboard: `./migrations-update.sh dashboard AppDbContext`

### Scenario 2: Creating a New Table

Let's say you want to add a new `Settings` table.

1. **Create the model class** in `Robot.ED.FacebookConnector.Common/Models/Settings.cs`
2. **Add DbSet to AppDbContext** in `Robot.ED.FacebookConnector.Common/Configuration/AppDbContext.cs`:
   ```csharp
   public DbSet<Settings> Settings { get; set; } = null!;
   ```
3. **Configure the entity** in `OnModelCreating`:
   ```csharp
   modelBuilder.Entity<Settings>(entity =>
   {
       entity.ToTable("settings");
       entity.HasKey(e => e.Id);
   });
   ```
4. **Add migration:**
   - API: `./migrations-add.sh api AddSettingsTable`
   - Dashboard: `./migrations-add.sh dashboard AddSettingsTable AppDbContext`
5. **Update database:** `./migrations-update.sh api` or `./migrations-update.sh dashboard AppDbContext`

### Scenario 3: Adding Identity-Related Fields

If you need to add custom fields to the Identity user:

1. **Modify** `Robot.ED.FacebookConnector.Dashboard/Models/ApplicationUser.cs`
2. **Add migration:**
   ```bash
   ./migrations-add.sh dashboard AddCustomFieldsToUser ApplicationDbContext
   ```
3. **Update database:**
   ```bash
   ./migrations-update.sh dashboard ApplicationDbContext
   ```

### Scenario 4: Reverting a Migration

If you need to undo the last migration:

1. **Remove the migration file:** `./migrations-remove.sh api` (or appropriate dashboard context)
2. **If already applied to database**, first revert:
   ```bash
   dotnet ef database update PreviousMigrationName --context AppDbContext
   ```
   Then remove: `./migrations-remove.sh dashboard AppDbContext`

### Scenario 5: Fresh Database Setup

To create a fresh database with all migrations:

```bash
# Drop and recreate database in PostgreSQL
psql -U postgres
DROP DATABASE IF EXISTS robotfacebookconnector;
CREATE DATABASE robotfacebookconnector;
\q

# Apply all migrations
./migrations-update.sh api
./migrations-update.sh dashboard ApplicationDbContext
./migrations-update.sh dashboard AppDbContext
```

## Troubleshooting

### Problem: "More than one DbContext was found"

**Error Message:**
```
More than one DbContext was found. Specify which one to use.
```

**Solution:**
This happens in the Dashboard project. Always use the `--context` parameter or the helper scripts.

**Correct:**
```bash
./migrations-list.sh dashboard ApplicationDbContext
```

**Incorrect:**
```bash
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations list  # ❌ Missing --context
```

### Problem: "Could not execute because the specified command or file was not found"

**Error Message:**
```
Could not execute because dotnet-ef does not exist.
```

**Solution:**
Install EF Core tools:
```bash
dotnet tool install --global dotnet-ef
```

Then restart your terminal/shell.

### Problem: Migrations Created in Wrong Directory

**Symptoms:** `AppDbContext` migrations appear in `Migrations/` instead of `Migrations/AppDb/`

**Solution:**
Always use `--output-dir Migrations/AppDb` when adding migrations for `AppDbContext` in the Dashboard:
```bash
dotnet ef migrations add MigrationName --context AppDbContext --output-dir Migrations/AppDb
```

Or use the helper script which does this automatically:
```bash
./migrations-add.sh dashboard MigrationName AppDbContext
```

### Problem: "Unable to create an object of type 'AppDbContext'"

**Possible Causes:**
1. Not in the correct project directory
2. Project doesn't build
3. Connection string is missing or invalid

**Solution:**
1. Ensure you're in the correct project directory
2. Build the project: `dotnet build`
3. Check `appsettings.json` has a valid connection string
4. Use the helper scripts which handle directory navigation automatically

### Problem: "Failed to connect to database"

**Symptoms:**
```
An error occurred while accessing the database.
Error: Failed to connect to 127.0.0.1:5432
```

**Solution:**
This is usually a warning, not an error. The migration commands will still work, but EF Core can't determine which migrations are applied to the database.

To fix:
1. Ensure PostgreSQL is running: `sudo systemctl status postgresql` (Linux) or check services (Windows)
2. Verify connection string in `appsettings.json`
3. Test connection: `psql -h localhost -U postgres -d robotfacebookconnector`

### Problem: Migration Already Applied to Database

**Error Message:**
```
The migration 'MigrationName' has already been applied to the database.
```

**Solution:**
If you want to remove it:
1. First, revert the database to the previous migration:
   ```bash
   dotnet ef database update PreviousMigrationName --context AppDbContext
   ```
2. Then remove the migration file:
   ```bash
   dotnet ef migrations remove --context AppDbContext
   ```

## Best Practices

1. **Always use helper scripts** - They handle context selection and output directories automatically
2. **Use descriptive migration names** - e.g., `AddEmailToRobot` instead of `Update1`
3. **Review migrations before applying** - Check the generated code in the Migrations folder
4. **Test in development first** - Never apply migrations directly to production without testing
5. **Keep migrations small** - One logical change per migration
6. **Don't modify applied migrations** - Create a new migration instead
7. **Back up production data** - Before applying migrations to production databases
8. **Use version control** - Commit migrations to git along with model changes

## Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Migrations Overview](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Tools Reference](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)
- [PostgreSQL with EF Core](https://www.npgsql.org/efcore/)
