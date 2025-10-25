@echo off
REM Helper script to list Entity Framework migrations
REM Usage: migrations-list.bat <project> [context]
REM
REM Examples:
REM   migrations-list.bat api
REM   migrations-list.bat dashboard ApplicationDbContext
REM   migrations-list.bat dashboard AppDbContext
REM   migrations-list.bat dashboard all

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
    echo   all                 - List migrations for both contexts
    echo.
    echo Examples:
    echo   %~nx0 api
    echo   %~nx0 dashboard ApplicationDbContext
    echo   %~nx0 dashboard AppDbContext
    echo   %~nx0 dashboard all
    exit /b 1
)

set PROJECT=%~1
set CONTEXT=%~2

REM Set project path and context based on project type
if /i "%PROJECT%"=="api" (
    set PROJECT_PATH=Robot.ED.FacebookConnector.Service.API
    echo INFO: Listing migrations for API project...
    echo.
    cd "%PROJECT_PATH%"
    dotnet ef migrations list
) else if /i "%PROJECT%"=="dashboard" (
    set PROJECT_PATH=Robot.ED.FacebookConnector.Dashboard
    if "%CONTEXT%"=="" (
        echo ERROR: Dashboard project requires context parameter!
        echo.
        echo Please specify the context:
        echo   ApplicationDbContext - For Identity-related tables
        echo   AppDbContext        - For shared data tables
        echo   all                 - List migrations for both contexts
        echo.
        echo Example: %~nx0 dashboard ApplicationDbContext
        exit /b 1
    )
    
    cd "%PROJECT_PATH%"
    
    if /i "%CONTEXT%"=="all" (
        echo INFO: Listing migrations for ApplicationDbContext ^(Identity^)...
        echo.
        dotnet ef migrations list --context ApplicationDbContext
        echo.
        echo -------------------------------------------------------------------
        echo.
        echo INFO: Listing migrations for AppDbContext ^(Shared Data^)...
        echo.
        dotnet ef migrations list --context AppDbContext
    ) else (
        set CONTEXT_ARG=--context %CONTEXT%
        echo INFO: Listing migrations for Dashboard project with context: %CONTEXT%
        echo.
        dotnet ef migrations list --context %CONTEXT%
    )
) else (
    echo ERROR: Invalid project: %PROJECT%
    echo Valid projects: api, dashboard
    exit /b 1
)
