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
    # Ensure NSIS patches (patch-package) are applied. In fresh clones or after
    # a manual node_modules wipe, the extractAppPackage.nsh template may still
    # contain the upstream multi-phase extraction that makes the installer
    # progress bar jump back to zero. Detect the marker and run npm install
    # (which triggers the postinstall patch-package hook).
    $extractNsh = Join-Path $electronPath "node_modules\app-builder-lib\templates\nsis\include\extractAppPackage.nsh"
    $needsInstall = $false
    if (-not (Test-Path $extractNsh)) {
        $needsInstall = $true
    } else {
        $content = Get-Content $extractNsh -Raw
        if ($content -match "LoopExtract7za") {
            $needsInstall = $true
        }
    }
    if ($needsInstall) {
        Write-Host "`nApplying NSIS patches (running npm install)..." -ForegroundColor Yellow
        $npmCmd = "$nodePath\npm.cmd"
        & $npmCmd install
        if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
    }

    # Run prebuild scripts
    Write-Host "`nRunning prebuild scripts..." -ForegroundColor Yellow
    & $nodeExe scripts/check-go-binaries.js
    if ($LASTEXITCODE -ne 0) { throw "check-go-binaries.js failed" }

    & $nodeExe scripts/check-release-state.js
    if ($LASTEXITCODE -ne 0) { throw "check-release-state.js failed" }

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
