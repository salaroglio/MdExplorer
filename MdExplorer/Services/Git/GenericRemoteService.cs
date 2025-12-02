using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Service for generic Git remote operations supporting multiple providers
    /// </summary>
    public class GenericRemoteService : IGenericRemoteService
    {
        private readonly ILogger<GenericRemoteService> _logger;
        private readonly IGitRemoteUrlParser _urlParser;
        private readonly IGitHubService _gitHubService;
        private readonly IModernGitService _modernGitService;

        public GenericRemoteService(
            ILogger<GenericRemoteService> logger,
            IGitRemoteUrlParser urlParser,
            IGitHubService gitHubService,
            IModernGitService modernGitService)
        {
            _logger = logger;
            _urlParser = urlParser;
            _gitHubService = gitHubService;
            _modernGitService = modernGitService;
        }

        public async Task<ValidateRemoteResult> ValidateRemoteWithCredentialsAsync(ValidateRemoteRequest request)
        {
            var result = new ValidateRemoteResult();

            try
            {
                _logger.LogInformation("Validating remote: {RemoteUrl}", request.RemoteUrl);

                // Parse URL first
                var urlInfo = _urlParser.ParseUrl(request.RemoteUrl);
                if (!urlInfo.IsValid)
                {
                    return new ValidateRemoteResult
                    {
                        IsReachable = false,
                        Error = urlInfo.Error ?? "Invalid remote URL"
                    };
                }

                result.Provider = urlInfo.Provider;

                // Try to list references with provided credentials
                var credentials = BuildCredentials(request);

                try
                {
                    var refs = Repository.ListRemoteReferences(request.RemoteUrl, (url, usernameFromUrl, types) => credentials);

                    // If we get here, the remote is reachable and credentials work
                    result.IsReachable = true;
                    result.CredentialsValid = true;
                    result.RepositoryExists = true;
                    result.RequiresAuth = !string.IsNullOrEmpty(request.Username) || !string.IsNullOrEmpty(request.Password);

                    _logger.LogInformation("Remote validation successful: {RefCount} references found", refs.Count());
                }
                catch (LibGit2SharpException ex)
                {
                    _logger.LogWarning(ex, "Remote validation failed: {Message}", ex.Message);

                    var errorMsg = ex.Message.ToLowerInvariant();

                    // Check for different error types
                    if (IsNetworkError(errorMsg))
                    {
                        result.IsReachable = false;
                        result.Error = "Cannot connect to remote server. Check your network connection or VPN.";
                    }
                    else if (IsForbiddenError(errorMsg))
                    {
                        result.IsReachable = true;
                        result.RequiresAuth = true;
                        result.CredentialsValid = false;
                        result.Error = "Access forbidden (403). Your token may lack required permissions. For GitHub, ensure the token has 'repo' scope. Go to github.com/settings/tokens to check/regenerate.";
                    }
                    else if (IsAuthError(errorMsg))
                    {
                        result.IsReachable = true;
                        result.RequiresAuth = true;
                        result.CredentialsValid = false;
                        result.Error = "Authentication failed. Check your username and password/token.";
                    }
                    else if (IsNotFoundError(errorMsg))
                    {
                        result.IsReachable = true;
                        result.RepositoryExists = false;

                        // GitHub returns "not found" for auth errors too (security measure)
                        // Provide more helpful message based on provider
                        var providerInfo = _urlParser.ParseUrl(request.RemoteUrl);
                        if (providerInfo.Provider == "github")
                        {
                            result.Error = "Repository not found OR invalid credentials. GitHub requires a Personal Access Token (PAT) instead of password. Create one at github.com/settings/tokens";
                            result.CredentialsValid = false;
                        }
                        else
                        {
                            result.Error = "Repository not found. Create it first or check the URL and credentials.";
                        }
                    }
                    else
                    {
                        result.Error = ex.Message;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating remote: {RemoteUrl}", request.RemoteUrl);
                return new ValidateRemoteResult
                {
                    IsReachable = false,
                    Error = $"Validation error: {ex.Message}"
                };
            }
        }

        public async Task<SetupRemoteGenericResult> SetupRemoteGenericAsync(SetupRemoteGenericRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new SetupRemoteGenericResult();

            try
            {
                _logger.LogInformation("Setting up generic remote: {RemoteUrl} for repository: {RepositoryPath}",
                    request.RemoteUrl, request.RepositoryPath);

                // Parse and validate URL
                var urlInfo = _urlParser.ParseUrl(request.RemoteUrl);
                if (!urlInfo.IsValid)
                {
                    return new SetupRemoteGenericResult
                    {
                        Success = false,
                        Error = urlInfo.Error ?? "Invalid remote URL",
                        DurationMs = stopwatch.ElapsedMilliseconds
                    };
                }

                // Validate repository path
                if (!Directory.Exists(request.RepositoryPath))
                {
                    return new SetupRemoteGenericResult
                    {
                        Success = false,
                        Error = $"Repository directory does not exist: {request.RepositoryPath}",
                        DurationMs = stopwatch.ElapsedMilliseconds
                    };
                }

                var gitPath = Path.Combine(request.RepositoryPath, ".git");
                if (!Directory.Exists(gitPath))
                {
                    return new SetupRemoteGenericResult
                    {
                        Success = false,
                        Error = "Directory is not a Git repository",
                        DurationMs = stopwatch.ElapsedMilliseconds
                    };
                }

                // Step 1: Optionally create remote repository (if supported and requested)
                if (request.CreateRemoteRepo && urlInfo.SupportsAutoCreate)
                {
                    var createResult = await CreateRemoteRepositoryAsync(urlInfo, request);
                    if (!createResult.success && !createResult.alreadyExists)
                    {
                        return new SetupRemoteGenericResult
                        {
                            Success = false,
                            Error = createResult.error,
                            DurationMs = stopwatch.ElapsedMilliseconds
                        };
                    }
                    result.RepositoryCreated = !createResult.alreadyExists;
                }

                // Step 2: Save credentials if requested
                if (request.SaveCredentials && !string.IsNullOrEmpty(request.Username))
                {
                    var effectivePassword = GetEffectivePassword(request);
                    await StoreCredentialsAsync(request.RemoteUrl, request.Username, effectivePassword);
                }

                // Step 3: Add remote to local repository
                var remoteUrl = urlInfo.CloneUrl;
                result.RemoteUrl = remoteUrl;

                // Variables to track push status (declared outside using block)
                bool pushAttempted = false;
                bool pushSucceeded = false;
                string pushError = null;

                using (var repo = new Repository(request.RepositoryPath))
                {
                    // Check if remote already exists
                    var existingRemote = repo.Network.Remotes[request.RemoteName];
                    if (existingRemote != null)
                    {
                        // Update existing remote URL
                        repo.Network.Remotes.Update(request.RemoteName, r => r.Url = remoteUrl);
                        _logger.LogInformation("Updated existing remote '{RemoteName}' to: {RemoteUrl}", request.RemoteName, remoteUrl);
                    }
                    else
                    {
                        // Add new remote
                        repo.Network.Remotes.Add(request.RemoteName, remoteUrl);
                        _logger.LogInformation("Added new remote '{RemoteName}': {RemoteUrl}", request.RemoteName, remoteUrl);
                    }

                    // Configure tracking branch
                    ConfigureTrackingBranch(repo, request.RemoteName);

                    // Step 4: Push if requested
                    if (request.PushAfterAdd)
                    {
                        // Check if there are commits to push
                        if (repo.Head?.Tip == null)
                        {
                            // No commits yet - create initial commit if there are files
                            var hasFiles = repo.RetrieveStatus().Any(s =>
                                s.State != FileStatus.Ignored &&
                                s.State != FileStatus.Nonexistent);

                            if (hasFiles)
                            {
                                _logger.LogInformation("No commits found, creating initial commit...");

                                // Stage all files
                                Commands.Stage(repo, "*");

                                // Create initial commit
                                var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
                                if (signature == null)
                                {
                                    // Use default signature if not configured
                                    signature = new Signature("MdExplorer", "mdexplorer@local", DateTimeOffset.Now);
                                }

                                repo.Commit("Initial commit", signature, signature);
                                _logger.LogInformation("Initial commit created successfully");
                            }
                            else
                            {
                                _logger.LogInformation("No files to commit, skipping push");
                            }
                        }

                        // Now try to push if we have commits
                        if (repo.Head?.Tip != null)
                        {
                            pushAttempted = true;
                            var pushResult = await PushWithCredentialsAsync(repo, request);
                            pushSucceeded = pushResult.success;
                            pushError = pushResult.error;
                        }
                    }
                }

                // Build appropriate message
                string message;
                if (!request.PushAfterAdd)
                {
                    message = "Remote configured successfully";
                }
                else if (pushSucceeded)
                {
                    message = "Remote configured and code pushed successfully";
                }
                else if (pushAttempted && !string.IsNullOrEmpty(pushError))
                {
                    result.Success = true;
                    result.Message = $"Remote configured successfully, but push failed: {pushError}";
                    result.DurationMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }
                else
                {
                    message = "Remote configured successfully (no commits to push)";
                }

                result.Success = true;
                result.Message = message;
                result.DurationMs = stopwatch.ElapsedMilliseconds;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up remote: {RemoteUrl}", request.RemoteUrl);
                return new SetupRemoteGenericResult
                {
                    Success = false,
                    Error = $"Failed to setup remote: {ex.Message}",
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public async Task<bool> StoreCredentialsAsync(string remoteUrl, string username, string password)
        {
            try
            {
                _logger.LogInformation("Storing credentials for: {RemoteUrl}", remoteUrl);

                // Use git credential helper to store credentials
                var urlInfo = _urlParser.ParseUrl(remoteUrl);
                if (!urlInfo.IsValid) return false;

                // Build credential input for git credential helper
                var credentialInput = $"protocol=https\nhost={urlInfo.Host}\nusername={username}\npassword={password}\n";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "credential approve",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    await process.StandardInput.WriteAsync(credentialInput);
                    process.StandardInput.Close();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("Credentials stored successfully for: {Host}", urlInfo.Host);
                        return true;
                    }

                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogWarning("Failed to store credentials: {Error}", error);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing credentials for: {RemoteUrl}", remoteUrl);
                return false;
            }
        }

        private Credentials BuildCredentials(ValidateRemoteRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) && string.IsNullOrEmpty(request.Password))
            {
                return new DefaultCredentials();
            }

            // For PAT, use token as password with username (or "git" as default username)
            var username = !string.IsNullOrEmpty(request.Username) ? request.Username : "git";
            var password = request.Password ?? string.Empty;

            return new UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            };
        }

        private string GetEffectivePassword(SetupRemoteGenericRequest request)
        {
            // If using PAT, the token is the password
            if (request.AuthMethod == "pat" && !string.IsNullOrEmpty(request.Token))
            {
                return request.Token;
            }
            return request.Password ?? string.Empty;
        }

        private async Task<(bool success, bool alreadyExists, string error)> CreateRemoteRepositoryAsync(
            RemoteUrlInfo urlInfo, SetupRemoteGenericRequest request)
        {
            try
            {
                if (urlInfo.Provider == "github")
                {
                    var result = await _gitHubService.CreateRepositoryAsync(
                        urlInfo.Owner,
                        urlInfo.RepoName,
                        request.RepoDescription,
                        request.IsPrivate);

                    return (result.Success, result.AlreadyExists, result.ErrorMessage);
                }

                // GitLab and other providers - not implemented yet
                _logger.LogWarning("Auto-create not supported for provider: {Provider}", urlInfo.Provider);
                return (false, false, $"Automatic repository creation not supported for {urlInfo.Provider}. Please create the repository manually.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating remote repository");
                return (false, false, ex.Message);
            }
        }

        private void ConfigureTrackingBranch(Repository repo, string remoteName)
        {
            try
            {
                var currentBranch = repo.Head;
                if (currentBranch != null && !currentBranch.IsRemote)
                {
                    var remoteBranchName = $"refs/remotes/{remoteName}/{currentBranch.FriendlyName}";
                    repo.Branches.Update(currentBranch, b => b.TrackedBranch = remoteBranchName);
                    repo.Config.Set($"branch.{currentBranch.FriendlyName}.remote", remoteName);
                    repo.Config.Set($"branch.{currentBranch.FriendlyName}.merge", $"refs/heads/{currentBranch.FriendlyName}");

                    _logger.LogInformation("Configured tracking branch: {Branch} -> {RemoteBranch}",
                        currentBranch.FriendlyName, remoteBranchName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to configure tracking branch");
            }
        }

        private async Task<(bool success, string error)> PushWithCredentialsAsync(
            Repository repo, SetupRemoteGenericRequest request)
        {
            try
            {
                var currentBranchName = repo.Head.FriendlyName;

                // Use ModernGitService for push (which handles credentials properly)
                var pushResult = await _modernGitService.PushAsync(request.RepositoryPath, request.RemoteName, currentBranchName);

                return (pushResult.Success, pushResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during push");
                return (false, ex.Message);
            }
        }

        private bool IsNetworkError(string errorMsg)
        {
            return errorMsg.Contains("failed to resolve")
                || errorMsg.Contains("could not resolve")
                || errorMsg.Contains("connection refused")
                || errorMsg.Contains("network is unreachable")
                || errorMsg.Contains("timed out")
                || errorMsg.Contains("timeout")
                || errorMsg.Contains("no route to host")
                || errorMsg.Contains("connection reset")
                || errorMsg.Contains("socket");
        }

        private bool IsAuthError(string errorMsg)
        {
            return errorMsg.Contains("authentication")
                || errorMsg.Contains("401")
                || errorMsg.Contains("unauthorized")
                || errorMsg.Contains("invalid credentials")
                || errorMsg.Contains("bad credentials");
        }

        private bool IsForbiddenError(string errorMsg)
        {
            return errorMsg.Contains("403") || errorMsg.Contains("forbidden");
        }

        private bool IsNotFoundError(string errorMsg)
        {
            return errorMsg.Contains("404")
                || errorMsg.Contains("not found")
                || errorMsg.Contains("repository not found")
                || errorMsg.Contains("does not exist");
        }
    }
}
