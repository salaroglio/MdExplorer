using MdExplorer.Abstractions.Models;
using MdExplorer.Services.DatabaseManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
                return new MdeAppsConfig { Version = "1", Apps = new List<MdeAppDefinition>() };

            var json = System.IO.File.ReadAllText(mdeAppsPath);
            return JsonSerializer.Deserialize<MdeAppsConfig>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new MdeAppsConfig { Version = "1", Apps = new List<MdeAppDefinition>() };
        }

        private void WriteConfig(string mdeAppsPath, MdeAppsConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
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
        public IActionResult AddApp([FromBody] MdeAppDefinition app)
        {
            if (app == null || string.IsNullOrWhiteSpace(app.Id))
                return BadRequest(new { error = "App id is required." });

            var projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Ensure ConnectionId is provided." });

            try
            {
                var mdeAppsPath = GetMdeAppsPath(projectPath);
                var config = ReadConfig(mdeAppsPath);

                // Remove existing entry with same id (upsert)
                config.Apps.RemoveAll(a => a.Id == app.Id);
                config.Apps.Add(app);

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
        public IActionResult DeleteApp(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "App id is required." });

            var projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return BadRequest(new { error = "Project path not available. Ensure ConnectionId is provided." });

            try
            {
                var mdeAppsPath = GetMdeAppsPath(projectPath);
                var config = ReadConfig(mdeAppsPath);

                var removed = config.Apps.RemoveAll(a => a.Id == id);
                if (removed == 0)
                    return NotFound(new { error = $"App '{id}' not found." });

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
    }
}
