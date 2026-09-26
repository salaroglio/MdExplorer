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
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly IGitRemoteUrlParser _urlParser;
        private readonly IGenericRemoteService _genericRemoteService;

        public ModernGitController(
            IModernGitService gitService,
            ILogger<ModernGitController> logger,
            IUserSettingsDB userSettingsDb,
            IHubContext<MonitorMDHub> hubContext,
            IEngineDB engineDB,
            IMdIgnoreService mdIgnoreService,
            IOptions<MdExplorerAppSettings> options,
            IGitRemoteUrlParser urlParser,
            IGenericRemoteService genericRemoteService,
            IDatabaseManager databaseManager = null,
            IFileSystemWatcherManager fileSystemWatcherManager = null)
            : base(logger, options, hubContext, userSettingsDb, engineDB, null, null, null, databaseManager, fileSystemWatcherManager)
        {
            _gitService = gitService;
            _mdIgnoreService = mdIgnoreService;
            _urlParser = urlParser;
            _genericRemoteService = genericRemoteService;
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

                var result = await _gitService.CloneAsync(
                    request.Url,
                    request.LocalPath,
                    request.BranchName,
                    request.UseSavedToken,
                    request.Username,
                    request.Password);

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
                _logger.LogError(ex, "Unexpected error during clone operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during clone operation"
                });
            }
        }

        /// <summary>
        /// Gets information about the current branch
        /// </summary>
        /// <param name="repositoryPath">Path to the repository</param>
        /// <returns>Current branch information</returns>
        /// <summary>
        /// Suggests a default, writable destination path for a clone (e.g. the demo
        /// project started by Mark). Cross-platform: resolves the user's Documents
        /// folder via the OS (falls back to the home directory, then to the temp dir).
        /// </summary>
        /// <param name="repoName">Repository name used as folder prefix</param>
        /// <returns>{ path: "&lt;suggested absolute path&gt;" }</returns>
        [HttpGet("default-clone-path")]
        public IActionResult GetDefaultClonePath([FromQuery] string repoName = "repository")
        {
            try
            {
                // Sanitize: the repo name becomes a folder name
                foreach (var invalid in Path.GetInvalidFileNameChars())
                {
                    repoName = repoName.Replace(invalid, '-');
                }

                var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrWhiteSpace(baseDir) || !Directory.Exists(baseDir))
                {
                    baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                }
                if (string.IsNullOrWhiteSpace(baseDir) || !Directory.Exists(baseDir))
                {
                    baseDir = Path.GetTempPath();
                }

                var stamp = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
                var suggested = Path.Combine(baseDir, $"{repoName}-{stamp}");
                return Ok(new { path = suggested });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building default clone path for {RepoName}", repoName);
                return StatusCode(500, new { error = "Internal server error building default clone path" });
            }
        }

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

                return Ok(new ParseRemoteUrlResponse
                {
                    IsValid = urlInfo.IsValid,
                    Provider = urlInfo.Provider,
                    Host = urlInfo.Host,
                    Owner = urlInfo.Owner,
                    RepoName = urlInfo.RepoName,
                    Protocol = urlInfo.Protocol,
                    SupportsAutoCreate = urlInfo.SupportsAutoCreate,
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
                _logger.LogInformation("Setting up remote: {RemoteUrl} for repository: {RepositoryPath}",
                    request.RemoteUrl, request.RepositoryPath);
                var result = await _genericRemoteService.SetupRemoteGenericAsync(
                    new Services.Git.Interfaces.SetupRemoteGenericRequest
                    {
                        RepositoryPath = request.RepositoryPath,
                        RemoteUrl = request.RemoteUrl,
                        RemoteName = request.RemoteName,
                        AccountUsername = request.AccountUsername,
                        PushAfterAdd = request.PushAfterAdd,
                    });
                var response = new GenericSetupRemoteResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Error = result.Error,
                    RemoteUrl = result.RemoteUrl,
                    PushAttempted = result.PushAttempted,
                    PushSucceeded = result.PushSucceeded,
                    DurationMs = result.DurationMs
                };
                return result.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up remote: {RemoteUrl}", request.RemoteUrl);
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
        /// Initializes a new Git repository in the specified directory
        /// </summary>
        /// <param name="request">Initialization request with repository path, branch name, and gitignore template</param>
        /// <returns>Initialization response with success status</returns>
        /// <summary>
        /// Il trasloco delle credenziali dal DB di MdExplorer al credential helper di git (vedi
        /// <see cref="Services.Git.GitCredentialMoveService"/>). POST lo esegue, GET riporta l'ultimo esito:
        /// la UI lo mostra una volta, con l'elenco di ciò che non si è potuto spostare e il perché.
        /// </summary>
        [HttpPost("credential-move")]
        public async Task<IActionResult> RunCredentialMove([FromServices] Services.Git.IGitCredentialMoveService mover)
        {
            var report = await mover.RunAsync(HttpContext.RequestAborted);
            return Ok(report);
        }

        [HttpGet("credential-move")]
        public IActionResult LastCredentialMove([FromServices] Services.Git.GitCredentialMoveReportHolder holder)
        {
            return Ok(holder.Last ?? new Services.Git.GitCredentialMoveReport());
        }

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