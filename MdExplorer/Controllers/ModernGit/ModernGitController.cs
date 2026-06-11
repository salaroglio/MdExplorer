using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Services.Git;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Ad.Tools.Dal.Extensions;
using Microsoft.AspNetCore.SignalR;
using MdExplorer.Hubs;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Services;
using System.IO;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Models;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Options;
using MdExplorer.Features.Utilities;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;

namespace MdExplorer.Controllers.ModernGit
{
    /// <summary>
    /// Modern Git controller using native credential management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ModernGitController : MdControllerBase<ModernGitController>
    {
        private readonly IModernGitService _gitService;
        private readonly IGitHubService _gitHubService;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly IGitRemoteUrlParser _urlParser;
        private readonly IGenericRemoteService _genericRemoteService;
        private readonly IGitAccountService _gitAccountService;
        private readonly GitCredentialHelperResolver _gitCredentialHelper;

        public ModernGitController(
            IModernGitService gitService,
            IGitHubService gitHubService,
            ILogger<ModernGitController> logger,
            IUserSettingsDB userSettingsDb,
            IHubContext<MonitorMDHub> hubContext,
            IEngineDB engineDB,
            IMdIgnoreService mdIgnoreService,
            IOptions<MdExplorerAppSettings> options,
            IGitRemoteUrlParser urlParser,
            IGenericRemoteService genericRemoteService,
            IGitAccountService gitAccountService,
            GitCredentialHelperResolver gitCredentialHelper,
            IDatabaseManager databaseManager = null,
            IFileSystemWatcherManager fileSystemWatcherManager = null)
            : base(logger, options, hubContext, userSettingsDb, engineDB, null, null, null, databaseManager, fileSystemWatcherManager)
        {
            _gitService = gitService;
            _gitHubService = gitHubService;
            _mdIgnoreService = mdIgnoreService;
            _urlParser = urlParser;
            _genericRemoteService = genericRemoteService;
            _gitAccountService = gitAccountService;
            _gitCredentialHelper = gitCredentialHelper;
        }

        /// <summary>
        /// Pulls changes from the remote repository
        /// </summary>
        /// <param name="request">Pull request parameters</param>
        /// <returns>Result of the pull operation</returns>
        [HttpPost("pull")]
        public async Task<IActionResult> Pull([FromBody] GitOperationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Pull request received for repository: {RepositoryPath}", request.RepositoryPath);

                var result = await _gitService.PullAsync(request.RepositoryPath);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        changes = result.Changes,
                        authenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during pull operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during pull operation"
                });
            }
        }

        /// <summary>
        /// Pushes local changes to the remote repository
        /// </summary>
        /// <param name="request">Push request parameters</param>
        /// <returns>Result of the push operation</returns>
        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] PushRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Push request received for repository: {RepositoryPath}", request.RepositoryPath);

                var result = await _gitService.PushAsync(request.RepositoryPath, request.RemoteName, request.BranchName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        authenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during push operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during push operation"
                });
            }
        }

        /// <summary>
        /// Commits changes to the local repository
        /// </summary>
        /// <param name="request">Commit request parameters</param>
        /// <returns>Result of the commit operation</returns>
        [HttpPost("commit")]
        public async Task<IActionResult> Commit([FromBody] CommitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Commit request received for repository: {RepositoryPath}", request.RepositoryPath);

                var author = new GitAuthor
                {
                    Name = request.AuthorName,
                    Email = request.AuthorEmail
                };

                var result = await _gitService.CommitAsync(request.RepositoryPath, request.CommitMessage, author);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        commitHash = result.CommitHash,
                        changes = result.Changes,
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during commit operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during commit operation"
                });
            }
        }

        /// <summary>
        /// Commits changes and immediately pushes to remote repository
        /// </summary>
        /// <param name="request">Commit and push request parameters</param>
        /// <returns>Result of the commit and push operation</returns>
        [HttpPost("commit-and-push")]
        public async Task<IActionResult> CommitAndPush([FromBody] CommitAndPushRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Commit and push request received for repository: {RepositoryPath}", request.RepositoryPath);

                var author = new GitAuthor
                {
                    Name = request.AuthorName,
                    Email = request.AuthorEmail
                };

                var result = await _gitService.CommitAndPushAsync(request.RepositoryPath, request.CommitMessage, author, request.RemoteName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        commitHash = result.CommitHash,
                        changes = result.Changes,
                        authenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    commitHash = result.CommitHash,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during commit and push operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during commit and push operation"
                });
            }
        }

        /// <summary>
        /// Clones a remote repository to a local directory
        /// </summary>
        /// <param name="request">Clone request parameters</param>
        /// <returns>Result of the clone operation</returns>
        [HttpPost("clone")]
        public async Task<IActionResult> Clone([FromBody] CloneRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Clone request received: {Url} to {LocalPath} (useSavedToken={UseSavedToken})",
                    request.Url, request.LocalPath, request.UseSavedToken);

                // DEBUG: Log all parameters to trace clone failure
                _logger.LogWarning("[CLONE DEBUG] Controller received: Url={Url}, UseSavedToken={UseSavedToken}, HasUsername={HasUsername}, HasPassword={HasPassword}",
                    request.Url, request.UseSavedToken,
                    !string.IsNullOrEmpty(request.Username),
                    !string.IsNullOrEmpty(request.Password));

                var result = await _gitService.CloneAsync(
                    request.Url,
                    request.LocalPath,
                    request.BranchName,
                    request.UseSavedToken,
                    request.Username,
                    request.Password);

                if (result.Success)
                {
                    // Save per-repository credentials if provided manually (not using saved token)
                    if (!request.UseSavedToken && !string.IsNullOrEmpty(request.Username))
                    {
                        try
                        {
                            await SaveCloneCredentialsAsync(request);
                            _logger.LogInformation("Saved credentials for cloned repository: {LocalPath}", request.LocalPath);
                        }
                        catch (Exception ex)
                        {
                            // Non-fatal: clone succeeded, credential saving is best-effort
                            _logger.LogWarning(ex, "Failed to save clone credentials (non-fatal)");
                        }
                    }
                    else
                    {
                        // Credentials were provided via GCM prompt (request.Username is empty)
                        // Try to detect and save credentials from GCM cache
                        try
                        {
                            _logger.LogInformation("🔐 Attempting to auto-detect and save credentials from GCM for: {LocalPath}", request.LocalPath);
                            var credentialsSaved = await _gitCredentialHelper.DetectAndSaveCredentialsForRepository(
                                request.LocalPath,
                                request.Url);

                            if (credentialsSaved)
                            {
                                _logger.LogInformation("✅ Successfully auto-detected and saved GCM credentials for: {LocalPath}", request.LocalPath);
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ Could not auto-detect credentials from GCM for: {LocalPath}", request.LocalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Non-fatal: clone succeeded, credential auto-detection is best-effort
                            _logger.LogWarning(ex, "Failed to auto-detect GCM credentials (non-fatal)");
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        authenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during clone operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during clone operation"
                });
            }
        }

        /// <summary>
        /// Saves credentials used during clone to the GitRepositoryAccount table
        /// for future authentication with this repository.
        /// </summary>
        private async Task SaveCloneCredentialsAsync(CloneRequest request)
        {
            var urlInfo = _urlParser.ParseUrl(request.Url);

            var accountType = urlInfo.Provider?.ToLower() switch
            {
                "github" => "GitHub",
                "gitlab" => "GitLab",
                "bitbucket" => "Bitbucket",
                _ => "Generic"
            };

            var repositoryPath = Path.GetFullPath(request.LocalPath);
            var accountName = $"{accountType} - {request.Username ?? "Account"}";

            // Use CreateAccountWithCredentialAsync to create account with linked credential
            var account = await _gitAccountService.CreateAccountWithCredentialAsync(
                repositoryPath,
                accountType,
                accountName,
                request.Username,
                gitHubPAT: accountType == "GitHub" ? request.Password : null,
                gitLabToken: accountType == "GitLab" ? request.Password : null,
                httpsPassword: request.Password,
                preferredAuthMethod: "username_password");

            _logger.LogInformation("Created GitRepositoryAccount for {RepoPath} with type {AccountType}",
                account.RepositoryPath, account.AccountType);
        }

        /// <summary>
        /// Gets information about the current branch
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Current branch information</returns>
        [HttpGet("current-branch")]
        public async Task<IActionResult> GetCurrentBranch([FromQuery] [Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                var branchInfo = await _gitService.GetCurrentBranchAsync(repositoryPath);

                if (branchInfo != null)
                {
                    return Ok(branchInfo);
                }

                return NotFound(new { error = "Could not get current branch information" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current branch for repository: {RepositoryPath}", repositoryPath);
                return StatusCode(500, new { error = "Internal server error getting current branch" });
            }
        }

        /// <summary>
        /// Gets a list of all branches in the repository
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <param name="includeRemote">Whether to include remote branches</param>
        /// <returns>List of branch information</returns>
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches([FromQuery] [Required] string repositoryPath, [FromQuery] bool includeRemote = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                var branches = await _gitService.GetBranchesAsync(repositoryPath, includeRemote);
                return Ok(branches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches for repository: {RepositoryPath}", repositoryPath);
                return StatusCode(500, new { error = "Internal server error getting branches" });
            }
        }

        /// <summary>
        /// Checks out a specific branch and triggers full tree refresh
        /// </summary>
        /// <param name="request">Checkout request parameters (includes connectionId for SignalR)</param>
        /// <returns>Result of the checkout operation</returns>
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckoutBranch([FromBody] CheckoutRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("🔄 Starting branch checkout to '{Branch}' with connectionId: {ConnectionId}",
                    request.BranchName, request.ConnectionId ?? "none");

                // A checkout rewrites potentially hundreds of files at once: the watcher
                // must be off for the whole operation (same contract as pull/add-submodule),
                // otherwise the event storm interleaves with the full reload on the client.
                SetFileSystemWatcherEnabled(false);
                GitOperationResult result;
                try
                {
                    result = await _gitService.CheckoutBranchAsync(request.RepositoryPath, request.BranchName);
                }
                finally
                {
                    SetFileSystemWatcherEnabled(true);
                }

                if (result.Success)
                {
                    _logger.LogInformation("✅ Branch checkout succeeded: {Branch}", result.BranchName);

                    // Notify the client: the tree rebuild happens client-side via loadAll()
                    // → GetShallowStructure → IndexingPipelineService (single, serialized path).
                    // No synchronous reindex here: it used to duplicate the pipeline's work.
                    int fileCount = 0;
                    try
                    {
                        fileCount = await NotifyTreeRefreshAsync(request.ConnectionId, result.Changes);
                    }
                    catch (Exception refreshEx)
                    {
                        _logger.LogError(refreshEx, "❌ Tree refresh notification failed after checkout (checkout itself succeeded)");
                        // Don't fail the entire operation if the notification fails
                    }

                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        durationMs = result.Duration.TotalMilliseconds,
                        branchName = result.BranchName,
                        fileCount = fileCount
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during checkout operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during checkout operation"
                });
            }
        }

        /// <summary>
        /// Adds a git submodule via native git CLI and triggers full tree refresh.
        /// Authentication relies entirely on git's own credential chain (Git Credential Manager).
        /// </summary>
        /// <param name="request">Submodule add request (includes connectionId for SignalR)</param>
        /// <returns>Result of the submodule add operation</returns>
        [HttpPost("add-submodule")]
        public async Task<IActionResult> AddSubmodule([FromBody] AddSubmoduleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("🔗 Adding submodule '{Url}' at '{Path}' (branch: {Branch}, connectionId: {ConnectionId})",
                request.Url, request.DestinationPath, request.BranchName ?? "default", request.ConnectionId ?? "none");

            // Suspend the watcher: a whole folder tree appears at once during submodule clone
            SetFileSystemWatcherEnabled(false);
            try
            {
                var result = await _gitService.AddSubmoduleAsync(
                    request.RepositoryPath, request.Url, request.DestinationPath, request.BranchName);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.ErrorMessage,
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                // Full tree refresh so the new submodule content shows up without manual reload
                int fileCount = 0;
                try
                {
                    fileCount = await NotifyTreeRefreshAsync(request.ConnectionId, result.Changes);
                }
                catch (Exception refreshEx)
                {
                    _logger.LogError(refreshEx, "❌ Tree refresh notification failed after submodule add (the add itself succeeded)");
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    fileCount = fileCount,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during submodule add");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during submodule add"
                });
            }
            finally
            {
                SetFileSystemWatcherEnabled(true);
            }
        }

        /// <summary>
        /// Gets the repository status
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Repository status information</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus([FromQuery] [Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                var status = await _gitService.GetStatusAsync(repositoryPath);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for repository: {RepositoryPath}", repositoryPath);
                return StatusCode(500, new { error = "Internal server error getting repository status" });
            }
        }

        /// <summary>
        /// Fetches changes from the remote repository without merging
        /// </summary>
        /// <param name="request">Fetch request parameters</param>
        /// <returns>Result of the fetch operation</returns>
        [HttpPost("fetch")]
        public async Task<IActionResult> Fetch([FromBody] FetchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _gitService.FetchAsync(request.RepositoryPath, request.RemoteName);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        authenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during fetch operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during fetch operation"
                });
            }
        }

        /// <summary>
        /// Gets the commit history for a repository
        /// </summary>
        /// <param name="request">History request parameters</param>
        /// <returns>List of commits with author and message</returns>
        [HttpPost("history")]
        public async Task<IActionResult> GetCommitHistory([FromBody] HistoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("History request received for repository: {RepositoryPath}", request.RepositoryPath);

                var commits = await _gitService.GetCommitHistoryAsync(request.RepositoryPath, request.MaxCommits ?? 50);

                return Ok(new
                {
                    success = true,
                    commits = commits,
                    count = commits.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during getting commit history");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error getting commit history"
                });
            }
        }

        /// <summary>
        /// Gets the remote URL for a repository (typically origin)
        /// Used by Share Project feature to generate shareable URLs
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Remote URL or null if not configured</returns>
        [HttpGet("remote-url")]
        public async Task<IActionResult> GetRemoteUrl([FromQuery] [Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { hasRemote = false, error = "Repository path is required" });
                }

                if (!Directory.Exists(repositoryPath))
                {
                    return Ok(new { hasRemote = false, error = "Directory does not exist" });
                }

                var gitPath = Path.Combine(repositoryPath, ".git");
                if (!Directory.Exists(gitPath))
                {
                    return Ok(new { hasRemote = false, error = "Not a Git repository" });
                }

                using (var repo = new LibGit2Sharp.Repository(repositoryPath))
                {
                    var origin = repo.Network.Remotes["origin"];
                    if (origin != null)
                    {
                        return Ok(new { hasRemote = true, remoteUrl = origin.Url });
                    }
                    else
                    {
                        return Ok(new { hasRemote = false });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remote URL for repository: {RepositoryPath}", repositoryPath);
                return Ok(new { hasRemote = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Validates if a remote Git URL is reachable (performs ls-remote check)
        /// Used before cloning to verify URL is accessible
        /// </summary>
        /// <param name="url">Git repository URL to validate</param>
        /// <returns>Whether the URL is reachable</returns>
        [HttpGet("validate-remote-url")]
        public async Task<IActionResult> ValidateRemoteUrl([FromQuery] [Required] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new { isReachable = false, error = "URL is required" });
            }

            var result = await _gitService.ValidateRemoteUrlAsync(url);

            return Ok(new
            {
                isReachable = result.IsReachable,
                referenceCount = result.ReferenceCount,
                error = result.Error,
                isAuthenticationError = result.IsAuthenticationError
            });
        }

        /// <summary>
        /// Checks if the repository has a remote configured
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Remote status information</returns>
        [HttpGet("remote-status")]
        public async Task<IActionResult> CheckRemoteStatus([FromQuery] [Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                var status = await _gitService.CheckRemoteStatusAsync(repositoryPath);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking remote status for repository: {RepositoryPath}", repositoryPath);
                return StatusCode(500, new { error = "Internal server error checking remote status" });
            }
        }

        /// <summary>
        /// Sets up a GitHub remote for the repository
        /// </summary>
        /// <param name="request">Remote setup parameters</param>
        /// <returns>Result of the setup operation</returns>
        [HttpPost("setup-remote")]
        public async Task<IActionResult> SetupRemote([FromBody] SetupRemoteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Setting up remote for repository: {RepositoryPath}", request.RepositoryPath);

                // Save organization if requested
                if (request.SaveOrganization)
                {
                    await SaveGitHubOrganization(request.Organization);
                }

                // First, try to create the repository on GitHub if it doesn't exist
                var gitHubResult = await _gitHubService.CreateRepositoryAsync(
                    request.Organization,
                    request.RepositoryName,
                    request.RepositoryDescription,
                    request.IsPrivate ?? true);

                if (!gitHubResult.Success)
                {
                    _logger.LogWarning("Failed to create GitHub repository: {Error}", gitHubResult.ErrorMessage);
                    // If it's not an "already exists" error, return the error
                    if (!gitHubResult.AlreadyExists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            error = gitHubResult.ErrorMessage,
                            needsToken = gitHubResult.ErrorMessage.Contains("token")
                        });
                    }
                }
                else
                {
                    _logger.LogInformation("GitHub repository {Status}: {Url}",
                        gitHubResult.AlreadyExists ? "already exists" : "created",
                        gitHubResult.RepositoryUrl);
                }

                // Add the remote
                var result = await _gitService.AddRemoteAsync(
                    request.RepositoryPath,
                    request.Organization,
                    request.RepositoryName,
                    request.PushAfterAdd);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        durationMs = result.Duration.TotalMilliseconds
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage,
                    durationMs = result.Duration.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during remote setup");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during remote setup"
                });
            }
        }

        #region Generic Remote Setup Endpoints

        /// <summary>
        /// Parses a remote URL and detects the provider
        /// </summary>
        /// <param name="request">URL to parse</param>
        /// <returns>Parsed URL information including provider detection</returns>
        [HttpPost("parse-remote-url")]
        public IActionResult ParseRemoteUrl([FromBody] ParseRemoteUrlRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Parsing remote URL: {Url}", request.Url);

                var urlInfo = _urlParser.ParseUrl(request.Url);
                var tokenUrl = _urlParser.GetTokenCreationUrl(urlInfo.Provider, urlInfo.Host);

                return Ok(new ParseRemoteUrlResponse
                {
                    IsValid = urlInfo.IsValid,
                    Provider = urlInfo.Provider,
                    Host = urlInfo.Host,
                    Owner = urlInfo.Owner,
                    RepoName = urlInfo.RepoName,
                    Protocol = urlInfo.Protocol,
                    SupportsAutoCreate = urlInfo.SupportsAutoCreate,
                    TokenCreationUrl = tokenUrl,
                    Error = urlInfo.Error
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing remote URL: {Url}", request.Url);
                return StatusCode(500, new ParseRemoteUrlResponse
                {
                    IsValid = false,
                    Error = "Internal server error parsing URL"
                });
            }
        }

        /// <summary>
        /// Validates remote URL with provided credentials
        /// </summary>
        /// <param name="request">Validation request with URL and credentials</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate-remote-auth")]
        public async Task<IActionResult> ValidateRemoteAuth([FromBody] ValidateRemoteAuthRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Validating remote auth for: {Url}", request.RemoteUrl);

                var result = await _genericRemoteService.ValidateRemoteWithCredentialsAsync(
                    new Services.Git.Interfaces.ValidateRemoteRequest
                    {
                        RemoteUrl = request.RemoteUrl,
                        Username = request.Username,
                        Password = request.Password,
                        AuthMethod = request.AuthMethod
                    });

                return Ok(new ValidateRemoteAuthResponse
                {
                    IsReachable = result.IsReachable,
                    RequiresAuth = result.RequiresAuth,
                    CredentialsValid = result.CredentialsValid,
                    RepositoryExists = result.RepositoryExists,
                    Provider = result.Provider,
                    Error = result.Error
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating remote auth: {Url}", request.RemoteUrl);
                return StatusCode(500, new ValidateRemoteAuthResponse
                {
                    IsReachable = false,
                    Error = "Internal server error during validation"
                });
            }
        }

        /// <summary>
        /// Sets up a generic remote (supports any Git provider)
        /// </summary>
        /// <param name="request">Generic remote setup parameters</param>
        /// <returns>Result of the setup operation</returns>
        [HttpPost("setup-remote-generic")]
        public async Task<IActionResult> SetupRemoteGeneric([FromBody] GenericSetupRemoteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Setting up generic remote: {RemoteUrl} for repository: {RepositoryPath}",
                    request.RemoteUrl, request.RepositoryPath);

                var result = await _genericRemoteService.SetupRemoteGenericAsync(
                    new Services.Git.Interfaces.SetupRemoteGenericRequest
                    {
                        RepositoryPath = request.RepositoryPath,
                        RemoteUrl = request.RemoteUrl,
                        RemoteName = request.RemoteName,
                        AuthMethod = request.AuthMethod,
                        Username = request.Username,
                        Password = request.Password,
                        Token = request.Token,
                        SaveCredentials = request.SaveCredentials,
                        PushAfterAdd = request.PushAfterAdd,
                        CreateRemoteRepo = request.CreateRemoteRepo,
                        RepoDescription = request.RepoDescription,
                        IsPrivate = request.IsPrivate,
                        UseSavedToken = request.UseSavedToken,
                        CopyFromCredentialId = request.CopyFromCredentialId
                    });

                if (result.Success)
                {
                    return Ok(new GenericSetupRemoteResponse
                    {
                        Success = true,
                        Message = result.Message,
                        RepositoryCreated = result.RepositoryCreated,
                        RemoteUrl = result.RemoteUrl,
                        DurationMs = result.DurationMs
                    });
                }

                return BadRequest(new GenericSetupRemoteResponse
                {
                    Success = false,
                    Error = result.Error,
                    DurationMs = result.DurationMs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up generic remote: {RemoteUrl}", request.RemoteUrl);
                return StatusCode(500, new GenericSetupRemoteResponse
                {
                    Success = false,
                    Error = "Internal server error during remote setup"
                });
            }
        }

        #endregion

        /// <summary>
        /// Remove a remote from the repository
        /// </summary>
        [HttpDelete("remove-remote")]
        public async Task<IActionResult> RemoveRemote([FromQuery] string repositoryPath, [FromQuery] string remoteName = "origin")
        {
            try
            {
                if (string.IsNullOrEmpty(repositoryPath))
                {
                    return BadRequest(new { success = false, error = "Repository path is required" });
                }

                var result = await _gitService.RemoveRemoteAsync(repositoryPath, remoteName);

                if (result.Success)
                {
                    _logger.LogInformation("Remote removed successfully for repository: {RepositoryPath}", repositoryPath);
                    return Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to remove remote: {Error}", result.ErrorMessage);
                    return Ok(new
                    {
                        success = false,
                        error = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing remote");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error while removing remote"
                });
            }
        }

        /// <summary>
        /// Gets the saved GitHub organization
        /// </summary>
        /// <returns>The saved organization name or empty string</returns>
        [HttpGet("github-organization")]
        public IActionResult GetGitHubOrganization()
        {
            try
            {
                var dal = _userSettingsDB.GetDal<Setting>();
                var setting = dal.GetList().Where(s => s.Name == "GitHubOrganization").FirstOrDefault();

                return Ok(new
                {
                    organization = setting?.ValueString ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GitHub organization");
                return Ok(new { organization = string.Empty });
            }
        }

        /// <summary>
        /// Saves the GitHub organization for future use
        /// </summary>
        /// <param name="request">Organization to save</param>
        /// <returns>Success status</returns>
        [HttpPost("github-organization")]
        public async Task<IActionResult> SaveGitHubOrganizationEndpoint([FromBody] OrganizationRequest request)
        {
            try
            {
                await SaveGitHubOrganization(request?.Organization);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving GitHub organization");
                return StatusCode(500, new { success = false, error = "Failed to save organization" });
            }
        }

        /// <summary>
        /// Sets the GitHub personal access token
        /// </summary>
        [HttpPost("github-token")]
        public async Task<IActionResult> SetGitHubToken([FromBody] TokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Token))
                {
                    return BadRequest(new { success = false, error = "Token is required" });
                }

                await _gitHubService.SetTokenAsync(request.Token);

                // Test the token to make sure it's valid
                var isValid = await _gitHubService.TestTokenAsync();

                return Ok(new
                {
                    success = true,
                    tokenValid = isValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting GitHub token");
                return StatusCode(500, new { success = false, error = "Failed to set token" });
            }
        }

        /// <summary>
        /// Gets the masked GitHub token (for display)
        /// </summary>
        [HttpGet("github-token")]
        public async Task<IActionResult> GetGitHubToken()
        {
            try
            {
                var maskedToken = await _gitHubService.GetMaskedTokenAsync();
                var username = await _gitHubService.GetTokenUsernameAsync();
                var isValid = !string.IsNullOrEmpty(maskedToken) ? await _gitHubService.TestTokenAsync() : false;

                return Ok(new
                {
                    hasToken = !string.IsNullOrEmpty(maskedToken),
                    maskedToken = maskedToken,
                    tokenValid = isValid,
                    username = username
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GitHub token");
                return Ok(new { hasToken = false, maskedToken = "", tokenValid = false, username = (string)null });
            }
        }

        /// <summary>
        /// Tests the GitHub token validity
        /// </summary>
        [HttpPost("test-github-token")]
        public async Task<IActionResult> TestGitHubToken()
        {
            try
            {
                var isValid = await _gitHubService.TestTokenAsync();

                return Ok(new
                {
                    success = true,
                    tokenValid = isValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing GitHub token");
                return Ok(new { success = false, tokenValid = false });
            }
        }

        /// <summary>
        /// Deletes the stored GitHub personal access token
        /// </summary>
        [HttpDelete("github-token")]
        public async Task<IActionResult> DeleteGitHubToken()
        {
            try
            {
                await _gitHubService.ClearTokenAsync();
                _logger.LogInformation("GitHub token deleted successfully");
                return Ok(new { success = true, message = "Token deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GitHub token");
                return StatusCode(500, new { success = false, error = "Failed to delete token" });
            }
        }

        private async Task SaveGitHubOrganization(string organization)
        {
            await Task.Run(() =>
            {
                _userSettingsDB.BeginTransaction();
                try
                {
                    var dal = _userSettingsDB.GetDal<Setting>();
                    var setting = dal.GetList().Where(s => s.Name == "GitHubOrganization").FirstOrDefault();

                    if (setting != null)
                    {
                        setting.ValueString = organization;
                    }
                    else
                    {
                        setting = new Setting
                        {
                            Name = "GitHubOrganization",
                            ValueString = organization
                        };
                    }

                    dal.Save(setting);
                    _userSettingsDB.Commit();
                    _logger.LogInformation("GitHub organization saved: {Organization}", organization);
                }
                catch (Exception ex)
                {
                    _userSettingsDB.Rollback();
                    throw;
                }
            });
        }

        /// <summary>
        /// Initializes a new Git repository in the specified directory
        /// </summary>
        /// <param name="request">Initialization request with repository path, branch name, and gitignore template</param>
        /// <returns>Initialization response with success status</returns>
        [HttpPost("init")]
        public async Task<IActionResult> InitRepository([FromBody] InitRepositoryRequest request)
        {
            _logger.LogInformation("Initializing Git repository at: {RepositoryPath}", request.RepositoryPath);

            try
            {
                var response = await _gitService.InitRepositoryAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation("✅ Git repository initialized successfully: {RepositoryPath}", request.RepositoryPath);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("⚠️ Git repository initialization failed: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error initializing Git repository: {RepositoryPath}", request.RepositoryPath);
                return StatusCode(500, new InitRepositoryResponse
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    IsGitRepository = false,
                    RepositoryPath = request.RepositoryPath
                });
            }
        }

        #region MD-Tree Refresh for Git Operations

        /// <summary>
        /// Notifies the client that the tree must be reloaded after a Git operation
        /// (branch switch, submodule add). The actual rebuild happens client-side:
        /// loadAll() → GetShallowStructure → IndexingPipelineService, which already
        /// performs cleanup + reindex. The synchronous Cleanup/Reindex that used to
        /// live here duplicated that work on the request thread.
        /// Sent per-connection, never broadcast.
        /// </summary>
        /// <returns>The number of files in the operation's diff (0 when unknown).</returns>
        private async Task<int> NotifyTreeRefreshAsync(string connectionId, IEnumerable<string> changedFiles)
        {
            var changedRelativePaths = (changedFiles ?? Enumerable.Empty<string>())
                .Select(p => p.Replace('\\', '/'))
                .ToList();

            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogError("[NotifyTreeRefresh] No connectionId — 'gitBranchSwitched' NOT sent, the client tree will be stale");
                return changedRelativePaths.Count;
            }

            await _hubContext.Clients.Client(connectionId).SendAsync("gitBranchSwitched", new
            {
                fileCount = changedRelativePaths.Count,
                changedFiles = changedRelativePaths,
                message = "Tree refresh required after git operation"
            });

            _logger.LogInformation("[NotifyTreeRefresh] SignalR event 'gitBranchSwitched' sent to client {ConnectionId} ({FileCount} files in diff)",
                connectionId, changedRelativePaths.Count);
            return changedRelativePaths.Count;
        }

        #endregion
    }
}