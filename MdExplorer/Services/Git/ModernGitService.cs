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
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.DB;

namespace MdExplorer.Services.Git
{
    public class ModernGitService : IModernGitService
    {
        private readonly IEnumerable<ICredentialResolver> _credentialResolvers;
        private readonly ILogger<ModernGitService> _logger;
        private readonly GitAuthenticationOptions _authOptions;
        private readonly GitOperationOptions _operationOptions;
        private readonly IUserSettingsDB _userSettingsDB;

        public ModernGitService(
            IEnumerable<ICredentialResolver> credentialResolvers,
            ILogger<ModernGitService> logger,
            IUserSettingsDB userSettingsDB,
            IOptions<GitAuthenticationOptions> authOptions = null,
            IOptions<GitOperationOptions> operationOptions = null)
        {
            _credentialResolvers = credentialResolvers?.OrderBy(r => r.GetPriority()) ?? throw new ArgumentNullException(nameof(credentialResolvers));
            _logger = logger;
            _userSettingsDB = userSettingsDB;
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
                    BranchName = branchName
                };

                cloneOptions.FetchOptions.CredentialsProvider = (repoUrl, usernameFromUrl, types) =>
                    ResolveCredentials(repoUrl, usernameFromUrl, types).GetAwaiter().GetResult();

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
                // Try to get from local repository config first
                var config = repo.Config;
                var name = config.Get<string>("user.name")?.Value;
                var email = config.Get<string>("user.email")?.Value;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                {
                    _logger.LogDebug("Using Git signature from local config: {Name} <{Email}>", name, email);
                    return new Signature(name, email, DateTimeOffset.Now);
                }

                // Try global Git config
                var globalName = config.Get<string>("user.name", ConfigurationLevel.Global)?.Value;
                var globalEmail = config.Get<string>("user.email", ConfigurationLevel.Global)?.Value;

                if (!string.IsNullOrEmpty(globalName) && !string.IsNullOrEmpty(globalEmail))
                {
                    _logger.LogDebug("Using Git signature from global config: {Name} <{Email}>", globalName, globalEmail);
                    return new Signature(globalName, globalEmail, DateTimeOffset.Now);
                }

                // Log warning but still return a fallback for pull operations
                _logger.LogWarning("Git user.name and user.email not configured in repository at {Path}. Using fallback signature.", repo.Info.Path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get Git signature from config");
            }

            // Fallback for pull operations (which need a signature for merge commits)
            // This should rarely be used as commits should use the GitAuthor passed from the controller
            return new Signature("Unknown User", "user@example.com", DateTimeOffset.Now);
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

                    _logger.LogWarning("[FETCH DEBUG] About to create FetchOptions with CredentialsProvider");

                    var fetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, userFromUrl, types) =>
                        {
                            fetchCredentialCallCount++;
                            _logger.LogWarning("[FETCH DEBUG] *** CREDENTIALS PROVIDER CALLED! *** Count: {Count}, URL: {Url}, User: {User}, Types: {Types}",
                                fetchCredentialCallCount, url, userFromUrl, types);

                            var task = ResolveCredentials(url, userFromUrl, types);
                            var result = task.GetAwaiter().GetResult();

                            _logger.LogWarning("[FETCH DEBUG] *** CREDENTIALS PROVIDER RETURNING *** HasCredentials: {HasCredentials}, Method: {Method}",
                                result != null, _lastUsedAuthMethod);

                            return result;
                        }
                    };

                    _logger.LogWarning("[FETCH DEBUG] FetchOptions created, looking for remote 'origin'");

                    var remote = repo.Network.Remotes["origin"];
                    if (remote != null)
                    {
                        try
                        {
                            _logger.LogWarning("[FETCH DEBUG] Remote found: {RemoteName}, URL: {RemoteUrl}", remote.Name, remote.Url);

                            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                            _logger.LogWarning("[FETCH DEBUG] About to call Commands.Fetch with {RefSpecCount} refspecs", refSpecs.Count());

                            // Log exact moment before fetch
                            _logger.LogWarning("[FETCH DEBUG] === CALLING Commands.Fetch NOW ===");
                            try
                            {
                                Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, string.Empty);
                                _logger.LogWarning("[FETCH DEBUG] === Commands.Fetch RETURNED NORMALLY ===");
                            }
                            catch (LibGit2SharpException libEx)
                            {
                                _logger.LogWarning("[FETCH DEBUG] === LibGit2SharpException in Commands.Fetch ===");
                                _logger.LogWarning("[FETCH DEBUG] Exception Type: {Type}", libEx.GetType().FullName);
                                _logger.LogWarning("[FETCH DEBUG] Exception Message: {Message}", libEx.Message);
                                _logger.LogWarning("[FETCH DEBUG] Exception StackTrace: {StackTrace}", libEx.StackTrace);

                                // Re-throw to be caught by outer catch
                                throw;
                            }

                            _logger.LogWarning("[FETCH DEBUG] Commands.Fetch completed successfully");
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

                    // Special case: check if we're dealing with an empty remote repository
                    // In this case, TrackingDetails might be null or incorrect
                    // The IsRemoteEmpty flag is set by the workaround when it detects an empty repository
                    if (result.IsRemoteEmpty && currentBranch.Tip != null)
                    {
                        // For empty remote, count all commits in the current branch as "ahead"
                        var allCommits = currentBranch.Commits.Count();
                        result.CommitsAhead = allCommits;
                        result.CommitsBehind = 0;
                        result.HasDataToPull = false;
                        _logger.LogInformation("[EMPTY REMOTE] Found {Count} commits to push to empty repository", allCommits);
                    }
                    else if (trackingDetails != null)
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
                    else if (currentBranch.Tip != null)
                    {
                        // No tracking details but we have commits - likely all need to be pushed
                        _logger.LogInformation("[NO TRACKING] No tracking details found, counting all commits as 'ahead'");
                        var allCommits = currentBranch.Commits.Count();
                        result.CommitsAhead = allCommits;
                        result.CommitsBehind = 0;
                        result.HasDataToPull = false;
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

        /// <summary>
        /// Gets the commit history for a repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="maxCommits">Maximum number of commits to retrieve</param>
        /// <returns>List of commits with author, message, and other details</returns>
        public async Task<IList<GitCommitInfo>> GetCommitHistoryAsync(string repositoryPath, int maxCommits = 50)
        {
            return await Task.Run(() =>
            {
                var commits = new List<GitCommitInfo>();

                try
                {
                    _logger.LogInformation("Getting commit history for repository: {RepositoryPath}, MaxCommits: {MaxCommits}",
                        repositoryPath, maxCommits);

                    if (!Directory.Exists(repositoryPath))
                    {
                        _logger.LogWarning("Repository directory does not exist: {RepositoryPath}", repositoryPath);
                        return commits;
                    }

                    using (var repo = new Repository(repositoryPath))
                    {
                        // Get the current branch name
                        var currentBranch = repo.Head?.FriendlyName ?? "unknown";

                        // Get commits from the current branch
                        var commitLog = repo.Commits.Take(maxCommits);

                        foreach (var commit in commitLog)
                        {
                            var commitInfo = new GitCommitInfo
                            {
                                Hash = commit.Sha,
                                Author = commit.Author.Name,
                                Email = commit.Author.Email,
                                Message = commit.Message?.Trim(),
                                Date = commit.Author.When.DateTime,
                                Branch = currentBranch,
                                Parents = commit.Parents?.Select(p => p.Sha).ToList() ?? new List<string>()
                            };

                            commits.Add(commitInfo);
                        }

                        _logger.LogInformation("Retrieved {CommitCount} commits from repository", commits.Count);
                    }
                }
                catch (RepositoryNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Repository not found at path: {RepositoryPath}", repositoryPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting commit history for repository: {RepositoryPath}", repositoryPath);
                }

                return commits;
            });
        }

        /// <summary>
        /// Checks if a remote repository is configured
        /// </summary>
        public async Task<RemoteStatus> CheckRemoteStatusAsync(string repositoryPath)
        {
            return await Task.Run(() =>
            {
                var status = new RemoteStatus();

                try
                {
                    _logger.LogInformation("Checking remote status for repository: {RepositoryPath}", repositoryPath);

                    if (!Directory.Exists(repositoryPath))
                    {
                        status.IsGitRepository = false;
                        status.ErrorMessage = "Directory does not exist";
                        return status;
                    }

                    var gitPath = Path.Combine(repositoryPath, ".git");
                    if (!Directory.Exists(gitPath))
                    {
                        status.IsGitRepository = false;
                        status.ErrorMessage = "Not a Git repository";
                        return status;
                    }

                    status.IsGitRepository = true;

                    using (var repo = new Repository(repositoryPath))
                    {
                        // Check for origin remote
                        var origin = repo.Network.Remotes["origin"];
                        if (origin != null)
                        {
                            status.HasRemote = true;
                            status.RemoteName = origin.Name;
                            status.RemoteUrl = origin.Url;
                            _logger.LogInformation("Remote found: {RemoteName} -> {RemoteUrl}", origin.Name, origin.Url);
                        }
                        else
                        {
                            status.HasRemote = false;
                            _logger.LogInformation("No remote configured for repository");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking remote status for repository: {RepositoryPath}", repositoryPath);
                    status.ErrorMessage = ex.Message;
                }

                return status;
            });
        }

        /// <summary>
        /// Removes a remote from the repository
        /// </summary>
        public async Task<GitOperationResult> RemoveRemoteAsync(string repositoryPath, string remoteName = "origin")
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Removing remote from repository: {RepositoryPath}, Remote: {RemoteName}",
                    repositoryPath, remoteName);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using (var repo = new Repository(repositoryPath))
                {
                    // Check if remote exists
                    var existingRemote = repo.Network.Remotes[remoteName];
                    if (existingRemote == null)
                    {
                        return new GitOperationResult
                        {
                            Success = false,
                            ErrorMessage = $"Remote '{remoteName}' does not exist",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    // Remove the remote
                    repo.Network.Remotes.Remove(remoteName);
                    _logger.LogInformation("Remote removed successfully: {RemoteName}", remoteName);

                    return new GitOperationResult
                    {
                        Success = true,
                        Message = $"Remote '{remoteName}' removed successfully",
                        Duration = stopwatch.Elapsed
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing remote from repository: {RepositoryPath}", repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to remove remote: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Adds a GitHub remote repository to the local repository
        /// </summary>
        public async Task<GitOperationResult> AddRemoteAsync(string repositoryPath, string organization, string repositoryName, bool pushAfterAdd = true)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Adding remote to repository: {RepositoryPath}, Org: {Org}, Repo: {Repo}",
                    repositoryPath, organization, repositoryName);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using (var repo = new Repository(repositoryPath))
                {
                    // Check if origin already exists
                    var existingRemote = repo.Network.Remotes["origin"];
                    if (existingRemote != null)
                    {
                        return new GitOperationResult
                        {
                            Success = false,
                            ErrorMessage = $"Remote 'origin' already exists: {existingRemote.Url}",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    // Construct GitHub URL
                    var remoteUrl = $"https://github.com/{organization}/{repositoryName}.git";

                    // Add the remote
                    repo.Network.Remotes.Add("origin", remoteUrl);
                    _logger.LogInformation("Remote added successfully: origin -> {RemoteUrl}", remoteUrl);

                    // Configure the branch to track the remote branch
                    var currentBranch = repo.Head;
                    if (currentBranch != null && !currentBranch.IsRemote)
                    {
                        var remoteBranchName = $"refs/remotes/origin/{currentBranch.FriendlyName}";
                        _logger.LogInformation("Setting up tracking branch: {LocalBranch} -> {RemoteBranch}",
                            currentBranch.FriendlyName, remoteBranchName);

                        try
                        {
                            // Set the upstream branch
                            repo.Branches.Update(currentBranch,
                                b => b.TrackedBranch = remoteBranchName);

                            // Also set the config directly to ensure it's properly configured
                            repo.Config.Set($"branch.{currentBranch.FriendlyName}.remote", "origin");
                            repo.Config.Set($"branch.{currentBranch.FriendlyName}.merge", $"refs/heads/{currentBranch.FriendlyName}");

                            _logger.LogInformation("Tracking branch configured successfully");
                        }
                        catch (Exception trackEx)
                        {
                            _logger.LogWarning(trackEx, "Failed to set up tracking branch, continuing anyway");
                        }
                    }

                    // If requested and there are commits, push to the remote
                    if (pushAfterAdd && repo.Head?.Tip != null)
                    {
                        _logger.LogInformation("Attempting initial push to remote");

                        // Create a simple push with the current branch
                        var currentBranchName = repo.Head.FriendlyName;
                        var pushResult = await PushAsync(repositoryPath, "origin", currentBranchName);

                        if (!pushResult.Success)
                        {
                            _logger.LogWarning("Initial push failed: {Error}. Remote was added but not pushed.", pushResult.ErrorMessage);
                            return new GitOperationResult
                            {
                                Success = true,
                                Message = $"Remote added successfully but initial push failed: {pushResult.ErrorMessage}",
                                Duration = stopwatch.Elapsed
                            };
                        }

                        return new GitOperationResult
                        {
                            Success = true,
                            Message = $"Remote added and initial push completed successfully",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    return new GitOperationResult
                    {
                        Success = true,
                        Message = "Remote added successfully",
                        Duration = stopwatch.Elapsed
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding remote to repository: {RepositoryPath}", repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to add remote: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

    }
}