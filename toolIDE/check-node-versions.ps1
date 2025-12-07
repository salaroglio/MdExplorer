# Check Node.js version aliases status
# Run this to verify your setup is correct

Write-Host "`n=== Node.js Version Check ===" -ForegroundColor Cyan

# Check if aliases are available
Write-Host "`nChecking aliases..." -ForegroundColor Yellow

try {
    $v14 = & node14 --version 2>&1
    Write-Host "node14: $v14" -ForegroundColor Green
} catch {
    Write-Host "node14: NOT AVAILABLE - run setup-node-aliases.ps1 first" -ForegroundColor Red
}

try {
    $v23 = & node23 --version 2>&1
    Write-Host "node23: $v23" -ForegroundColor Green
} catch {
    Write-Host "node23: NOT AVAILABLE - run setup-node-aliases.ps1 first" -ForegroundColor Red
}

Write-Host "`nCurrent default node:" -ForegroundColor Yellow
node --version

Write-Host "`n=== NVM Installed Versions ===" -ForegroundColor Cyan
nvm list
