# Build client2 with Node.js 16.20.2
# This script sets the correct Node version before building

$ErrorActionPreference = "Stop"

$nvmHome = $env:NVM_HOME
if (-not $nvmHome) {
    $nvmHome = "$env:APPDATA\nvm"
}

$nodePath = "$nvmHome\v16.20.2"
$nodeExe = "$nodePath\node.exe"

if (-not (Test-Path $nodeExe)) {
    Write-Host "ERROR: Node 16.20.2 not found. Run: nvm install 16.20.2" -ForegroundColor Red
    exit 1
}

Write-Host "=== Building client2 with Node.js 16.20.2 ===" -ForegroundColor Cyan

# Verify version
$nodeVersion = & $nodeExe --version
Write-Host "Using Node.js: $nodeVersion" -ForegroundColor Green

# Navigate to client2 directory
$client2Path = "$PSScriptRoot\..\MdExplorer\client2"
Push-Location $client2Path

try {
    # Run the original build command
    Write-Host "`nRunning: node update-version.js" -ForegroundColor Yellow
    & $nodeExe update-version.js
    if ($LASTEXITCODE -ne 0) { throw "update-version.js failed" }

    Write-Host "`nRunning: ng build --base-href /client2/" -ForegroundColor Yellow
    # Use node to run Angular CLI directly from node_modules
    & $nodeExe node_modules/@angular/cli/bin/ng build --base-href /client2/
    if ($LASTEXITCODE -ne 0) { throw "ng build failed" }

    Write-Host "`n=== Build completed successfully! ===" -ForegroundColor Green
}
finally {
    Pop-Location
}
