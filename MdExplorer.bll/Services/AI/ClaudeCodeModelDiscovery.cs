using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Modelli selezionabili per Claude Code: quelli che il CLI dichiara per questo account
    /// (<see cref="ClaudeCodeModelSource"/>, <c>initialize</c> sullo stream-json), salvati nella tabella
    /// <c>AvailableModel</c> con provider <see cref="ProviderKey"/> — la stessa che la combo di Copilot legge da
    /// <c>/api/aimodels/cached</c>.
    ///
    /// <para><b>Prima</b> qui c'era una lista scritta a mano (sonnet/opus/haiku/fable) con
    /// <c>SupportsDiscovery() = false</c> e il commento «il CLI non espone un elenco»: era vero allora, non lo è
    /// più (verificato il 13/09/2026 su claude 2.1.270), e la lista era già sbagliata — mancava <c>default</c>,
    /// e l'Opus dell'account è <c>opus[1m]</c>.</para>
    ///
    /// <para>Nessun ripiego. Un CLI assente dà una lista vuota, perché è la verità: su questa macchina non
    /// ci sono modelli Claude Code. Un CLI presente che non risponde è un'eccezione col motivo.</para>
    ///
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Modello-Claude-Code.md, fase F1.</para>
    /// </summary>
    public class ClaudeCodeModelDiscovery : IModelDiscoveryProvider
    {
        /// <summary>Valore della colonna <c>AvailableModel.Provider</c> (è anche il nome dell'enum).</summary>
        public const string ProviderKey = nameof(ProviderType.ClaudeCode);

        private readonly ILogger<ClaudeCodeModelDiscovery> _logger;
        private readonly ClaudeCodeModelSource _source;
        private readonly IDALFactory<IUserSettingsDB> _dalFactory;

        public ProviderType ProviderType => ProviderType.ClaudeCode;

        public ClaudeCodeModelDiscovery(
            ILogger<ClaudeCodeModelDiscovery> logger,
            ClaudeCodeModelSource source,
            IDALFactory<IUserSettingsDB> dalFactory)
        {
            _logger = logger;
            _source = source;
            _dalFactory = dalFactory;
        }

        public bool SupportsDiscovery() => true;

        /// <summary>L'elenco salvato; se non ce n'è ancora uno, lo chiede al CLI (0,6 s, una volta sola).</summary>
        public async Task<List<AiProviderModel>> GetModelsAsync()
        {
            var cached = ReadCached();
            if (cached.Count > 0) return cached;

            if (!ClaudeCodeProcessLauncher.IsResolvable())
            {
                _logger.LogInformation("[ClaudeCodeModelDiscovery] `claude` non è nel PATH: nessun modello da offrire");
                return new List<AiProviderModel>();
            }
            return await RefreshModelsAsync().ConfigureAwait(false);
        }

        /// <summary>Chiede l'elenco al CLI e sostituisce quello salvato. Fallisce forte, col motivo.</summary>
        public async Task<List<AiProviderModel>> RefreshModelsAsync()
        {
            var models = await _source.ListModelsAsync().ConfigureAwait(false);
            Persist(models);
            return models;
        }

        private List<AiProviderModel> ReadCached()
        {
            // Sessione ISOLATA: una lettura sulla sessione condivisa fuori transazione rompe il Commit successivo.
            using var session = _dalFactory.OpenSession();
            return session.GetDal<AvailableModel>()
                .GetList()
                .ToList()
                .Where(m => m.Provider == ProviderKey)
                .Select(m => new AiProviderModel
                {
                    Id = m.ModelId,
                    Name = m.Name,
                    Provider = ProviderType.ClaudeCode,
                    CreatedAt = m.DiscoveredAt,
                })
                .ToList();
        }

        /// <summary>Inserisce i modelli nuovi, aggiorna i presenti, toglie quelli che il CLI non dichiara più.</summary>
        private void Persist(IReadOnlyCollection<AiProviderModel> models)
        {
            using var session = _dalFactory.OpenSession();
            var dal = session.GetDal<AvailableModel>();
            session.BeginTransaction();
            try
            {
                var existing = dal.GetList().ToList().Where(m => m.Provider == ProviderKey).ToList();
                var now = DateTime.UtcNow;
                var ids = new HashSet<string>(models.Select(m => m.Id));

                foreach (var model in models)
                {
                    var record = existing.FirstOrDefault(e => e.ModelId == model.Id)
                                 ?? new AvailableModel { ModelId = model.Id, Provider = ProviderKey };
                    record.Name = model.Name;
                    record.DiscoveredAt = now;
                    dal.Save(record);
                }

                foreach (var stale in existing.Where(e => !ids.Contains(e.ModelId)))
                {
                    dal.Delete(stale);
                }

                session.Commit();
                _logger.LogInformation("[ClaudeCodeModelDiscovery] {Count} modelli salvati in AvailableModel", ids.Count);
            }
            catch
            {
                session.Rollback();
                throw;
            }
        }
    }
}
