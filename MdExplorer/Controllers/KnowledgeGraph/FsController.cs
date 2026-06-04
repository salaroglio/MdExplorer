using System;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Service.Controllers.KnowledgeGraph
{
    /// <summary>
    /// Settings + connection management endpoints per l'integrazione Apache Jena Fuseki.
    /// Specchio di <see cref="KgController"/> ma per il triplestore RDF.
    /// </summary>
    [Route("api/fs")]
    [ApiController]
    public class FsController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IPasswordProtector _passwordProtector;
        private readonly IFusekiClient _fusekiClient;
        private readonly ILogger<FsController> _logger;

        private const string PasswordMask = "********";

        public FsController(
            IUserSettingsDB userSettingsDB,
            IPasswordProtector passwordProtector,
            IFusekiClient fusekiClient,
            ILogger<FsController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _passwordProtector = passwordProtector;
            _fusekiClient = fusekiClient;
            _logger = logger;
        }

        // ============================================================
        //   GET /api/fs/settings/{projectId}
        // ============================================================
        [HttpGet("settings/{projectId}")]
        public IActionResult GetSettings(Guid projectId)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return NotFound(new { error = $"Project {projectId} not found" });
                }
                var settings = _userSettingsDB.GetDal<ProjectFusekiSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == projectId);
                _userSettingsDB.Commit();

                // Se settings non esiste ancora, proponi default sanitizzato dal nome progetto
                var defaultDataset = _fusekiClient.SanitizeDatasetName(project.Name);

                return Ok(new
                {
                    projectId,
                    enabled = settings?.Enabled ?? false,
                    uri = settings?.Uri ?? "http://localhost:3030",
                    dataset = string.IsNullOrWhiteSpace(settings?.Dataset) ? defaultDataset : settings.Dataset,
                    username = settings?.Username ?? "",
                    hasPassword = !string.IsNullOrEmpty(settings?.PasswordEncrypted),
                    syncOnTocGeneration = settings?.SyncOnTocGeneration ?? true,
                    syncOnKgFileSave = settings?.SyncOnKgFileSave ?? true,
                    lastTestedAt = settings?.LastTestedAt,
                    lastTestSuccess = settings?.LastTestSuccess,
                    defaultDataset
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FsController] GetSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class FusekiSettingsRequest
        {
            public bool Enabled { get; set; }
            public string Uri { get; set; }
            public string Dataset { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }   // plaintext; empty/null = keep existing
            public bool SyncOnTocGeneration { get; set; } = true;
            public bool SyncOnKgFileSave { get; set; } = true;
        }

        // ============================================================
        //   PUT /api/fs/settings/{projectId}
        // ============================================================
        [HttpPut("settings/{projectId}")]
        public IActionResult SaveSettings(Guid projectId, [FromBody] FusekiSettingsRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return NotFound(new { error = $"Project {projectId} not found" });
                }

                var settingsDal = _userSettingsDB.GetDal<ProjectFusekiSettings>();
                var settings = settingsDal.GetList().FirstOrDefault(s => s.Project.Id == projectId);
                bool isNew = settings == null;
                if (isNew)
                {
                    settings = new ProjectFusekiSettings { Project = project };
                }

                settings.Enabled = req.Enabled;
                settings.Uri = string.IsNullOrWhiteSpace(req.Uri) ? settings.Uri : req.Uri.Trim();

                // Dataset: applica sanitizzazione lato server come safety net.
                // Se l'utente non l'ha specificato, default dal nome progetto.
                var datasetRaw = string.IsNullOrWhiteSpace(req.Dataset)
                    ? project.Name
                    : req.Dataset.Trim();
                settings.Dataset = _fusekiClient.SanitizeDatasetName(datasetRaw);

                settings.Username = (req.Username ?? string.Empty).Trim();
                settings.SyncOnTocGeneration = req.SyncOnTocGeneration;
                settings.SyncOnKgFileSave = req.SyncOnKgFileSave;

                if (!string.IsNullOrEmpty(req.Password) && req.Password != PasswordMask)
                {
                    settings.PasswordEncrypted = _passwordProtector.Protect(req.Password);
                }

                settingsDal.Save(settings);
                var projectPath = project.Path;
                _userSettingsDB.Commit();

                // Now that the project is configured for Fuseki, deploy the Fuseki/Jena
                // skills (TBox/ABox/SHACL) into .github immediately — without waiting for
                // the next project open. Gated on Enabled; never removes on disable.
                if (req.Enabled && !string.IsNullOrWhiteSpace(projectPath))
                {
                    try { MdeSkillUpdater.EnsureAllSkillsInstalled(projectPath, fusekiEnabled: true); }
                    catch (Exception skEx) { _logger.LogWarning(skEx, "[FsController] Fuseki skill install failed for {ProjectId}", projectId); }
                }

                return Ok(new { ok = true, dataset = settings.Dataset });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FsController] SaveSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class TestConnectionRequest
        {
            public Guid? ProjectId { get; set; }
            public string Uri { get; set; }
            public string Dataset { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool AutoCreateDataset { get; set; } = false;
        }

        // ============================================================
        //   POST /api/fs/test-connection
        // ============================================================
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Uri))
                return BadRequest(new { error = "uri required" });

            string passwordPlain = req.Password;
            if (req.ProjectId.HasValue && (string.IsNullOrEmpty(passwordPlain) || passwordPlain == PasswordMask))
            {
                _userSettingsDB.BeginTransaction();
                var stored = _userSettingsDB.GetDal<ProjectFusekiSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == req.ProjectId.Value);
                _userSettingsDB.Commit();
                if (stored != null && !string.IsNullOrEmpty(stored.PasswordEncrypted))
                    passwordPlain = _passwordProtector.Unprotect(stored.PasswordEncrypted);
            }

            var dataset = string.IsNullOrWhiteSpace(req.Dataset) ? null : _fusekiClient.SanitizeDatasetName(req.Dataset);
            var result = await _fusekiClient.TestAsync(req.Uri, dataset, req.Username ?? string.Empty, passwordPlain ?? string.Empty);

            // Auto-create dataset se richiesto e mancante
            bool datasetCreated = false;
            if (req.AutoCreateDataset && result.ServerReachable && !result.DatasetExists && !string.IsNullOrWhiteSpace(dataset))
            {
                datasetCreated = await _fusekiClient.EnsureDatasetAsync(req.Uri, dataset, req.Username ?? string.Empty, passwordPlain ?? string.Empty);
                if (datasetCreated)
                {
                    // Ri-verifica dopo creazione
                    result = await _fusekiClient.TestAsync(req.Uri, dataset, req.Username ?? string.Empty, passwordPlain ?? string.Empty);
                }
            }

            // Persisti LastTested* sul progetto
            if (req.ProjectId.HasValue)
            {
                try
                {
                    _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<ProjectFusekiSettings>();
                    var stored = dal.GetList().FirstOrDefault(s => s.Project.Id == req.ProjectId.Value);
                    if (stored != null)
                    {
                        stored.LastTestedAt = DateTime.UtcNow;
                        stored.LastTestSuccess = result.Success;
                        dal.Save(stored);
                    }
                    _userSettingsDB.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[FsController] could not persist LastTested* for {ProjectId}", req.ProjectId.Value);
                }
            }

            return Ok(new
            {
                success = result.Success,
                serverReachable = result.ServerReachable,
                datasetExists = result.DatasetExists,
                datasetCreated,
                dataset,
                error = result.Error,
                latencyMs = result.LatencyMs
            });
        }

        public class EnsureDatasetRequest
        {
            public Guid ProjectId { get; set; }
        }

        // ============================================================
        //   POST /api/fs/ensure-dataset
        //   Endpoint di convenienza: usa settings persistite per creare il dataset.
        // ============================================================
        [HttpPost("ensure-dataset")]
        public async Task<IActionResult> EnsureDataset([FromBody] EnsureDatasetRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });

            _userSettingsDB.BeginTransaction();
            var settings = _userSettingsDB.GetDal<ProjectFusekiSettings>().GetList()
                .FirstOrDefault(s => s.Project.Id == req.ProjectId);
            _userSettingsDB.Commit();

            if (settings == null || !settings.Enabled)
                return BadRequest(new { error = "Fuseki is disabled for this project" });
            if (string.IsNullOrWhiteSpace(settings.Dataset))
                return BadRequest(new { error = "Dataset name not configured" });

            var password = string.IsNullOrEmpty(settings.PasswordEncrypted)
                ? string.Empty
                : _passwordProtector.Unprotect(settings.PasswordEncrypted);

            var ok = await _fusekiClient.EnsureDatasetAsync(settings.Uri, settings.Dataset, settings.Username, password);
            return Ok(new { ok, dataset = settings.Dataset });
        }
    }
}
