using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Utilities;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Service.HostedServices
{
    /// <summary>
    /// Hosted service that runs initialization tasks at application startup
    /// </summary>
    public class ApplicationInitializationService : IHostedService
    {
        private readonly ILogger<ApplicationInitializationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ApplicationInitializationService(
            ILogger<ApplicationInitializationService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting application initialization...");

            try
            {
                // Create a scope to resolve scoped services
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    
                    // Check and auto-discover VS Code if needed
                    _logger.LogInformation("Checking VS Code editor configuration...");
                    
                    var settingsDal = session.GetDal<Setting>();
                    var editorSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "EditorPath");

                    if (editorSetting == null || string.IsNullOrWhiteSpace(editorSetting.ValueString))
                    {
                        _logger.LogInformation("EditorPath is empty or not found. Starting VS Code auto-discovery...");
                        
                        var discoveredPath = CrossPlatformProcess.DiscoverVSCodePath();
                        
                        if (!string.IsNullOrEmpty(discoveredPath))
                        {
                            _logger.LogInformation($"VS Code discovered at: {discoveredPath}");
                            
                            // Save to database
                            session.BeginTransaction();
                            
                            if (editorSetting == null)
                            {
                                editorSetting = new Setting
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "EditorPath",
                                    ValueString = discoveredPath
                                };
                            }
                            else
                            {
                                editorSetting.ValueString = discoveredPath;
                            }
                            
                            settingsDal.Save(editorSetting);
                            session.Commit();
                            
                            _logger.LogInformation($"Editor path saved to database: {discoveredPath}");
                        }
                        else
                        {
                            _logger.LogWarning("VS Code auto-discovery failed. No VS Code installation found.");
                        }
                    }
                    else if (File.Exists(editorSetting.ValueString))
                    {
                        _logger.LogInformation($"VS Code already configured at: {editorSetting.ValueString}");
                    }
                    else
                    {
                        _logger.LogWarning($"Configured VS Code path does not exist: {editorSetting.ValueString}. Consider re-running auto-discovery.");
                    }
                    
                    // Check and auto-discover Java if needed
                    _logger.LogInformation("Checking Java configuration...");
                    
                    var javaSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "JavaPath");
                    
                    if (javaSetting == null || string.IsNullOrWhiteSpace(javaSetting.ValueString))
                    {
                        _logger.LogInformation("JavaPath is empty or not found. Starting Java auto-discovery...");
                        
                        var discoveredJavaPath = JavaDiscovery.DiscoverJavaPath();
                        
                        if (!string.IsNullOrEmpty(discoveredJavaPath))
                        {
                            _logger.LogInformation($"Java discovered at: {discoveredJavaPath}");
                            
                            // Verify Java is working
                            var javaInfo = JavaDiscovery.GetJavaInfo(discoveredJavaPath);
                            if (javaInfo.IsValid)
                            {
                                _logger.LogInformation($"Java version: {javaInfo.Version}");
                                
                                // Save to database
                                session.BeginTransaction();
                                
                                if (javaSetting == null)
                                {
                                    javaSetting = new Setting
                                    {
                                        Id = Guid.NewGuid(),
                                        Name = "JavaPath",
                                        ValueString = discoveredJavaPath
                                    };
                                }
                                else
                                {
                                    javaSetting.ValueString = discoveredJavaPath;
                                }
                                
                                settingsDal.Save(javaSetting);
                                session.Commit();
                                
                                _logger.LogInformation($"Java path saved to database: {discoveredJavaPath}");
                            }
                            else
                            {
                                _logger.LogWarning($"Java found at {discoveredJavaPath} but validation failed");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Java auto-discovery failed. No Java installation found. PlantUML features will not be available.");
                        }
                    }
                    else if (File.Exists(javaSetting.ValueString))
                    {
                        _logger.LogInformation($"Java already configured at: {javaSetting.ValueString}");
                        
                        // Verify it's still working
                        var javaInfo = JavaDiscovery.GetJavaInfo(javaSetting.ValueString);
                        if (javaInfo.IsValid)
                        {
                            _logger.LogInformation($"Java version: {javaInfo.Version}");
                        }
                        else
                        {
                            _logger.LogWarning($"Configured Java path exists but is not valid. Consider re-running auto-discovery.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Configured Java path does not exist: {javaSetting.ValueString}. Starting auto-discovery...");
                        
                        // Try auto-discovery
                        var discoveredJavaPath = JavaDiscovery.DiscoverJavaPath();
                        if (!string.IsNullOrEmpty(discoveredJavaPath))
                        {
                            _logger.LogInformation($"Java discovered at: {discoveredJavaPath}");
                            
                            session.BeginTransaction();
                            javaSetting.ValueString = discoveredJavaPath;
                            settingsDal.Save(javaSetting);
                            session.Commit();
                            
                            _logger.LogInformation($"Java path updated in database: {discoveredJavaPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application initialization");
                // Don't throw - we don't want to prevent the app from starting
            }

            _logger.LogInformation("Application initialization completed");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}