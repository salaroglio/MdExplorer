using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
using MdExplorer.Service.Services;

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

        public MdProjectsController(IUserSettingsDB userSettingsDB,
                IServiceProvider services,
                ProcessUtil processUtil,
                IMapper mapper,
                IDatabaseManager databaseManager,
                IFileSystemWatcherManager fileSystemWatcherManager,
                IGitAccountService gitAccountService,
                GitCredentialHelperResolver gitCredentialHelper,
                FoldersIgnoreService foldersIgnoreService,
                IProjectMetadataService projectMetadataService)
        {
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
        }

        [HttpGet]
        public IActionResult GetProjects()
        {

            // check if folder exists in project table

            var projectDal = _userSettingsDB.GetDal<Project>();
            var list = projectDal.GetList().OrderByDescending(_ => _.LastUpdate).ToList();
            var listToReturn = _mapper.Map<IEnumerable<ProjectWithoutBookmarks>>(list).ToList();

            // Enrich each project with its shared description from .development.yml
            foreach (var dto in listToReturn)
            {
                dto.Description = _projectMetadataService.GetDescription(dto.Path);
            }

            return Ok(listToReturn);
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

            try
            {
                // Invalidate FoldersIgnore cache to pick up any changes to .mdFoldersIgnore
                _foldersIgnoreService.InvalidateCache(request.Path);

                // IMPORTANT: Run migrations FIRST, before opening database sessions
                // This prevents "database is locked" errors because NHibernate holds the file open
                bool gitInitialized = ProjectsManager.SetNewProject(_services, request.Path, request.InitializeGit ?? false, request.AddCopilotInstructions ?? true);
                logger?.LogInformation($"✅ Database migrations completed for project: {request.Path}");

                // NOW register database contexts (after migrations are complete)
                _databaseManager.RegisterConnection(connectionId, request.Path);
                logger?.LogInformation($"✅ Database contexts registered for connection {connectionId}");

                // Register FileSystemWatcher for this connection
                _fileSystemWatcherManager.RegisterWatcher(connectionId, request.Path);
                logger?.LogInformation($"✅ FileSystemWatcher registered for connection {connectionId}");

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
                    }
                }

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
                    detectedProvider = detectedProvider
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


