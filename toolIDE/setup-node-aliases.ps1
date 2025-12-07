# Setup Node.js version aliases for PowerShell
# This script creates permanent aliases for specific Node.js versions via nvm
# Run this script ONCE to install the aliases in your PowerShell profile

$ErrorActionPreference = "Stop"

# NVM paths - adjust if your nvm is installed elsewhere
$nvmHome = $env:NVM_HOME
if (-not $nvmHome) {
    $nvmHome = "$env:APPDATA\nvm"
}

# Node version paths
$node14Path = "$nvmHome\v14.21.3\node.exe"
$node23Path = "$nvmHome\v23.11.0\node.exe"
$npm14Path = "$nvmHome\v14.21.3\npm.cmd"
$npm23Path = "$nvmHome\v23.11.0\npm.cmd"

# Verify installations exist
Write-Host "Checking Node.js installations..." -ForegroundColor Cyan

if (-not (Test-Path $node14Path)) {
    Write-Host "WARNING: Node 14.21.3 not found at $node14Path" -ForegroundColor Yellow
    Write-Host "Run: nvm install 14.21.3" -ForegroundColor Yellow
}
else {
    Write-Host "OK: Node 14.21.3 found" -ForegroundColor Green
}

if (-not (Test-Path $node23Path)) {
    Write-Host "WARNING: Node 23.11.0 not found at $node23Path" -ForegroundColor Yellow
    Write-Host "Run: nvm install 23.11.0" -ForegroundColor Yellow
}
else {
    Write-Host "OK: Node 23.11.0 found" -ForegroundColor Green
}

# PowerShell profile path
$profilePath = $PROFILE.CurrentUserAllHosts
$profileDir = Split-Path $profilePath -Parent

# Create profile directory if it doesn't exist
if (-not (Test-Path $profileDir)) {
    New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
    Write-Host "Created profile directory: $profileDir" -ForegroundColor Cyan
}

# Content to add to profile
$aliasContent = @"

# ==================== Node.js Version Aliases (MdExplorer toolIDE) ====================
# Added by setup-node-aliases.ps1

function node14 {
    & "$nvmHome\v14.21.3\node.exe" @args
}

function node23 {
    & "$nvmHome\v23.11.0\node.exe" @args
}

function npm14 {
    `$env:PATH = "$nvmHome\v14.21.3;`$env:PATH"
    & "$nvmHome\v14.21.3\npm.cmd" @args
}

function npm23 {
    `$env:PATH = "$nvmHome\v23.11.0;`$env:PATH"
    & "$nvmHome\v23.11.0\npm.cmd" @args
}

# Helper function to run npm with specific node version
function Run-WithNode14 {
    param([string]`$Command)
    `$env:PATH = "$nvmHome\v14.21.3;`$env:PATH"
    Invoke-Expression `$Command
}

function Run-WithNode23 {
    param([string]`$Command)
    `$env:PATH = "$nvmHome\v23.11.0;`$env:PATH"
    Invoke-Expression `$Command
}

# ==================== End Node.js Aliases ====================
"@

# Check if aliases already exist in profile
$existingContent = ""
if (Test-Path $profilePath) {
    $existingContent = Get-Content $profilePath -Raw -ErrorAction SilentlyContinue
}

if ($existingContent -and $existingContent.Contains("Node.js Version Aliases (MdExplorer toolIDE)")) {
    Write-Host "`nAliases already installed in profile. Updating..." -ForegroundColor Yellow

    # Remove old aliases section and add new one
    $pattern = "# ==================== Node.js Version Aliases \(MdExplorer toolIDE\) ====================[\s\S]*?# ==================== End Node.js Aliases ===================="
    $newContent = [regex]::Replace($existingContent, $pattern, $aliasContent.Trim())
    Set-Content -Path $profilePath -Value $newContent -Encoding UTF8
}
else {
    # Append to profile
    Add-Content -Path $profilePath -Value $aliasContent -Encoding UTF8
}

Write-Host "`n SUCCESS: Node.js aliases installed!" -ForegroundColor Green
Write-Host "`nAvailable commands in new PowerShell windows:" -ForegroundColor Cyan
Write-Host "  node14        - Run Node.js 14.21.3"
Write-Host "  node23        - Run Node.js 23.11.0"
Write-Host "  npm14         - Run npm with Node.js 14.21.3"
Write-Host "  npm23         - Run npm with Node.js 23.11.0"
Write-Host "`nTo use immediately in this session, run:" -ForegroundColor Yellow
Write-Host "  . `$PROFILE" -ForegroundColor White
