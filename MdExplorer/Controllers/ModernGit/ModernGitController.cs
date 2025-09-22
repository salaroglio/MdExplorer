using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.ModernGit
{
    /// <summary>
    /// Modern Git controller using native credential management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ModernGitController : ControllerBase
    {
        private readonly IModernGitService _gitService;
        private readonly ILogger<ModernGitController> _logger;

        public ModernGitController(IModernGitService gitService, ILogger<ModernGitController> logger)
        {
            _gitService = gitService;
            _logger = logger;
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

                _logger.LogInformation("Clone request received: {Url} to {LocalPath}", request.Url, request.LocalPath);

                var result = await _gitService.CloneAsync(request.Url, request.LocalPath, request.BranchName);

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
        /// Checks out a specific branch
        /// </summary>
        /// <param name="request">Checkout request parameters</param>
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

                var result = await _gitService.CheckoutBranchAsync(request.RepositoryPath, request.BranchName);

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
                _logger.LogError(ex, "Unexpected error during checkout operation");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Internal server error during checkout operation"
                });
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
    }
}