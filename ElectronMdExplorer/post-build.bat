@echo off
setlocal

set "SOURCE_DIR=..\..\Binaries\ElectronMdExplorer\win-unpacked"
set "DEST_DIR=..\..\Binaries\ElectronMdExplorer"

echo Moving files from %SOURCE_DIR% to %DEST_DIR%...
REM Check if source directory exists before moving
if exist "%SOURCE_DIR%" (
    move /Y "%SOURCE_DIR%\*" "%DEST_DIR%\"
    echo Removing empty directory %SOURCE_DIR%...
    rd /S /Q "%SOURCE_DIR%"
) else (
    echo Source directory %SOURCE_DIR% not found. Skipping move.
)

echo Post-build steps completed.
endlocal
