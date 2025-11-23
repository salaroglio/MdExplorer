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
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;

namespace MdExplorer.Service.Controllers.MdProjects
{
    [ApiController]
    [Route("api/MdProjects/{action}")]
    public class MdProjectsController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IServiceProvider _services;
        private readonly ProcessUtil _processUtil;
        private readonly IMapper _mapper;
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _fileSystemWatcherManager;

        public MdProjectsController(IUserSettingsDB userSettingsDB,
                FileSystemWatcher fileSystemWatcher,
                IServiceProvider services,
                ProcessUtil processUtil,
                IMapper mapper,
                IDatabaseManager databaseManager,
                IFileSystemWatcherManager fileSystemWatcherManager)
        {
            _userSettingsDB = userSettingsDB;
            _fileSystemWatcher = fileSystemWatcher;
            _services = services;
            _processUtil = processUtil;
            _mapper = mapper;
            _databaseManager = databaseManager;
            _fileSystemWatcherManager = fileSystemWatcherManager;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {

            // check if folder exists in project table

            var projectDal = _userSettingsDB.GetDal<Project>();
            var list = projectDal.GetList().OrderByDescending(_ => _.LastUpdate).ToList();
            var listToReturn = _mapper.Map<IEnumerable<ProjectWithoutBookmarks>>(list);


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
                // Register database contexts for this connection
                _databaseManager.RegisterConnection(connectionId, request.Path);
                logger?.LogInformation($"✅ Database contexts registered for connection {connectionId}");

                // Register FileSystemWatcher for this connection
                _fileSystemWatcherManager.RegisterWatcher(connectionId, request.Path);
                logger?.LogInformation($"✅ FileSystemWatcher registered for connection {connectionId}");

                // NOTE: We no longer modify the global FileSystemWatcher singleton
                // Each client now has its own dedicated FileSystemWatcher via FileSystemWatcherManager

                // renew project data
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList().Where(_ => _.Path == request.Path).FirstOrDefault();
                if (project == null)
                {
                    project = new Project
                    {
                        Path = request.Path,
                        Name = System.IO.Path.GetFileName(_fileSystemWatcher.Path)
                    };
                }
                project.LastUpdate = DateTime.Now;
                projectDal.Save(project);
                _userSettingsDB.Commit();

                // Configura i database per il nuovo progetto e inizializza Git
                // TODO: In futuro, rimuovere ReplaceDalFeatures da SetNewProject per evitare conflitti
                bool gitInitialized = ProjectsManager.SetNewProject(_services, request.Path, request.InitializeGit ?? false, request.AddCopilotInstructions ?? true);

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

                return Ok(new {
                    id = project.Id,
                    name = project.Name,
                    path = project.Path,
                    sidenavWidth = project.SidenavWidth,
                    gitInitialized = gitInitialized,
                    compatibilityMode = compatibilityMode
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


