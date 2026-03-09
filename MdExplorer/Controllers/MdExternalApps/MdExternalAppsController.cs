using MdExplorer.Abstractions.Models;
using MdExplorer.Services.DatabaseManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MdExplorer.Controllers.MdExternalApps
{
    [ApiController]
    [Route("api/MdExternalApps")]
    public class MdExternalAppsController : ControllerBase
    {
        private readonly IDatabaseManager _databaseManager;
        private readonly ILogger<MdExternalAppsController> _logger;

        public MdExternalAppsController(IDatabaseManager databaseManager, ILogger<MdExternalAppsController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        private string GetProjectPath()
        {
            var connectionId = Request.Query["ConnectionId"].ToString();
            if (string.IsNullOrEmpty(connectionId)) return string.Empty;
            try
            {
                return _databaseManager.GetContext(connectionId)?.ProjectPath ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MdExternalApps] Failed to get project path for connectionId {ConnectionId}", connectionId);
                return string.Empty;
            }
        }

        private string GetMdeAppsPath(string projectPath) =>
            Path.Combine(projectPath, ".mdeapps.json");

        private MdeAppsConfig ReadConfig(string mdeAppsPath)
        {
            if (!System.IO.File.Exists(mdeAppsPath))
                return new MdeAppsConfig { Version = "2", Apps = new List<MdeAppDefinition>(), Tree = new List<MdeTreeNode>() };

            var json = System.IO.File.ReadAllText(mdeAppsPath);
            var config = JsonSerializer.Deserialize<MdeAppsConfig>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new MdeAppsConfig { Version = "2", Apps = new List<MdeAppDefinition>(), Tree = new List<MdeTreeNode>() };

            // Migrate v1 → v2: build tree from flat apps list
            if (config.Version != "2" || config.Tree == null)
            {
                config.Tree = new List<MdeTreeNode>();
                foreach (var app in config.Apps)
                {
                    if (!string.IsNullOrWhiteSpace(app.Id))
                        config.Tree.Add(new MdeTreeNode { Type = "app", AppId = app.Id });
                }
                config.Version = "2";
                WriteConfig(mdeAppsPath, config);
                _logger.LogInformation("[MdExternalApps] Migrated .mdeapps.json from v1 to v2");
            }

            return config;
        }

        private static readonly JsonSerializerOptions _writeOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private void WriteConfig(string mdeAppsPath, MdeAppsConfig config)
        {
            // Pulisci nodi tree orfani (appId non presente in apps)
            if (config.Tree != null && config.Apps != null)
            {
                var validIds = new HashSet<string>(config.Apps.Where(a => !string.IsNullOrWhiteSpace(a.Id)).Select(a => a.Id));
                config.Tree.RemoveAll(n => n.Type == "app" && !string.IsNullOrWhiteSpace(n.AppId) && !validIds.Contains(n.AppId));
                foreach (var cat in config.Tree.Where(n => n.Type == "category" && n.Children != null))
                {
                    cat.Children.RemoveAll(c => c.Type == "app" && !string.IsNullOrWhiteSpace(c.AppId) && !validIds.Contains(c.AppId));
                }
            }

            var json = JsonSerializer.Serialize(config, _writeOptions);
            System.IO.File.WriteAllText(mdeAppsPath, json);
        }

        /// <summary>
        /// GET /api/MdExternalApps — returns the list of external apps for the current project
        /// </summary>
        [HttpGet]
        public IActionResult GetApps()
        {
            var projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Ensure ConnectionId is provided." });

            try
            {
                var config = ReadConfig(GetMdeAppsPath(projectPath));
                return Ok(config.Apps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExternalApps] Error reading .mdeapps.json");
                return StatusCode(500, new { error = "Failed to read external apps configuration." });
            }
        }

        /// <summary>
        /// POST /api/MdExternalApps/Add — adds or updates an external app in .mdeapps.json
        /// </summary>
        [HttpPost("Add")]
        public IActionResult AddApp([FromBody] MdeAppDefinition app, [FromQuery] string projectPath = null)
        {
            if (app == null || string.IsNullOrWhiteSpace(app.Id))
                return BadRequest(new { error = "App id is required." });

            if (string.IsNullOrEmpty(projectPath))
                projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Provide projectPath or ConnectionId." });

            try
            {
                var mdeAppsPath = GetMdeAppsPath(projectPath);
                var config = ReadConfig(mdeAppsPath);

                // Remove existing entry with same id (upsert)
                config.Apps.RemoveAll(a => a.Id == app.Id);
                config.Apps.Add(app);

                // Add to tree if not already present
                if (config.Tree != null && !TreeContainsApp(config.Tree, app.Id))
                {
                    config.Tree.Add(new MdeTreeNode { Type = "app", AppId = app.Id });
                }

                WriteConfig(mdeAppsPath, config);
                _logger.LogInformation("[MdExternalApps] App '{AppId}' saved to .mdeapps.json", app.Id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExternalApps] Error writing .mdeapps.json");
                return StatusCode(500, new { error = "Failed to save external app configuration." });
            }
        }

        /// <summary>
        /// DELETE /api/MdExternalApps/{id} — removes an external app from .mdeapps.json
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteApp(string id, [FromQuery] string projectPath = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "App id is required." });

            if (string.IsNullOrEmpty(projectPath))
                projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Provide projectPath or ConnectionId." });

            try
            {
                var mdeAppsPath = GetMdeAppsPath(projectPath);
                var config = ReadConfig(mdeAppsPath);

                var removed = config.Apps.RemoveAll(a => a.Id == id);
                if (removed == 0)
                    return NotFound(new { error = $"App '{id}' not found." });

                // Remove from tree (root level and inside categories)
                if (config.Tree != null)
                    RemoveAppFromTree(config.Tree, id);

                WriteConfig(mdeAppsPath, config);
                _logger.LogInformation("[MdExternalApps] App '{AppId}' removed from .mdeapps.json", id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExternalApps] Error writing .mdeapps.json");
                return StatusCode(500, new { error = "Failed to delete external app configuration." });
            }
        }

        /// <summary>
        /// GET /api/MdExternalApps/config — returns the full MdeAppsConfig (apps + tree)
        /// Accepts projectPath as query param; falls back to ConnectionId lookup.
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetConfig([FromQuery] string projectPath = null)
        {
            if (string.IsNullOrEmpty(projectPath))
                projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Provide projectPath or ConnectionId." });

            try
            {
                var config = ReadConfig(GetMdeAppsPath(projectPath));
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExternalApps] Error reading .mdeapps.json config");
                return StatusCode(500, new { error = "Failed to read external apps configuration." });
            }
        }

        /// <summary>
        /// PUT /api/MdExternalApps/config — saves the entire config (apps + tree) at once.
        /// Accepts projectPath as query param; falls back to ConnectionId lookup.
        /// </summary>
        [HttpPut("config")]
        public IActionResult SaveConfig([FromBody] MdeAppsConfig config, [FromQuery] string projectPath = null)
        {
            if (config == null)
                return BadRequest(new { error = "Config is required." });

            if (string.IsNullOrEmpty(projectPath))
                projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Provide projectPath or ConnectionId." });

            try
            {
                var mdeAppsPath = GetMdeAppsPath(projectPath);
                config.Version = "2";
                config.Apps ??= new List<MdeAppDefinition>();
                config.Tree ??= new List<MdeTreeNode>();
                WriteConfig(mdeAppsPath, config);
                _logger.LogInformation("[MdExternalApps] Full config saved ({AppCount} apps, {TreeCount} root nodes)", config.Apps.Count, config.Tree.Count);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExternalApps] Error saving config");
                return StatusCode(500, new { error = "Failed to save config." });
            }
        }

        /// <summary>
        /// PUT /api/MdExternalApps/tree — saves the entire tree array (frontend manages tree in memory)
        /// Accepts projectPath as query param; falls back to ConnectionId lookup.
        /// </summary>
        [HttpPut("tree")]
        public IActionResult SaveTree([FromBody] List<MdeTreeNode> tree, [FromQuery] string projectPath = null)
        {
            if (tree == null)
                return BadRequest(new { error = "Tree array is required." });

            if (string.IsNullOrEmpty(projectPath))
                projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Provide projectPath or ConnectionId." });

            try
            {
                var mdeAppsPath = GetMdeAppsPath(projectPath);
                var config = ReadConfig(mdeAppsPath);
                config.Tree = tree;
                WriteConfig(mdeAppsPath, config);
                _logger.LogInformation("[MdExternalApps] Tree saved ({Count} root nodes)", tree.Count);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdExternalApps] Error saving tree");
                return StatusCode(500, new { error = "Failed to save tree." });
            }
        }

        // ── Helpers ──────────────────────────────────

        private static bool TreeContainsApp(List<MdeTreeNode> nodes, string appId)
        {
            foreach (var node in nodes)
            {
                if (node.Type == "app" && node.AppId == appId)
                    return true;
                if (node.Children != null && TreeContainsApp(node.Children, appId))
                    return true;
            }
            return false;
        }

        private static void RemoveAppFromTree(List<MdeTreeNode> nodes, string appId)
        {
            nodes.RemoveAll(n => n.Type == "app" && n.AppId == appId);
            foreach (var node in nodes)
            {
                if (node.Children != null)
                    RemoveAppFromTree(node.Children, appId);
            }
        }
    }
}
