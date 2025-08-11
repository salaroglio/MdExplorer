---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 11/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Git Authentication Analysis: "Too Many Redirects" Issue Investigation

## Overview
This analysis investigates the "too many redirects or authentication replays" issue that occurs specifically during Git push operations but not pull operations when using LibGit2Sharp.

## Enhanced Logging Implementation

I've added comprehensive logging to track credential resolution patterns and identify the root cause of the authentication replay issue. The enhanced logging includes:

### 1. Credential Callback Counting
- **Pull Operations**: Track how many times `CredentialsProvider` is called during pull
- **Push Operations**: Track how many times `CredentialsProvider` is called during push  
- **Fetch Operations**: Track calls in `GetPullPushDataAsync` method

### 2. Detailed Credential Resolution Tracking
- Unique call ID for each credential resolution attempt
- Call count tracking for the same URL/user/types combination
- Detection of repeated calls (potential authentication loops)
- Warning when >5 calls made to same credential combination

### 3. Credential Type and Protocol Analysis
- SSH vs HTTPS URL detection
- Credential type logging (UsernamePasswordCredentials, DefaultCredentials, etc.)
- Authentication method tracking (SSH, GitCredentialHelper, SystemCredentialStore)
- Resolver priority and success/failure tracking

### 4. State Change Detection
- Credential caching to detect if same credentials are returned multiple times
- Historical tracking of credential calls per URL
- Timestamp tracking for debugging temporal issues

## Key Questions These Logs Will Answer

### 1. Credential Callback Frequency
**Question**: How many times is the CredentialsProvider callback invoked during push vs pull?

**What to look for**:
```
PULL CREDENTIAL CALLBACK #1 - URL: https://github.com/user/repo.git
PULL CREDENTIAL CALLBACK #2 - URL: https://github.com/user/repo.git
```
vs
```
PUSH CREDENTIAL CALLBACK #1 - URL: https://github.com/user/repo.git
PUSH CREDENTIAL CALLBACK #2 - URL: https://github.com/user/repo.git
PUSH CREDENTIAL CALLBACK #3 - URL: https://github.com/user/repo.git
```

### 2. Authentication Flow Differences
**Question**: Are there differences in how LibGit2Sharp handles push vs pull authentication flows?

**What to look for**:
- Different SupportedCredentialTypes requested
- Different resolver chains being used
- Different credential types being returned

### 3. Retry Loop Detection
**Question**: Are there retry mechanisms causing multiple authentication attempts?

**What to look for**:
```
CREDENTIAL RESOLUTION [abc12345] - REPEATED CALL #3 for same URL/user/types combination
CREDENTIAL RESOLUTION [abc12345] - TOO MANY CALLS (6) - This may be the source of 'too many redirects' error
```

### 4. SystemCredentialStore Behavior
**Question**: Is the SystemCredentialStore resolver returning different credentials on subsequent calls?

**What to look for**:
- Same resolver being called multiple times
- Different authentication methods being used in sequence
- Credential resolution failures followed by retries

### 5. State Changes Between Calls
**Question**: Are there state changes in credential resolvers between calls?

**What to look for**:
- Same credential resolution call ID getting different results
- Changes in resolver priority or availability
- Temporal issues (timestamps showing rapid successive calls)

### 6. SSH vs HTTPS Authentication Differences
**Question**: Are there differences in how SSH vs HTTPS authentication is handled in push operations?

**What to look for**:
```
CREDENTIAL RESOLUTION [abc12345] - SUCCESS using SSHKeyCredentialResolver: SSHKey, CredType: DefaultCredentials, SSH: True, HTTPS: False
```
vs
```
CREDENTIAL RESOLUTION [abc12345] - SUCCESS using GitCredentialHelperResolver: GitCredentialHelper, CredType: UsernamePasswordCredentials, SSH: False, HTTPS: True
```

## Potential Root Causes to Investigate

Based on the code analysis, here are the most likely scenarios:

### 1. **SSH Agent Authentication Loop**
- SSH resolver returns `DefaultCredentials` which delegates to SSH agent
- If SSH agent authentication fails, LibGit2Sharp may retry multiple times
- Each retry calls the CredentialsProvider again

### 2. **Credential Helper State Issues**
- GitCredentialHelperResolver executes external `git credential fill` process
- If the process returns inconsistent results, it could cause authentication loops
- Timeout or process failure could trigger retries

### 3. **Windows Credential Store Inconsistency**
- WindowsCredentialStoreResolver might return different credentials on subsequent calls
- Credential store lookups might be failing intermittently
- Generic fallback credentials might be causing conflicts

### 4. **LibGit2Sharp Push vs Pull Differences**
- Push operations may require different credential types than pull operations
- Push may involve multiple authentication stages (e.g., push to multiple refs)
- Push operations might be more strict about credential validation

### 5. **Resolver Priority Conflicts**
- Multiple resolvers might be returning credentials that work for pull but not push
- Resolver chain might be getting confused by mixed authentication methods

## How to Use This Analysis

1. **Run a push operation** that typically fails with "too many redirects"
2. **Examine the logs** for patterns matching the scenarios above
3. **Count credential callbacks** - if push has significantly more calls than pull, we've identified the issue
4. **Look for repeated resolution calls** - this indicates the authentication loop
5. **Analyze credential types** - mismatched credential types might be the root cause

## Expected Findings

Based on the investigation, I expect to find:

1. **Push operations trigger 3+ credential callbacks** while pull operations only trigger 1-2
2. **Multiple credential resolvers being tried** in sequence during push
3. **SSH authentication falling back to HTTPS** or vice versa
4. **Specific credential types being rejected** by the push operation
5. **Temporal clustering** of authentication attempts indicating retry loops

## Next Steps

Once we identify the specific pattern causing the issue, we can implement targeted fixes:

1. **Credential caching** to prevent repeated resolution calls
2. **Resolver chain optimization** to prevent fallback loops  
3. **Authentication method limiting** to prevent mixed authentication
4. **Retry logic enhancement** to handle transient failures better
5. **LibGit2Sharp configuration tuning** to optimize for specific scenarios

This enhanced logging will provide the detailed visibility needed to finally resolve the "too many redirects" authentication issue in Git push operations.