@echo off
REM Helper script to remove the last Entity Framework migration
REM Usage: migrations-remove.bat <project> [context]
REM
REM Examples:
REM   migrations-remove.bat api
REM   migrations-remove.bat dashboard ApplicationDbContext
REM   migrations-remove.bat dashboard AppDbContext

setlocal

REM Check if dotnet ef is installed
dotnet ef --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: dotnet-ef tools not found!
    echo Please install it using: dotnet tool install --global dotnet-ef
    exit /b 1
)

REM Validate arguments
if "%~1"=="" (
    echo ERROR: Insufficient arguments
    echo.
    echo Usage: %~nx0 ^<project^> [context]
    echo.
    echo Projects:
    echo   api       - Robot.ED.FacebookConnector.Service.API
    echo   dashboard - Robot.ED.FacebookConnector.Dashboard
    echo.
    echo Dashboard Contexts ^(required for dashboard^):
    echo   ApplicationDbContext - For Identity tables
    echo   AppDbContext        - For shared data tables
    echo.
    echo Examples:
    echo   %~nx0 api
    echo   %~nx0 dashboard ApplicationDbContext
    echo   %~nx0 dashboard AppDbContext
    exit /b 1
)

set PROJECT=%~1
set CONTEXT=%~2

REM Set project path and context based on project type
if /i "%PROJECT%"=="api" (
    set PROJECT_PATH=Robot.ED.FacebookConnector.Service.API
    set CONTEXT_ARG=
    echo WARNING: Removing last migration from API project...
) else if /i "%PROJECT%"=="dashboard" (
    set PROJECT_PATH=Robot.ED.FacebookConnector.Dashboard
    if "%CONTEXT%"=="" (
        echo ERROR: Dashboard project requires context parameter!
        echo.
        echo Please specify the context:
        echo   ApplicationDbContext - For Identity-related tables
        echo   AppDbContext        - For shared data tables
        echo.
        echo Example: %~nx0 dashboard ApplicationDbContext
        exit /b 1
    )
    set CONTEXT_ARG=--context %CONTEXT%
    echo WARNING: Removing last migration from Dashboard project with context: %CONTEXT%
) else (
    echo ERROR: Invalid project: %PROJECT%
    echo Valid projects: api, dashboard
    exit /b 1
)

REM Navigate to project directory and remove migration
cd "%PROJECT_PATH%"

echo INFO: Executing: dotnet ef migrations remove %CONTEXT_ARG%
echo.

dotnet ef migrations remove %CONTEXT_ARG%
if %errorlevel% equ 0 (
    echo SUCCESS: Last migration removed successfully!
) else (
    echo ERROR: Failed to remove migration!
    echo INFO: If the migration was already applied to the database, you need to revert it first using migrations-update.bat
    exit /b 1
)
