using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Model discovery for GitHub Copilot CLI.
    ///
    /// <para>
    /// I modelli si chiedono al CLI con l'SDK ufficiale (<see cref="CopilotSdkModelSource"/>,
    /// RPC <c>models.list</c>): domanda tipizzata, nessuna inferenza, nessun credito.
    /// La lista viene messa in cache in UserDB perché la schermata dei modelli non paghi
    /// un giro al CLI ogni volta che si apre.
    /// </para>
    ///
    /// <para>
    /// <b>Non esiste più una lista di ripiego cablata.</b> Se il CLI non risponde, questa
    /// classe restituisce una lista vuota e scrive il motivo nel log. Mostrare modelli
    /// inventati era peggio che non mostrarne: l'utente ne selezionava uno, la preferenza
    /// veniva salvata, e il guasto si manifestava molto più tardi — alla prima domanda —
    /// dove nessuno lo collegava più alla schermata di scelta.
    /// </para>
    /// </summary>
    public class CopilotCliModelDiscovery : IModelDiscoveryProvider
    {
        private readonly ILogger<CopilotCliModelDiscovery> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CopilotSdkModelSource _sdkModelSource;

        public ProviderType ProviderType => ProviderType.CopilotCli;

        private const string DISCOVERED_MODELS_SETTING = "CopilotCli_DiscoveredModels";


        public CopilotCliModelDiscovery(
            ILogger<CopilotCliModelDiscovery> logger,
            IServiceProvider serviceProvider,
            CopilotSdkModelSource sdkModelSource)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _sdkModelSource = sdkModelSource;
        }

        public bool SupportsDiscovery() => true;

        public async Task<List<AiProviderModel>> GetModelsAsync()
        {
            try
            {
                var cachedModels = await LoadModelsFromDbAsync();
                if (cachedModels != null && cachedModels.Count > 0)
                {
                    _logger.LogInformation("[CopilotCliModelDiscovery] Returning {Count} cached models from DB", cachedModels.Count);
                    return cachedModels.OrderBy(m => m.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CopilotCliModelDiscovery] Error reading cached models from DB");
            }

            // Cache vuota: si chiede al CLI invece di inventare. Costa ~2 secondi e
            // succede una volta sola, perché subito dopo la risposta viene messa in cache.
            _logger.LogInformation("[CopilotCliModelDiscovery] No cached models, asking the CLI");
            return await RefreshModelsAsync();
        }

        /// <summary>
        /// Refreshes the model list by invoking the Copilot CLI and caching the result in UserDB.
        /// </summary>
        public async Task<List<AiProviderModel>> RefreshModelsAsync()
        {
            _logger.LogInformation("[CopilotCliModelDiscovery] Refreshing models from Copilot CLI...");

            List<AiProviderModel> models;
            try
            {
                models = await _sdkModelSource.ListModelsAsync();
            }
            catch (Exception ex)
            {
                // Lista vuota, non di ripiego: la schermata mostrerà che non ci sono modelli,
                // e il log dice perché. È l'informazione utile; un elenco inventato non lo è.
                _logger.LogError(ex, "[CopilotCliModelDiscovery] Impossibile leggere i modelli dal CLI");
                return new List<AiProviderModel>();
            }

            if (models.Count == 0)
            {
                _logger.LogWarning("[CopilotCliModelDiscovery] Il CLI non ha dichiarato alcun modello");
                return models;
            }

            try
            {
                await SaveModelsToDbAsync(models);
                _logger.LogInformation("[CopilotCliModelDiscovery] Saved {Count} models to DB cache", models.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CopilotCliModelDiscovery] Error saving models to DB cache");
            }

            return models.OrderBy(m => m.Name).ToList();
        }

        // NOTA: qui vivevano ~210 righe che scoprivano i modelli chiedendoli all'LLM
        // a parole e facendone il parsing con tre regex (RunCopilotDiscoveryAsync,
        // RunFastModelDiscoveryAsync, ParseModelIds, IsValidModelId,
        // ModelIdToReadableName). Sostituite da CopilotSdkModelSource, che chiede la
        // stessa cosa al CLI con una RPC tipizzata. Rimosse il 04/09/2026.


        #region DB persistence

        private async Task<List<AiProviderModel>> LoadModelsFromDbAsync()
        {
            return await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    var settingsDal = session.GetDal<Setting>();
                    var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == DISCOVERED_MODELS_SETTING);

                    if (setting == null || string.IsNullOrWhiteSpace(setting.ValueString))
                        return null;

                    var cached = JsonSerializer.Deserialize<List<CachedModelEntry>>(setting.ValueString);
                    if (cached == null || cached.Count == 0)
                        return null;

                    return cached.Select(c => new AiProviderModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = $"{c.Name} via GitHub Copilot",
                        Provider = ProviderType.CopilotCli,
                        InputTokenLimit = 128000,
                        OutputTokenLimit = 16000,
                        IsDeprecated = false,
                        CreatedAt = c.DiscoveredAt,
                        Capabilities = new ProviderCapabilities
                        {
                            SupportsStreaming = true,
                            SupportsFunctionCalling = false,
                            SupportsEmbeddings = false,
                            SupportsVision = false,
                            MaxInputTokens = 128000,
                            MaxOutputTokens = 16000
                        }
                    }).ToList();
                }
            });
        }

        private async Task SaveModelsToDbAsync(List<AiProviderModel> models)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    var settingsDal = session.GetDal<Setting>();

                    try
                    {
                        session.BeginTransaction();

                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == DISCOVERED_MODELS_SETTING);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                Name = DISCOVERED_MODELS_SETTING,
                                Description = "Cached Copilot CLI discovered models (JSON)"
                            };
                        }

                        var cached = models.Select(m => new CachedModelEntry
                        {
                            Id = m.Id,
                            Name = m.Name,
                            DiscoveredAt = DateTime.UtcNow
                        }).ToList();

                        setting.ValueString = JsonSerializer.Serialize(cached);
                        settingsDal.Save(setting);
                        session.Commit();
                    }
                    catch
                    {
                        session.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Lightweight DTO for JSON serialization of cached model entries.
        /// </summary>
        private class CachedModelEntry
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime DiscoveredAt { get; set; }
        }

        #endregion
    }
}
