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
                    // Il trasloco delle credenziali git dal DB al credential helper di git: una volta,
                    // idempotente, mai fatale per l'avvio. Ciò che non si sposta resta e lo dice nel log.
                    try
                    {
                        var moved = await scope.ServiceProvider
                            .GetRequiredService<MdExplorer.Services.Git.IGitCredentialMoveService>()
                            .RunAsync(cancellationToken);
                        if (!moved.NothingLeft)
                            _logger.LogInformation("[trasloco] credenziali git: {Moved} spostate, {Failed} da sistemare", moved.Moved, moved.Failed);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[trasloco] credenziali git: fallito, si riprova al prossimo avvio");
                    }

                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    
                    // Check and auto-discover IDEs (VS Code and IntelliJ IDEA)
                    _logger.LogInformation("Checking IDE configurations...");

                    var settingsDal = session.GetDal<Setting>();

                    // VS Code discovery
                    var editorSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "EditorPath");
                    if (editorSetting == null || string.IsNullOrWhiteSpace(editorSetting.ValueString))
                    {
                        _logger.LogInformation("EditorPath is empty or not found. Starting VS Code auto-discovery...");

                        var discoveredPath = CrossPlatformProcess.DiscoverVSCodePath();

                        if (!string.IsNullOrEmpty(discoveredPath))
                        {
                            _logger.LogInformation($"VS Code discovered at: {discoveredPath}");

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

                    // Check and auto-discover IntelliJ IDEA if needed
                    _logger.LogInformation("Checking IntelliJ IDEA configuration...");

                    var intellijSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "IntelliJPath");

                    if (intellijSetting == null || string.IsNullOrWhiteSpace(intellijSetting.ValueString))
                    {
                        _logger.LogInformation("IntelliJPath is empty or not found. Starting IntelliJ IDEA auto-discovery...");

                        var discoveredIntellijPath = CrossPlatformProcess.DiscoverIntelliJPath();

                        if (!string.IsNullOrEmpty(discoveredIntellijPath))
                        {
                            _logger.LogInformation($"IntelliJ IDEA discovered at: {discoveredIntellijPath}");

                            // Save to database
                            session.BeginTransaction();

                            if (intellijSetting == null)
                            {
                                intellijSetting = new Setting
                                {
                                    // Let NHibernate auto-generate Id via GuidComb
                                    Name = "IntelliJPath",
                                    ValueString = discoveredIntellijPath
                                };
                            }
                            else
                            {
                                intellijSetting.ValueString = discoveredIntellijPath;
                            }

                            settingsDal.Save(intellijSetting);
                            session.Commit();

                            _logger.LogInformation($"IntelliJ path saved to database: {discoveredIntellijPath}");
                        }
                        else
                        {
                            _logger.LogWarning("IntelliJ IDEA auto-discovery failed. No IntelliJ IDEA installation found.");
                        }
                    }
                    else if (File.Exists(intellijSetting.ValueString))
                    {
                        _logger.LogInformation($"IntelliJ IDEA already configured at: {intellijSetting.ValueString}");
                    }
                    else
                    {
                        _logger.LogWarning($"Configured IntelliJ IDEA path does not exist: {intellijSetting.ValueString}. Consider re-running auto-discovery.");
                    }

                    // Java: il controllo di salute è in Services.Java.JavaHealthService (12/10/2026): parte in sfondo
                    // dopo che il server è in ascolto e sostituisce un JavaPath vecchio scritto dalla scoperta automatica.
                    _logger.LogInformation("Java: controllo di salute rimandato a JavaHealthService (dopo l'avvio del server)");
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