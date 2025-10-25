# Entity Framework Migrations - Quick Reference

## Installation

```bash
dotnet tool install --global dotnet-ef
```

## Helper Scripts Quick Reference

### List Migrations

```bash
# Linux/macOS                          # Windows
./migrations-list.sh api               migrations-list.bat api
./migrations-list.sh dashboard all     migrations-list.bat dashboard all
```

### Add Migration

```bash
# Linux/macOS                                              # Windows
./migrations-add.sh api MigrationName                      migrations-add.bat api MigrationName
./migrations-add.sh dashboard MigrationName AppDbContext   migrations-add.bat dashboard MigrationName AppDbContext
```

### Update Database

```bash
# Linux/macOS                                  # Windows
./migrations-update.sh api                     migrations-update.bat api
./migrations-update.sh dashboard AppDbContext  migrations-update.bat dashboard AppDbContext
```

### Remove Last Migration

```bash
# Linux/macOS                                  # Windows
./migrations-remove.sh api                     migrations-remove.bat api
./migrations-remove.sh dashboard AppDbContext  migrations-remove.bat dashboard AppDbContext
```

## Dashboard Context Cheat Sheet

The Dashboard has TWO contexts - choose the right one:

| Context | Use For | Migrations Location |
|---------|---------|-------------------|
| `ApplicationDbContext` | ASP.NET Identity (users, roles, claims) | `Migrations/` |
| `AppDbContext` | Application data (queues, robots, tokens) | `Migrations/AppDb/` |

## Manual Commands

When you need more control, use these direct commands:

### API Project

```bash
cd Robot.ED.FacebookConnector.Service.API
dotnet ef migrations list
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef migrations remove
```

### Dashboard Project - Identity

```bash
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations list --context ApplicationDbContext
dotnet ef migrations add MigrationName --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
dotnet ef migrations remove --context ApplicationDbContext
```

### Dashboard Project - Shared Data

```bash
cd Robot.ED.FacebookConnector.Dashboard
dotnet ef migrations list --context AppDbContext
dotnet ef migrations add MigrationName --context AppDbContext --output-dir Migrations/AppDb
dotnet ef database update --context AppDbContext
dotnet ef migrations remove --context AppDbContext
```

## Common Errors

**Error:** "More than one DbContext was found"  
**Fix:** Use `--context` parameter or helper scripts

**Error:** "dotnet-ef does not exist"  
**Fix:** Run `dotnet tool install --global dotnet-ef`

**Error:** Migrations in wrong directory  
**Fix:** Use `--output-dir Migrations/AppDb` for AppDbContext or use helper scripts

## More Information

See [MIGRATIONS.md](MIGRATIONS.md) for detailed documentation with examples and troubleshooting.
