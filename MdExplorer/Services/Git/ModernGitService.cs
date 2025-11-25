using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = repositoryPath;

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

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = repositoryPath;

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

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = localPath;

                var cloneOptions = new CloneOptions
                {
                    BranchName = branchName
                };

                cloneOptions.FetchOptions.CredentialsProvider = (repoUrl, usernameFromUrl, types) =>
                    ResolveCredentials(repoUrl, usernameFromUrl, types).GetAwaiter().GetResult();

                var clonedRepoPath = Repository.Clone(url, localPath, cloneOptions);

                // Diagnostic logging to investigate clone behavior
                try
                {
                    using (var repo = new Repository(localPath))
                    {
                        _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Local HEAD: {LocalHead}", repo.Head.Tip?.Sha);
                        _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Branch: {Branch}", repo.Head.FriendlyName);
                        _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Tracking branch: {TrackingBranch}",
                            repo.Head.TrackedBranch?.FriendlyName ?? "none");

                        var origin = repo.Network.Remotes["origin"];
                        if (origin != null)
                        {
                            _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Remote URL: {RemoteUrl}", origin.Url);

                            // Check what remote HEAD points to
                            var remoteHead = repo.Refs["refs/remotes/origin/HEAD"];
                            if (remoteHead != null)
                            {
                                _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Remote HEAD ref: {RemoteHead}",
                                    remoteHead.TargetIdentifier);
                            }
                            else
                            {
                                _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Remote HEAD ref: NOT SET");
                            }
                        }

                        // List all remote branches with their commit SHAs
                        var remoteBranches = repo.Branches.Where(b => b.IsRemote).ToList();
                        _logger.LogWarning("🔍 CLONE DIAGNOSTIC - Found {Count} remote branches:", remoteBranches.Count);
                        foreach (var branch in remoteBranches.Take(10)) // Limit to first 10 to avoid log spam
                        {
                            _logger.LogWarning("   📍 {Branch} -> {Commit}",
                                branch.FriendlyName, branch.Tip?.Sha?.Substring(0, 8));
                        }

                        // Check if local branch is behind remote
                        if (repo.Head.TrackedBranch != null)
                        {
                            var localTip = repo.Head.Tip;
                            var remoteTip = repo.Head.TrackedBranch.Tip;

                            if (localTip?.Sha != remoteTip?.Sha)
                            {
                                _logger.LogError("⚠️ CLONE DIAGNOSTIC - LOCAL IS BEHIND REMOTE!");
                                _logger.LogError("   Local commit:  {LocalSha}", localTip?.Sha);
                                _logger.LogError("   Remote commit: {RemoteSha}", remoteTip?.Sha);

                                // Calculate how many commits behind
                                var filter = new CommitFilter
                                {
                                    IncludeReachableFrom = remoteTip,
                                    ExcludeReachableFrom = localTip
                                };
                                var commitsBehind = repo.Commits.QueryBy(filter).Count();
                                _logger.LogError("   Commits behind: {Count}", commitsBehind);

                                // FIX: Force checkout of the BRANCH (not commit) to sync local with remote
                                var remoteBranchName = repo.Head.TrackedBranch?.FriendlyName;
                                _logger.LogWarning("🔧 FIX - Forcing checkout of tracked branch: {RemoteBranch} (tip: {RemoteSha})",
                                    remoteBranchName, remoteTip?.Sha);

                                try
                                {
                                    // Find the local branch that tracks this remote branch
                                    var localBranchName = repo.Head.FriendlyName;
                                    var localBranch = repo.Branches[localBranchName];

                                    if (localBranch != null)
                                    {
                                        _logger.LogInformation("🔧 FIX - Resetting local branch {LocalBranch} to match remote", localBranchName);

                                        // Reset the local branch to match the remote
                                        repo.Reset(ResetMode.Hard, remoteTip);

                                        _logger.LogInformation("✅ FIX - Successfully reset local branch to remote HEAD");
                                        _logger.LogInformation("   New local HEAD: {NewLocalSha}", repo.Head.Tip?.Sha);
                                        _logger.LogInformation("   Branch: {Branch}",
                                            repo.Head.FriendlyName);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("⚠️ FIX - Could not find local branch {LocalBranch}", localBranchName);
                                    }
                                }
                                catch (Exception checkoutEx)
                                {
                                    _logger.LogError(checkoutEx, "❌ FIX - Failed to reset local branch to remote HEAD");
                                }
                            }
                            else
                            {
                                _logger.LogInformation("✅ CLONE DIAGNOSTIC - Local HEAD matches remote HEAD");
                            }
                        }

                        // FINAL CHECK: Ensure we're not in detached HEAD state
                        // In LibGit2Sharp, detached HEAD is indicated by FriendlyName = "(no branch)"
                        var isDetached = repo.Head.FriendlyName == "(no branch)" ||
                                         !repo.Head.CanonicalName.StartsWith("refs/heads/");

                        if (isDetached)
                        {
                            _logger.LogError("⚠️ POST-CLONE CHECK - Repository is in DETACHED HEAD state!");
                            _logger.LogError("   Current HEAD: {HeadSha}", repo.Head.Tip?.Sha);

                            try
                            {
                                // Find the default branch from remote (usually origin/main or origin/master)
                                var remoteHead = repo.Refs["refs/remotes/origin/HEAD"];
                                string defaultBranchName = null;

                                if (remoteHead != null && remoteHead is SymbolicReference symRef)
                                {
                                    // Extract branch name from "refs/remotes/origin/main" -> "main"
                                    defaultBranchName = symRef.Target.CanonicalName.Replace("refs/remotes/origin/", "");
                                    _logger.LogInformation("🔧 Found default branch from origin/HEAD: {DefaultBranch}", defaultBranchName);
                                }
                                else
                                {
                                    // Fallback: try common default branches
                                    var commonDefaults = new[] { "main", "master", "develop" };
                                    foreach (var commonBranch in commonDefaults)
                                    {
                                        var remoteBranch = repo.Branches[$"origin/{commonBranch}"];
                                        if (remoteBranch != null)
                                        {
                                            defaultBranchName = commonBranch;
                                            _logger.LogInformation("🔧 Found common default branch: {DefaultBranch}", defaultBranchName);
                                            break;
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(defaultBranchName))
                                {
                                    var localBranch = repo.Branches[defaultBranchName];
                                    var remoteBranch = repo.Branches[$"origin/{defaultBranchName}"];

                                    if (localBranch == null && remoteBranch != null)
                                    {
                                        // Create local branch tracking the remote
                                        _logger.LogInformation("🔧 Creating local branch {Branch} to track origin/{Branch}", defaultBranchName, defaultBranchName);
                                        localBranch = repo.CreateBranch(defaultBranchName, remoteBranch.Tip);
                                        repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                                    }

                                    if (localBranch != null)
                                    {
                                        // Checkout the branch
                                        _logger.LogInformation("🔧 Checking out branch {Branch}", defaultBranchName);
                                        Commands.Checkout(repo, localBranch);

                                        _logger.LogInformation("✅ POST-CLONE FIX - Successfully checked out branch {Branch}", defaultBranchName);
                                        _logger.LogInformation("   HEAD is now at: {HeadSha}", repo.Head.Tip?.Sha);
                                    }
                                }
                                else
                                {
                                    _logger.LogError("❌ POST-CLONE FIX - Could not determine default branch");
                                }
                            }
                            catch (Exception detachedEx)
                            {
                                _logger.LogError(detachedEx, "❌ POST-CLONE FIX - Failed to resolve detached HEAD state");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("✅ POST-CLONE CHECK - Repository has a proper branch checked out: {Branch}",
                                repo.Head.FriendlyName);
                        }

                        // FINAL STEP: Pull to ensure we have the latest commits from remote
                        _logger.LogInformation("🔄 POST-CLONE - Performing pull to sync with remote");

                        try
                        {
                            var pullOptions = new PullOptions
                            {
                                FetchOptions = new FetchOptions
                                {
                                    CredentialsProvider = (repoUrl, usernameFromUrl, types) =>
                                        ResolveCredentials(repoUrl, usernameFromUrl, types).GetAwaiter().GetResult()
                                }
                            };

                            var signature = GetGitSignature(repo);
                            var pullResult = Commands.Pull(repo, signature, pullOptions);

                            _logger.LogInformation("✅ POST-CLONE - Pull completed: {Status}", pullResult.Status);

                            if (pullResult.Status == MergeStatus.UpToDate)
                            {
                                _logger.LogInformation("   Repository is up to date with remote");
                            }
                            else if (pullResult.Status == MergeStatus.FastForward)
                            {
                                _logger.LogInformation("   Fast-forwarded to latest commit: {CommitSha}", repo.Head.Tip?.Sha);
                            }
                        }
                        catch (Exception pullEx)
                        {
                            // Pull failure is non-fatal - the clone already succeeded
                            _logger.LogWarning(pullEx, "⚠️ POST-CLONE - Pull failed (non-fatal): {Message}", pullEx.Message);
                        }
                    }
                }
                catch (Exception diagEx)
                {
                    _logger.LogError(diagEx, "Error during clone diagnostics (non-fatal)");
                }

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

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = repositoryPath;

                using var repo = new Repository(repositoryPath);

                // STEP 1: Try to find local branch first
                var branch = repo.Branches[branchName];

                // STEP 2: If not found locally, check if it's a remote branch
                if (branch == null)
                {
                    _logger.LogInformation("Local branch '{BranchName}' not found, searching remote branches", branchName);

                    // Try to find remote branch (check all remotes, typically "origin/branchName")
                    var remoteBranch = repo.Branches.FirstOrDefault(b =>
                        b.IsRemote && b.FriendlyName.EndsWith($"/{branchName}"));

                    if (remoteBranch != null)
                    {
                        _logger.LogInformation("Found remote branch: {RemoteBranch}, creating local tracking branch '{LocalBranch}'",
                            remoteBranch.FriendlyName, branchName);

                        // Create local branch from remote tip
                        branch = repo.CreateBranch(branchName, remoteBranch.Tip);

                        // Set up tracking relationship
                        repo.Branches.Update(branch, b => b.TrackedBranch = remoteBranch.CanonicalName);

                        _logger.LogInformation("✅ Created local tracking branch '{BranchName}' → '{RemoteBranch}'",
                            branchName, remoteBranch.FriendlyName);
                    }
                    else
                    {
                        // Still not found in local or remote - return error
                        return new GitOperationResult
                        {
                            Success = false,
                            ErrorMessage = $"Branch '{branchName}' not found in local or remote branches",
                            Duration = stopwatch.Elapsed
                        };
                    }
                }
                else
                {
                    _logger.LogInformation("Found local branch: {BranchName}", branchName);
                }

                // STEP 3: Checkout the branch
                _logger.LogInformation("Checking out branch: {BranchName}", branchName);
                Commands.Checkout(repo, branch);

                // STEP 4: If it has a remote tracking branch, pull latest changes
                if (branch.TrackedBranch != null)
                {
                    _logger.LogInformation("Branch has remote tracking: {TrackedBranch}, pulling latest changes",
                        branch.TrackedBranch.FriendlyName);

                    try
                    {
                        var pullOptions = new PullOptions
                        {
                            FetchOptions = new FetchOptions
                            {
                                CredentialsProvider = (url, usernameFromUrl, types) =>
                                    ResolveCredentials(url, usernameFromUrl, types).GetAwaiter().GetResult()
                            }
                        };

                        var signature = GetGitSignature(repo);
                        var pullResult = Commands.Pull(repo, signature, pullOptions);

                        _logger.LogInformation("✅ Pull completed: {PullStatus}", pullResult.Status);

                        if (pullResult.Status == MergeStatus.UpToDate)
                        {
                            _logger.LogInformation("   Repository is up to date with remote");
                        }
                        else if (pullResult.Status == MergeStatus.FastForward)
                        {
                            _logger.LogInformation("   Fast-forwarded to latest commit: {CommitSha}", repo.Head.Tip?.Sha);
                        }
                    }
                    catch (Exception pullEx)
                    {
                        // Pull failure is non-fatal - checkout already succeeded
                        _logger.LogWarning(pullEx, "⚠️ Pull after checkout failed (non-fatal): {Message}", pullEx.Message);
                    }
                }
                else
                {
                    _logger.LogInformation("Branch has no remote tracking, skipping pull");
                }

                stopwatch.Stop();

                // Verify the current branch with a fresh repository instance to avoid caching issues
                string currentBranchName;
                using (var freshRepo = new Repository(repositoryPath))
                {
                    currentBranchName = freshRepo.Head.FriendlyName;
                    _logger.LogInformation("✅ Verified current branch from fresh repository: {CurrentBranch}", currentBranchName);
                }

                _logger.LogInformation("✅ Checkout operation completed successfully: {BranchName}, Duration: {Duration}ms",
                    branchName, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully checked out branch '{branchName}'",
                    BranchName = currentBranchName,  // Return verified branch name
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
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

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = repositoryPath;

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

        // STATIC cache shared across all instances - per-project and permanent until application close
        // Key format: "repositoryPath|url|username|types"
        private static readonly Dictionary<string, CachedCredential> _credentialCache = new Dictionary<string, CachedCredential>();
        private static readonly Dictionary<string, int> _credentialCallHistory = new Dictionary<string, int>();
        private static readonly Dictionary<string, SemaphoreSlim> _credentialResolutionLocks = new Dictionary<string, SemaphoreSlim>();
        private static readonly object _cacheLock = new object(); // Thread safety for cache access
        private static readonly object _lockDictionaryLock = new object(); // Thread safety for lock dictionary

        private const int MaxAuthenticationAttempts = 3;

        private class CachedCredential
        {
            public Credentials Credentials { get; set; }
            public DateTime CachedAt { get; set; }
            public AuthenticationMethod AuthMethod { get; set; }
            public string RepositoryPath { get; set; }
        }

        private async Task<Credentials> ResolveCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var resolverCallId = Guid.NewGuid().ToString("N")[..8];

            // Get repository path from execution context for per-project caching
            var repositoryPath = GitExecutionContext.CurrentRepositoryPath ?? "global";

            // Create per-project cache key
            var cacheKey = $"{repositoryPath}|{url}|{usernameFromUrl}|{types}";

            // IMPORTANT: Check cache FIRST before doing anything else - with thread safety
            // Cache is PERMANENT (no timeout) - credentials persist until application close
            lock (_cacheLock)
            {
                if (_credentialCache.ContainsKey(cacheKey))
                {
                    var cached = _credentialCache[cacheKey];
                    var age = DateTime.UtcNow - cached.CachedAt;

                    _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Using CACHED credentials for {Url} in project {Project} (age: {Age:F1} seconds)",
                        resolverCallId, url, repositoryPath, age.TotalSeconds);
                    _lastUsedAuthMethod = cached.AuthMethod;
                    return cached.Credentials;
                }
            }

            // Get or create a semaphore for this specific cache key to prevent concurrent resolution
            SemaphoreSlim resolutionLock;
            lock (_lockDictionaryLock)
            {
                if (!_credentialResolutionLocks.ContainsKey(cacheKey))
                {
                    _credentialResolutionLocks[cacheKey] = new SemaphoreSlim(1, 1);
                }
                resolutionLock = _credentialResolutionLocks[cacheKey];
            }

            // Wait for any ongoing credential resolution for this cache key
            _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Waiting for resolution lock for {Url}", resolverCallId, url);
            await resolutionLock.WaitAsync();

            try
            {
                // Double-check cache after acquiring lock (another thread might have resolved it)
                lock (_cacheLock)
                {
                    if (_credentialCache.ContainsKey(cacheKey))
                    {
                        var cached = _credentialCache[cacheKey];
                        var age = DateTime.UtcNow - cached.CachedAt;

                        _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Using CACHED credentials (found after lock wait) for {Url} in project {Project} (age: {Age:F1} seconds)",
                            resolverCallId, url, repositoryPath, age.TotalSeconds);
                        _lastUsedAuthMethod = cached.AuthMethod;
                        return cached.Credentials;
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

                            // Cache the successful credential for future use - with thread safety
                            // Credentials are cached per-project and persist until application close
                            lock (_cacheLock)
                            {
                                _credentialCache[cacheKey] = new CachedCredential
                                {
                                    Credentials = credentials,
                                    CachedAt = DateTime.UtcNow,
                                    AuthMethod = _lastUsedAuthMethod,
                                    RepositoryPath = repositoryPath
                                };

                                _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Credentials CACHED PERMANENTLY for {Url} in project {Project} (valid until application close)",
                                    resolverCallId, url, repositoryPath);

                                // Reset call history on success
                                _credentialCallHistory[cacheKey] = 0;
                            }
                            
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
            finally
            {
                // Always release the semaphore
                resolutionLock.Release();
                _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - Released resolution lock for {Url}", resolverCallId, url);
            }
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
            // Clear the call history to prevent false positives on next operation - with thread safety
            lock (_cacheLock)
            {
                _credentialCallHistory.Clear();
                _logger.LogDebug("Credential call history cleared after successful operation");
            }
        }

        /// <summary>
        /// Clears all cached credentials for a specific repository
        /// Useful when changing credentials for a project
        /// </summary>
        public void ClearProjectCache(string repositoryPath)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _credentialCache
                    .Where(kvp => kvp.Value.RepositoryPath == repositoryPath)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _credentialCache.Remove(key);
                }

                if (keysToRemove.Any())
                {
                    _logger.LogInformation("Cleared {Count} cached credentials for project: {RepositoryPath}", keysToRemove.Count, repositoryPath);
                }
            }
        }

        #endregion

        public async Task<GitPullPushData> GetPullPushDataAsync(string repositoryPath)
        {
            var callId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogWarning("🟢 [GET PULL PUSH DATA - START] CallId: {CallId}, Repository: {RepositoryPath}", callId, repositoryPath);

            try
            {
                _logger.LogInformation("🟢 [GET PULL PUSH DATA {CallId}] Getting pull/push data for repository: {RepositoryPath}", callId, repositoryPath);

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = repositoryPath;

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
            var callId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogWarning("🔵 [CHECK REMOTE STATUS - START] CallId: {CallId}, Repository: {RepositoryPath}", callId, repositoryPath);

            return await Task.Run(() =>
            {
                var status = new RemoteStatus
                {
                    CanAuthenticate = false,
                    AuthenticationMethod = null
                };

                try
                {
                    _logger.LogInformation("🔵 [CHECK REMOTE STATUS {CallId}] Checking remote status for repository: {RepositoryPath}", callId, repositoryPath);

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

                            // Test authentication by attempting a lightweight fetch
                            status.CanAuthenticate = TestRemoteAuthentication(repo, origin, repositoryPath);
                            if (status.CanAuthenticate)
                            {
                                status.AuthenticationMethod = _lastUsedAuthMethod.ToString();
                                _logger.LogInformation("Authentication test successful using method: {Method}", status.AuthenticationMethod);
                            }
                            else
                            {
                                _logger.LogWarning("Authentication test failed for remote: {RemoteUrl}", origin.Url);
                            }
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
        /// Tests if authentication to the remote works by attempting a lightweight fetch
        /// </summary>
        private bool TestRemoteAuthentication(Repository repo, Remote remote, string repositoryPath)
        {
            try
            {
                _logger.LogWarning("🔐 [CHECK REMOTE STATUS] Testing authentication for remote: {RemoteUrl}", remote.Url);

                // Set repository path in execution context for credential resolvers
                GitExecutionContext.CurrentRepositoryPath = repositoryPath;

                // Attempt to list remote references (lightweight operation that tests authentication)
                // Using inline delegate for credentials provider
                _logger.LogWarning("🔐 [CHECK REMOTE STATUS] About to call ListReferences...");
                var refs = repo.Network.ListReferences(remote, (url, usernameFromUrl, types) =>
                {
                    _logger.LogWarning("🔐 [CHECK REMOTE STATUS - CREDENTIAL CALLBACK] Resolving credentials for: {Url}", url);
                    var task = ResolveCredentials(url, usernameFromUrl, types);
                    var result = task.GetAwaiter().GetResult();
                    _logger.LogWarning("🔐 [CHECK REMOTE STATUS - CREDENTIAL CALLBACK] Resolved: {HasCreds}, Method: {Method}",
                        result != null, _lastUsedAuthMethod);
                    return result;
                });

                // If we get here without exception, authentication worked
                _logger.LogWarning("🔐 [CHECK REMOTE STATUS] Authentication test successful - {RefCount} references found", refs.Count());
                return true;
            }
            catch (LibGit2SharpException ex)
            {
                _logger.LogWarning(ex, "🔐 [CHECK REMOTE STATUS] Authentication test failed: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "🔐 [CHECK REMOTE STATUS] Authentication test error: {Message}", ex.Message);
                return false;
            }
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

        /// <summary>
        /// Initializes a new Git repository
        /// </summary>
        public async Task<InitRepositoryResponse> InitRepositoryAsync(InitRepositoryRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var callId = Guid.NewGuid().ToString("N").Substring(0, 8);

            _logger.LogWarning($"🔵 [INIT REPOSITORY - START] CallId: {callId}, Path: {request.RepositoryPath}");

            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.RepositoryPath))
                {
                    return new InitRepositoryResponse
                    {
                        Success = false,
                        Message = "Repository path is required",
                        IsGitRepository = false
                    };
                }

                if (!Directory.Exists(request.RepositoryPath))
                {
                    return new InitRepositoryResponse
                    {
                        Success = false,
                        Message = $"Directory does not exist: {request.RepositoryPath}",
                        IsGitRepository = false,
                        RepositoryPath = request.RepositoryPath
                    };
                }

                var gitPath = Path.Combine(request.RepositoryPath, ".git");

                // Check if Git repository already exists
                if (Directory.Exists(gitPath))
                {
                    _logger.LogInformation($"[INIT REPOSITORY {callId}] Git repository already exists");
                    return new InitRepositoryResponse
                    {
                        Success = false,
                        Message = "Git repository already initialized",
                        IsGitRepository = true,
                        RepositoryPath = request.RepositoryPath
                    };
                }

                // Initialize Git repository
                _logger.LogInformation($"[INIT REPOSITORY {callId}] Initializing Git repository");
                Repository.Init(request.RepositoryPath);

                // Create .gitignore file
                await CreateGitignoreFileAsync(request.RepositoryPath, request.GitignoreTemplate);

                // Set initial branch name if specified
                if (!string.IsNullOrWhiteSpace(request.InitialBranch) && request.InitialBranch != "master")
                {
                    using (var repo = new Repository(request.RepositoryPath))
                    {
                        // Create initial commit to establish branch
                        var signature = new Signature("MdExplorer", "noreply@mdexplorer.net", DateTimeOffset.Now);
                        repo.Commit($"Initial commit", signature, signature, new CommitOptions { AllowEmptyCommit = true });

                        // Rename branch
                        var currentBranch = repo.Head;
                        repo.Branches.Rename(currentBranch, request.InitialBranch);
                    }
                }

                stopwatch.Stop();
                _logger.LogInformation($"✅ [INIT REPOSITORY {callId}] Repository initialized successfully in {stopwatch.ElapsedMilliseconds}ms");

                return new InitRepositoryResponse
                {
                    Success = true,
                    Message = $"Git repository initialized successfully with branch '{request.InitialBranch}'",
                    IsGitRepository = true,
                    RepositoryPath = request.RepositoryPath,
                    InitialBranch = request.InitialBranch
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError($"❌ [INIT REPOSITORY {callId}] Error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                return new InitRepositoryResponse
                {
                    Success = false,
                    Message = $"Failed to initialize Git repository: {ex.Message}",
                    IsGitRepository = false,
                    RepositoryPath = request.RepositoryPath
                };
            }
        }

        /// <summary>
        /// Creates a .gitignore file based on the selected template
        /// </summary>
        private async Task CreateGitignoreFileAsync(string repositoryPath, string template)
        {
            var gitignorePath = Path.Combine(repositoryPath, ".gitignore");

            // Don't overwrite existing .gitignore
            if (File.Exists(gitignorePath))
            {
                _logger.LogInformation($"[CREATE GITIGNORE] .gitignore already exists, skipping");
                return;
            }

            var content = GetGitignoreTemplate(template);
            await File.WriteAllTextAsync(gitignorePath, content);
            _logger.LogInformation($"[CREATE GITIGNORE] Created .gitignore with template: {template}");
        }

        /// <summary>
        /// Gets the .gitignore template content
        /// </summary>
        private string GetGitignoreTemplate(string template)
        {
            var sb = new StringBuilder();

            switch (template?.ToLower())
            {
                case "mdexplorer":
                    sb.AppendLine("# MdExplorer specific files and folders");
                    sb.AppendLine(".md/");
                    sb.AppendLine("");
                    sb.AppendLine("# Database files");
                    sb.AppendLine("*.db");
                    sb.AppendLine("*.db-shm");
                    sb.AppendLine("*.db-wal");
                    sb.AppendLine("");
                    sb.AppendLine("# Temporary files");
                    sb.AppendLine("*.tmp");
                    sb.AppendLine("*.temp");
                    sb.AppendLine("~*");
                    sb.AppendLine("");
                    sb.AppendLine("# Log files");
                    sb.AppendLine("*.log");
                    sb.AppendLine("");
                    sb.AppendLine("# OS specific files");
                    sb.AppendLine(".DS_Store");
                    sb.AppendLine("Thumbs.db");
                    sb.AppendLine("desktop.ini");
                    break;

                case "node":
                    sb.AppendLine("# Node.js");
                    sb.AppendLine("node_modules/");
                    sb.AppendLine("npm-debug.log*");
                    sb.AppendLine("yarn-debug.log*");
                    sb.AppendLine("yarn-error.log*");
                    sb.AppendLine(".npm");
                    sb.AppendLine(".env");
                    sb.AppendLine(".env.local");
                    sb.AppendLine("");
                    sb.AppendLine("# Build outputs");
                    sb.AppendLine("dist/");
                    sb.AppendLine("build/");
                    sb.AppendLine("*.tgz");
                    break;

                case "python":
                    sb.AppendLine("# Python");
                    sb.AppendLine("__pycache__/");
                    sb.AppendLine("*.py[cod]");
                    sb.AppendLine("*$py.class");
                    sb.AppendLine("*.so");
                    sb.AppendLine(".Python");
                    sb.AppendLine("");
                    sb.AppendLine("# Virtual environments");
                    sb.AppendLine("venv/");
                    sb.AppendLine("ENV/");
                    sb.AppendLine(".venv");
                    sb.AppendLine("");
                    sb.AppendLine("# Distribution / packaging");
                    sb.AppendLine("dist/");
                    sb.AppendLine("build/");
                    sb.AppendLine("*.egg-info/");
                    break;

                case "csharp":
                    sb.AppendLine("# .NET / C#");
                    sb.AppendLine("bin/");
                    sb.AppendLine("obj/");
                    sb.AppendLine("*.dll");
                    sb.AppendLine("*.exe");
                    sb.AppendLine("*.pdb");
                    sb.AppendLine("");
                    sb.AppendLine("# User-specific files");
                    sb.AppendLine("*.user");
                    sb.AppendLine("*.suo");
                    sb.AppendLine("*.userosscache");
                    sb.AppendLine("");
                    sb.AppendLine("# Visual Studio");
                    sb.AppendLine(".vs/");
                    sb.AppendLine("*.sln.docstates");
                    break;

                case "none":
                    // Empty .gitignore
                    sb.AppendLine("# No template selected");
                    break;

                default:
                    // Default to mdexplorer template
                    return GetGitignoreTemplate("mdexplorer");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Discards changes to a specific file (equivalent to git restore/checkout -- file)
        /// </summary>
        public async Task<GitOperationResult> DiscardFileChangesAsync(string repositoryPath, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Discarding changes for file: {FilePath} in repository: {RepositoryPath}", filePath, repositoryPath);

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

                // Check if the file exists in the current commit (HEAD)
                var treeEntry = repo.Head.Tip?.Tree[filePath];
                if (treeEntry == null)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"File '{filePath}' does not exist in the current commit. Use UnstageFileAsync for new files.",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Restore the file from HEAD using CheckoutPaths
                var checkoutOptions = new CheckoutOptions
                {
                    CheckoutModifiers = CheckoutModifiers.Force
                };

                repo.CheckoutPaths(repo.Head.FriendlyName, new[] { filePath }, checkoutOptions);

                stopwatch.Stop();

                _logger.LogInformation("Successfully discarded changes for file: {FilePath}, Duration: {Duration}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully discarded changes for '{filePath}'",
                    Changes = new[] { filePath },
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error discarding changes for file: {FilePath} in repository: {RepositoryPath}",
                    filePath, repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to discard changes: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Removes a file from staging area (equivalent to git reset HEAD file)
        /// The file remains on disk as untracked
        /// </summary>
        public async Task<GitOperationResult> UnstageFileAsync(string repositoryPath, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Unstaging file: {FilePath} in repository: {RepositoryPath}", filePath, repositoryPath);

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

                // Unstage the file (removes it from the index)
                Commands.Unstage(repo, filePath);

                stopwatch.Stop();

                _logger.LogInformation("Successfully unstaged file: {FilePath}, Duration: {Duration}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully removed '{filePath}' from staging",
                    Changes = new[] { filePath },
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error unstaging file: {FilePath} in repository: {RepositoryPath}",
                    filePath, repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to unstage file: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Deletes an untracked file from disk
        /// </summary>
        public async Task<GitOperationResult> DeleteUntrackedFileAsync(string repositoryPath, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Deleting untracked file: {FilePath} in repository: {RepositoryPath}", filePath, repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                var fullPath = Path.Combine(repositoryPath, filePath);

                if (!File.Exists(fullPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"File does not exist: {filePath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Verify the file is untracked or newly added (not part of HEAD)
                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus(filePath);

                // Allow deletion only for untracked or newly added files
                if (status != FileStatus.NewInIndex && status != FileStatus.NewInWorkdir &&
                    !status.HasFlag(FileStatus.NewInIndex) && !status.HasFlag(FileStatus.NewInWorkdir))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"File '{filePath}' is not a new/untracked file and cannot be deleted this way.",
                        Duration = stopwatch.Elapsed
                    };
                }

                // First unstage if it's staged
                if (status.HasFlag(FileStatus.NewInIndex))
                {
                    Commands.Unstage(repo, filePath);
                }

                // Delete the file
                File.Delete(fullPath);

                stopwatch.Stop();

                _logger.LogInformation("Successfully deleted file: {FilePath}, Duration: {Duration}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully deleted '{filePath}'",
                    Changes = new[] { filePath },
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting file: {FilePath} in repository: {RepositoryPath}",
                    filePath, repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to delete file: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Gets detailed information about all changed files in the repository
        /// </summary>
        public async Task<GitDetailedStatus> GetDetailedStatusAsync(string repositoryPath)
        {
            try
            {
                _logger.LogInformation("Getting detailed status for repository: {RepositoryPath}", repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitDetailedStatus { Files = new List<GitChangedFileInfo>() };
                }

                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus();

                var files = new List<GitChangedFileInfo>();

                // Process Modified files
                foreach (var item in status.Modified)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Modified",
                        IsNew = false
                    });
                }

                // Process Added (staged new files)
                foreach (var item in status.Added)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Added",
                        IsNew = true
                    });
                }

                // Process Removed files
                foreach (var item in status.Removed)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Deleted",
                        IsNew = false
                    });
                }

                // Process Untracked files
                foreach (var item in status.Untracked)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Untracked",
                        IsNew = true
                    });
                }

                _logger.LogInformation("Found {Count} changed files in repository", files.Count);

                return new GitDetailedStatus { Files = files };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed status for repository: {RepositoryPath}", repositoryPath);
                return new GitDetailedStatus { Files = new List<GitChangedFileInfo>() };
            }
        }

    }
}