#!/bin/bash
# Helper script to add Entity Framework migrations
# Usage: ./migrations-add.sh <project> <migration_name> [context]
#
# Examples:
#   ./migrations-add.sh api AddNewColumn
#   ./migrations-add.sh dashboard AddNewTable ApplicationDbContext
#   ./migrations-add.sh dashboard AddSharedTable AppDbContext

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
if [ $# -lt 2 ]; then
    print_error "Insufficient arguments"
    echo ""
    echo "Usage: $0 <project> <migration_name> [context]"
    echo ""
    echo "Projects:"
    echo "  api       - Robot.ED.FacebookConnector.Service.API"
    echo "  dashboard - Robot.ED.FacebookConnector.Dashboard"
    echo ""
    echo "Dashboard Contexts (required for dashboard):"
    echo "  ApplicationDbContext - For Identity tables (default location: Migrations/)"
    echo "  AppDbContext        - For shared data tables (default location: Migrations/AppDb/)"
    echo ""
    echo "Examples:"
    echo "  $0 api AddNewColumn"
    echo "  $0 dashboard AddNewTable ApplicationDbContext"
    echo "  $0 dashboard AddSharedTable AppDbContext"
    exit 1
fi

PROJECT=$1
MIGRATION_NAME=$2
CONTEXT=$3

# Set project path and context based on project type
case $PROJECT in
    api)
        PROJECT_PATH="Robot.ED.FacebookConnector.Service.API"
        CONTEXT_ARG=""
        print_info "Adding migration to API project..."
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
            echo "Example: $0 dashboard $MIGRATION_NAME ApplicationDbContext"
            exit 1
        fi
        
        # Set output directory based on context
        if [ "$CONTEXT" == "AppDbContext" ]; then
            OUTPUT_DIR="Migrations/AppDb"
        else
            OUTPUT_DIR="Migrations"
        fi
        
        CONTEXT_ARG="--context $CONTEXT --output-dir $OUTPUT_DIR"
        print_info "Adding migration to Dashboard project with context: $CONTEXT"
        ;;
    *)
        print_error "Invalid project: $PROJECT"
        echo "Valid projects: api, dashboard"
        exit 1
        ;;
esac

# Navigate to project directory and add migration
cd "$PROJECT_PATH"

print_info "Executing: dotnet ef migrations add $MIGRATION_NAME $CONTEXT_ARG"
echo ""

if dotnet ef migrations add "$MIGRATION_NAME" $CONTEXT_ARG; then
    print_success "Migration '$MIGRATION_NAME' added successfully!"
    echo ""
    print_info "To update the database, run: ./migrations-update.sh $PROJECT $CONTEXT"
else
    print_error "Failed to add migration!"
    exit 1
fi
