#!/bin/bash

# MdExplorer Launcher Script
# This script starts MdExplorer and opens the browser

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Locate dotnet: PATH first, then DOTNET_ROOT, then the per-user install
if command -v dotnet >/dev/null 2>&1; then
    DOTNET="$(command -v dotnet)"
else
    export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
    export PATH="$DOTNET_ROOT:$PATH"
    DOTNET="$DOTNET_ROOT/dotnet"
fi

# CLIs installed with npm under nvm (copilot, claude, opencode) are in PATH only once nvm is loaded:
# a shell started from a desktop shortcut or a terminal profile does not read .bashrc, and the
# service would not find them (29/09/2026: «spiega il diagramma» failed to start copilot).
export NVM_DIR="${NVM_DIR:-$HOME/.nvm}"
if [ -s "$NVM_DIR/nvm.sh" ]; then
    . "$NVM_DIR/nvm.sh" >/dev/null 2>&1
    echo "nvm loaded: node $(command -v node 2>/dev/null || echo 'not found')"
fi

# Change to MdExplorer directory
cd "$SCRIPT_DIR/MdExplorer" || {
    echo "Error: Cannot find MdExplorer directory at $SCRIPT_DIR/MdExplorer"
    exit 1
}

# Kill any existing MdExplorer instances
pkill -f "dotnet.*MdExplorer" 2>/dev/null

# Start MdExplorer with a specific port
echo "Starting MdExplorer from $(pwd)..."
echo "Using dotnet at: $DOTNET"

if [ ! -f "$DOTNET" ]; then
    echo "Error: dotnet not found at $DOTNET"
    exit 1
fi

if [ ! -f "MdExplorer.Service.csproj" ]; then
    echo "Error: MdExplorer.Service.csproj not found in $(pwd)"
    ls -la
    exit 1
fi

$DOTNET run --project MdExplorer.Service.csproj 5000 &

# Get the PID of the dotnet process
DOTNET_PID=$!

# Wait a bit for the server to start
echo "Waiting for server to start..."
sleep 3

# Check if the process is still running
if ps -p $DOTNET_PID > /dev/null; then
    echo "MdExplorer started successfully!"
    echo "The browser should open automatically..."
else
    echo "Error: MdExplorer failed to start"
    exit 1
fi

echo ""
echo "MdExplorer is running on http://127.0.0.1:5000"
echo "Press Ctrl+C to stop the server..."
echo ""

# Keep the script running and wait for the dotnet process
wait $DOTNET_PID