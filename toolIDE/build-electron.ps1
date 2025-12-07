# Build ElectronMdExplorer with Node.js 23.11.0
# This script sets the correct Node version before building

$ErrorActionPreference = "Stop"

$nvmHome = $env:NVM_HOME
if (-not $nvmHome) {
    $nvmHome = "$env:APPDATA\nvm"
}

$nodePath = "$nvmHome\v23.11.0"
$nodeExe = "$nodePath\node.exe"

if (-not (Test-Path $nodeExe)) {
    Write-Host "ERROR: Node 23.11.0 not found. Run: nvm install 23.11.0" -ForegroundColor Red
    exit 1
}

Write-Host "=== Building ElectronMdExplorer with Node.js 23.11.0 ===" -ForegroundColor Cyan

# Verify version
$nodeVersion = & $nodeExe --version
Write-Host "Using Node.js: $nodeVersion" -ForegroundColor Green

# Navigate to Electron directory
$electronPath = "$PSScriptRoot\..\ElectronMdExplorer"
Push-Location $electronPath

try {
    # Run prebuild scripts
    Write-Host "`nRunning prebuild scripts..." -ForegroundColor Yellow
    & $nodeExe scripts/check-go-binaries.js
    if ($LASTEXITCODE -ne 0) { throw "check-go-binaries.js failed" }

    & $nodeExe scripts/bump-version.js
    if ($LASTEXITCODE -ne 0) { throw "bump-version.js failed" }

    # Run electron-builder
    Write-Host "`nRunning: electron-builder" -ForegroundColor Yellow
    # Use node to run electron-builder directly from node_modules
    & $nodeExe node_modules/electron-builder/cli.js
    if ($LASTEXITCODE -ne 0) { throw "electron-builder failed" }

    # Run postbuild
    Write-Host "`nRunning postbuild script..." -ForegroundColor Yellow
    & $nodeExe scripts/post-build.js
    if ($LASTEXITCODE -ne 0) { throw "post-build.js failed" }

    Write-Host "`n=== Build completed successfully! ===" -ForegroundColor Green
}
finally {
    Pop-Location
}
