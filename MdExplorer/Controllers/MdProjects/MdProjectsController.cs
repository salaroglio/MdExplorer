using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ad.Tools.Dal;
using MdExplorer.Service.Utilities;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdProjects;
using AutoMapper;
using MdExplorer.Utilities;
using MdExplorer.Service.Controllers.MdProjects.dto;
using MdExplorer.Services;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using MdExplorer.Services.Git;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Service.Models;
using MdExplorer.Service.Services;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Features.Services.AI;

namespace MdExplorer.Service.Controllers.MdProjects
{
    [ApiController]
    [Route("api/MdProjects/{action}")]
    public class MdProjectsController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IServiceProvider _services;
        private readonly ProcessUtil _processUtil;
        private readonly IMapper _mapper;
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _fileSystemWatcherManager;
        private readonly IGitAccountService _gitAccountService;
        private readonly GitCredentialHelperResolver _gitCredentialHelper;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        private readonly IProjectMetadataService _projectMetadataService;
        private readonly IGitAuthorsService _gitAuthorsService;
        private readonly MdExplorer.Services.Federation.IProjectRelaySettingsService _relaySettings;
        private readonly IEnumerable<IAiProvider> _aiProviders;

        public MdProjectsController(IUserSettingsDB userSettingsDB,
                IServiceProvider services,
                ProcessUtil processUtil,
                IMapper mapper,
                IDatabaseManager databaseManager,
                IFileSystemWatcherManager fileSystemWatcherManager,
                IGitAccountService gitAccountService,
                GitCredentialHelperResolver gitCredentialHelper,
                FoldersIgnoreService foldersIgnoreService,
                IProjectMetadataService projectMetadataService,
                IGitAuthorsService gitAuthorsService,
                MdExplorer.Services.Federation.IProjectRelaySettingsService relaySettings,
                IEnumerable<IAiProvider> aiProviders)
        {
            _relaySettings = relaySettings;
            _userSettingsDB = userSettingsDB;
            _services = services;
            _processUtil = processUtil;
            _mapper = mapper;
            _databaseManager = databaseManager;
            _fileSystemWatcherManager = fileSystemWatcherManager;
            _gitAccountService = gitAccountService;
            _gitCredentialHelper = gitCredentialHelper;
            _foldersIgnoreService = foldersIgnoreService;
            _projectMetadataService = projectMetadataService;
            _gitAuthorsService = gitAuthorsService;
            _aiProviders = aiProviders;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {

            // check if folder exists in project table

            var projectDal = _userSettingsDB.GetDal<Project>();
            var list = projectDal.GetList().OrderByDescending(_ => _.LastUpdate).ToList();
            var listToReturn = _mapper.Map<IEnumerable<ProjectWithoutBookmarks>>(list).ToList();

            // Enrich each project with its shared description from .development.yml.
            // Participants are intentionally NOT eager-loaded here: BuildMergedParticipants
            // enumerates git commits (LibGit2Sharp) which can be slow for large repos and
            // would block the initial projects grid render. The client fetches participants
            // per-card in parallel so a single slow repo does not block the others.
            foreach (var dto in listToReturn)
            {
                dto.Description = _projectMetadataService.GetDescription(dto.Path);
                // Custom icon metadata: cheap to read (yaml + file existence check),
                // safe to keep eager — unlike participants which walk the git log.
                var icon = _projectMetadataService.GetIcon(dto.Path);
                dto.HasCustomIcon = icon != null;
                dto.IconUpdatedAt = icon?.UpdatedAt;
            }

            return Ok(listToReturn);
        }

        /// <summary>
        /// Merges participants stored in .development.yml with the fresh list of
        /// git authors. Saved overrides (ChatEmail, DisplayName) are preserved;
        /// new git authors are surfaced as non-manual entries defaulting ChatEmail
        /// to GitEmail. Ordering follows commit count desc, with manual entries last.
        /// </summary>
        private IList<ProjectParticipant> BuildMergedParticipants(string projectPath)
        {
            var saved = _projectMetadataService.GetParticipants(projectPath) ?? new List<ProjectParticipant>();
            var savedByKey = saved
                .Where(p => !string.IsNullOrWhiteSpace(p?.GitEmail))
                .GroupBy(p => p.GitEmail.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.Last());

            var authors = _gitAuthorsService.GetAuthors(projectPath) ?? new List<GitAuthorInfo>();
            var merged = new List<(ProjectParticipant p, int commitCount, bool manual)>();
            var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var author in authors)
            {
                var key = author.Email;
                usedKeys.Add(key);
                if (savedByKey.TryGetValue(key, out var storedP))
                {
                    // Refresh GitName from the latest commit but preserve overrides.
                    storedP.GitName = author.Name ?? storedP.GitName;
                    storedP.Manual = false;
                    if (string.IsNullOrWhiteSpace(storedP.ChatEmail))
                    {
                        storedP.ChatEmail = storedP.GitEmail;
                    }
                    merged.Add((storedP, author.CommitCount, false));
                }
                else
                {
                    merged.Add((new ProjectParticipant
                    {
                        GitEmail = key,
                        GitName = author.Name,
                        DisplayName = author.Name,
                        ChatEmail = key,
                        Manual = false
                    }, author.CommitCount, false));
                }
            }

            // Manual-only participants (no matching git author) go after git authors,
            // preserving their saved order.
            foreach (var stored in saved)
            {
                if (string.IsNullOrWhiteSpace(stored?.GitEmail)) continue;
                var key = stored.GitEmail.Trim().ToLowerInvariant();
                if (usedKeys.Contains(key)) continue;
                stored.Manual = true;
                merged.Add((stored, 0, true));
            }

            return merged
                .OrderBy(t => t.manual ? 1 : 0)
                .ThenByDescending(t => t.commitCount)
                .Select(t => t.p)
                .ToList();
        }

        [HttpGet]
        public IActionResult GetParticipants([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { message = "path is required" });
            }
            return Ok(BuildMergedParticipants(path));
        }

        [HttpPut]
        public IActionResult Participants([FromQuery] string path, [FromBody] IList<ProjectParticipant> participants)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { message = "path is required" });
            }
            try
            {
                _projectMetadataService.SetParticipants(path, participants ?? new List<ProjectParticipant>());
                return Ok(BuildMergedParticipants(path));
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();
                logger?.LogError(ex, "Failed to save participants for {Path}", path);
                return StatusCode(500, new { message = "Failed to save participants", error = ex.Message });
            }
        }

        /// <summary>
        /// Stato di attivazione della città degli agenti (§12.4). Il room secret NON è
        /// esposto (credenziale, vive nel .development.yml condiviso via git): il client
        /// sa solo se esiste.
        /// </summary>
        [HttpGet]
        public IActionResult AgentCity([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "path is required" });

            var cfg = _projectMetadataService.GetAgentCity(path);
            return Ok(ToAgentCityDto(cfg, path));
        }

        /// <summary>Attiva/disattiva la città e imposta il doc di ownership (§12.4).</summary>
        [HttpPost]
        public IActionResult SetAgentCity([FromQuery] string path, [FromBody] AgentCityRequest request)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "path is required" });
            if (request == null)
                return BadRequest(new { message = "request body is required" });

            try
            {
                // I flag opt-in (worktree, auto-merge) sono booleani: non possono distinguere
                // "non inviato" da "false". La UI oggi manda solo enabled+ownershipDoc, quindi
                // senza questa preservazione il primo salvataggio dalle impostazioni li
                // SPEGNEREBBE in silenzio — stessa forma del difetto gia' chiuso su RelayUrl e
                // RoomSecret. Nullable nel DTO: null = lascia com'e'.
                var current = _projectMetadataService.GetAgentCity(path);

                var saved = _projectMetadataService.SetAgentCity(path, new AgentCityConfig
                {
                    Enabled = request.Enabled,
                    OwnershipDoc = request.OwnershipDoc,
                    RelayUrl = request.RelayUrl,
                    UseAgentWorktrees = request.UseAgentWorktrees ?? current?.UseAgentWorktrees ?? false,
                    AutoMergeAgentDeliverables = request.AutoMergeAgentDeliverables ?? current?.AutoMergeAgentDeliverables ?? false,
                });
                return Ok(ToAgentCityDto(saved, path));
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();
                logger?.LogError(ex, "Failed to save agentCity for {Path}", path);
                return StatusCode(500, new { message = "Failed to save agent city activation", error = ex.Message });
            }
        }

        private static object ToAgentCityDto(AgentCityConfig cfg, string projectPath) => new
        {
            enabled = cfg?.Enabled ?? false,
            ownershipDoc = cfg?.OwnershipDoc,
            relayUrl = cfg?.RelayUrl,
            hasRoomSecret = !string.IsNullOrWhiteSpace(cfg?.RoomSecret),
            useAgentWorktrees = cfg?.UseAgentWorktrees ?? false,
            autoMergeAgentDeliverables = cfg?.AutoMergeAgentDeliverables ?? false,
            // Senza git non esistono né worktree né merge: la UI disabilita le due opzioni
            // invece di lasciarle spuntabili e poi inerti.
            isGitRepository = !string.IsNullOrWhiteSpace(projectPath)
                              && (Directory.Exists(Path.Combine(projectPath, ".git"))
                                  || System.IO.File.Exists(Path.Combine(projectPath, ".git"))),
        };

        /// <summary>
        /// Campi nullable di proposito (memoria <c>dto_nullable_implicit_required</c>): la UI
        /// invia solo enabled+ownershipDoc — con reference type non-nullable la validazione
        /// automatica di <c>[ApiController]</c> risponderebbe 400 "RelayUrl is required" prima
        /// di entrare nell'action, rendendo l'attivazione città impossibile dalla UI.
        /// </summary>
        public class AgentCityRequest
        {
            public bool Enabled { get; set; }
            public string? OwnershipDoc { get; set; }
            public string? RelayUrl { get; set; }

            /// <summary>Opt-in isolamento worktree (Fase 7c). <c>null</c> = non toccare.</summary>
            public bool? UseAgentWorktrees { get; set; }

            /// <summary>Opt-in auto-merge dei deliverable-doc (Fase 7g). <c>null</c> = non toccare.</summary>
            public bool? AutoMergeAgentDeliverables { get; set; }
        }

        /// <summary>
        /// Impostazioni del relay per QUESTO progetto: indirizzo e presenza della chiave. La
        /// chiave non esce mai dal server — il client sa solo se c'è e da dove arriva.
        /// </summary>
        [HttpGet]
        public IActionResult RelaySettings([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "path is required" });

            var cfg = _projectMetadataService.GetAgentCity(path);
            var view = _relaySettings.Get(path, cfg?.RelayUrl);
            return Ok(ToRelayDto(view));
        }

        /// <summary>Salva indirizzo e/o chiave del relay per questo progetto.</summary>
        [HttpPost]
        public IActionResult SetRelaySettings([FromQuery] string path, [FromBody] RelaySettingsRequest request)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "path is required" });
            if (request == null)
                return BadRequest(new { message = "request body is required" });

            try
            {
                _relaySettings.Save(path, request.RelayUrl, request.ApiKey, request.ClearApiKey);

                var cfg = _projectMetadataService.GetAgentCity(path);
                return Ok(ToRelayDto(_relaySettings.Get(path, cfg?.RelayUrl)));
            }
            catch (InvalidOperationException ex)
            {
                // Precondizione non soddisfatta (progetto non registrato): messaggio azionabile,
                // non un 500 generico.
                return UnprocessableEntity(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();
                logger?.LogError(ex, "Failed to save relay settings for {Path}", path);
                return StatusCode(500, new { message = "Failed to save relay settings", error = ex.Message });
            }
        }

        /// <summary>Bussa al relay con la chiave configurata e riporta cosa ha risposto.</summary>
        [HttpPost]
        public async Task<IActionResult> TestRelaySettings([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "path is required" });

            var cfg = _projectMetadataService.GetAgentCity(path);
            var result = await _relaySettings.TestAsync(path, cfg?.RelayUrl, HttpContext.RequestAborted);
            return Ok(new { success = result.Success, statusCode = result.StatusCode, message = result.Message });
        }

        private static object ToRelayDto(MdExplorer.Services.Federation.RelaySettingsView view) => new
        {
            relayUrl = view.RelayUrl,
            relayUrlSource = view.RelayUrlSource.ToString(),
            hasApiKey = view.HasApiKey,
            apiKeySource = view.ApiKeySource.ToString(),
            lastTestedAt = view.LastTestedAt,
            lastTestSuccess = view.LastTestSuccess,
        };

        /// <summary>
        /// Nullable di proposito, come <see cref="AgentCityRequest"/>: la UI manda solo i campi
        /// che cambia, e un reference type non-nullable farebbe scattare il 400 automatico di
        /// <c>[ApiController]</c> prima di entrare nell'action (memoria dto_nullable_implicit_required).
        /// </summary>
        public class RelaySettingsRequest
        {
            public string? RelayUrl { get; set; }
            /// <summary>Vuoto/assente ⇒ chiave invariata (la UI non rimanda mai quella salvata).</summary>
            public string? ApiKey { get; set; }
            /// <summary>Richiesta esplicita di rimuovere la chiave salvata.</summary>
            public bool ClearApiKey { get; set; }
        }

        [HttpGet]
        public IActionResult GitAuthors([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { message = "path is required" });
            }
            return Ok(_gitAuthorsService.GetAuthors(path));
        }

        [HttpGet]
        public IActionResult CurrentGitUser([FromQuery] string path)
        {
            var user = _gitAuthorsService.GetCurrentUser(path);
            if (user == null)
            {
                return Ok(new { email = (string)null, name = (string)null });
            }
            return Ok(new { email = user.Email, name = user.Name });
        }

        [HttpPost]
        public IActionResult DeleteProject([FromBody] DeleteProjectRequest request)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                
                var projectDal = _userSettingsDB.GetDal<Project>();
                var projectFromDb = projectDal.GetList().Where(_ => _.Id == request.Id).FirstOrDefault();
                
                if (projectFromDb == null)
                {
                    _userSettingsDB.Rollback();
                    return NotFound(new { message = "Project not found" });
                }
                
                // Prima cancella tutti i bookmark associati al progetto
                var bookmarkDal = _userSettingsDB.GetDal<Bookmark>();
                var bookmarksToDelete = bookmarkDal.GetList().Where(b => b.Project.Id == request.Id).ToList();
                foreach (var bookmark in bookmarksToDelete)
                {
                    bookmarkDal.Delete(bookmark);
                }
                
                // Cancella anche le entry di TocDescriptionCache associate (ProjectId è int, non Guid)
                // Nota: ProjectId in TocDescriptionCache è int, ma Project.Id è Guid
                // Questo potrebbe essere un problema di design, ma per ora gestiamo entrambi i casi
                var tocCacheDal = _userSettingsDB.GetDal<TocDescriptionCache>();
                // Non possiamo fare un match diretto tra Guid e int, quindi rimuoviamo solo se ci sono cache orfane
                
                // Poi cancella il progetto
                projectDal.Delete(projectFromDb);
                
                _userSettingsDB.Commit();
                return Ok(new { message = "done!" });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();
                logger?.LogError(ex, "Error deleting project with ID: {ProjectId}", request.Id);
                return StatusCode(500, new { message = "Error deleting project", error = ex.Message });
            }
        }
        
        public class DeleteProjectRequest
        {
            public Guid Id { get; set; }
        }

        [HttpPost]
        public IActionResult UpdateProject([FromBody] UpdateProjectRequest request)
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();

            if (request == null || request.Id == Guid.Empty)
            {
                return BadRequest(new { message = "Project id is required" });
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest(new { message = "Project name is required" });
            }
            if (name.Length > 255)
            {
                return BadRequest(new { message = "Project name exceeds 255 characters" });
            }

            var description = request.Description?.Trim();
            if (!string.IsNullOrEmpty(description) && description.Length > 200)
            {
                return BadRequest(new { message = "Description exceeds 200 characters" });
            }

            try
            {
                // Name → per-user label in UserDB
                _userSettingsDB.BeginTransaction();

                var projectDal = _userSettingsDB.GetDal<Project>();
                var projectFromDb = projectDal.GetList().Where(_ => _.Id == request.Id).FirstOrDefault();

                if (projectFromDb == null)
                {
                    _userSettingsDB.Rollback();
                    return NotFound(new { message = "Project not found" });
                }

                projectFromDb.Name = name;
                projectDal.Save(projectFromDb);

                _userSettingsDB.Commit();

                // Description → shared across users in .development.yml
                try
                {
                    _projectMetadataService.SetDescription(projectFromDb.Path, description);
                }
                catch (Exception yamlEx)
                {
                    // Name was saved successfully; surface YAML failure explicitly so the user can retry.
                    logger?.LogError(yamlEx, "Failed to persist description to .development.yml for project {ProjectId}", request.Id);
                    return StatusCode(500, new { message = "Project name saved but description could not be written to .development.yml", error = yamlEx.Message });
                }

                var dto = _mapper.Map<ProjectWithoutBookmarks>(projectFromDb);
                dto.Description = _projectMetadataService.GetDescription(projectFromDb.Path);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                logger?.LogError(ex, "Error updating project with ID: {ProjectId}", request.Id);
                return StatusCode(500, new { message = "Error updating project", error = ex.Message });
            }
        }

        public class UpdateProjectRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Serves the custom project icon as a PNG. Returns 404 when the project
        /// has no custom icon (the client falls back to the default SVG).
        /// </summary>
        [HttpGet]
        public IActionResult ProjectIcon([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "Project id is required" });
            }

            var projectDal = _userSettingsDB.GetDal<Project>();
            var project = projectDal.GetList().FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var iconPath = _projectMetadataService.GetIconAbsolutePath(project.Path);
            if (string.IsNullOrEmpty(iconPath) || !System.IO.File.Exists(iconPath))
            {
                return NotFound();
            }

            // No-cache here: the client is expected to bust via ?v=updatedAt;
            // long-lived caching would defeat that.
            var bytes = System.IO.File.ReadAllBytes(iconPath);
            return File(bytes, "image/png");
        }

        public class SetProjectIconRequest
        {
            public Guid Id { get; set; }
            /// <summary>
            /// Base64-encoded PNG produced by the in-app icon editor.
            /// May include the "data:image/png;base64," prefix.
            /// </summary>
            public string PngBase64 { get; set; }
        }

        /// <summary>
        /// Persists a custom icon (PNG) for the project. The PNG lives at
        /// .md/project-icon.png and a reference is written to .development.yml
        /// so the icon follows the project across users.
        /// </summary>
        [HttpPost]
        public IActionResult SetProjectIcon([FromBody] SetProjectIconRequest request)
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();

            if (request == null || request.Id == Guid.Empty)
            {
                return BadRequest(new { message = "Project id is required" });
            }
            if (string.IsNullOrWhiteSpace(request.PngBase64))
            {
                return BadRequest(new { message = "PngBase64 is required" });
            }

            var projectDal = _userSettingsDB.GetDal<Project>();
            var project = projectDal.GetList().FirstOrDefault(p => p.Id == request.Id);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var payload = request.PngBase64;
            var commaIdx = payload.IndexOf(',');
            if (commaIdx >= 0 && payload.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                payload = payload.Substring(commaIdx + 1);
            }

            byte[] pngBytes;
            try
            {
                pngBytes = Convert.FromBase64String(payload);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "PngBase64 is not a valid base64 string" });
            }

            // Sanity cap: 5 MB is way more than a 256x256 PNG should ever need;
            // anything larger almost certainly indicates a client bug.
            if (pngBytes.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Icon payload exceeds 5 MB" });
            }

            try
            {
                _projectMetadataService.SetIcon(project.Path, pngBytes);
                var icon = _projectMetadataService.GetIcon(project.Path);
                return Ok(new { hasCustomIcon = icon != null, iconUpdatedAt = icon?.UpdatedAt });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to set icon for project {ProjectId}", request.Id);
                return StatusCode(500, new { message = "Failed to save project icon", error = ex.Message });
            }
        }

        public class RemoveProjectIconRequest
        {
            public Guid Id { get; set; }
        }

        [HttpPost]
        public IActionResult RemoveProjectIcon([FromBody] RemoveProjectIconRequest request)
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();

            if (request == null || request.Id == Guid.Empty)
            {
                return BadRequest(new { message = "Project id is required" });
            }

            var projectDal = _userSettingsDB.GetDal<Project>();
            var project = projectDal.GetList().FirstOrDefault(p => p.Id == request.Id);
            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            try
            {
                _projectMetadataService.RemoveIcon(project.Path);
                return Ok(new { hasCustomIcon = false, iconUpdatedAt = (string)null });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to remove icon for project {ProjectId}", request.Id);
                return StatusCode(500, new { message = "Failed to remove project icon", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SetFolderProject([FromBody] ProjectCreationRequest request)
        {
            var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();

            // Get ConnectionId from query parameter
            var connectionId = Request.Query["ConnectionId"].ToString();
            if (string.IsNullOrEmpty(connectionId))
            {
                logger?.LogWarning("⚠️ SetFolderProject called without ConnectionId");
                return BadRequest(new { error = "ConnectionId is required" });
            }

            logger?.LogInformation($"📁 Opening project for connection {connectionId}: {request.Path}");

            // [PERF] Temporary phase timing to localize where SetFolderProject spends its time.
            var __perfTotal = Stopwatch.StartNew();
            var __perfPhase = Stopwatch.StartNew();
            Action<string> logPhase = name =>
            {
                __perfPhase.Stop();
                logger?.LogWarning("⏱️ [SetFolderProject PERF] {Phase}: {Ms} ms", name, __perfPhase.ElapsedMilliseconds);
                __perfPhase.Restart();
            };

            try
            {
                // Invalidate FoldersIgnore cache to pick up any changes to .mdFoldersIgnore
                _foldersIgnoreService.InvalidateCache(request.Path);
                logPhase("InvalidateCache");

                // IMPORTANT: Run migrations FIRST, before opening database sessions
                // This prevents "database is locked" errors because NHibernate holds the file open
                bool gitInitialized = ProjectsManager.SetNewProject(_services, request.Path, request.InitializeGit ?? false, request.AddCopilotInstructions ?? true);
                logger?.LogInformation($"✅ Database migrations completed for project: {request.Path}");
                logPhase("ProjectsManager.SetNewProject (migrations+init)");

                // NOW register database contexts (after migrations are complete)
                _databaseManager.RegisterConnection(connectionId, request.Path);
                logger?.LogInformation($"✅ Database contexts registered for connection {connectionId}");
                logPhase("DatabaseManager.RegisterConnection");

                // Register FileSystemWatcher for this connection
                _fileSystemWatcherManager.RegisterWatcher(connectionId, request.Path);
                logger?.LogInformation($"✅ FileSystemWatcher registered for connection {connectionId}");
                logPhase("FileSystemWatcherManager.RegisterWatcher");

                // NOTE: We no longer modify the global FileSystemWatcher singleton
                // Each client now has its own dedicated FileSystemWatcher via FileSystemWatcherManager

                // renew project data
                _userSettingsDB.Clear(); // Ensure fresh data from DB, no stale session cache
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList().Where(_ => _.Path == request.Path).FirstOrDefault();
                if (project == null)
                {
                    project = new Project
                    {
                        Path = request.Path,
                        Name = System.IO.Path.GetFileName(request.Path)
                    };
                    logger?.LogInformation($"📝 Creating new Project record for: {request.Path}");
                }
                else
                {
                    logger?.LogInformation($"📝 Loaded existing Project: LinkIndexingEnabled={project.LinkIndexingEnabled}");
                }
                project.LastUpdate = DateTime.Now;
                projectDal.Save(project);
                _userSettingsDB.Commit();
                logger?.LogInformation($"📝 Project saved. LinkIndexingEnabled={project.LinkIndexingEnabled}");
                logPhase("UserSettingsDB Project upsert");

                // Hook "project opened" (es. schedule di agenti .agent.md con trigger
                // projectOpen). Ogni handler è isolato: un hook rotto non deve mai
                // impedire l'apertura del progetto.
                foreach (var projectOpenedHandler in HttpContext.RequestServices
                             .GetServices<MdExplorer.Abstractions.Services.IProjectOpenedEventHandler>())
                {
                    try
                    {
                        projectOpenedHandler.OnProjectOpened(request.Path);
                    }
                    catch (Exception hookEx)
                    {
                        logger?.LogError(hookEx, "Project-opened hook {Handler} failed",
                            projectOpenedHandler.GetType().Name);
                    }
                }

                // Log Git initialization status
                if (gitInitialized)
                {
                    logger?.LogInformation($"✅ Git repository initialized for project: {request.Path}");
                }

                // Leggi la compatibility mode dal file .development.yml
                string compatibilityMode = "mdexplorer"; // default
                var devConfigPath = System.IO.Path.Combine(request.Path, ".development.yml");
                if (System.IO.File.Exists(devConfigPath))
                {
                    try
                    {
                        var yamlContent = System.IO.File.ReadAllText(devConfigPath);
                        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                            .Build();
                        var fullConfig = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);

                        if (fullConfig != null && fullConfig.ContainsKey("compatibility"))
                        {
                            var compatibilityYaml = new YamlDotNet.Serialization.SerializerBuilder()
                                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                                .Build()
                                .Serialize(fullConfig["compatibility"]);
                            var compatConfig = deserializer.Deserialize<MdExplorer.Features.Configuration.Models.CompatibilityConfig>(compatibilityYaml);
                            compatibilityMode = compatConfig.Mode ?? "mdexplorer";
                            logger?.LogInformation($"📖 Loaded compatibility mode from .development.yml: {compatibilityMode}");
                        }
                        else
                        {
                            logger?.LogInformation($"📖 No compatibility section in .development.yml, using default: {compatibilityMode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Could not read compatibility mode from .development.yml, using default");
                    }
                }
                else
                {
                    logger?.LogInformation($"📖 No .development.yml file found at {devConfigPath}, using default mode: {compatibilityMode}");
                }
                logPhase("CompatibilityMode YAML parse");

                // Check if it's a Git repository and if it has an account configured
                var isGitRepository = Directory.Exists(Path.Combine(request.Path, ".git"));
                var hasGitAccount = false;
                string detectedRemoteUrl = null;
                string detectedProvider = null;
                bool needsManualCredentials = false;

                if (isGitRepository)
                {
                    try
                    {
                        hasGitAccount = _gitAccountService.HasAccountForRepositoryAsync(request.Path).GetAwaiter().GetResult();
                        logger?.LogInformation($"🔐 Git account check for {request.Path}: hasAccount={hasGitAccount}");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Could not check Git account status");
                    }
                    logPhase("HasAccountForRepositoryAsync");

                    // Auto-detect credentials from Git Credential Manager if no account configured
                    if (!hasGitAccount)
                    {
                        try
                        {
                            detectedRemoteUrl = GetRemoteUrlFromRepository(request.Path);
                            if (!string.IsNullOrEmpty(detectedRemoteUrl))
                            {
                                // Detect provider type
                                detectedProvider = DetectProviderFromUrl(detectedRemoteUrl);
                                logger?.LogInformation($"🔍 [CredentialAutoDetect] Attempting auto-detection for {request.Path}, remote: {detectedRemoteUrl}, provider: {detectedProvider}");

                                hasGitAccount = _gitCredentialHelper.DetectAndSaveCredentialsForRepository(request.Path, detectedRemoteUrl).GetAwaiter().GetResult();
                                if (hasGitAccount)
                                {
                                    logger?.LogInformation($"✅ [CredentialAutoDetect] Credentials auto-detected and saved for {request.Path}");
                                }
                                else
                                {
                                    logger?.LogInformation($"⚠️ [CredentialAutoDetect] No credentials found in Git Credential Manager for {detectedRemoteUrl}");
                                    // Signal frontend that manual credentials are needed
                                    needsManualCredentials = true;
                                }
                            }
                            else
                            {
                                logger?.LogInformation($"ℹ️ [CredentialAutoDetect] No remote URL configured for {request.Path}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.LogWarning(ex, "[CredentialAutoDetect] Auto-credential detection failed (non-fatal)");
                            needsManualCredentials = !string.IsNullOrEmpty(detectedRemoteUrl);
                        }
                        logPhase("DetectAndSaveCredentialsForRepository (GCM subprocess)");
                    }
                }

                // Copilot CLI auto-select probe: synchronous, deterministic. If the project prefers
                // Copilot CLI as default AI, we MUST return a real availability — no provisional
                // values, no fire-and-forget warm-up. Worst case is one `copilot --version` spawn
                // (~1-2s on Windows) at the first open after a restart; subsequent opens hit the
                // 5-minute availability cache inside the provider.
                bool copilotCliAutoSelect = project.UseCopilotCliAsDefault;
                bool copilotCliAvailable = false;
                string copilotCliDefaultModel = null;
                if (copilotCliAutoSelect)
                {
                    var copilotProvider = _aiProviders?
                        .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;
                    if (copilotProvider == null)
                    {
                        throw new InvalidOperationException(
                            "Project has UseCopilotCliAsDefault=true but CopilotCliProvider was not resolved from DI. " +
                            "Check Startup.cs IAiProvider registrations.");
                    }
                    copilotProvider.WorkingDirectory = request.Path;
                    copilotCliDefaultModel = "claude-sonnet-5";
                    copilotCliAvailable = copilotProvider.IsAvailable();
                    logger?.LogInformation(
                        "🤖 CopilotCli auto-select: available={Available}, model={Model}, cwd={Cwd}",
                        copilotCliAvailable, copilotCliDefaultModel, request.Path);
                }
                logPhase("CopilotCli availability (sync probe)");

                // Claude Code auto-select: stesso probe deterministico, provider diverso.
                // Qui il controllo di installazione è una scansione del PATH, quindi costa
                // millisecondi e non ha nemmeno il problema del cold start di Copilot.
                bool claudeCodeAutoSelect = project.UseClaudeCodeAsDefault;
                bool claudeCodeAvailable = false;
                string claudeCodeDefaultModel = null;
                if (claudeCodeAutoSelect)
                {
                    var claudeProvider = _aiProviders?
                        .FirstOrDefault(p => p.GetProviderType() == ProviderType.ClaudeCode) as ClaudeCodeProvider;
                    if (claudeProvider == null)
                    {
                        throw new InvalidOperationException(
                            "Il progetto ha UseClaudeCodeAsDefault=true ma ClaudeCodeProvider non è stato risolto dalla DI. " +
                            "Controlla le registrazioni IAiProvider in Startup.cs.");
                    }
                    claudeProvider.WorkingDirectory = request.Path;
                    // Alias, non nome pieno: punta sempre all'ultimo Sonnet e non invecchia.
                    claudeCodeDefaultModel = "sonnet";
                    claudeCodeAvailable = claudeProvider.IsAvailable();
                    logger?.LogInformation(
                        "🤖 ClaudeCode auto-select: available={Available}, model={Model}, cwd={Cwd}",
                        claudeCodeAvailable, claudeCodeDefaultModel, request.Path);
                }
                logPhase("ClaudeCode availability (sync probe)");

                // Precedenza quando sono accesi entrambi: vince Claude Code. Il suo flag nasce
                // OFF, quindi trovarlo acceso è una scelta deliberata; quello di Copilot nasce
                // ON e potrebbe essere solo il default mai toccato. La scelta esplicita batte
                // il default — e il client riceve UN solo auto-select acceso, così non deve
                // arbitrare da solo (due sottoscrizioni che si contendono la chat sarebbero
                // una corsa, non una regola).
                if (claudeCodeAutoSelect && claudeCodeAvailable && copilotCliAutoSelect)
                {
                    logger?.LogInformation(
                        "🤖 Auto-select: accesi sia ClaudeCode sia CopilotCli → vince ClaudeCode (scelta esplicita sul default)");
                    copilotCliAutoSelect = false;
                    copilotCliAvailable = false;
                    copilotCliDefaultModel = null;
                }

                __perfTotal.Stop();
                logger?.LogWarning("⏱️ [SetFolderProject PERF] TOTAL: {Ms} ms", __perfTotal.ElapsedMilliseconds);

                return Ok(new {
                    id = project.Id,
                    name = project.Name,
                    path = project.Path,
                    sidenavWidth = project.SidenavWidth,
                    gitInitialized = gitInitialized,
                    compatibilityMode = compatibilityMode,
                    isGitRepository = isGitRepository,
                    hasGitAccount = hasGitAccount,
                    needsManualCredentials = needsManualCredentials,
                    remoteUrl = detectedRemoteUrl,
                    detectedProvider = detectedProvider,
                    copilotCliAutoSelect = copilotCliAutoSelect,
                    copilotCliAvailable = copilotCliAvailable,
                    copilotCliDefaultModel = copilotCliDefaultModel,
                    claudeCodeAutoSelect = claudeCodeAutoSelect,
                    claudeCodeAvailable = claudeCodeAvailable,
                    claudeCodeDefaultModel = claudeCodeDefaultModel
                });
            }
            catch (Exception ex)
            {
                // TODO: Consider cleaning up registered database and watcher on error
                throw;
            }
        }

        [HttpPost]
        public IActionResult SetSideNavWidth([FromBody] SetSideNavWidthRequest request)
        {
            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
            var projectDB = projectDal.GetList().Where(_=>_.Id == request.Id).FirstOrDefault();
            if (projectDB != null)
            {
                projectDB.SidenavWidth = request.SidenavWidth;
            }
            _userSettingsDB.Commit();
            return Ok();
        }

        /// <summary>
        /// Closes the current project and deallocates resources (FileSystemWatcher, database contexts).
        /// Called when the user navigates back to the projects list.
        /// </summary>
        [HttpPost]
        public IActionResult CloseProject()
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<MdProjectsController>>();
            var connectionId = Request.Query["ConnectionId"].ToString();

            if (string.IsNullOrEmpty(connectionId))
            {
                logger?.LogWarning("⚠️ CloseProject called without ConnectionId");
                return BadRequest(new { error = "ConnectionId is required" });
            }

            logger?.LogInformation($"📁 Closing project for connection {connectionId}");

            try
            {
                // Unregister FileSystemWatcher for this connection
                if (_fileSystemWatcherManager.HasWatcher(connectionId))
                {
                    _fileSystemWatcherManager.UnregisterWatcher(connectionId);
                    logger?.LogInformation($"✅ FileSystemWatcher unregistered for connection {connectionId}");
                }

                // Unregister database contexts for this connection
                if (_databaseManager.HasConnection(connectionId))
                {
                    _databaseManager.UnregisterConnection(connectionId);
                    logger?.LogInformation($"✅ Database contexts unregistered for connection {connectionId}");
                }

                return Ok(new { message = "Project closed successfully" });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"❌ Error closing project for connection {connectionId}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the remote URL (origin) from a Git repository using git command
        /// </summary>
        private string GetRemoteUrlFromRepository(string repositoryPath)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "config --get remote.origin.url",
                        WorkingDirectory = repositoryPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    return output;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Detects the Git provider type from a remote URL
        /// </summary>
        private string DetectProviderFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "generic";

            var urlLower = url.ToLowerInvariant();

            if (urlLower.Contains("github.com"))
                return "github";
            if (urlLower.Contains("gitlab.com") || urlLower.Contains("gitlab"))
                return "gitlab";
            if (urlLower.Contains("dev.azure.com") || urlLower.Contains("visualstudio.com"))
                return "azure";
            if (urlLower.Contains("bitbucket.org") || urlLower.Contains("bitbucket"))
                return "bitbucket";
            if (urlLower.Contains("scm-manager") || urlLower.Contains("/scm/"))
                return "scm-manager";
            if (urlLower.Contains("gitea") || urlLower.Contains(":3000/"))
                return "gitea";

            return "generic";
        }

        [HttpPost]
        public IActionResult InitializeProjectTemplates([FromBody] FolderPath folderPath)
        {
            try
            {
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();
                logger?.LogInformation($"🔧 [TemplateInit] Initializing templates for: {folderPath.Path}");
                
                // Chiama ConfigTemplates per creare la struttura template
                ProjectsManager.ConfigTemplates(folderPath.Path);
                
                logger?.LogInformation($"✅ [TemplateInit] Templates initialized successfully for: {folderPath.Path}");
                
                return Ok(new { message = "Templates initialized successfully", path = folderPath.Path });
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();
                logger?.LogError($"❌ [TemplateInit] Error initializing templates: {ex.Message}");
                
                return StatusCode(500, new { error = "Failed to initialize templates", details = ex.Message });
            }
        }
    }

    public class FolderPath
    {
        public string Path { get; set; }
    }

    public class ProjectCreationRequest
    {
        public string Path { get; set; }
        public bool? InitializeGit { get; set; }
        public bool? AddCopilotInstructions { get; set; }
    }
}


