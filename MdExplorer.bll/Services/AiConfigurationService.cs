using System;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services
{
    public interface IAiConfigurationService
    {
        Task<string> GetSystemPromptAsync(string modelId);
        Task SaveSystemPromptAsync(string modelId, string systemPrompt);
        Task<string> GetDefaultSystemPromptAsync();
        Task SaveDefaultSystemPromptAsync(string systemPrompt);
    }

    public class AiConfigurationService : IAiConfigurationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AiConfigurationService> _logger;
        private const string SystemPromptPrefix = "AI_SystemPrompt_";
        private const string DefaultSystemPromptKey = "AI_DefaultSystemPrompt";

        public AiConfigurationService(IServiceProvider serviceProvider, ILogger<AiConfigurationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<string> GetSystemPromptAsync(string modelId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                        var settingsDal = session.GetDal<Setting>();
                        var settingName = $"{SystemPromptPrefix}{modelId}";
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == settingName);
                        
                        if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                        {
                            _logger.LogInformation($"Found custom system prompt for model {modelId}");
                            return setting.ValueString;
                        }
                        
                        // If no custom prompt, return the default
                        return GetDefaultSystemPromptAsync().Result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting system prompt for model {modelId}");
                    return GetDefaultSystemPromptAsync().Result;
                }
            });
        }

        public async Task SaveSystemPromptAsync(string modelId, string systemPrompt)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    
                    try
                    {
                        var settingsDal = session.GetDal<Setting>();
                        var settingName = $"{SystemPromptPrefix}{modelId}";
                        
                        session.BeginTransaction();
                        
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == settingName);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                Id = Guid.NewGuid(),
                                Name = settingName,
                                Description = $"System prompt for AI model {modelId}"
                            };
                        }
                        
                        setting.ValueString = systemPrompt;
                        settingsDal.Save(setting);
                        
                        session.Commit();
                        _logger.LogInformation($"Saved system prompt for model {modelId}");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, $"Error saving system prompt for model {modelId}");
                        throw;
                    }
                }
            });
        }

        public async Task<string> GetDefaultSystemPromptAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                        var settingsDal = session.GetDal<Setting>();
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == DefaultSystemPromptKey);
                        
                        if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                        {
                            return setting.ValueString;
                        }
                        
                        // Return a default system prompt
                        return @"You are a helpful AI assistant specialized in markdown editing and document management. 
You excel at creating well-structured markdown documents, generating summaries, and helping with technical documentation.
Always provide clear, concise, and well-formatted responses using proper markdown syntax when appropriate.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting default system prompt");
                    return "You are a helpful AI assistant.";
                }
            });
        }

        public async Task SaveDefaultSystemPromptAsync(string systemPrompt)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    
                    try
                    {
                        var settingsDal = session.GetDal<Setting>();
                        
                        session.BeginTransaction();
                        
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == DefaultSystemPromptKey);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                Id = Guid.NewGuid(),
                                Name = DefaultSystemPromptKey,
                                Description = "Default system prompt for AI models"
                            };
                        }
                        
                        setting.ValueString = systemPrompt;
                        settingsDal.Save(setting);
                        
                        session.Commit();
                        _logger.LogInformation("Saved default system prompt");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, "Error saving default system prompt");
                        throw;
                    }
                }
            });
        }
    }
}