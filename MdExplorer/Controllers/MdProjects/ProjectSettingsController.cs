using Ad.Tools.Dal;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace MdExplorer.Service.Controllers.MdProjects
{
    [ApiController]
    [Route("api/ProjectSettings/{action}")]
    public class ProjectSettingsController : ControllerBase
    {
        private readonly IProjectDB _projectDB;
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly ILogger<ProjectSettingsController> _logger;

        public ProjectSettingsController(IProjectDB projectDB, IUserSettingsDB userSettingsDB, ILogger<ProjectSettingsController> logger)
        {
            _projectDB = projectDB;
            _userSettingsDB = userSettingsDB;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetProjectSettings()
        {
            try
            {
                var settingsDal = _projectDB.GetDal<ProjectSetting>();
                var settings = settingsDal.GetList()
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Description,
                        s.ValueString,
                        s.ValueBool,
                        s.ValueInt,
                        s.ValueDateTime,
                        s.ValueDecimal
                    })
                    .ToList();

                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project settings");
                return StatusCode(500, new { error = "Failed to get project settings" });
            }
        }

        [HttpPost]
        public IActionResult SaveProjectSetting([FromBody] SaveProjectSettingRequest request)
        {
            try
            {
                _projectDB.BeginTransaction();

                var settingsDal = _projectDB.GetDal<ProjectSetting>();

                // Check if setting already exists
                var existingSetting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == request.Name);

                if (existingSetting != null)
                {
                    // Update existing setting
                    existingSetting.ValueBool = request.ValueBool;
                    existingSetting.ValueString = request.ValueString;
                    existingSetting.ValueInt = request.ValueInt;
                    existingSetting.ValueDateTime = request.ValueDateTime;
                    existingSetting.ValueDecimal = request.ValueDecimal;
                    existingSetting.Description = request.Description;
                    settingsDal.Save(existingSetting);
                }
                else
                {
                    // Create new setting
                    var newSetting = new ProjectSetting
                    {
                        Name = request.Name,
                        Description = request.Description,
                        ValueBool = request.ValueBool,
                        ValueString = request.ValueString,
                        ValueInt = request.ValueInt,
                        ValueDateTime = request.ValueDateTime,
                        ValueDecimal = request.ValueDecimal
                    };
                    settingsDal.Save(newSetting);
                }

                _projectDB.Commit();
                return Ok(new { message = "Setting saved successfully" });
            }
            catch (Exception ex)
            {
                _projectDB.Rollback();
                _logger.LogError(ex, "Error saving project setting");
                return StatusCode(500, new { error = "Failed to save project setting" });
            }
        }

        [HttpGet]
        public IActionResult GetRule1Setting(Guid projectId)
        {
            try
            {
                var settingsDal = _projectDB.GetDal<ProjectSetting>();
                var rule1Setting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "Rule1_CheckH1MatchesFilename");

                return Ok(new
                {
                    enabled = rule1Setting?.ValueBool ?? false,
                    description = rule1Setting?.Description ?? "Check if H1 title matches filename"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Rule 1 setting for project {ProjectId}", projectId);
                return StatusCode(500, new { error = "Failed to get Rule 1 setting" });
            }
        }

        [HttpPost]
        public IActionResult SetRule1Setting([FromBody] SetRule1Request request)
        {
            var saveRequest = new SaveProjectSettingRequest
            {
                Name = "Rule1_CheckH1MatchesFilename",
                Description = "Check if H1 title matches filename",
                ValueBool = request.Enabled
            };

            return SaveProjectSetting(saveRequest);
        }

        [HttpGet]
        public IActionResult GetStickyScrollSetting()
        {
            try
            {
                var settingsDal = _projectDB.GetDal<ProjectSetting>();
                var setting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "Tree_StickyScrollEnabled");
                return Ok(new { enabled = setting?.ValueBool ?? true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sticky scroll setting");
                return StatusCode(500, new { error = "Failed to get sticky scroll setting" });
            }
        }

        [HttpPost]
        public IActionResult SetStickyScrollSetting([FromBody] SetRule1Request request)
        {
            return SaveProjectSetting(new SaveProjectSettingRequest
            {
                Name = "Tree_StickyScrollEnabled",
                Description = "Enable VS Code-style sticky scroll in the file tree",
                ValueBool = request.Enabled
            });
        }

        [HttpGet]
        public IActionResult GetLinkIndexingSetting([FromQuery] string projectPath)
        {
            try
            {
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == projectPath);

                if (project == null)
                {
                    // Fallback: case-insensitive comparison for path matching
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(new
                {
                    enabled = project?.LinkIndexingEnabled ?? true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LinkIndexing setting");
                return StatusCode(500, new { error = "Failed to get LinkIndexing setting" });
            }
        }

        [HttpPost]
        public IActionResult SetLinkIndexingSetting([FromBody] SetLinkIndexingRequest request)
        {
            try
            {
                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == request.ProjectPath);

                if (project == null)
                {
                    // Fallback: case-insensitive comparison for path matching
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, request.ProjectPath, StringComparison.OrdinalIgnoreCase));
                }

                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning($"[SetLinkIndexingSetting] Project not found for path: '{request.ProjectPath}'");
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation($"[SetLinkIndexingSetting] Setting LinkIndexingEnabled={request.Enabled} for project '{project.Name}' (path: '{project.Path}')");
                project.LinkIndexingEnabled = request.Enabled;
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { message = "LinkIndexing setting saved successfully" });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving LinkIndexing setting");
                return StatusCode(500, new { error = "Failed to save LinkIndexing setting" });
            }
        }

        [HttpGet]
        public IActionResult GetPlantUmlKeepOriginalColorsSetting([FromQuery] string projectPath)
        {
            try
            {
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == projectPath);

                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(new
                {
                    enabled = project?.PlantUmlKeepOriginalColorsInDarkMode ?? false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PlantUmlKeepOriginalColors setting");
                return StatusCode(500, new { error = "Failed to get PlantUmlKeepOriginalColors setting" });
            }
        }

        [HttpPost]
        public IActionResult SetPlantUmlKeepOriginalColorsSetting([FromBody] SetPlantUmlKeepOriginalColorsRequest request)
        {
            try
            {
                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == request.ProjectPath);

                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, request.ProjectPath, StringComparison.OrdinalIgnoreCase));
                }

                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning($"[SetPlantUmlKeepOriginalColorsSetting] Project not found for path: '{request.ProjectPath}'");
                    return NotFound(new { error = "Project not found" });
                }

                project.PlantUmlKeepOriginalColorsInDarkMode = request.Enabled;
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { message = "PlantUmlKeepOriginalColors setting saved successfully" });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving PlantUmlKeepOriginalColors setting");
                return StatusCode(500, new { error = "Failed to save PlantUmlKeepOriginalColors setting" });
            }
        }

        [HttpGet]
        public IActionResult GetCopilotCliAutoSelectSetting([FromQuery] string projectPath)
        {
            try
            {
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == projectPath);

                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(new
                {
                    enabled = project?.UseCopilotCliAsDefault ?? true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CopilotCliAutoSelect setting");
                return StatusCode(500, new { error = "Failed to get CopilotCliAutoSelect setting" });
            }
        }

        [HttpPost]
        public IActionResult SetCopilotCliAutoSelectSetting([FromBody] SetCopilotCliAutoSelectRequest request)
        {
            try
            {
                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == request.ProjectPath);

                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, request.ProjectPath, StringComparison.OrdinalIgnoreCase));
                }

                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning($"[SetCopilotCliAutoSelectSetting] Project not found for path: '{request.ProjectPath}'");
                    return NotFound(new { error = "Project not found" });
                }

                project.UseCopilotCliAsDefault = request.Enabled;
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { message = "CopilotCliAutoSelect setting saved successfully" });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving CopilotCliAutoSelect setting");
                return StatusCode(500, new { error = "Failed to save CopilotCliAutoSelect setting" });
            }
        }

        [HttpGet]
        /// <summary>
        /// Isolamento worktree per-agente: preferenza di QUESTA macchina (UserDB), non del repo.
        /// Se non è mai stata decisa qui, si importa una-tantum l'eventuale valore esplicito del
        /// <c>.development.yml</c> — dove il flag viveva prima — così una scelta già espressa non
        /// viene ignorata in silenzio.
        /// </summary>
        [HttpGet]
        public IActionResult GetAgentWorktreesSetting([FromQuery] string projectPath)
        {
            try
            {
                var pref = HttpContext.RequestServices
                    .GetRequiredService<MdExplorer.Services.AgentRun.IAgentWorktreePreference>();

                // L'import dalla vecchia sede lo fa il servizio: unico punto, stessa verita'
                // per UI e dispatcher.
                var raw = pref.GetRaw(projectPath);

                return Ok(new
                {
                    enabled = raw ?? pref.DefaultFor(projectPath),
                    isExplicit = raw != null,
                    defaultValue = pref.DefaultFor(projectPath),
                    // Posti del pool: quanti agenti possono lavorare insieme su questa macchina.
                    slots = pref.SlotsFor(projectPath),
                    defaultSlots = MdExplorer.Services.AgentRun.AgentWorktreePreference.DefaultSlots,
                    maxSlots = MdExplorer.Services.AgentRun.AgentWorktreePreference.MaxSlots,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAgentWorktreesSetting] fallito");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SetAgentWorktreesSetting([FromBody] SetAgentWorktreesRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath))
                return BadRequest(new { error = "projectPath è obbligatorio" });

            try
            {
                var pref = HttpContext.RequestServices
                    .GetRequiredService<MdExplorer.Services.AgentRun.IAgentWorktreePreference>();
                pref.Set(request.ProjectPath, request.Enabled);
                if (request.Slots != null) pref.SetSlots(request.ProjectPath, request.Slots);
                return Ok(new { enabled = request.Enabled, slots = pref.SlotsFor(request.ProjectPath) });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetAgentWorktreesSetting] fallito");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary><c>Enabled</c> nullable: null = torna al default del progetto.</summary>
        public class SetAgentWorktreesRequest
        {
            public string ProjectPath { get; set; }
            public bool? Enabled { get; set; }

            /// <summary>Posti del pool. <c>null</c> = non toccare (la UI puo' salvare solo il flag).</summary>
            public int? Slots { get; set; }
        }

        public IActionResult GetExcludeSubmodulesSetting([FromQuery] string projectPath)
        {
            try
            {
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == projectPath);

                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
                }

                return Ok(new
                {
                    enabled = project?.ExcludeSubmodulesFromGitStatus ?? true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ExcludeSubmodules setting");
                return StatusCode(500, new { error = "Failed to get ExcludeSubmodules setting" });
            }
        }

        [HttpPost]
        public IActionResult SetExcludeSubmodulesSetting([FromBody] SetExcludeSubmodulesRequest request)
        {
            try
            {
                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList()
                    .FirstOrDefault(p => p.Path == request.ProjectPath);

                if (project == null)
                {
                    project = projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, request.ProjectPath, StringComparison.OrdinalIgnoreCase));
                }

                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning($"[SetExcludeSubmodulesSetting] Project not found for path: '{request.ProjectPath}'");
                    return NotFound(new { error = "Project not found" });
                }

                project.ExcludeSubmodulesFromGitStatus = request.Enabled;
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { message = "ExcludeSubmodules setting saved successfully" });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving ExcludeSubmodules setting");
                return StatusCode(500, new { error = "Failed to save ExcludeSubmodules setting" });
            }
        }

        /// <summary>
        /// Reads the per-project text-index settings: IndexAllTextFiles flag + the
        /// stored allow-list (null when the project uses the central default). Also
        /// returns the central default so the UI can show it as placeholder.
        /// </summary>
        [HttpGet]
        public IActionResult GetTextIndexingSetting([FromQuery] string projectPath)
        {
            try
            {
                _userSettingsDB.Clear();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList().FirstOrDefault(p => p.Path == projectPath)
                    ?? projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));

                return Ok(new
                {
                    enabled = project?.IndexAllTextFiles ?? false,
                    extensions = project?.TextFileExtensions,
                    defaultExtensions = MdExplorer.Abstractions.Services.TextFileClassifier.DefaultExtensionsCsv
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TextIndexing setting");
                return StatusCode(500, new { error = "Failed to get TextIndexing setting" });
            }
        }

        /// <summary>
        /// Persists the text-index flag + allow-list. Blank extensions are stored as
        /// null (project falls back to the central default).
        /// </summary>
        [HttpPost]
        public IActionResult SetTextIndexingSetting([FromBody] SetTextIndexingRequest request)
        {
            try
            {
                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList().FirstOrDefault(p => p.Path == request.ProjectPath)
                    ?? projectDal.GetList().ToList()
                        .FirstOrDefault(p => string.Equals(p.Path, request.ProjectPath, StringComparison.OrdinalIgnoreCase));

                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning($"[SetTextIndexingSetting] Project not found for path: '{request.ProjectPath}'");
                    return NotFound(new { error = "Project not found" });
                }

                project.IndexAllTextFiles = request.Enabled;
                project.TextFileExtensions = string.IsNullOrWhiteSpace(request.Extensions) ? null : request.Extensions.Trim();
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { message = "TextIndexing setting saved successfully" });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving TextIndexing setting");
                return StatusCode(500, new { error = "Failed to save TextIndexing setting" });
            }
        }
    }

    public class SaveProjectSettingRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueString { get; set; }
        public bool? ValueBool { get; set; }
        public int? ValueInt { get; set; }
        public DateTime? ValueDateTime { get; set; }
        public decimal? ValueDecimal { get; set; }
    }

    public class SetRule1Request
    {
        public bool Enabled { get; set; }
    }

    public class SetLinkIndexingRequest
    {
        public bool Enabled { get; set; }
        public string ProjectPath { get; set; }
    }

    public class SetPlantUmlKeepOriginalColorsRequest
    {
        public bool Enabled { get; set; }
        public string ProjectPath { get; set; }
    }

    public class SetCopilotCliAutoSelectRequest
    {
        public bool Enabled { get; set; }
        public string ProjectPath { get; set; }
    }

    public class SetExcludeSubmodulesRequest
    {
        public bool Enabled { get; set; }
        public string ProjectPath { get; set; }
    }

    public class SetTextIndexingRequest
    {
        public bool Enabled { get; set; }
        /// <summary>Comma-separated allow-list; null/blank → central default.</summary>
        public string Extensions { get; set; }
        public string ProjectPath { get; set; }
    }
}