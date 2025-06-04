using LibGit2Sharp;
using Microsoft.Extensions.Logging;
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

        public ModernGitService(IEnumerable<ICredentialResolver> credentialResolvers, ILogger<ModernGitService> logger)
        {
            _credentialResolvers = credentialResolvers?.OrderBy(r => r.GetPriority()) ?? throw new ArgumentNullException(nameof(credentialResolvers));
            _logger = logger;
        }

        public async Task<GitOperationResult> PullAsync(string repositoryPath)
        {
            var stopwatch = Stopwatch.StartNew();
            
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
                            ResolveCredentials(url, usernameFromUrl, types).GetAwaiter().GetResult()
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

                _logger.LogInformation("Pull operation completed: {Status}, HasChanges: {HasChanges}, Duration: {Duration}ms",
                    pullResult.Status, hasChanges, stopwatch.ElapsedMilliseconds);

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
                        ResolveCredentials(url, usernameFromUrl, types).GetAwaiter().GetResult()
                };

                // Push the branch
                repo.Network.Push(branch, pushOptions);

                stopwatch.Stop();

                _logger.LogInformation("Push operation completed successfully, Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);

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

        private async Task<Credentials> ResolveCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            _logger.LogDebug("Resolving credentials for URL: {Url}, Types: {Types}", url, types);

            foreach (var resolver in _credentialResolvers)
            {
                try
                {
                    if (resolver.CanResolveCredentials(url, types))
                    {
                        _logger.LogDebug("Trying credential resolver: {ResolverType}", resolver.GetType().Name);
                        
                        var credentials = await resolver.ResolveCredentialsAsync(url, usernameFromUrl, types);
                        if (credentials != null)
                        {
                            _lastUsedAuthMethod = resolver.GetAuthenticationMethod();
                            _logger.LogInformation("Successfully resolved credentials using: {AuthMethod}", _lastUsedAuthMethod);
                            return credentials;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Credential resolver {ResolverType} failed", resolver.GetType().Name);
                }
            }

            _logger.LogWarning("No credential resolver could provide credentials for URL: {Url}", url);
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

        #endregion
    }
}