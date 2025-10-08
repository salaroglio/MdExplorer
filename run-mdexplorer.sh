#\!/bin/bash

# Script to run MdExplorer with proper native library configuration

# Set the native library path for LLamaSharp
export LD_LIBRARY_PATH="/home/carlo/Documents/sviluppo/MdExplorer/MdExplorer/bin/Debug/net8.0/linux-x64:$LD_LIBRARY_PATH"

# Run the application
dotnet run --project MdExplorer/MdExplorer.Service.csproj
