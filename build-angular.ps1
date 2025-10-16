#!/usr/bin/env pwsh
# Simple build script for MdExplorer Angular frontend
# This script only builds the main application

$ErrorActionPreference = "Stop"

Write-Host "🚀 Building MdExplorer Angular Frontend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Push-Location ".\MdExplorer\client2"

# Use Node 14.21.3 if nvm is available
if (Get-Command nvm -ErrorAction SilentlyContinue) {
    Write-Host "📦 Using Node.js 14.21.3" -ForegroundColor Yellow
    nvm use 14.21.3
}

Write-Host "📥 Installing dependencies..." -ForegroundColor Yellow
npm install

Write-Host "🔨 Building Angular application..." -ForegroundColor Yellow
npm run build

Pop-Location

Write-Host "`n✅ Angular build completed successfully!" -ForegroundColor Green
Write-Host "`nOutput: MdExplorer\wwwroot\client2\" -ForegroundColor Cyan
