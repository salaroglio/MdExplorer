using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.IO;
using System.Linq;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/ideConfiguration")]
    public class IdeConfigurationController : ControllerBase
    {
        private readonly ILogger<IdeConfigurationController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IUserSettingsDB _session;

        public IdeConfigurationController(
            ILogger<IdeConfigurationController> logger,
            FileSystemWatcher fileSystemWatcher,
            IUserSettingsDB session)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            _session = session;
        }

        /// <summary>
        /// Get current IDE configuration from Project database
        /// </summary>
        /// <param name="projectPath">Optional project path. If not provided, uses current FileSystemWatcher path.</param>
        [HttpGet("config")]
        public IActionResult GetIdeConfiguration([FromQuery] string projectPath = null)
        {
            try
            {
                var targetPath = string.IsNullOrEmpty(projectPath) ? _fileSystemWatcher.Path : projectPath;

                _logger.LogInformation($"GetIdeConfiguration - Reading for project path: {targetPath}");

                // Get project from database
                var projectDal = _session.GetDal<Project>();
                var project = projectDal.GetList().FirstOrDefault(p => p.Path == targetPath);

                string selectedIde = "vscode"; // Default

                if (project != null && !string.IsNullOrWhiteSpace(project.SelectedIde))
                {
                    selectedIde = project.SelectedIde;
                    _logger.LogInformation($"GetIdeConfiguration - Found project with SelectedIde: {selectedIde}");
                }
                else
                {
                    _logger.LogInformation($"GetIdeConfiguration - No project found or SelectedIde is null, using default: {selectedIde}");
                }

                // Get IDE paths from database
                var settingsDal = _session.GetDal<Setting>();
                var vscodePathSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "EditorPath");
                var intellijPathSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "IntelliJPath");

                return Ok(new
                {
                    selectedIde = selectedIde,
                    vscodePath = vscodePathSetting?.ValueString ?? "",
                    intellijPath = intellijPathSetting?.ValueString ?? ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting IDE configuration");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update IDE configuration in Project database
        /// </summary>
        [HttpPost("config")]
        public IActionResult SetIdeConfiguration([FromBody] SetIdeConfigurationRequest request)
        {
            try
            {
                var targetPath = string.IsNullOrEmpty(request.ProjectPath) ? _fileSystemWatcher.Path : request.ProjectPath;

                _logger.LogInformation($"SetIdeConfiguration - Saving for project path: {targetPath}, SelectedIde: {request.SelectedIde}");

                // Get project from database
                var projectDal = _session.GetDal<Project>();
                var project = projectDal.GetList().FirstOrDefault(p => p.Path == targetPath);

                if (project == null)
                {
                    _logger.LogWarning($"SetIdeConfiguration - Project not found for path: {targetPath}");
                    return NotFound(new { error = "Project not found" });
                }

                // Update SelectedIde
                _session.BeginTransaction();
                project.SelectedIde = request.SelectedIde;
                projectDal.Save(project);
                _session.Commit();

                _logger.LogInformation($"SetIdeConfiguration - Successfully saved SelectedIde: {request.SelectedIde}");

                return Ok(new { message = "IDE configuration updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting IDE configuration");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for setting IDE configuration
    /// </summary>
    public class SetIdeConfigurationRequest
    {
        /// <summary>
        /// Selected IDE: "vscode" or "intellij"
        /// </summary>
        public string SelectedIde { get; set; }

        /// <summary>
        /// Optional project path. If not provided, uses current FileSystemWatcher path.
        /// </summary>
        public string ProjectPath { get; set; }
    }
}
