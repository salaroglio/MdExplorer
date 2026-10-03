using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.Services.AgentMemory
{
    /// <summary>
    /// Risolve le coordinate Fuseki di un progetto dai suoi <see cref="ProjectFusekiSettings"/>
    /// (memoria, Fase 5). Punto unico: lo usano sia il <c>MemoryController</c> (assert/query
    /// dell'agente) sia il dispatcher (recupero al risveglio, 5c). Ritorna <c>null</c> quando la
    /// memoria non è abilitata per il progetto — il chiamante decide se è un errore (controller)
    /// o un semplice "niente memoria" (dispatcher).
    /// <para>
    /// Due modi (<see cref="ProjectFusekiSettings.Managed"/>): <b>gestito</b> = il Service avvia
    /// un'istanza Fuseki propria su porta loopback random (addon on-demand); <b>esterno</b> =
    /// usa l'<c>Uri</c> configurato dall'utente.
    /// </para>
    /// </summary>
    public interface IFusekiConnectionResolver
    {
        /// <summary>Coordinate Fuseki del progetto, o <c>null</c> se Fuseki è disabilitato/non configurato.</summary>
        Task<FusekiConnection> ResolveAsync(string projectPath, CancellationToken ct = default);
    }

    public class FusekiConnectionResolver : IFusekiConnectionResolver
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPasswordProtector _passwordProtector;
        private readonly IFusekiClient _fusekiClient;
        private readonly IFusekiProcessManager _processManager;

        public FusekiConnectionResolver(
            IServiceScopeFactory scopeFactory,
            IPasswordProtector passwordProtector,
            IFusekiClient fusekiClient,
            IFusekiProcessManager processManager)
        {
            _scopeFactory = scopeFactory;
            _passwordProtector = passwordProtector;
            _fusekiClient = fusekiClient;
            _processManager = processManager;
        }

        public async Task<FusekiConnection> ResolveAsync(string projectPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return null;

            ProjectFusekiSettings settings;
            string projectName;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                // Materializza prima del confronto path (AgentPathComparer non traducibile in SQL).
                db.BeginTransaction();
                var project = db.GetDal<Project>().GetList().ToList()
                    .FirstOrDefault(p => AgentPathComparer.Equals(p.Path, projectPath));
                settings = project == null ? null : db.GetDal<ProjectFusekiSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == project.Id);
                projectName = project?.Name;
                db.Commit();
            }

            if (settings == null || !settings.Enabled)
                return null;

            // Nome dataset: quello configurato, altrimenti default sanitizzato dal nome progetto.
            var dataset = string.IsNullOrWhiteSpace(settings.Dataset)
                ? _fusekiClient.SanitizeDatasetName(projectName)
                : settings.Dataset;
            if (string.IsNullOrWhiteSpace(dataset)) return null;

            if (settings.Managed)
            {
                // Istanza gestita: avviala (fail-loud se l'addon manca) e punta al suo base URI.
                // Nessuna credenziale (loopback, di proprietà del Service).
                var baseUri = await _processManager.EnsureRunningAsync(ct);
                return new FusekiConnection
                {
                    BaseUri = baseUri,
                    Dataset = dataset,
                    Username = string.Empty,
                    Password = string.Empty,
                };
            }

            // Fuseki esterno configurato dall'utente.
            if (string.IsNullOrWhiteSpace(settings.Uri)) return null;
            var password = string.IsNullOrEmpty(settings.PasswordEncrypted)
                ? string.Empty
                : _passwordProtector.Unprotect(settings.PasswordEncrypted);

            return new FusekiConnection
            {
                BaseUri = settings.Uri,
                Dataset = dataset,
                Username = settings.Username ?? string.Empty,
                Password = password,
            };
        }
    }
}
