#!/bin/bash

# Script to run MdExplorer with proper native library configuration

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Set the native library path for LLamaSharp
export LD_LIBRARY_PATH="$SCRIPT_DIR/MdExplorer/bin/Debug/net8.0/linux-x64:$LD_LIBRARY_PATH"

# Run the application
dotnet run --project "$SCRIPT_DIR/MdExplorer/MdExplorer.Service.csproj"
