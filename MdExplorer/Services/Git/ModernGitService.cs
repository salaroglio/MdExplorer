using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Services.Git
{
    public class ModernGitService : IModernGitService
    {
        private readonly IEnumerable<ICredentialResolver> _credentialResolvers;
        private readonly ILogger<ModernGitService> _logger;
        private readonly GitAuthenticationOptions _authOptions;
        private readonly GitOperationOptions _operationOptions;

        public ModernGitService(
            IEnumerable<ICredentialResolver> credentialResolvers, 
            ILogger<ModernGitService> logger,
            IOptions<GitAuthenticationOptions> authOptions = null,
            IOptions<GitOperationOptions> operationOptions = null)
        {
            _credentialResolvers = credentialResolvers?.OrderBy(r => r.GetPriority()) ?? throw new ArgumentNullException(nameof(credentialResolvers));
            _logger = logger;
            _authOptions = authOptions?.Value ?? new GitAuthenticationOptions();
            _operationOptions = operationOptions?.Value ?? new GitOperationOptions();
        }

        public async Task<GitOperationResult> PullAsync(string repositoryPath)
        {
            var stopwatch = Stopwatch.StartNew();
            var credentialCallCount = 0;
            
            try
            {
                _logger.LogInformation("Starting pull operation for repository: {RepositoryPath}", repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using var repo = new Repository(repositoryPath);
                
                var pullOptions = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, usernameFromUrl, types) =>
                        {
                            credentialCallCount++;
                            _logger.LogInformation("PULL CREDENTIAL CALLBACK #{Count} - URL: {Url}, User: {User}, Types: {Types}", 
                                credentialCallCount, url, usernameFromUrl, types);
                            
                            var result = ResolveCredentials(url, usernameFromUrl, types).GetAwaiter().GetResult();
                            
                            _logger.LogInformation("PULL CREDENTIAL CALLBACK #{Count} - Resolved: {HasCredentials}, Method: {Method}", 
                                credentialCallCount, result != null, _lastUsedAuthMethod);
                            
                            return result;
                        }
                    }
                };

                // Get current HEAD to compare later
                var headCommitBefore = repo.Head.Tip?.Sha;

                // Get or create signature for the merge
                var signature = GetGitSignature(repo);

                // Perform pull
                var pullResult = Commands.Pull(repo, signature, pullOptions);

                var headCommitAfter = repo.Head.Tip?.Sha;
                var hasChanges = headCommitBefore != headCommitAfter;

                stopwatch.Stop();

                var message = pullResult.Status switch
                {
                    MergeStatus.UpToDate => "Repository is up to date",
                    MergeStatus.FastForward => "Fast-forward merge completed",
                    MergeStatus.NonFastForward => "Merge completed (non-fast-forward)",
                    MergeStatus.Conflicts => "Pull completed but conflicts need to be resolved",
                    _ => "Pull completed"
                };

                _logger.LogInformation("Pull operation completed: {Status}, HasChanges: {HasChanges}, Duration: {Duration}ms, CredentialCalls: {CredentialCalls}",
                    pullResult.Status, hasChanges, stopwatch.ElapsedMilliseconds, credentialCallCount);

                // Clear credential call history after successful operation
                ClearCredentialCallHistory();

                return new GitOperationResult
                {
                    Success = true,
                    Message = message,
                    Changes = hasChanges ? GetChangedFiles(repo) : new string[0],
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during pull operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Pull failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> PushAsync(string repositoryPath, string remoteName = "origin", string branchName = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var credentialCallCount = 0;
            
            try
            {
                _logger.LogInformation("Starting push operation for repository: {RepositoryPath}, Remote: {Remote}, Branch: {Branch}",
                    repositoryPath, remoteName, branchName ?? "current");

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using var repo = new Repository(repositoryPath);
                
                var remote = repo.Network.Remotes[remoteName];
                if (remote == null)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Remote '{remoteName}' not found",
                        Duration = stopwatch.Elapsed
                    };
                }

                var branch = string.IsNullOrEmpty(branchName) ? repo.Head : repo.Branches[branchName];
                if (branch == null)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Branch '{branchName}' not found",
                        Duration = stopwatch.Elapsed
                    };
                }

                var pushOptions = new PushOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) =>
                    {
                        credentialCallCount++;
                        _logger.LogInformation("PUSH CREDENTIAL CALLBACK #{Count} - URL: {Url}, User: {User}, Types: {Types}", 
                            credentialCallCount, url, usernameFromUrl, types);
                        
                        var task = ResolveCredentials(url, usernameFromUrl, types);
                        var result = task.GetAwaiter().GetResult();
                        
                        if (result == null)
                        {
                            _logger.LogError("PUSH CREDENTIAL CALLBACK #{Count} - No credentials resolved for URL: {Url}", 
                                credentialCallCount, url);
                            throw new InvalidOperationException($"No credentials available for {url} (Attempt #{credentialCallCount})");
                        }
                        
                        _logger.LogInformation("PUSH CREDENTIAL CALLBACK #{Count} - Resolved: {HasCredentials}, Method: {Method}", 
                            credentialCallCount, result != null, _lastUsedAuthMethod);
                        
                        return result;
                    }
                };

                // Push the branch
                _logger.LogInformation("Executing push to remote: {Remote}, Branch: {Branch}", remoteName, branch.FriendlyName);
                repo.Network.Push(branch, pushOptions);
                _logger.LogInformation("Push executed successfully");

                stopwatch.Stop();

                _logger.LogInformation("Push operation completed successfully, Duration: {Duration}ms, CredentialCalls: {CredentialCalls}", 
                    stopwatch.ElapsedMilliseconds, credentialCallCount);

                // Clear credential call history after successful operation
                ClearCredentialCallHistory();

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully pushed {branch.FriendlyName} to {remoteName}",
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during push operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Push failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> CommitAsync(string repositoryPath, string message, GitAuthor author)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting commit operation for repository: {RepositoryPath}", repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = "Commit message cannot be empty",
                        Duration = stopwatch.Elapsed
                    };
                }

                using var repo = new Repository(repositoryPath);
                
                // Stage all changes
                Commands.Stage(repo, "*");

                // Check if there are any changes to commit
                var status = repo.RetrieveStatus();
                if (!status.IsDirty)
                {
                    stopwatch.Stop();
                    return new GitOperationResult
                    {
                        Success = true,
                        Message = "No changes to commit",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Create signature
                var signature = new Signature(author.Name, author.Email, DateTimeOffset.Now);

                // Commit
                var commit = repo.Commit(message, signature, signature);

                stopwatch.Stop();

                _logger.LogInformation("Commit operation completed successfully: {CommitHash}, Duration: {Duration}ms",
                    commit.Sha, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully committed changes",
                    CommitHash = commit.Sha,
                    Changes = GetStagedFiles(repo),
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during commit operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Commit failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> CommitAndPushAsync(string repositoryPath, string message, GitAuthor author, string remoteName = "origin")
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting commit and push operation for repository: {RepositoryPath}", repositoryPath);

                // First commit
                var commitResult = await CommitAsync(repositoryPath, message, author);
                if (!commitResult.Success)
                {
                    return commitResult;
                }

                // If no changes were committed, don't try to push
                if (commitResult.Message == "No changes to commit")
                {
                    return commitResult;
                }

                // Then push
                var pushResult = await PushAsync(repositoryPath, remoteName);
                if (!pushResult.Success)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Commit succeeded but push failed: {pushResult.ErrorMessage}",
                        CommitHash = commitResult.CommitHash,
                        Duration = stopwatch.Elapsed
                    };
                }

                stopwatch.Stop();

                return new GitOperationResult
                {
                    Success = true,
                    Message = "Successfully committed and pushed changes",
                    CommitHash = commitResult.CommitHash,
                    Changes = commitResult.Changes,
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = pushResult.AuthenticationMethodUsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during commit and push operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Commit and push failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> CloneAsync(string url, string localPath, string branchName = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting clone operation: {Url} to {LocalPath}", url, localPath);

                if (Directory.Exists(localPath) && Directory.GetFileSystemEntries(localPath).Length > 0)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Target directory is not empty: {localPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                var cloneOptions = new CloneOptions
                {
                    CredentialsProvider = (repoUrl, usernameFromUrl, types) =>
                        ResolveCredentials(repoUrl, usernameFromUrl, types).GetAwaiter().GetResult(),
                    BranchName = branchName
                };

                var clonedRepoPath = Repository.Clone(url, localPath, cloneOptions);

                stopwatch.Stop();

                _logger.LogInformation("Clone operation completed successfully, Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully cloned repository to {clonedRepoPath}",
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during clone operation: {Url} to {LocalPath}", url, localPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Clone failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitBranchInfo> GetCurrentBranchAsync(string repositoryPath)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var currentBranch = repo.Head;

                return new GitBranchInfo
                {
                    Name = currentBranch.FriendlyName,
                    IsCurrent = true,
                    IsRemote = currentBranch.IsRemote,
                    CommitHash = currentBranch.Tip?.Sha,
                    LastCommitDate = currentBranch.Tip?.Committer.When.DateTime ?? DateTime.MinValue,
                    LastCommitMessage = currentBranch.Tip?.MessageShort,
                    CommitsAhead = currentBranch.TrackingDetails?.AheadBy ?? 0,
                    CommitsBehind = currentBranch.TrackingDetails?.BehindBy ?? 0,
                    RemoteTrackingBranch = currentBranch.TrackedBranch?.FriendlyName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current branch for repository: {RepositoryPath}", repositoryPath);
                return null;
            }
        }

        public async Task<IEnumerable<GitBranchInfo>> GetBranchesAsync(string repositoryPath, bool includeRemote = true)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var branches = includeRemote ? repo.Branches : repo.Branches.Where(b => !b.IsRemote);

                return branches.Select(branch => new GitBranchInfo
                {
                    Name = branch.FriendlyName,
                    IsCurrent = branch.IsCurrentRepositoryHead,
                    IsRemote = branch.IsRemote,
                    CommitHash = branch.Tip?.Sha,
                    LastCommitDate = branch.Tip?.Committer.When.DateTime ?? DateTime.MinValue,
                    LastCommitMessage = branch.Tip?.MessageShort,
                    CommitsAhead = branch.TrackingDetails?.AheadBy ?? 0,
                    CommitsBehind = branch.TrackingDetails?.BehindBy ?? 0,
                    RemoteTrackingBranch = branch.TrackedBranch?.FriendlyName
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches for repository: {RepositoryPath}", repositoryPath);
                return new List<GitBranchInfo>();
            }
        }

        public async Task<GitOperationResult> CheckoutBranchAsync(string repositoryPath, string branchName)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting checkout operation for repository: {RepositoryPath}, Branch: {Branch}",
                    repositoryPath, branchName);

                using var repo = new Repository(repositoryPath);
                
                var branch = repo.Branches[branchName];
                if (branch == null)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Branch '{branchName}' not found",
                        Duration = stopwatch.Elapsed
                    };
                }

                Commands.Checkout(repo, branch);

                stopwatch.Stop();

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully checked out branch '{branchName}'",
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during checkout operation for repository: {RepositoryPath}, Branch: {Branch}",
                    repositoryPath, branchName);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Checkout failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitRepositoryStatus> GetStatusAsync(string repositoryPath)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus();
                var currentBranch = repo.Head;

                return new GitRepositoryStatus
                {
                    Added = status.Added.Select(s => s.FilePath).ToList(),
                    Modified = status.Modified.Select(s => s.FilePath).ToList(),
                    Removed = status.Removed.Select(s => s.FilePath).ToList(),
                    Untracked = status.Untracked.Select(s => s.FilePath).ToList(),
                    CommitsAhead = currentBranch.TrackingDetails?.AheadBy ?? 0,
                    CommitsBehind = currentBranch.TrackingDetails?.BehindBy ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for repository: {RepositoryPath}", repositoryPath);
                return new GitRepositoryStatus
                {
                    Added = new List<string>(),
                    Modified = new List<string>(),
                    Removed = new List<string>(),
                    Untracked = new List<string>()
                };
            }
        }

        public async Task<GitOperationResult> FetchAsync(string repositoryPath, string remoteName = "origin")
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting fetch operation for repository: {RepositoryPath}, Remote: {Remote}",
                    repositoryPath, remoteName);

                using var repo = new Repository(repositoryPath);
                
                var remote = repo.Network.Remotes[remoteName];
                if (remote == null)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Remote '{remoteName}' not found",
                        Duration = stopwatch.Elapsed
                    };
                }

                var fetchOptions = new FetchOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) =>
                        ResolveCredentials(url, usernameFromUrl, types).GetAwaiter().GetResult()
                };

                Commands.Fetch(repo, remoteName, new string[0], fetchOptions, null);

                stopwatch.Stop();

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully fetched from {remoteName}",
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during fetch operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Fetch failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        #region Private Helper Methods

        private AuthenticationMethod _lastUsedAuthMethod = AuthenticationMethod.UserPrompt;
        private readonly Dictionary<string, CachedCredential> _credentialCache = new Dictionary<string, CachedCredential>();
        private readonly Dictionary<string, int> _credentialCallHistory = new Dictionary<string, int>();
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
        private const int MaxAuthenticationAttempts = 3;

        private class CachedCredential
        {
            public Credentials Credentials { get; set; }
            public DateTime CachedAt { get; set; }
            public AuthenticationMethod AuthMethod { get; set; }
        }

        private async Task<Credentials> ResolveCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var resolverCallId = Guid.NewGuid().ToString("N")[..8];
            var cacheKey = $"{url}:{usernameFromUrl}:{types}";
            
            // IMPORTANT: Check cache FIRST before doing anything else
            if (_credentialCache.ContainsKey(cacheKey))
            {
                var cached = _credentialCache[cacheKey];
                var age = DateTime.UtcNow - cached.CachedAt;
                
                if (age < _cacheTimeout)
                {
                    _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Using CACHED credentials for {Url} (age: {Age:F1} seconds)", 
                        resolverCallId, url, age.TotalSeconds);
                    _lastUsedAuthMethod = cached.AuthMethod;
                    return cached.Credentials;
                }
                else
                {
                    _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - Cache expired for {Url} (age: {Age:F1} minutes)", 
                        resolverCallId, url, age.TotalMinutes);
                    _credentialCache.Remove(cacheKey);
                }
            }
            
            // Track call history for this URL
            if (_credentialCallHistory.ContainsKey(cacheKey))
            {
                _credentialCallHistory[cacheKey]++;
            }
            else
            {
                _credentialCallHistory[cacheKey] = 1;
            }
            
            var callCount = _credentialCallHistory[cacheKey];
            
            _logger.LogInformation("CREDENTIAL RESOLUTION CALL [{CallId}] - URL: {Url}, User: {User}, Types: {Types}, CallCount: {CallCount}", 
                resolverCallId, url, usernameFromUrl, types, callCount);
                
            // Log warning if this is a repeated call
            if (callCount > 1)
            {
                _logger.LogWarning("CREDENTIAL RESOLUTION [{CallId}] - REPEATED CALL #{Count} for same URL/user/types combination", 
                    resolverCallId, callCount);
                    
                // If we've been called too many times, fail fast to prevent infinite loops
                if (callCount > MaxAuthenticationAttempts)
                {
                    _logger.LogError("CREDENTIAL RESOLUTION [{CallId}] - EXCEEDED MAX ATTEMPTS ({Count}/{Max}) - Failing to prevent infinite loop", 
                        resolverCallId, callCount, MaxAuthenticationAttempts);
                    return null;
                }
            }

            var resolverIndex = 0;
            foreach (var resolver in _credentialResolvers)
            {
                resolverIndex++;
                try
                {
                    _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - Checking resolver #{Index}: {ResolverType}, Priority: {Priority}", 
                        resolverCallId, resolverIndex, resolver.GetType().Name, resolver.GetPriority());

                    if (resolver.CanResolveCredentials(url, types))
                    {
                        _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Trying resolver #{Index}: {ResolverType}", 
                            resolverCallId, resolverIndex, resolver.GetType().Name);
                        
                        var credentials = await resolver.ResolveCredentialsAsync(url, usernameFromUrl, types);
                        if (credentials != null)
                        {
                            _lastUsedAuthMethod = resolver.GetAuthenticationMethod();
                            
                            // Log detailed credential type information
                            var credType = credentials.GetType().Name;
                            var isSSH = url.StartsWith("git@") || url.StartsWith("ssh://");
                            var isHTTPS = url.StartsWith("https://");
                            
                            _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - SUCCESS using {ResolverType}: {AuthMethod}, CredType: {CredType}, SSH: {IsSSH}, HTTPS: {IsHTTPS}", 
                                resolverCallId, resolver.GetType().Name, _lastUsedAuthMethod, credType, isSSH, isHTTPS);
                                
                            // Cache the successful credential for future use
                            _credentialCache[cacheKey] = new CachedCredential
                            {
                                Credentials = credentials,
                                CachedAt = DateTime.UtcNow,
                                AuthMethod = _lastUsedAuthMethod
                            };
                            
                            _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Credentials CACHED for {Url} (timeout: {Timeout} minutes)", 
                                resolverCallId, url, _cacheTimeout.TotalMinutes);
                            
                            // Reset call history on success
                            _credentialCallHistory[cacheKey] = 0;
                            
                            return credentials;
                        }
                        else
                        {
                            _logger.LogWarning("CREDENTIAL RESOLUTION [{CallId}] - FAILED {ResolverType} returned null", 
                                resolverCallId, resolver.GetType().Name);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - SKIPPED {ResolverType}: cannot handle URL/types", 
                            resolverCallId, resolver.GetType().Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CREDENTIAL RESOLUTION [{CallId}] - ERROR in {ResolverType}: {Error}", 
                        resolverCallId, resolver.GetType().Name, ex.Message);
                }
            }

            _logger.LogError("CREDENTIAL RESOLUTION [{CallId}] - FAILED: No resolver could provide credentials for URL: {Url}", 
                resolverCallId, url);
            return null;
        }

        private Signature GetGitSignature(Repository repo)
        {
            try
            {
                // Try to get from Git config
                var config = repo.Config;
                var name = config.Get<string>("user.name")?.Value;
                var email = config.Get<string>("user.email")?.Value;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                {
                    return new Signature(name, email, DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get Git signature from config");
            }

            // Fallback to default
            return new Signature("MdExplorer User", "user@mdexplorer.local", DateTimeOffset.Now);
        }

        private IEnumerable<string> GetChangedFiles(Repository repo)
        {
            try
            {
                var status = repo.RetrieveStatus();
                return status.Modified.Concat(status.Added).Concat(status.Removed)
                    .Select(s => s.FilePath).ToList();
            }
            catch
            {
                return new string[0];
            }
        }

        private IEnumerable<string> GetStagedFiles(Repository repo)
        {
            try
            {
                var status = repo.RetrieveStatus();
                return status.Staged.Select(s => s.FilePath).ToList();
            }
            catch
            {
                return new string[0];
            }
        }

        private void ClearCredentialCallHistory()
        {
            // Clear the call history to prevent false positives on next operation
            _credentialCallHistory.Clear();
            _logger.LogDebug("Credential call history cleared after successful operation");
        }

        private void CleanExpiredCache()
        {
            var expiredKeys = _credentialCache
                .Where(kvp => DateTime.UtcNow - kvp.Value.CachedAt > _cacheTimeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _credentialCache.Remove(key);
            }

            if (expiredKeys.Any())
            {
                _logger.LogDebug("Removed {Count} expired credentials from cache", expiredKeys.Count);
            }
        }

        #endregion

        public async Task<GitPullPushData> GetPullPushDataAsync(string repositoryPath)
        {
            try
            {
                _logger.LogInformation("Getting pull/push data for repository: {RepositoryPath}", repositoryPath);

                using var repo = new Repository(repositoryPath);
                var currentBranch = repo.Head;

                // Initialize the result
                var result = new GitPullPushData
                {
                    HasDataToPull = false,
                    CommitsBehind = 0,
                    CommitsAhead = 0,
                    FilesToPull = new List<GitFileChange>(),
                    IsRemoteAvailable = false,
                    RemoteConnectionError = null
                };

                // Check if we have a tracked branch
                if (currentBranch.TrackedBranch == null)
                {
                    _logger.LogWarning("Current branch {Branch} has no tracked remote branch", currentBranch.FriendlyName);
                    return result;
                }

                try
                {
                    // Fetch latest from remote to ensure accurate comparison
                    var fetchCredentialCallCount = 0;
                    var fetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, userFromUrl, types) => 
                        {
                            fetchCredentialCallCount++;
                            _logger.LogInformation("FETCH CREDENTIAL CALLBACK (GetPullPushData) #{Count} - URL: {Url}, User: {User}, Types: {Types}", 
                                fetchCredentialCallCount, url, userFromUrl, types);
                            
                            var task = ResolveCredentials(url, userFromUrl, types);
                            var result = task.GetAwaiter().GetResult();
                            
                            _logger.LogInformation("FETCH CREDENTIAL CALLBACK (GetPullPushData) #{Count} - Resolved: {HasCredentials}, Method: {Method}", 
                                fetchCredentialCallCount, result != null, _lastUsedAuthMethod);
                            
                            return result;
                        }
                    };

                    var remote = repo.Network.Remotes["origin"];
                    if (remote != null)
                    {
                        try
                        {
                            _logger.LogDebug("Fetching from remote: {RemoteName}", remote.Name);
                            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                            Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, string.Empty);
                            result.IsRemoteAvailable = true;
                            _logger.LogDebug("Fetch completed successfully");
                        }
                        catch (Exception fetchEx)
                        {
                            _logger.LogWarning(fetchEx, "Fetch failed, but continuing with cached tracking information. Error: {Error}", fetchEx.Message);
                            // Don't fail the whole operation - use cached tracking information
                            result.IsRemoteAvailable = false;
                            result.RemoteConnectionError = $"Fetch failed: {fetchEx.Message}";
                        }
                    }

                    // Get tracking details after fetch
                    var trackingDetails = currentBranch.TrackingDetails;
                    if (trackingDetails != null)
                    {
                        result.CommitsBehind = trackingDetails.BehindBy ?? 0;
                        result.CommitsAhead = trackingDetails.AheadBy ?? 0;
                        result.HasDataToPull = result.CommitsBehind > 0;

                        // Get incoming changes if there are commits behind
                        if (result.CommitsBehind > 0)
                        {
                            var trackedBranch = currentBranch.TrackedBranch;
                            var currentCommit = currentBranch.Tip;
                            var remoteCommit = trackedBranch.Tip;

                            // Get all commits between current and remote
                            var filter = new CommitFilter
                            {
                                ExcludeReachableFrom = currentCommit,
                                IncludeReachableFrom = remoteCommit,
                                SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
                            };

                            var incomingCommits = repo.Commits.QueryBy(filter).ToList();
                            _logger.LogDebug("Found {Count} incoming commits", incomingCommits.Count);

                            // Analyze changes in incoming commits
                            var changedFiles = new Dictionary<string, GitFileChange>();
                            
                            foreach (var commit in incomingCommits)
                            {
                                var parent = commit.Parents.FirstOrDefault();
                                if (parent != null)
                                {
                                    var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
                                    
                                    foreach (var change in changes)
                                    {
                                        var filePath = change.Path;
                                        if (!changedFiles.ContainsKey(filePath))
                                        {
                                            changedFiles[filePath] = new GitFileChange
                                            {
                                                FilePath = filePath,
                                                Author = commit.Author.Name,
                                                ChangeType = ConvertChangeKind(change.Status),
                                                CommitMessage = commit.MessageShort,
                                                ChangeDate = commit.Author.When.DateTime
                                            };
                                        }
                                    }
                                }
                            }

                            result.FilesToPull = changedFiles.Values.ToList();
                        }
                    }

                    _logger.LogInformation("Pull/push data retrieved: Behind={Behind}, Ahead={Ahead}, Files={Files}",
                        result.CommitsBehind, result.CommitsAhead, result.FilesToPull?.Count() ?? 0);

                    return result;
                }
                catch (LibGit2SharpException ex)
                {
                    _logger.LogWarning(ex, "Error fetching from remote, returning local data only");
                    result.RemoteConnectionError = ex.Message;
                    
                    // Return local data without remote info
                    var trackingDetails = currentBranch.TrackingDetails;
                    if (trackingDetails != null)
                    {
                        result.CommitsBehind = trackingDetails.BehindBy ?? 0;
                        result.CommitsAhead = trackingDetails.AheadBy ?? 0;
                        result.HasDataToPull = result.CommitsBehind > 0;
                    }
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pull/push data for repository: {RepositoryPath}", repositoryPath);
                
                return new GitPullPushData
                {
                    HasDataToPull = false,
                    CommitsBehind = 0,
                    CommitsAhead = 0,
                    FilesToPull = new List<GitFileChange>(),
                    IsRemoteAvailable = false,
                    RemoteConnectionError = ex.Message
                };
            }
        }

        private string ConvertChangeKind(ChangeKind changeKind)
        {
            return changeKind switch
            {
                ChangeKind.Added => "Added",
                ChangeKind.Deleted => "Deleted",
                ChangeKind.Modified => "Modified",
                ChangeKind.Renamed => "Renamed",
                ChangeKind.Copied => "Copied",
                ChangeKind.TypeChanged => "TypeChanged",
                _ => "Unknown"
            };
        }
    }
}