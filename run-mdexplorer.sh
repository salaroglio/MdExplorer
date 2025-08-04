#!/bin/bash

# Run MdExplorer on Linux
cd "$(dirname "$0")/MdExplorer"

echo "Starting MdExplorer on http://localhost:5000"
echo "Press Ctrl+C to stop"
echo ""

dotnet run --urls "http://localhost:5000"