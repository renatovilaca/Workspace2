#!/bin/bash
# Helper script to update database with Entity Framework migrations
# Usage: ./migrations-update.sh <project> [context]
#
# Examples:
#   ./migrations-update.sh api
#   ./migrations-update.sh dashboard ApplicationDbContext
#   ./migrations-update.sh dashboard AppDbContext

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_error() {
    echo -e "${RED}ERROR: $1${NC}"
}

print_success() {
    echo -e "${GREEN}SUCCESS: $1${NC}"
}

print_info() {
    echo -e "${YELLOW}INFO: $1${NC}"
}

# Check if dotnet ef is installed
if ! command -v dotnet-ef &> /dev/null; then
    print_error "dotnet-ef tools not found!"
    echo "Please install it using: dotnet tool install --global dotnet-ef"
    exit 1
fi

# Validate arguments
if [ $# -lt 1 ]; then
    print_error "Insufficient arguments"
    echo ""
    echo "Usage: $0 <project> [context]"
    echo ""
    echo "Projects:"
    echo "  api       - Robot.ED.FacebookConnector.Service.API"
    echo "  dashboard - Robot.ED.FacebookConnector.Dashboard"
    echo ""
    echo "Dashboard Contexts (required for dashboard):"
    echo "  ApplicationDbContext - For Identity tables"
    echo "  AppDbContext        - For shared data tables"
    echo ""
    echo "Examples:"
    echo "  $0 api"
    echo "  $0 dashboard ApplicationDbContext"
    echo "  $0 dashboard AppDbContext"
    exit 1
fi

PROJECT=$1
CONTEXT=$2

# Set project path and context based on project type
case $PROJECT in
    api)
        PROJECT_PATH="Robot.ED.FacebookConnector.Service.API"
        CONTEXT_ARG=""
        print_info "Updating database for API project..."
        ;;
    dashboard)
        PROJECT_PATH="Robot.ED.FacebookConnector.Dashboard"
        if [ -z "$CONTEXT" ]; then
            print_error "Dashboard project requires context parameter!"
            echo ""
            echo "Please specify the context:"
            echo "  ApplicationDbContext - For Identity-related tables"
            echo "  AppDbContext        - For shared data tables"
            echo ""
            echo "Example: $0 dashboard ApplicationDbContext"
            exit 1
        fi
        CONTEXT_ARG="--context $CONTEXT"
        print_info "Updating database for Dashboard project with context: $CONTEXT"
        ;;
    *)
        print_error "Invalid project: $PROJECT"
        echo "Valid projects: api, dashboard"
        exit 1
        ;;
esac

# Navigate to project directory and update database
cd "$PROJECT_PATH"

print_info "Executing: dotnet ef database update $CONTEXT_ARG"
echo ""

if dotnet ef database update $CONTEXT_ARG; then
    print_success "Database updated successfully!"
else
    print_error "Failed to update database!"
    exit 1
fi
