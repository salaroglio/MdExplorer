# Build and deploy React Editor (DocsPilot) to MdExplorer
# This script builds the Milkdown-based React editor and copies it to wwwroot/milk_react/
# Requires: Node.js 20+ (via nvm)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Step { param($msg) Write-Host "`n=== $msg ===" -ForegroundColor Cyan }
function Write-OK { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Warn { param($msg) Write-Host $msg -ForegroundColor Yellow }
function Write-Err { param($msg) Write-Host $msg -ForegroundColor Red }

# Paths
$projectRoot = "$PSScriptRoot\.."
$reactEditorPath = "$projectRoot\MdEditor.React"
$distPath = "$reactEditorPath\dist"
$targetPath = "$projectRoot\MdExplorer\wwwroot\milk_react"

Write-Step "Build React Editor (DocsPilot)"
Write-Host "Source: $reactEditorPath"
Write-Host "Target: $targetPath"

# Find Node 20+ via nvm
$nvmHome = $env:NVM_HOME
if (-not $nvmHome) {
    $nvmHome = "$env:APPDATA\nvm"
}

# Look for Node 20, 22, or 23
$nodeVersion = $null
$nodeExe = $null

foreach ($ver in @("v22", "v23", "v20")) {
    $candidates = Get-ChildItem -Path $nvmHome -Directory -Filter "$ver*" -ErrorAction SilentlyContinue
    if ($candidates) {
        $nodePath = $candidates | Sort-Object Name -Descending | Select-Object -First 1
        $testExe = Join-Path $nodePath.FullName "node.exe"
        if (Test-Path $testExe) {
            $nodeExe = $testExe
            $nodeVersion = $nodePath.Name
            break
        }
    }
}

if (-not $nodeExe) {
    Write-Err "ERROR: Node.js 20+ not found. Install with: nvm install 22"
    exit 1
}

Write-OK "Using Node.js: $nodeVersion"
Write-Host "Path: $nodeExe"

# Verify it's really Node 20+
$actualVersion = & $nodeExe --version
Write-Host "Actual version: $actualVersion"

# Navigate to React editor directory
Push-Location $reactEditorPath

try {
    # Step 1: Build the React editor
    Write-Step "Building React Editor"

    $npmCmd = Join-Path (Split-Path $nodeExe) "npm.cmd"

    Write-Host "Running: npm run build:vite"
    & $npmCmd run build:vite
    if ($LASTEXITCODE -ne 0) {
        throw "npm run build failed with exit code $LASTEXITCODE"
    }
    Write-OK "Build completed successfully"

    # Step 2: Verify dist folder exists
    Write-Step "Verifying build output"

    if (-not (Test-Path $distPath)) {
        throw "dist/ folder not found after build"
    }

    $distFiles = Get-ChildItem $distPath -File
    Write-Host "Found $($distFiles.Count) files in dist/"

    if (-not (Test-Path "$distPath\docspilot.es.js")) {
        throw "docspilot.es.js not found in dist/"
    }
    Write-OK "docspilot.es.js found"

    # Step 3: Clean target directory (preserve structure, remove old JS files)
    Write-Step "Cleaning target directory"

    if (Test-Path $targetPath) {
        # Remove all .js files from root
        Get-ChildItem "$targetPath\*.js" -File | Remove-Item -Force
        Write-Host "Removed old .js files from root"

        # Keep assets/css structure but we'll overwrite
    } else {
        New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
        Write-Host "Created target directory"
    }

    # Step 4: Copy all files from dist/
    Write-Step "Copying build output to wwwroot/milk_react/"

    # Copy all JS files from dist root
    $jsFiles = Get-ChildItem "$distPath\*.js" -File
    foreach ($file in $jsFiles) {
        Copy-Item $file.FullName "$targetPath\" -Force
    }
    Write-Host "Copied $($jsFiles.Count) JS files"

    # Copy assets folder (includes css)
    if (Test-Path "$distPath\assets") {
        # Ensure target assets/css exists
        if (-not (Test-Path "$targetPath\assets\css")) {
            New-Item -ItemType Directory -Path "$targetPath\assets\css" -Force | Out-Null
        }

        # Copy CSS files
        $cssFiles = Get-ChildItem "$distPath\assets\css\*.css" -File -ErrorAction SilentlyContinue
        foreach ($file in $cssFiles) {
            Copy-Item $file.FullName "$targetPath\assets\css\" -Force
        }
        Write-Host "Copied $($cssFiles.Count) CSS files to assets/css/"
    }

    # Copy css folder if exists (for backwards compatibility)
    if (Test-Path "$distPath\css") {
        if (-not (Test-Path "$targetPath\css")) {
            New-Item -ItemType Directory -Path "$targetPath\css" -Force | Out-Null
        }
        Copy-Item "$distPath\css\*" "$targetPath\css\" -Force -Recurse
        Write-Host "Copied css/ folder"
    }

    # Step 5: Copy KaTeX CSS (not included in Vite build)
    Write-Step "Copying KaTeX CSS"

    $katexSource = "$reactEditorPath\node_modules\katex\dist\katex.min.css"
    $katexTarget = "$targetPath\assets\css\katex\dist"

    if (Test-Path $katexSource) {
        if (-not (Test-Path $katexTarget)) {
            New-Item -ItemType Directory -Path $katexTarget -Force | Out-Null
        }
        Copy-Item $katexSource "$katexTarget\" -Force
        Write-OK "Copied katex.min.css"
    } else {
        Write-Warn "KaTeX CSS not found at: $katexSource"
    }

    # Step 6: Final verification
    Write-Step "Final Verification"

    $requiredFiles = @(
        "$targetPath\docspilot.es.js",
        "$targetPath\docspilot.umd.js",
        "$targetPath\assets\css\milkdown-all.css"
    )

    $allOK = $true
    foreach ($file in $requiredFiles) {
        if (Test-Path $file) {
            Write-OK "[OK] $file"
        } else {
            Write-Err "[MISSING] $file"
            $allOK = $false
        }
    }

    if (-not $allOK) {
        throw "Some required files are missing!"
    }

    # Count total files
    $totalFiles = (Get-ChildItem $targetPath -Recurse -File).Count

    Write-Step "BUILD AND DEPLOY COMPLETED SUCCESSFULLY"
    Write-OK "Total files deployed: $totalFiles"
    Write-Host "`nThe React editor is now available at: /milk_react/docspilot.es.js"

} catch {
    Write-Err "`nBUILD FAILED: $_"
    exit 1
} finally {
    Pop-Location
}
