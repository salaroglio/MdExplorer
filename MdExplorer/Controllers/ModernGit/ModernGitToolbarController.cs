using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.Controllers.ModernGit;
using MdExplorer.Abstractions.Models;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MdExplorer.Controllers.ModernGit
{
    /// <summary>
    /// Modern Git controller for toolbar operations using native authentication
    /// </summary>
    [ApiController]
    [Route("api/ModernGitToolbar")]
    public class ModernGitToolbarController : ControllerBase
    {
        private readonly IModernGitService _modernGitService;
        private readonly ILogger<ModernGitToolbarController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public ModernGitToolbarController(
            IModernGitService modernGitService, 
            ILogger<ModernGitToolbarController> logger,
            FileSystemWatcher fileSystemWatcher)
        {
            _modernGitService = modernGitService;
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            
            _logger.LogInformation("ModernGitToolbarController instantiated successfully");
        }

        /// <summary>
        /// Pulls changes from remote repository
        /// </summary>
        /// <param name="request">Pull request with repository path</param>
        /// <returns>Result of pull operation with changed files</returns>
        [HttpPost("pull")]
        public async Task<IActionResult> Pull([FromBody] ToolbarGitRequest request)
        {
            try
            {

                _logger.LogInformation("Pull request received for path: {Path}", request.ProjectPath);

                // Disable file watcher during Git operations to prevent conflicts
                _fileSystemWatcher.EnableRaisingEvents = false;
                
                try
                {
                    var result = await _modernGitService.PullAsync(request.ProjectPath);

                    var response = new PullResponse
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ErrorMessage = result.ErrorMessage,
                        AuthenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        DurationMs = result.Duration.TotalMilliseconds,
                        ThereAreConflicts = !result.Success && result.ErrorMessage?.Contains("conflict") == true,
                        ChangedFiles = ConvertToChangedFileInfo(result.Changes?.ToList(), request.ProjectPath),
                        CommitsPulled = result.Changes?.Count() ?? 0,
                        FilesChanged = result.Changes?.Count() ?? 0
                    };

                    if (result.Success)
                    {
                        return Ok(response);
                    }

                    return BadRequest(response);
                }
                finally
                {
                    // Re-enable file watcher
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during pull operation");
                return StatusCode(500, new PullResponse
                {
                    Success = false,
                    ErrorMessage = "Internal server error during pull operation"
                });
            }
        }

        /// <summary>
        /// Commits all changes and pushes to remote repository
        /// </summary>
        /// <param name="request">Commit and push request</param>
        /// <returns>Result of commit and push operation</returns>
        [HttpPost("commit-and-push")]
        public async Task<IActionResult> CommitAndPush([FromBody] ToolbarGitRequest request)
        {
            _logger.LogInformation("🚀 CommitAndPush method entered with request: {@Request}", request);
            
            try
            {
                if (string.IsNullOrEmpty(request?.ProjectPath))
                {
                    _logger.LogError("ProjectPath is null or empty");
                    return BadRequest("ProjectPath is required");
                }

                if (!Directory.Exists(request.ProjectPath))
                {
                    _logger.LogError("Directory does not exist: {ProjectPath}", request.ProjectPath);
                    return BadRequest($"Directory does not exist: {request.ProjectPath}");
                }

                _logger.LogInformation("Commit and push request received for path: {Path}", request.ProjectPath);

                // Disable file watcher during Git operations
                _fileSystemWatcher.EnableRaisingEvents = false;
                
                try
                {
                    // Use current git user for author info
                    var author = await GetGitAuthorAsync(request.ProjectPath);
                    
                    // Generate commit message with user name
                    var commitMessage = string.IsNullOrEmpty(request.CommitMessage) 
                        ? $"Committato da {author.Name} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                        : $"Committato da {author.Name}: {request.CommitMessage}";

                    var result = await _modernGitService.CommitAndPushAsync(request.ProjectPath, commitMessage, author);

                    var response = new CommitResponse
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ErrorMessage = result.ErrorMessage,
                        AuthenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        DurationMs = result.Duration.TotalMilliseconds,
                        ThereAreConflicts = !result.Success && result.ErrorMessage?.Contains("conflict") == true,
                        CommitHash = result.CommitHash,
                        FilesChanged = result.Changes?.Count() ?? 0
                    };

                    if (result.Success)
                    {
                        return Ok(response);
                    }

                    return BadRequest(response);
                }
                finally
                {
                    // Re-enable file watcher
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during commit and push operation");
                return StatusCode(500, new CommitResponse
                {
                    Success = false,
                    ErrorMessage = "Internal server error during commit and push operation"
                });
            }
        }

        /// <summary>
        /// Commits all changes locally (without pushing)
        /// </summary>
        /// <param name="request">Commit request</param>
        /// <returns>Result of commit operation</returns>
        [HttpPost("commit")]
        public async Task<IActionResult> Commit([FromBody] ToolbarGitRequest request)
        {
            try
            {

                _logger.LogInformation("Commit request received for path: {Path}", request.ProjectPath);

                // Disable file watcher during Git operations
                _fileSystemWatcher.EnableRaisingEvents = false;
                
                try
                {
                    var author = await GetGitAuthorAsync(request.ProjectPath);
                    
                    // Generate commit message with user name
                    var commitMessage = string.IsNullOrEmpty(request.CommitMessage) 
                        ? $"Committato da {author.Name} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                        : $"Committato da {author.Name}: {request.CommitMessage}";

                    var result = await _modernGitService.CommitAsync(request.ProjectPath, commitMessage, author);

                    var response = new CommitResponse
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ErrorMessage = result.ErrorMessage,
                        DurationMs = result.Duration.TotalMilliseconds,
                        CommitHash = result.CommitHash,
                        FilesChanged = result.Changes?.Count() ?? 0
                    };

                    if (result.Success)
                    {
                        return Ok(response);
                    }

                    return BadRequest(response);
                }
                finally
                {
                    // Re-enable file watcher
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during commit operation");
                return StatusCode(500, new CommitResponse
                {
                    Success = false,
                    ErrorMessage = "Internal server error during commit operation"
                });
            }
        }

        /// <summary>
        /// Pushes committed changes to remote repository
        /// </summary>
        /// <param name="request">Push request</param>
        /// <returns>Result of push operation</returns>
        [HttpPost("push")]
        [HttpPost("push-v2")] // Add alternative route for testing
        public async Task<IActionResult> Push([FromBody] PushOnlyRequest request)
        {
            _logger.LogInformation("🚀 TOOLBAR CONTROLLER Push method entered - request is null: {IsNull}", request == null);
            
            try
            {
                if (request == null)
                {
                    _logger.LogError("Push request is null");
                    return BadRequest("Request body is required");
                }

                if (string.IsNullOrEmpty(request.ProjectPath))
                {
                    _logger.LogError("ProjectPath is null or empty in push request");
                    return BadRequest("ProjectPath is required");
                }

                _logger.LogInformation("Push request received for path: {Path}", request.ProjectPath);

                // Disable file watcher during Git operations
                _fileSystemWatcher.EnableRaisingEvents = false;
                
                try
                {
                    var result = await _modernGitService.PushAsync(request.ProjectPath);

                    var response = new GitOperationResponse
                    {
                        Success = result.Success,
                        Message = result.Message,
                        ErrorMessage = result.ErrorMessage,
                        AuthenticationMethod = result.AuthenticationMethodUsed.ToString(),
                        DurationMs = result.Duration.TotalMilliseconds,
                        ThereAreConflicts = !result.Success && result.ErrorMessage?.Contains("conflict") == true
                    };

                    if (result.Success)
                    {
                        return Ok(response);
                    }

                    return BadRequest(response);
                }
                finally
                {
                    // Re-enable file watcher
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during push operation");
                return StatusCode(500, new GitOperationResponse
                {
                    Success = false,
                    ErrorMessage = "Internal server error during push operation"
                });
            }
        }

        /// <summary>
        /// Test endpoint to verify the controller is working
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("ModernGitToolbar test endpoint called");
            return Ok(new { message = "ModernGitToolbar controller is working", timestamp = DateTime.Now });
        }

        /// <summary>
        /// Test commit endpoint with simple parameters
        /// </summary>
        [HttpPost("test-commit")]
        public IActionResult TestCommit([FromQuery] string projectPath, [FromQuery] string commitMessage = null)
        {
            _logger.LogInformation("TestCommit called with projectPath: {ProjectPath}, commitMessage: {CommitMessage}", projectPath, commitMessage);
            
            if (string.IsNullOrEmpty(projectPath))
            {
                return BadRequest("ProjectPath is required");
            }
            
            return Ok(new { 
                message = "Test commit endpoint reached successfully", 
                projectPath = projectPath,
                commitMessage = commitMessage,
                timestamp = DateTime.Now 
            });
        }

        /// <summary>
        /// Simple POST test without parameters
        /// </summary>
        [HttpPost("test-simple")]
        public IActionResult TestSimple()
        {
            _logger.LogInformation("🎯 TestSimple method called successfully!");
            return Ok(new { message = "Simple POST test successful", timestamp = DateTime.Now });
        }

        /// <summary>
        /// Test push request deserialization
        /// </summary>
        [HttpPost("test-push-request")]
        public IActionResult TestPushRequest([FromBody] ToolbarGitRequest request)
        {
            _logger.LogInformation("TestPushRequest method entered - request is null: {IsNull}", request == null);
            
            if (request == null)
            {
                _logger.LogError("TestPushRequest - request is null");
                return BadRequest("Request is null");
            }
            
            _logger.LogInformation("TestPushRequest - ProjectPath: {ProjectPath}, CommitMessage: {CommitMessage}", 
                request.ProjectPath, request.CommitMessage);
            
            return Ok(new { 
                message = "Request deserialized successfully", 
                projectPath = request.ProjectPath,
                commitMessage = request.CommitMessage,
                timestamp = DateTime.Now 
            });
        }

        /// <summary>
        /// Test push route - exact same signature as push
        /// </summary>
        [HttpPost("test-push")]
        public IActionResult TestPush([FromBody] ToolbarGitRequest request)
        {
            _logger.LogInformation("🔥 TestPush method entered - request is null: {IsNull}", request == null);
            return Ok(new { message = "TestPush method reached", timestamp = DateTime.Now });
        }

        /// <summary>
        /// Gets current branch status
        /// </summary>
        /// <param name="projectPath">Path to repository</param>
        /// <returns>Branch status information</returns>
        [HttpGet("branch-status")]
        public async Task<IActionResult> GetBranchStatus([FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest("Project path is required");
                }

                var branchInfo = await _modernGitService.GetCurrentBranchAsync(projectPath);
                var status = await _modernGitService.GetStatusAsync(projectPath);

                if (branchInfo == null)
                {
                    return NotFound("Branch information not available");
                }

                var response = new BranchStatusResponse
                {
                    Name = branchInfo.Name,
                    FullPath = projectPath,
                    SomethingIsChangedInTheBranch = status?.IsDirty ?? false,
                    HowManyFilesAreChanged = (status?.Modified?.Count() ?? 0) + (status?.Added?.Count() ?? 0) + (status?.Removed?.Count() ?? 0) + (status?.Untracked?.Count() ?? 0),
                    HowManyCommitAreToPush = status?.CommitsAhead ?? 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch status for path: {Path}", projectPath);
                return StatusCode(500, new { error = "Internal server error getting branch status" });
            }
        }

        /// <summary>
        /// Gets data about commits to pull and push
        /// </summary>
        /// <param name="projectPath">Path to repository</param>
        /// <returns>Pull/push data information</returns>
        [HttpGet("get-data-to-pull")]
        public async Task<IActionResult> GetDataToPull([FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest("Project path is required");
                }

                _logger.LogInformation("GetDataToPull request for path: {Path}", projectPath);

                // Get pull/push data from the modern Git service
                var pullPushData = await _modernGitService.GetPullPushDataAsync(projectPath);

                if (pullPushData == null)
                {
                    return Ok(new DataToPullResponse
                    {
                        SomethingIsToPull = false,
                        HowManyFilesAreToPull = 0,
                        HowManyCommitAreToPush = 0,
                        ConnectionIsActive = false,
                        WhatFilesWillBeChanged = new List<FileNameAndAuthor>()
                    });
                }

                var response = new DataToPullResponse
                {
                    SomethingIsToPull = pullPushData.CommitsBehind > 0,
                    HowManyFilesAreToPull = pullPushData.FilesToPull?.Count() ?? 0,
                    HowManyCommitAreToPush = pullPushData.CommitsAhead,
                    ConnectionIsActive = pullPushData.IsRemoteAvailable,
                    WhatFilesWillBeChanged = pullPushData.FilesToPull?.Select(change => new FileNameAndAuthor
                    {
                        FileName = change.FilePath,
                        Author = change.Author
                    }).ToList() ?? new List<FileNameAndAuthor>()
                };

                _logger.LogInformation("GetDataToPull response: CommitsBehind={Behind}, CommitsAhead={Ahead}, Files={Files}",
                    pullPushData.CommitsBehind, pullPushData.CommitsAhead, response.WhatFilesWillBeChanged.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pull/push data for path: {Path}", projectPath);
                
                // Return a response indicating connection failure
                return Ok(new DataToPullResponse
                {
                    SomethingIsToPull = false,
                    HowManyFilesAreToPull = 0,
                    HowManyCommitAreToPush = 0,
                    ConnectionIsActive = false,
                    WhatFilesWillBeChanged = new List<FileNameAndAuthor>()
                });
            }
        }

        /// <summary>
        /// Gets Git author information from repository configuration
        /// </summary>
        private async Task<GitAuthor> GetGitAuthorAsync(string repositoryPath)
        {
            try
            {
                // Read from Git configuration
                using var repo = new LibGit2Sharp.Repository(repositoryPath);
                var config = repo.Config;

                var name = config.Get<string>("user.name")?.Value;
                var email = config.Get<string>("user.email")?.Value;

                // If we have valid Git config, use it
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                {
                    return new GitAuthor
                    {
                        Name = name,
                        Email = email
                    };
                }

                // Try to get from global Git config
                var globalConfig = config.Get<string>("user.name", LibGit2Sharp.ConfigurationLevel.Global)?.Value;
                var globalEmail = config.Get<string>("user.email", LibGit2Sharp.ConfigurationLevel.Global)?.Value;

                if (!string.IsNullOrEmpty(globalConfig) && !string.IsNullOrEmpty(globalEmail))
                {
                    return new GitAuthor
                    {
                        Name = globalConfig,
                        Email = globalEmail
                    };
                }

                // Fallback if no Git config found
                _logger.LogWarning("Git user.name and user.email not configured for repository: {RepositoryPath}", repositoryPath);
                return new GitAuthor
                {
                    Name = "Unknown User",
                    Email = "user@example.com"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Git author from repository: {RepositoryPath}", repositoryPath);
                return new GitAuthor
                {
                    Name = "Unknown User",
                    Email = "user@example.com"
                };
            }
        }

        /// <summary>
        /// Converts Git operation changes to frontend-compatible format
        /// </summary>
        private List<ChangedFileInfo> ConvertToChangedFileInfo(List<string> changes, string projectPath)
        {
            if (changes == null) return new List<ChangedFileInfo>();

            return changes.Select(change => 
            {
                var fullPath = Path.Combine(projectPath, change);
                var relativePath = change.Replace('\\', '/');
                
                return new ChangedFileInfo
                {
                    FullPath = fullPath,
                    RelativePath = relativePath,
                    FileName = Path.GetFileName(change),
                    Status = "Modified", // TODO: Determine actual status
                    Author = "Unknown", // TODO: Get actual author
                    MdFiles = CreateFileStructure(change, projectPath)
                };
            }).ToList();
        }

        /// <summary>
        /// Creates hierarchical file structure for frontend tree
        /// </summary>
        private List<FileStructureNode> CreateFileStructure(string relativePath, string projectPath)
        {
            var nodes = new List<FileStructureNode>();
            var parts = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";
            
            for (int i = 0; i < parts.Length; i++)
            {
                currentPath = Path.Combine(currentPath, parts[i]);
                var fullPath = Path.Combine(projectPath, currentPath);
                
                nodes.Add(new FileStructureNode
                {
                    Name = parts[i],
                    FullPath = fullPath,
                    RelativePath = currentPath.Replace('\\', '/'),
                    Level = i,
                    Type = i == parts.Length - 1 ? "mdFile" : "folder",
                    Expandable = i < parts.Length - 1
                });
            }
            
            return nodes;
        }
    }

    /// <summary>
    /// Request model for toolbar Git operations
    /// </summary>
    public class ToolbarGitRequest
    {
        /// <summary>
        /// Path to the Git repository/project
        /// </summary>
        // [Required] - Temporarily disabled to debug 400 error
        public string ProjectPath { get; set; }

        /// <summary>
        /// Commit message (optional, will auto-generate if empty)
        /// </summary>
        public string? CommitMessage { get; set; }
    }

    /// <summary>
    /// Request model for push-only operations (no commit message required)
    /// </summary>
    public class PushOnlyRequest
    {
        /// <summary>
        /// Path to the Git repository/project
        /// </summary>
        public string ProjectPath { get; set; }
    }
}