using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiPreferencesController : ControllerBase
    {
        private readonly IUserSettingsDB _session;
        private readonly ILogger<AiPreferencesController> _logger;

        private const string DefaultProviderKey = "AI_DefaultProvider";
        private const string DefaultModelKey = "AI_DefaultModel";

        public AiPreferencesController(
            IUserSettingsDB session,
            ILogger<AiPreferencesController> logger)
        {
            _session = session;
            _logger = logger;
        }

        [HttpGet("default")]
        public IActionResult GetDefaultPreferences()
        {
            try
            {
                var settingsDal = _session.GetDal<Setting>();
                var settings = settingsDal.GetList();

                var defaultProvider = settings.FirstOrDefault(_ => _.Name == DefaultProviderKey);
                var defaultModel = settings.FirstOrDefault(_ => _.Name == DefaultModelKey);

                return Ok(new
                {
                    provider = defaultProvider?.ValueString,
                    model = defaultModel?.ValueString,
                    hasDefault = defaultProvider != null && defaultModel != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI default preferences");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("default")]
        public IActionResult SaveDefaultPreferences([FromBody] AiPreferenceRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Provider) || string.IsNullOrEmpty(request?.Model))
                {
                    return BadRequest(new { error = "Provider and Model are required" });
                }

                var settingsDal = _session.GetDal<Setting>();
                var settings = settingsDal.GetList();

                _session.BeginTransaction(System.Data.IsolationLevel.Unspecified);

                // Save or update provider
                var providerSetting = settings.FirstOrDefault(_ => _.Name == DefaultProviderKey);
                if (providerSetting == null)
                {
                    providerSetting = new Setting
                    {
                        Id = Guid.NewGuid(),
                        Name = DefaultProviderKey,
                        Description = "Default AI Provider (Local, OpenAI, Gemini, etc.)"
                    };
                }
                providerSetting.ValueString = request.Provider;
                settingsDal.Save(providerSetting);

                // Save or update model
                var modelSetting = settings.FirstOrDefault(_ => _.Name == DefaultModelKey);
                if (modelSetting == null)
                {
                    modelSetting = new Setting
                    {
                        Id = Guid.NewGuid(),
                        Name = DefaultModelKey,
                        Description = "Default AI Model ID"
                    };
                }
                modelSetting.ValueString = request.Model;
                settingsDal.Save(modelSetting);

                _session.Commit();

                _logger.LogInformation("AI preferences saved: Provider={Provider}, Model={Model}",
                    request.Provider, request.Model);

                return Ok(new
                {
                    success = true,
                    message = "AI preferences saved successfully",
                    provider = request.Provider,
                    model = request.Model
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving AI preferences");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("default")]
        public IActionResult ClearDefaultPreferences()
        {
            try
            {
                var settingsDal = _session.GetDal<Setting>();
                var settings = settingsDal.GetList();

                _session.BeginTransaction(System.Data.IsolationLevel.Unspecified);

                var providerSetting = settings.FirstOrDefault(_ => _.Name == DefaultProviderKey);
                if (providerSetting != null)
                {
                    settingsDal.Delete(providerSetting);
                }

                var modelSetting = settings.FirstOrDefault(_ => _.Name == DefaultModelKey);
                if (modelSetting != null)
                {
                    settingsDal.Delete(modelSetting);
                }

                _session.Commit();

                _logger.LogInformation("AI preferences cleared");

                return Ok(new
                {
                    success = true,
                    message = "AI preferences cleared successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing AI preferences");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class AiPreferenceRequest
    {
        public string Provider { get; set; }
        public string Model { get; set; }
    }
}
