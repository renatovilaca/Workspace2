@echo off
REM Helper script to add Entity Framework migrations
REM Usage: migrations-add.bat <project> <migration_name> [context]
REM
REM Examples:
REM   migrations-add.bat api AddNewColumn
REM   migrations-add.bat dashboard AddNewTable ApplicationDbContext
REM   migrations-add.bat dashboard AddSharedTable AppDbContext

setlocal enabledelayedexpansion

REM Check if dotnet ef is installed
dotnet ef --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: dotnet-ef tools not found!
    echo Please install it using: dotnet tool install --global dotnet-ef
    exit /b 1
)

REM Validate arguments
if "%~2"=="" (
    echo ERROR: Insufficient arguments
    echo.
    echo Usage: %~nx0 ^<project^> ^<migration_name^> [context]
    echo.
    echo Projects:
    echo   api       - Robot.ED.FacebookConnector.Service.API
    echo   dashboard - Robot.ED.FacebookConnector.Dashboard
    echo.
    echo Dashboard Contexts ^(required for dashboard^):
    echo   ApplicationDbContext - For Identity tables ^(default location: Migrations/^)
    echo   AppDbContext        - For shared data tables ^(default location: Migrations/AppDb/^)
    echo.
    echo Examples:
    echo   %~nx0 api AddNewColumn
    echo   %~nx0 dashboard AddNewTable ApplicationDbContext
    echo   %~nx0 dashboard AddSharedTable AppDbContext
    exit /b 1
)

set PROJECT=%~1
set MIGRATION_NAME=%~2
set CONTEXT=%~3

REM Set project path and context based on project type
if /i "%PROJECT%"=="api" (
    set PROJECT_PATH=Robot.ED.FacebookConnector.Service.API
    set CONTEXT_ARG=
    echo INFO: Adding migration to API project...
) else if /i "%PROJECT%"=="dashboard" (
    set PROJECT_PATH=Robot.ED.FacebookConnector.Dashboard
    if "%CONTEXT%"=="" (
        echo ERROR: Dashboard project requires context parameter!
        echo.
        echo Please specify the context:
        echo   ApplicationDbContext - For Identity-related tables
        echo   AppDbContext        - For shared data tables
        echo.
        echo Example: %~nx0 dashboard %MIGRATION_NAME% ApplicationDbContext
        exit /b 1
    )
    
    REM Set output directory based on context
    if /i "%CONTEXT%"=="AppDbContext" (
        set OUTPUT_DIR=Migrations/AppDb
    ) else (
        set OUTPUT_DIR=Migrations
    )
    
    set CONTEXT_ARG=--context %CONTEXT% --output-dir !OUTPUT_DIR!
    echo INFO: Adding migration to Dashboard project with context: %CONTEXT%
) else (
    echo ERROR: Invalid project: %PROJECT%
    echo Valid projects: api, dashboard
    exit /b 1
)

REM Navigate to project directory and add migration
cd "%PROJECT_PATH%"

echo INFO: Executing: dotnet ef migrations add %MIGRATION_NAME% %CONTEXT_ARG%
echo.

dotnet ef migrations add "%MIGRATION_NAME%" %CONTEXT_ARG%
if %errorlevel% equ 0 (
    echo SUCCESS: Migration '%MIGRATION_NAME%' added successfully!
    echo.
    echo INFO: To update the database, run: migrations-update.bat %PROJECT% %CONTEXT%
) else (
    echo ERROR: Failed to add migration!
    exit /b 1
)
