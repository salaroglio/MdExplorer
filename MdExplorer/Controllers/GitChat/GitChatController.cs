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
        private readonly MdExplorer.Services.IProjectMetadataService _metadata;
        private readonly IChatRoomKeys _roomKeys;
        private readonly MdExplorer.Services.Git.INativeGitTransport _transport;

        public GitChatController(
            ILogger<GitChatController> logger,
            ITeamChatService chatService,
            IHubContext<TeamChatHub> hubContext,
            MdExplorer.Services.IProjectMetadataService metadata,
            IChatRoomKeys roomKeys,
            MdExplorer.Services.Git.INativeGitTransport transport)
        {
            _transport = transport;
            _logger = logger;
            _chatService = chatService;
            _hubContext = hubContext;
            _metadata = metadata;
            _roomKeys = roomKeys;
        }

        /// <summary>
        /// Gets the chat room information for a repository
        /// Returns roomId (derived from the project room secret and the normalized remote URL), repository name, and user info
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

                // La chiave della stanza è il segreto del PROGETTO, in git: si distribuisce, non si ricrea. Se questa copia
                // non ce l'ha, prima si chiede all'origin; se nemmeno lì c'è, si crea qui e servono commit e push.
                // Un .development.yml che non si legge è un errore detto, mai «nessun segreto».
                using (var probe = new Repository(repositoryPath))
                {
                    if (probe.Network.Remotes["origin"] == null)
                        return Ok(new { hasRemote = false, error = "No remote 'origin' configured" });
                }
                var local = _metadata.ReadRoomSecret(repositoryPath);
                if (local == null)
                {
                    var fetch = await _transport.FetchAsync(repositoryPath, "origin");
                    if (!fetch.Ok)
                        throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say(
                            $"Questa copia non ha ancora il segreto della chat e non riesco a chiedere all'origin se c'è già ({fetch.Error}): " +
                            "non ne creo uno nuovo, chiuderebbe fuori i colleghi. Riprova quando l'origin risponde.",
                            $"This copy does not have the chat secret yet and I cannot ask the origin whether it already exists ({fetch.Error}): " +
                            "I do not create a new one, it would lock your colleagues out. Try again when the origin answers."));
                }

                using (var repo = new Repository(repositoryPath))
                {
                    // Get remote URL
                    var origin = repo.Network.Remotes["origin"];

                    var remoteUrl = origin.Url;
                    var normalizedUrl = NormalizeGitUrl(remoteUrl);
                    var repositoryName = ExtractRepositoryName(remoteUrl);

                    // Get git user info
                    var userName = repo.Config.Get<string>("user.name")?.Value ?? "Anonymous";
                    var userEmail = repo.Config.Get<string>("user.email")?.Value ?? "anonymous@unknown";
                    var userId = ComputeUserId(userEmail);

                    var published = PublishedRoomSecret(repo, repositoryPath);
                    string secret, keyState;
                    var created = false;
                    if (local == null && published != null)
                        (secret, keyState) = (published, "notPulled");     // c'è già: si usa quella, arriva con lo scaricamento
                    else if (local == null)
                    {
                        (secret, created) = _metadata.EnsureRoomSecret(repositoryPath);
                        keyState = KeyState(repo, repositoryPath, secret);
                    }
                    else if (published != null && published != local)
                        (secret, keyState) = (published, "differs");       // i colleghi sono nella stanza di quella pubblicata
                    else
                        (secret, keyState) = (local, KeyState(repo, repositoryPath, local));
                    // La stanza si ricava dal segreto scelto: è la stessa dei colleghi che hanno quel segreto.
                    var roomId = ComputeRoomId(normalizedUrl, secret);
                    _roomKeys.Register(roomId, repositoryPath, secret);
                    var relay = await _chatService.CheckRoomAsync(roomId);

                    _logger.LogInformation("Chat room info requested for {RepositoryPath}, roomId: {RoomId}, key: {KeyState}{Created}, relay: {Relay}",
                        repositoryPath, roomId, keyState, created ? " (creata ora)" : "", relay.Ok ? "ok" : relay.Code);

                    return Ok(new
                    {
                        hasRemote = true,
                        roomId,
                        repositoryName,
                        remoteUrl,
                        keyState,
                        relayOk = relay.Ok,
                        relayCode = relay.Code,
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

                    // Senza il segreto del progetto non c'è stanza (si ricava da lì): niente conteggio.
                    // Aprire un progetto non deve scrivere .development.yml, quindi il segreto non si crea qui.
                    var secret = _metadata.ReadRoomSecret(dto.RepositoryPath);
                    if (secret == null)
                        return Ok(new { success = false, error = "No room secret: the project has no chat room yet" });

                    var normalizedUrl = NormalizeGitUrl(origin.Url);
                    var roomId = ComputeRoomId(normalizedUrl, secret);
                    _roomKeys.Register(roomId, dto.RepositoryPath, secret);

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
        // Normalizzazione + room delegate all'UNICA sorgente di verità (bll FederationRoom):
        // Team Chat e Federazione calcolano SEMPRE la stessa stanza per lo stesso repo, e un
        // fix all'algoritmo non fa più divergere i due sottosistemi. NB: ComputeUserId resta
        // locale (16 hex) — l'ownerId federato è un namespace diverso (/mdfed, 12 hex) e non
        // deve coincidere con lo userId della chat.
        private static string NormalizeGitUrl(string url)
            => MdExplorer.Features.Federation.FederationRoom.NormalizeGitOrigin(url);

        /// <summary>
        /// Dove sta il segreto del progetto rispetto ai colleghi: <c>shared</c> (nel ramo remoto),
        /// <c>notPushed</c> (committato, non ancora pushato), <c>notCommitted</c> (solo nel file).
        /// </summary>
        /// <summary>
        /// Il segreto della stanza nella versione pubblicata di <c>.development.yml</c>: il ramo remoto che questo segue,
        /// oppure il ramo principale dell'origin se questo non ne segue nessuno. Null se lì non c'è.
        /// </summary>
        private static string PublishedRoomSecret(Repository repo, string repositoryPath)
        {
            var file = Path.GetRelativePath(repo.Info.WorkingDirectory, Path.Combine(repositoryPath, ".development.yml"))
                .Replace('\\', '/');
            var tip = repo.Head?.TrackedBranch?.Tip
                      ?? (repo.Refs["refs/remotes/origin/HEAD"]?.ResolveToDirectReference()?.Target as Commit);
            if (tip?[file]?.Target is not Blob blob) return null;
            return MdExplorer.Services.ProjectMetadataService.RoomSecretIn(blob.GetContentText(), "origin");
        }

        private static string KeyState(Repository repo, string repositoryPath, string secret)
        {
            var file = Path.GetRelativePath(repo.Info.WorkingDirectory, Path.Combine(repositoryPath, ".development.yml"))
                .Replace('\\', '/');
            static bool Has(Commit commit, string path, string secret)
                => commit?[path]?.Target is Blob blob && blob.GetContentText().Contains(secret, StringComparison.Ordinal);

            if (!Has(repo.Head?.Tip, file, secret)) return "notCommitted";
            if (!Has(repo.Head?.TrackedBranch?.Tip, file, secret)) return "notPushed";
            return "shared";
        }

        private static string ComputeRoomId(string normalizedUrl, string roomSecret)
            => MdExplorer.Features.Federation.FederationRoom.RoomIdFromNormalized(normalizedUrl, roomSecret);

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
