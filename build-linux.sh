#!/bin/bash

# MdExplorer Linux Build Script
# Author: MdExplorer Team
# Date: 2025-08-04
# Description: Builds MdExplorer for Linux, excluding Windows-only projects

set -e  # Exit on error

echo "================================================"
echo "MdExplorer Linux Build"
echo "================================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_info() {
    echo -e "${GREEN}[i]${NC} $1"
}

# Parse command line arguments
CONFIGURATION="Release"
CLEAN_BUILD=false
RUNTIME="linux-x64"

while [[ $# -gt 0 ]]; do
    case $1 in
        --debug)
            CONFIGURATION="Debug"
            shift
            ;;
        --clean)
            CLEAN_BUILD=true
            shift
            ;;
        --runtime)
            RUNTIME="$2"
            shift 2
            ;;
        --help)
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  --debug        Build in Debug configuration (default: Release)"
            echo "  --clean        Clean build (remove bin/obj folders)"
            echo "  --runtime      Target runtime (default: linux-x64)"
            echo "  --help         Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

echo "Configuration: $CONFIGURATION"
echo "Runtime: $RUNTIME"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet SDK not found!"
    print_info "Please install .NET Core SDK 3.1 or run ./install-linux-dependencies.sh"
    exit 1
fi

# Check dotnet version
DOTNET_VERSION=$(dotnet --version)
print_status "Found .NET SDK: $DOTNET_VERSION"

# Clean if requested
if [ "$CLEAN_BUILD" = true ]; then
    print_info "Cleaning previous build..."
    find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
    find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
    print_status "Clean complete"
fi

echo ""
echo "Building projects..."
echo ""

# List of projects to build (excluding Windows-only projects)
PROJECTS=(
    "Ad.Tools.Dal.Abstractions/Ad.Tools.Dal.Abstractions/Ad.Tools.Dal.Abstractions.csproj"
    "Ad.Tools.Dal/Ad.Tools.Dal/Ad.Tools.Dal.csproj"
    "Ad.Tools.FluentMigrator/Ad.Tools.FluentMigrator/Ad.Tools.FluentMigrator.csproj"
    "MdExplorer.Abstractions/MdExplorer.Abstractions.csproj"
    "MDExplorer.dal/MDExplorer.DataAccess.csproj"
    "MdExplorer.DataAccess.Engine/MdExplorer.DataAccess.Engine.csproj"
    "MdExplorer.DataAccess.Project/MdExplorer.DataAccess.Project.csproj"
    "MdExplorer.Migrations/MdExplorer.Migrations.csproj"
    "MdExplorer.Migrations.EngineDb/MdExplorer.Migrations.EngineDb.csproj"
    "MdExplorer.Migrations.ProjectDb/MdExplorer.Migrations.ProjectDb.csproj"
    "MdExplorer.bll/MdExplorer.Features.csproj"
    "MdExplorer/MdExplorer.Service.csproj"
)

# Projects excluded on Linux
EXCLUDED_PROJECTS=(
    "MdImageNumbering"           # Windows Forms application
    "MdExplorer.Setup"           # WiX installer
    "MdExplorer.Bootstrapper"    # WiX bootstrapper
    "Ad.Tools.FluentMigrator.UnitTest"  # .NET Framework 4.7.2
    "WebViewControl"             # .NET Framework 4.7.2
)

echo "Projects to build: ${#PROJECTS[@]}"
echo "Projects excluded: ${#EXCLUDED_PROJECTS[@]}"
echo ""

print_info "Excluded Windows-only projects:"
for proj in "${EXCLUDED_PROJECTS[@]}"; do
    echo "  - $proj"
done
echo ""

# Build each project
FAILED_PROJECTS=()
SUCCESS_COUNT=0

for PROJECT in "${PROJECTS[@]}"; do
    if [ -f "$PROJECT" ]; then
        PROJECT_NAME=$(basename "$PROJECT" .csproj)
        echo "Building $PROJECT_NAME..."
        
        if dotnet build "$PROJECT" --configuration "$CONFIGURATION" -v minimal > /tmp/build_${PROJECT_NAME}.log 2>&1; then
            
            print_status "$PROJECT_NAME built successfully"
            ((SUCCESS_COUNT++))
        else
            print_error "$PROJECT_NAME build failed"
            FAILED_PROJECTS+=("$PROJECT_NAME")
            print_info "Check /tmp/build_${PROJECT_NAME}.log for details"
        fi
    else
        print_warning "Project file not found: $PROJECT"
    fi
    echo ""
done

echo "================================================"
echo "Build Summary"
echo "================================================"
echo ""

if [ ${#FAILED_PROJECTS[@]} -eq 0 ]; then
    print_status "All projects built successfully! ($SUCCESS_COUNT/${#PROJECTS[@]})"
    echo ""
    echo "To run MdExplorer:"
    echo "  cd MdExplorer"
    echo "  dotnet run --configuration $CONFIGURATION"
    echo ""
    echo "Or to run the built executable:"
    echo "  ./MdExplorer/bin/$CONFIGURATION/netcoreapp3.1/$RUNTIME/MdExplorer.Service"
else
    print_error "Build failed for ${#FAILED_PROJECTS[@]} project(s):"
    for proj in "${FAILED_PROJECTS[@]}"; do
        echo "  - $proj"
    done
    echo ""
    print_info "Check the log files in /tmp/ for details"
    exit 1
fi

# Build frontend if Node.js is available
if command -v npm &> /dev/null; then
    echo ""
    echo "================================================"
    echo "Building Frontend"
    echo "================================================"
    echo ""
    
    # Angular client
    if [ -d "MdExplorer/client2" ]; then
        print_info "Building Angular client..."
        cd MdExplorer/client2
        
        if [ ! -d "node_modules" ]; then
            print_info "Installing npm dependencies..."
            npm install
        fi
        
        if npm run build; then
            print_status "Angular client built successfully"
        else
            print_warning "Angular client build failed"
        fi
        cd ../..
    fi
    
    # React editor
    if [ -d "MdEditor.React" ]; then
        print_info "Building React editor..."
        cd MdEditor.React
        
        if [ ! -d "node_modules" ]; then
            print_info "Installing npm dependencies..."
            npm install
        fi
        
        if npm run build; then
            print_status "React editor built successfully"
        else
            print_warning "React editor build failed"
        fi
        cd ..
    fi
else
    print_warning "Node.js not found. Skipping frontend build."
    print_info "Install Node.js to build the frontend components."
fi

echo ""
echo "================================================"
echo "Build complete!"
echo "================================================"