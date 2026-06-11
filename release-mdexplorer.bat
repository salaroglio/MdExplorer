@echo off
REM One-click release MdExplorer (Windows).
REM Doppio click: decide la versione via git, pubblica il servizio .NET in
REM service_payload, builda l'installer Electron e pusha il tag di release.
REM Logica in ElectronMdExplorer\scripts\release.js.

cd /d "%~dp0"

where node >nul 2>nul
if errorlevel 1 (
    echo ERRORE: Node.js non trovato nel PATH. Installa/attiva node con nvm.
    pause
    exit /b 1
)

node ElectronMdExplorer\scripts\release.js %*

echo.
pause
