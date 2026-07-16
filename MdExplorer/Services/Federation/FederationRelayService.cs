using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>La città locale attiva su una stanza, dal punto di vista di questa macchina.</summary>
    public sealed class LocalCity
    {
        public string ProjectPath { get; init; }
        public string ProjectName { get; init; }
        public string RoomId { get; init; }
        public string RelayUrl { get; init; }
    }

    /// <summary>Vista dello stato federato locale, per la UI/rubrica (§12.5).</summary>
    public interface IFederationState
    {
        IReadOnlyList<LocalCity> GetLocalCities();
    }

    /// <summary>
    /// Il presidio della federazione lato Service (§12.7 regola 2: la città vive col Service,
    /// non con la UI). Hosted <see cref="BackgroundService"/> — a differenza della vecchia
    /// <c>VpsChatStreamingService</c> (singleton client-triggered) non dipende da un client
    /// connesso. Scansiona periodicamente l'elenco dei progetti (tabella <c>Project</c>,
    /// persistente) e, per quelli con la città attiva, <b>assembla l'annuncio cifrato</b> in
    /// uno snapshot locale.
    /// <para>
    /// <b>Dormiente per default</b>: nessun progetto con <c>agentCity.enabled</c> → nessun
    /// lavoro. Il <b>collegamento reale al relay</b> (canale <c>/mdfed</c> in uscita + presence
    /// verso il server) è la parte outward-facing rimandata: è un seam esplicito
    /// (<see cref="TransmitPendingAsync"/>) che oggi <b>non apre alcuna connessione</b> e
    /// attende il server <c>/mdfed</c> (estensione di <c>mdexplorer-chat-server</c>, non ancora
    /// deployata). Così presence e discovery sono pronte e testabili senza toccare la rete.
    /// </para>
    /// </summary>
    public class FederationRelayService : BackgroundService, IFederationState
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(60);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFederationPresenceService _presence;
        private readonly ILogger<FederationRelayService> _logger;

        private volatile IReadOnlyList<LocalCity> _snapshot = Array.Empty<LocalCity>();

        public FederationRelayService(
            IServiceScopeFactory scopeFactory,
            IFederationPresenceService presence,
            ILogger<FederationRelayService> logger)
        {
            _scopeFactory = scopeFactory;
            _presence = presence;
            _logger = logger;
        }

        public IReadOnlyList<LocalCity> GetLocalCities() => _snapshot;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (cities, announces) = RebuildSnapshot();
                    _snapshot = cities;
                    await TransmitPendingAsync(announces, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Federation] Scansione presenza fallita");
                }
                try { await Task.Delay(ScanInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>
        /// Costruisce lo snapshot delle città locali attive e i relativi annunci cifrati.
        /// Fail-soft per-progetto: un progetto il cui git non è leggibile viene semplicemente
        /// saltato (loggato), non fa fallire l'intera scansione.
        /// </summary>
        private (IReadOnlyList<LocalCity> Cities, IReadOnlyList<FederationAnnounce> Announces) RebuildSnapshot()
        {
            var cities = new List<LocalCity>();
            var announces = new List<FederationAnnounce>();

            using var scope = _scopeFactory.CreateScope();
            var metadata = scope.ServiceProvider.GetRequiredService<IProjectMetadataService>();
            var registry = scope.ServiceProvider.GetRequiredService<IAgentRegistryService>();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();

            List<Project> projects;
            db.BeginTransaction();
            projects = db.GetDal<Project>().GetList().ToList();
            db.Commit();

            foreach (var project in projects)
            {
                // Pre-filtro economico: git-resolviamo SOLO i progetti con la città attiva.
                var cfg = Safe(() => metadata.GetAgentCity(project.Path));
                if (cfg == null || !cfg.Enabled) continue;

                var (origin, email) = ResolveGit(project.Path);
                var roster = BuildTrustedRoster(registry, project.Path);

                FederationAnnounce announce;
                try
                {
                    announce = _presence.BuildAnnounce(project.Path, origin, email, roster);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Federation] Annuncio non costruibile per '{Project}'", project.Path);
                    continue;
                }
                if (announce == null) continue;

                cities.Add(new LocalCity
                {
                    ProjectPath = project.Path,
                    ProjectName = project.Name,
                    RoomId = announce.RoomId,
                    RelayUrl = announce.RelayUrl,
                });
                announces.Add(announce);
            }

            return (cities, announces);
        }

        /// <summary>
        /// Seam del trasporto verso il relay (§12.5, outward-facing) — <b>rimandato di scelta</b>.
        /// Oggi NON apre alcuna connessione: registra soltanto che ci sarebbero annunci da
        /// spedire, in attesa del server <c>/mdfed</c> e del cablaggio Socket.IO in uscita. Il
        /// contratto (annunci cifrati per stanza) è già quello definitivo, così l'aggiunta del
        /// canale non toccherà la logica di presence/discovery.
        /// </summary>
        private Task TransmitPendingAsync(IReadOnlyList<FederationAnnounce> announces, CancellationToken ct)
        {
            if (announces.Count > 0)
                _logger.LogInformation(
                    "[Federation] {N} città attive pronte all'annuncio; trasmissione al relay /mdfed in attesa del server (non ancora deployato).",
                    announces.Count);
            return Task.CompletedTask;
        }

        private (string Origin, string Email) ResolveGit(string projectPath)
        {
            try
            {
                using var repo = new LibGit2Sharp.Repository(projectPath);
                var origin = repo.Network.Remotes["origin"]?.Url;
                var email = repo.Config.Get<string>("user.email")?.Value;
                return (origin, email);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[Federation] git non leggibile per '{Project}' (non un repo?)", projectPath);
                return (null, null);
            }
        }

        private static IReadOnlyList<AgentRosterEntry> BuildTrustedRoster(IAgentRegistryService registry, string projectPath)
        {
            var catalog = registry.RefreshCatalog(projectPath);
            return catalog
                .Where(e => e.IsCitizen && e.Trusted)
                .Select(e => new AgentRosterEntry
                {
                    Name = e.Name,
                    Role = e.Role,
                    Skills = e.Skills?
                        .Select(s => s.Id)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList() ?? new List<string>(),
                })
                .ToList();
        }

        private T Safe<T>(Func<T> f) where T : class
        {
            try { return f(); } catch { return null; }
        }
    }
}
