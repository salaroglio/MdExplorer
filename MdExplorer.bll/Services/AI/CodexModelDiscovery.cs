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
using MdExplorer.Features.Services.AI.Codex;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Modelli selezionabili per Codex: quelli dell'account (<see cref="CodexModelSource"/>, <c>model/list</c>), salvati
    /// in <c>AvailableModel</c> con provider <see cref="ProviderKey"/>, la stessa tabella delle combo degli altri motori.
    /// Nessun ripiego: Codex non installato dà una lista vuota; installato ma senza risposta, un'eccezione col motivo.
    /// <para>Gemella di <c>OpenCodeModelDiscovery</c>. Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public class CodexModelDiscovery : IModelDiscoveryProvider
    {
        /// <summary>Valore della colonna <c>AvailableModel.Provider</c> (è anche il nome dell'enum).</summary>
        public const string ProviderKey = nameof(ProviderType.Codex);

        private readonly ILogger<CodexModelDiscovery> _logger;
        private readonly CodexModelSource _source;
        private readonly IDALFactory<IUserSettingsDB> _dalFactory;

        public ProviderType ProviderType => ProviderType.Codex;

        public CodexModelDiscovery(
            ILogger<CodexModelDiscovery> logger,
            CodexModelSource source,
            IDALFactory<IUserSettingsDB> dalFactory)
        {
            _logger = logger;
            _source = source;
            _dalFactory = dalFactory;
        }

        public bool SupportsDiscovery() => true;

        /// <summary>L'elenco salvato; se non ce n'è ancora uno, lo chiede a Codex (che per questo viene acceso e spento).</summary>
        public async Task<List<AiProviderModel>> GetModelsAsync()
        {
            var cached = ReadCached();
            if (cached.Count > 0) return cached;

            if (!CodexProcessLauncher.IsResolvable())
            {
                _logger.LogInformation("[CodexModelDiscovery] `codex` non è nel PATH: nessun modello da offrire");
                return new List<AiProviderModel>();
            }
            return await RefreshModelsAsync().ConfigureAwait(false);
        }

        /// <summary>Richiede l'elenco a Codex e sostituisce quello salvato. Fallisce forte, col motivo.</summary>
        public async Task<List<AiProviderModel>> RefreshModelsAsync()
        {
            var models = await _source.ListModelsAsync().ConfigureAwait(false);
            Persist(models);
            return models;
        }

        private List<AiProviderModel> ReadCached()
        {
            // Sessione ISOLATA: una lettura sulla sessione condivisa fuori transazione rompe il
            // Commit successivo di chiunque altro.
            using var session = _dalFactory.OpenSession();
            return session.GetDal<AvailableModel>()
                .GetList()
                .ToList()
                .Where(m => m.Provider == ProviderKey)
                .Select(m => new AiProviderModel
                {
                    Id = m.ModelId,
                    Name = m.Name,
                    Description = m.Description,
                    Provider = ProviderType.Codex,
                    CreatedAt = m.DiscoveredAt,
                })
                .ToList();
        }

        /// <summary>Inserisce i nuovi, aggiorna i presenti, toglie quelli che Codex non dichiara più.</summary>
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
                    record.Description = model.Description?.Length > 1000
                        ? model.Description.Substring(0, 1000)
                        : model.Description;
                    record.DiscoveredAt = now;
                    dal.Save(record);
                }

                foreach (var stale in existing.Where(e => !ids.Contains(e.ModelId)))
                {
                    dal.Delete(stale);
                }

                session.Commit();
                _logger.LogInformation("[CodexModelDiscovery] {Count} modelli salvati in AvailableModel", ids.Count);
            }
            catch
            {
                session.Rollback();
                throw;
            }
        }
    }
}
