# Test Credential Target Variations Script
Write-Host "==========================================="
Write-Host "Testing Credential Target Variations"
Write-Host "==========================================="
Write-Host ""

# URL from the error log
$originalUrl = "https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test"
Write-Host "Original URL: $originalUrl"
Write-Host ""

# Possible target variations that could be used
$targetVariations = @(
    # Our current implementation
    "git:https://dbs-svn.dedagroup.it",
    
    # With port included
    "git:https://dbs-svn.dedagroup.it:8443",
    
    # Without git prefix
    "https://dbs-svn.dedagroup.it",
    "https://dbs-svn.dedagroup.it:8443",
    
    # Host only variations
    "dbs-svn.dedagroup.it",
    "dbs-svn.dedagroup.it:8443",
    
    # Full path variations
    "git:https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test",
    "https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test",
    
    # Generic fallback (our current fallback)
    "git:https://github.com",
    
    # Other possible patterns
    "git:dbs-svn.dedagroup.it",
    "git:dbs-svn.dedagroup.it:8443"
)

Write-Host "Testing $($targetVariations.Count) target variations..."
Write-Host ""

foreach ($target in $targetVariations) {
    Write-Host "--- Testing target: '$target' ---"
    
    try {
        # Test with cmdkey
        $cmdkeyResult = cmdkey /list:$target 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "FOUND with cmdkey: $target"
            Write-Host "Details: $cmdkeyResult"
        } else {
            Write-Host "NOT FOUND with cmdkey: $target"
        }
    } catch {
        Write-Host "ERROR with cmdkey for '$target': $($_.Exception.Message)"
    }
    
    Write-Host ""
}

Write-Host "==========================================="
Write-Host "Summary: Checking all generic Git credentials"
Write-Host "==========================================="

# List all credentials to see what's actually stored
Write-Host "All stored credentials containing 'git' or 'dbs-svn':"
try {
    $allCreds = cmdkey /list | Select-String -Pattern "(git|dbs-svn)"
    if ($allCreds) {
        foreach ($cred in $allCreds) {
            Write-Host "Found: $cred"
        }
    } else {
        Write-Host "No Git or dbs-svn related credentials found"
    }
} catch {
    Write-Host "Error listing credentials: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "==========================================="
Write-Host "How to find the correct target format:"
Write-Host "==========================================="
Write-Host "1. Make sure Visual Studio Code can authenticate to the repository"
Write-Host "2. Clear all credentials: cmdkey /delete:* (be careful!)"
Write-Host "3. Use VS Code to authenticate again"
Write-Host "4. Run cmdkey /list to see what target format VS Code created"
Write-Host "5. Update our WindowsCredentialStoreResolver to use the same format"
Write-Host ""

Read-Host "Press Enter to continue"