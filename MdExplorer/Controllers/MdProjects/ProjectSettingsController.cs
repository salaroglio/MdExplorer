using Ad.Tools.Dal;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Utilities;
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

        /// <summary>
        /// Il motore di MarkAgent per un progetto, e da dove viene.
        /// <para>
        /// Una risposta sola perché la domanda è una sola: qual è l'ambiente agentico di questo
        /// progetto. <c>linked</c> dice che il motore lo detta l'harness del repository (il caso
        /// normale); <c>harness</c> lo riporta comunque, così la UI non deve fare una seconda
        /// chiamata per disegnare la stessa riga.
        /// </para>
        /// </summary>
        [HttpGet]
        public IActionResult GetMarkAgentEngine([FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectPath))
                    return BadRequest(new { error = "projectPath is required" });

                _userSettingsDB.Clear();
                var project = FindProject(projectPath);

                var harness = MdExplorer.Utilities.MarkAgentEngines.HarnessOf(projectPath);
                var engine = MdExplorer.Utilities.MarkAgentEngines.Resolve(
                    project?.MarkAgentEngine, projectPath, out var linked);

                return Ok(new
                {
                    engine = MdExplorer.Utilities.MarkAgentEngines.IdOf(engine),
                    linked,
                    harness = MdExplorer.Utilities.HarnessSettings.IdOf(harness),
                    declared = MdExplorer.Utilities.HarnessSettings.Read(projectPath).HasValue
                });
            }
            catch (InvalidOperationException ex)
            {
                // Motore o harness scritti a mano con un valore che non esiste: si dice qual e'.
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetMarkAgentEngine] fallito per {ProjectPath}", projectPath);
                return StatusCode(500, new { error = "Failed to read the MarkAgent engine" });
            }
        }

        /// <summary>
        /// Scollega il motore dall'harness, o lo ricollega.
        /// <para>
        /// <c>Engine</c> null o vuoto = <b>collegato</b>: la colonna torna NULL e il motore ridiventa
        /// quello dell'ambiente. Un valore = scelta di QUESTA macchina, che vince sull'harness. Non si
        /// tocca il <c>.development.yml</c>: cambiare l'ambiente e' un'altra chiamata (SetHarness),
        /// perche' quel file e' committato e vale per tutto il team.
        /// </para>
        /// </summary>
        [HttpPost]
        public IActionResult SetMarkAgentEngine([FromBody] SetMarkAgentEngineRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath))
                    return BadRequest(new { error = "projectPath is required" });

                string stored = null;
                if (!string.IsNullOrWhiteSpace(request.Engine))
                {
                    if (!MdExplorer.Utilities.MarkAgentEngines.TryParseId(request.Engine, out var parsed))
                    {
                        return BadRequest(new
                        {
                            error = $"Unknown MarkAgent engine '{request.Engine}'. Allowed values: {MdExplorer.Utilities.MarkAgentEngines.AllowedIds}."
                        });
                    }
                    stored = MdExplorer.Utilities.MarkAgentEngines.IdOf(parsed);
                }

                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var project = FindProject(request.ProjectPath);
                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning("[SetMarkAgentEngine] Project not found for path: '{ProjectPath}'", request.ProjectPath);
                    return NotFound(new { error = "Project not found" });
                }

                project.MarkAgentEngine = stored;
                _userSettingsDB.GetDal<Project>().Save(project);
                _userSettingsDB.Commit();

                var engine = MdExplorer.Utilities.MarkAgentEngines.Resolve(stored, request.ProjectPath, out var linked);
                _logger.LogInformation("[SetMarkAgentEngine] {ProjectPath} → {Engine} (linked={Linked})",
                    request.ProjectPath, MdExplorer.Utilities.MarkAgentEngines.IdOf(engine), linked);

                return Ok(new
                {
                    engine = MdExplorer.Utilities.MarkAgentEngines.IdOf(engine),
                    linked
                });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "[SetMarkAgentEngine] fallito per {ProjectPath}", request?.ProjectPath);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Il progetto con questo percorso. Due passaggi come altrove in questo controller: prima il
        /// confronto che il DB sa fare, poi quello che ignora le maiuscole, perche' su Windows lo
        /// stesso progetto puo' essere stato registrato con un'altra grafia del percorso.
        /// </summary>

        /// <summary>
        /// I gruppi di funzionalità MCP di un progetto: quali esistono, quanto pesano, quali sono accesi.
        /// <para>
        /// L'elenco non è scritto qui: lo dichiara il server MCP (<c>--list-groups</c>), pesi compresi,
        /// così la UI resta allineata da sola quando nasce un tool nuovo. <c>chosen</c> distingue
        /// «l'utente ha scelto» da «proposta in base alle integrazioni configurate».
        /// </para>
        /// </summary>
        [HttpGet]
        public IActionResult GetMcpToolGroups([FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectPath))
                    return BadRequest(new { error = "projectPath is required" });

                _userSettingsDB.Clear();
                var project = FindProject(projectPath);
                if (project == null)
                    return NotFound(new { error = "Project not found" });

                var catalog = McpToolGroupsSettings.Catalog();
                var enabled = McpToolGroupsSettings.Resolve(project, _userSettingsDB, out var chosen);

                return Ok(new
                {
                    chosen,
                    groups = catalog.Select(g => new
                    {
                        id = g.Id,
                        mandatory = g.Mandatory,
                        summary = g.Summary,
                        toolCount = g.Tools.Length,
                        approxTokens = g.ApproxTokens,
                        enabled = enabled.Contains(g.Id)
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                // Il server MCP non c'è o non risponde: si dice, invece di disegnare zero gruppi.
                _logger.LogError(ex, "[GetMcpToolGroups] catalogo non disponibile per {ProjectPath}", projectPath);
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetMcpToolGroups] fallito per {ProjectPath}", projectPath);
                return StatusCode(500, new { error = "Failed to read the MCP tool groups" });
            }
        }

        /// <summary>
        /// Salva i gruppi MCP accesi per un progetto e riscrive subito le configurazioni degli
        /// ambienti agentici, altrimenti la scelta resterebbe nel database senza arrivare a nessuno.
        /// <para>
        /// Vale dalla <b>prossima</b> sessione di chat: i tool di una sessione già aperta sono già
        /// nel suo contesto. La UI lo dice.
        /// </para>
        /// </summary>
        [HttpPost]
        public IActionResult SetMcpToolGroups([FromBody] SetMcpToolGroupsRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath))
                    return BadRequest(new { error = "projectPath is required" });

                var catalog = McpToolGroupsSettings.Catalog();
                var wanted = (request.Groups ?? Array.Empty<string>())
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Select(g => g.Trim().ToLowerInvariant())
                    .Distinct()
                    .ToList();

                var unknown = wanted.Where(w => !catalog.Any(g => g.Id == w)).ToList();
                if (unknown.Count > 0)
                {
                    return BadRequest(new
                    {
                        error = $"Gruppo sconosciuto '{string.Join("', '", unknown)}'. " +
                                $"Gli id validi sono: {string.Join(", ", catalog.Select(g => g.Id))}."
                    });
                }

                // I gruppi obbligatori si salvano comunque: la stringa dice per intero cosa è acceso,
                // così chi la legge non deve conoscere un'altra regola per interpretarla.
                var stored = string.Join(",", catalog
                    .Where(g => g.Mandatory || wanted.Contains(g.Id))
                    .Select(g => g.Id));

                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var project = FindProject(request.ProjectPath);
                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    return NotFound(new { error = "Project not found" });
                }

                project.McpToolGroups = stored;
                _userSettingsDB.GetDal<Project>().Save(project);
                _userSettingsDB.Commit();

                // La scelta deve arrivare dove la leggono gli ambienti agentici, non restare nel DB.
                ProjectsManager.RefreshMcpRegistration(
                    request.ProjectPath,
                    McpToolGroupsSettings.ToArgument(
                        McpToolGroupsSettings.Resolve(project, _userSettingsDB, out _)));

                _logger.LogInformation("[SetMcpToolGroups] {ProjectPath} → {Groups}", request.ProjectPath, stored);
                return Ok(new { groups = stored.Split(','), chosen = true });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "[SetMcpToolGroups] catalogo non disponibile");
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "[SetMcpToolGroups] fallito per {ProjectPath}", request?.ProjectPath);
                return StatusCode(500, new { error = "Failed to save the MCP tool groups" });
            }
        }

        private Project FindProject(string projectPath)
        {
            var projectDal = _userSettingsDB.GetDal<Project>();
            return projectDal.GetList().FirstOrDefault(p => p.Path == projectPath)
                ?? projectDal.GetList().ToList()
                    .FirstOrDefault(p => string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Salva il modello Copilot della chat per un progetto. <c>ModelId</c> null o vuoto =
        /// lo sceglie il CLI.
        /// <para>
        /// Endpoint a sé, con la propria transazione, invece di scrivere dall'hub della chat:
        /// l'hub non possiede una transazione sulla sessione condivisa del DB utente, e una
        /// scrittura fuori transazione rompe il Commit successivo di chiunque.
        /// </para>
        /// </summary>
        [HttpPost]
        public IActionResult SetCopilotChatModelSetting([FromBody] SetCopilotChatModelRequest request)
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
                    _logger.LogWarning("[SetCopilotChatModelSetting] Project not found for path: '{Path}'", request.ProjectPath);
                    return NotFound(new { error = "Project not found" });
                }

                project.CopilotChatModel = string.IsNullOrWhiteSpace(request.ModelId) ? null : request.ModelId.Trim();
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { modelId = project.CopilotChatModel });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving CopilotChatModel setting");
                return StatusCode(500, new { error = "Failed to save CopilotChatModel setting" });
            }
        }

        /// <summary>
        /// Salva il modello di Claude Code per MarkAgent in un progetto: un <c>value</c> dell'elenco che il CLI
        /// dichiara. <c>ModelId</c> null o vuoto = torna a «mai scelto» (la chat usa <c>sonnet</c>). Gemello di
        /// <see cref="SetCopilotChatModelSetting"/>, con la stessa transazione propria.
        /// </summary>
        [HttpPost]
        public IActionResult SetClaudeCodeChatModelSetting([FromBody] SetClaudeCodeChatModelRequest request)
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
                    _logger.LogWarning("[SetClaudeCodeChatModelSetting] Project not found for path: '{Path}'", request.ProjectPath);
                    return NotFound(new { error = "Project not found" });
                }

                project.ClaudeCodeChatModel = string.IsNullOrWhiteSpace(request.ModelId) ? null : request.ModelId.Trim();
                projectDal.Save(project);
                _userSettingsDB.Commit();

                return Ok(new { modelId = project.ClaudeCodeChatModel });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving ClaudeCodeChatModel setting");
                return StatusCode(500, new { error = "Failed to save ClaudeCodeChatModel setting" });
            }
        }

        /// <summary>
        /// Salva il modello di opencode per MarkAgent in un progetto, scritto
        /// <c>provider/modello</c>. <c>ModelId</c> null o vuoto = torna a «mai scelto», e decide
        /// il server. Terzo gemello di <see cref="SetCopilotChatModelSetting"/>, con la stessa
        /// transazione propria.
        /// </summary>
        [HttpPost]
        public IActionResult SetOpenCodeChatModelSetting([FromBody] SetOpenCodeChatModelRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath))
                    return BadRequest(new { error = "projectPath is required" });

                var modelId = string.IsNullOrWhiteSpace(request.ModelId) ? null : request.ModelId.Trim();
                if (modelId != null)
                {
                    // 'provider/modello' e non un nome secco: il provider giusto dipende da come
                    // e' configurato opencode, e indovinarlo qui vorrebbe dire sbagliarlo su
                    // meta' delle installazioni.
                    try { MdExplorer.Features.Services.AI.OpenCode.OpenCodeSession.SplitModel(modelId); }
                    catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
                }

                _userSettingsDB.Clear();
                _userSettingsDB.BeginTransaction();
                var project = FindProject(request.ProjectPath);
                if (project == null)
                {
                    _userSettingsDB.Rollback();
                    _logger.LogWarning("[SetOpenCodeChatModelSetting] Project not found for path: '{Path}'", request.ProjectPath);
                    return NotFound(new { error = "Project not found" });
                }

                project.OpenCodeChatModel = modelId;
                _userSettingsDB.GetDal<Project>().Save(project);
                _userSettingsDB.Commit();

                return Ok(new { modelId = project.OpenCodeChatModel });
            }
            catch (Exception ex)
            {
                _userSettingsDB.Rollback();
                _logger.LogError(ex, "Error saving OpenCodeChatModel setting");
                return StatusCode(500, new { error = "Failed to save OpenCodeChatModel setting" });
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

        /// <summary>
        /// Harness agentico del progetto: dove MdExplorer installa skill, agent e prompt.
        /// Vive in <c>.development.yml</c> e non in UserDB — e' una caratteristica del
        /// repository, condivisa dal team, non una preferenza della macchina.
        /// </summary>
        [HttpGet]
        public IActionResult GetHarness([FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectPath))
                    return BadRequest(new { error = "projectPath is required" });

                // Dichiarato nel yml; se il progetto non e' ancora migrato, quello che dice
                // il disco. Nessuna scrittura: leggere le impostazioni non cambia il progetto.
                var declared = MdExplorer.Utilities.HarnessSettings.Read(projectPath);
                var target = declared ?? MdExplorer.Utilities.HarnessSettings.DetectFromDisk(projectPath);

                return Ok(new
                {
                    target = MdExplorer.Utilities.HarnessSettings.IdOf(target),
                    declared = declared.HasValue
                });
            }
            catch (InvalidOperationException ex)
            {
                // harness.target scritto a mano con un valore che non esiste: si dice qual e'.
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetHarness] fallito per {ProjectPath}", projectPath);
                return StatusCode(500, new { error = "Failed to read the project harness" });
            }
        }

        [HttpPost]
        public IActionResult SetHarness([FromBody] SetHarnessRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath))
                    return BadRequest(new { error = "projectPath is required" });

                if (!MdExplorer.Utilities.HarnessLayout.TryParseId(request.Target, out var target))
                {
                    return BadRequest(new
                    {
                        error = $"Unknown harness '{request.Target}'. Allowed values: {MdExplorer.Utilities.HarnessLayout.AllowedIds}."
                    });
                }

                var services = HttpContext.RequestServices;
                MdExplorer.Service.ProjectsManager.ApplyHarness(services, request.ProjectPath, target);

                _logger.LogInformation("[SetHarness] {ProjectPath} → {Target}", request.ProjectPath, request.Target);
                return Ok(new { target = MdExplorer.Utilities.HarnessSettings.IdOf(target) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetHarness] fallito per {ProjectPath}", request?.ProjectPath);
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

    public class SetCopilotChatModelRequest
    {
        public string ProjectPath { get; set; }

        /// <summary>
        /// <c>string?</c> e non <c>string</c>: in un progetto con Nullable annotations una string
        /// non nullable di un DTO e' un [Required] implicito, e "lascia scegliere il CLI" (null)
        /// verrebbe respinto con un 400 opaco prima di entrare nel metodo.
        /// </summary>
        public string? ModelId { get; set; }
    }

    public class SetClaudeCodeChatModelRequest
    {
        public string ProjectPath { get; set; }

        /// <summary><c>string?</c> per la stessa ragione di <see cref="SetCopilotChatModelRequest.ModelId"/>: null è un valore valido.</summary>
        public string? ModelId { get; set; }
    }

    /// <summary>
    /// <c>Engine</c> nullable per due ragioni: null e' un valore valido (= il motore torna a seguire
    /// l'harness) e, con &lt;Nullable&gt;annotations&lt;/Nullable&gt;, una string non nullable sarebbe un
    /// [Required] implicito che risponderebbe 400 proprio al caso normale.
    /// </summary>
    /// <summary><c>ModelId</c> nullable: null e' un valore valido (= decide il server).</summary>
    public class SetOpenCodeChatModelRequest
    {
        public string? ProjectPath { get; set; }
        public string? ModelId { get; set; }
    }

    public class SetMarkAgentEngineRequest
    {
        public string? ProjectPath { get; set; }

        /// <summary><c>copilot</c>, <c>opencode</c>, <c>claude</c>, <c>none</c>, oppure null = collegato all'harness.</summary>
        public string? Engine { get; set; }
    }

    public class SetTextIndexingRequest
    {
        public bool Enabled { get; set; }
        /// <summary>Comma-separated allow-list; null/blank → central default.</summary>
        public string Extensions { get; set; }
        public string ProjectPath { get; set; }
    }

    /// <summary>
    /// Nullable per scelta: con &lt;Nullable&gt;annotations&lt;/Nullable&gt; una string non nullable
    /// e' un [Required] implicito, e un campo mancante darebbe un 400 opaco invece del messaggio
    /// esplicito che l'endpoint sa produrre.
    /// </summary>
    public class SetMcpToolGroupsRequest
    {
        public string? ProjectPath { get; set; }

        /// <summary>
        /// Gli id dei gruppi accesi (<c>jira</c>, <c>confluence</c>, <c>kg</c>, <c>agents</c>,
        /// <c>plantuml</c>…). I gruppi obbligatori si aggiungono da soli.
        /// </summary>
        public string[]? Groups { get; set; }
    }

    public class SetHarnessRequest
    {
        public string? ProjectPath { get; set; }

        /// <summary><c>copilot</c>, <c>opencode</c> oppure <c>none</c>.</summary>
        public string? Target { get; set; }
    }
}