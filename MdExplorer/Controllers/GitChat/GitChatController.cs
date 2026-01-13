using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibGit2Sharp;
using MdExplorer.Services.TeamChat;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MdExplorer.Controllers.GitChat
{
    /// <summary>
    /// Controller for Team Chat feature - provides room identification and user info
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GitChatController : ControllerBase
    {
        private readonly ILogger<GitChatController> _logger;
        private readonly ITeamChatService _chatService;
        private readonly IHubContext<TeamChatHub> _hubContext;

        public GitChatController(
            ILogger<GitChatController> logger,
            ITeamChatService chatService,
            IHubContext<TeamChatHub> hubContext)
        {
            _logger = logger;
            _chatService = chatService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Gets the chat room information for a repository
        /// Returns roomId (hash of normalized remote URL), repository name, and user info
        /// </summary>
        [HttpGet("room-info")]
        public async Task<IActionResult> GetRoomInfo([FromQuery][Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
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

                using (var repo = new Repository(repositoryPath))
                {
                    // Get remote URL
                    var origin = repo.Network.Remotes["origin"];
                    if (origin == null)
                    {
                        return Ok(new { hasRemote = false, error = "No remote 'origin' configured" });
                    }

                    var remoteUrl = origin.Url;
                    var normalizedUrl = NormalizeGitUrl(remoteUrl);
                    var roomId = ComputeRoomId(normalizedUrl);
                    var repositoryName = ExtractRepositoryName(remoteUrl);

                    // Get git user info
                    var userName = repo.Config.Get<string>("user.name")?.Value ?? "Anonymous";
                    var userEmail = repo.Config.Get<string>("user.email")?.Value ?? "anonymous@unknown";
                    var userId = ComputeUserId(userEmail);

                    _logger.LogInformation("Chat room info requested for {RepositoryPath}, roomId: {RoomId}",
                        repositoryPath, roomId);

                    return Ok(new
                    {
                        hasRemote = true,
                        roomId,
                        repositoryName,
                        remoteUrl,
                        user = new
                        {
                            userId,
                            userName,
                            userEmail
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat room info for repository: {RepositoryPath}", repositoryPath);
                return Ok(new { hasRemote = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Gets only the git user info for a repository (lighter endpoint)
        /// </summary>
        [HttpGet("git-user")]
        public IActionResult GetGitUser([FromQuery][Required] string repositoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(repositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                var gitPath = Path.Combine(repositoryPath, ".git");
                if (!Directory.Exists(gitPath))
                {
                    return Ok(new { found = false, error = "Not a Git repository" });
                }

                using (var repo = new Repository(repositoryPath))
                {
                    var userName = repo.Config.Get<string>("user.name")?.Value ?? "Anonymous";
                    var userEmail = repo.Config.Get<string>("user.email")?.Value ?? "anonymous@unknown";
                    var userId = ComputeUserId(userEmail);

                    return Ok(new
                    {
                        found = true,
                        userId,
                        userName,
                        userEmail
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting git user for repository: {RepositoryPath}", repositoryPath);
                return Ok(new { found = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Register that a user has opened a project
        /// This updates the project users count for the associated chat room
        /// </summary>
        [HttpPost("project-opened")]
        public async Task<IActionResult> ProjectOpened([FromBody] ProjectOpenedDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto?.RepositoryPath))
                {
                    return BadRequest(new { error = "Repository path is required" });
                }

                // Get room info
                var gitPath = Path.Combine(dto.RepositoryPath, ".git");
                if (!Directory.Exists(gitPath))
                {
                    return Ok(new { success = false, error = "Not a Git repository" });
                }

                using (var repo = new Repository(dto.RepositoryPath))
                {
                    var origin = repo.Network.Remotes["origin"];
                    if (origin == null)
                    {
                        return Ok(new { success = false, error = "No remote origin" });
                    }

                    var normalizedUrl = NormalizeGitUrl(origin.Url);
                    var roomId = ComputeRoomId(normalizedUrl);

                    // Get user info from git config
                    var userName = repo.Config.Get<string>("user.name")?.Value ?? "Anonymous";
                    var userEmail = repo.Config.Get<string>("user.email")?.Value ?? "anonymous@unknown";
                    var userId = ComputeUserId(userEmail);

                    // Generate unique oderId for this session
                    var oderId = dto.OderId ?? $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}".Substring(0, 20);

                    var user = new ChatUserInfo
                    {
                        OderId = oderId,
                        UserId = userId,
                        UserName = userName,
                        UserEmail = userEmail,
                        Online = true,
                        LastSeen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    // Register in service
                    await _chatService.RegisterProjectOpen(roomId, oderId, user);

                    // Broadcast updated count to all clients in the room
                    var count = _chatService.GetProjectUsersCount(roomId);
                    await _hubContext.Clients.Group(roomId).SendAsync("ProjectUsersCountUpdate", count);

                    _logger.LogInformation("Project opened: {RepositoryPath}, roomId: {RoomId}, user: {UserName}",
                        dto.RepositoryPath, roomId, userName);

                    return Ok(new { success = true, roomId, oderId, projectUsersCount = count });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering project open: {RepositoryPath}", dto?.RepositoryPath);
                return Ok(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Register that a user has closed a project
        /// </summary>
        [HttpPost("project-closed")]
        public async Task<IActionResult> ProjectClosed([FromBody] ProjectClosedDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto?.RoomId) || string.IsNullOrWhiteSpace(dto?.OderId))
                {
                    return BadRequest(new { error = "RoomId and OderId are required" });
                }

                await _chatService.UnregisterProjectOpen(dto.RoomId, dto.OderId);

                // Broadcast updated count
                var count = _chatService.GetProjectUsersCount(dto.RoomId);
                await _hubContext.Clients.Group(dto.RoomId).SendAsync("ProjectUsersCountUpdate", count);

                _logger.LogInformation("Project closed: roomId: {RoomId}, oderId: {OderId}", dto.RoomId, dto.OderId);

                return Ok(new { success = true, projectUsersCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering project close");
                return Ok(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get current project users count for a room
        /// </summary>
        [HttpGet("project-users-count")]
        public IActionResult GetProjectUsersCount([FromQuery][Required] string roomId)
        {
            var count = _chatService.GetProjectUsersCount(roomId);
            return Ok(new { count });
        }

        /// <summary>
        /// Normalizes a Git URL to a consistent format for hashing
        /// Handles SSH, HTTPS, and various Git hosting formats
        /// </summary>
        private static string NormalizeGitUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            var normalized = url.Trim().ToLowerInvariant();

            // Remove .git suffix
            if (normalized.EndsWith(".git"))
                normalized = normalized.Substring(0, normalized.Length - 4);

            // Convert SSH format to standard format
            // git@github.com:user/repo -> github.com/user/repo
            var sshMatch = Regex.Match(normalized, @"^git@([^:]+):(.+)$");
            if (sshMatch.Success)
            {
                normalized = $"{sshMatch.Groups[1].Value}/{sshMatch.Groups[2].Value}";
            }

            // Remove protocol prefix (https://, http://, ssh://)
            normalized = Regex.Replace(normalized, @"^(https?|ssh|git)://", "");

            // Remove trailing slashes
            normalized = normalized.TrimEnd('/');

            // Remove authentication info (user:pass@)
            normalized = Regex.Replace(normalized, @"^[^@]+@", "");

            return normalized;
        }

        /// <summary>
        /// Computes a room ID from a normalized URL using SHA256
        /// Returns first 16 characters of the hash
        /// </summary>
        private static string ComputeRoomId(string normalizedUrl)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(normalizedUrl);
                var hash = sha256.ComputeHash(bytes);
                var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return hashString.Substring(0, 16);
            }
        }

        /// <summary>
        /// Computes a user ID from email using SHA256
        /// Returns first 12 characters of the hash
        /// </summary>
        private static string ComputeUserId(string email)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(email.ToLowerInvariant().Trim());
                var hash = sha256.ComputeHash(bytes);
                var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return hashString.Substring(0, 12);
            }
        }

        /// <summary>
        /// Extracts repository name from a Git URL
        /// </summary>
        private static string ExtractRepositoryName(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "Unknown Repository";

            // Remove .git suffix
            var name = url;
            if (name.EndsWith(".git"))
                name = name.Substring(0, name.Length - 4);

            // Get the last part of the path
            var lastSlash = name.LastIndexOfAny(new[] { '/', ':' });
            if (lastSlash >= 0 && lastSlash < name.Length - 1)
                name = name.Substring(lastSlash + 1);

            return name;
        }
    }

    /// <summary>
    /// DTO for project-opened endpoint
    /// </summary>
    public class ProjectOpenedDto
    {
        public string RepositoryPath { get; set; }
        public string? OderId { get; set; }
    }

    /// <summary>
    /// DTO for project-closed endpoint
    /// </summary>
    public class ProjectClosedDto
    {
        public string? RoomId { get; set; }
        public string? OderId { get; set; }
    }
}
