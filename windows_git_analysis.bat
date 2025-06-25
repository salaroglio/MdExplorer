@echo off
echo ====================================
echo Windows Git Configuration Analysis
echo ====================================
echo.

echo --- Git Version ---
git --version
echo.

echo --- Global Git Configuration ---
git config --global --list
echo.

echo --- System Git Configuration ---
git config --system --list
echo.

echo --- Git Configuration in Target Repository ---
cd "C:\sviluppo\dedagroup\BCCEnd2End"
echo Current directory: %CD%
echo.

echo --- Local Repository Git Configuration ---
git config --local --list
echo.

echo --- Git Remote Configuration ---
git remote -v
echo.

echo --- Test Git Credential Helper ---
echo url=https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test | git credential fill
echo.

echo --- Test Git Credential Manager ---
echo url=https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test | git credential-manager get
echo.

echo --- Test Simple Git Remote Access ---
git ls-remote origin
echo.

echo --- Git Credential Helper Configuration ---
git config credential.helper
echo.

echo --- Check if Git Credential Manager is installed ---
where git-credential-manager
where git-credential-manager-core
echo.

echo ====================================
echo Analysis Complete
echo ====================================
pause