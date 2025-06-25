# Windows Credential Manager Analysis Script
Write-Host "======================================"
Write-Host "Windows Credential Manager Analysis"
Write-Host "======================================"
Write-Host ""

# Function to get credentials from Windows Credential Manager
function Get-StoredCredentials {
    param(
        [string]$Target
    )
    
    try {
        $creds = cmdkey /list | Select-String $Target
        if ($creds) {
            Write-Host "Found credential target: $creds"
            return $true
        } else {
            Write-Host "No credentials found for target: $Target"
            return $false
        }
    } catch {
        Write-Host "Error searching for credentials: $($_.Exception.Message)"
        return $false
    }
}

# Check for various Git-related credential targets
Write-Host "--- Searching for Git Credentials ---"
$targets = @(
    "git:https://dbs-svn.dedagroup.it:8443",
    "git:https://dbs-svn.dedagroup.it",
    "https://dbs-svn.dedagroup.it:8443",
    "https://dbs-svn.dedagroup.it",
    "dbs-svn.dedagroup.it",
    "git:https://github.com",
    "git:*"
)

foreach ($target in $targets) {
    Write-Host "Checking target pattern: $target"
    Get-StoredCredentials -Target $target
    Write-Host ""
}

Write-Host "--- All Stored Credentials ---"
try {
    cmdkey /list
} catch {
    Write-Host "Error listing credentials: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "--- Windows Credential Manager via PowerShell ---"
try {
    # Try to use Windows Credential Manager APIs
    Add-Type -AssemblyName System.Web
    
    # Check if CredentialManager module is available
    if (Get-Module -ListAvailable -Name CredentialManager) {
        Import-Module CredentialManager
        Write-Host "CredentialManager module is available"
        
        # Get all stored credentials
        $allCreds = Get-StoredCredential -AsCredentialObject
        Write-Host "Total stored credentials: $($allCreds.Count)"
        
        foreach ($cred in $allCreds) {
            if ($cred.TargetName -like "*git*" -or $cred.TargetName -like "*dbs-svn*") {
                Write-Host "Git-related credential found: $($cred.TargetName)"
            }
        }
    } else {
        Write-Host "CredentialManager PowerShell module not available"
        Write-Host "Install with: Install-Module -Name CredentialManager"
    }
} catch {
    Write-Host "Error accessing PowerShell credential APIs: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "======================================"
Write-Host "Analysis Complete"
Write-Host "======================================"
Read-Host "Press Enter to continue..."