using Ad.Tools.Dal;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<ProjectSettingsController> _logger;

        public ProjectSettingsController(IProjectDB projectDB, ILogger<ProjectSettingsController> logger)
        {
            _projectDB = projectDB;
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
}