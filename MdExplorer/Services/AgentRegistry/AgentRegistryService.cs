using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.Agents;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Services.DatabaseManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRegistry
{
    /// <summary>
    /// Shell I/O del registry (§6). La logica deterministica sta nel
    /// <see cref="AgentRegistryReconciler"/> (bll, testabile); qui vivono la lettura
    /// dalle fonti e la persistenza:
    /// <list type="bullet">
    /// <item>agenti LLM: elenco dei <c>.agent.md</c> dall'Engine DB, frontmatter
    /// riletto e parsato dal disco (autorità = filesystem);</item>
    /// <item>agenti algoritmici: <c>IEnumerable&lt;IAlgorithmicAgent&gt;</c> dalla DI;</item>
    /// <item>persistenza trust/identità: <c>AgentIdentity</c> nella UserDB via scope.</item>
    /// </list>
    /// Singleton: accede alla UserDB (Scoped) tramite <see cref="IServiceScopeFactory"/> e
    /// all'Engine DB per-progetto tramite <see cref="IDatabaseManager.CreateIsolatedEngineDBForProjectPath"/>.
    /// </summary>
    public class AgentRegistryService : IAgentRegistryService
    {
        private readonly ILogger<AgentRegistryService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDatabaseManager _databaseManager;
        private readonly IEnumerable<IAlgorithmicAgent> _algorithmicAgents;
        private readonly IYamlAgentCardParser _cardParser;
        private readonly AgentRegistryReconciler _reconciler = new AgentRegistryReconciler();

        private readonly ConcurrentDictionary<string, IReadOnlyList<AgentRegistryEntry>> _cache = new();
        private readonly ConcurrentDictionary<string, object> _locks = new();

        public AgentRegistryService(
            ILogger<AgentRegistryService> logger,
            IServiceScopeFactory scopeFactory,
            IDatabaseManager databaseManager,
            IEnumerable<IAlgorithmicAgent> algorithmicAgents,
            IYamlAgentCardParser cardParser)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _databaseManager = databaseManager;
            _algorithmicAgents = algorithmicAgents;
            _cardParser = cardParser;
        }

        public IReadOnlyList<AgentRegistryEntry> GetCatalog(string projectPath)
        {
            var normalized = Normalize(projectPath);
            if (normalized != null && _cache.TryGetValue(normalized, out var cached))
                return cached;
            return RefreshCatalog(projectPath);
        }

        public IReadOnlyList<AgentRegistryEntry> RefreshCatalog(string projectPath)
        {
            var normalized = Normalize(projectPath);
            if (string.IsNullOrWhiteSpace(normalized))
                return Array.Empty<AgentRegistryEntry>();

            // Serializza i refresh dello STESSO progetto: evita corse sulle righe AgentIdentity
            // (l'indice unique (ProjectPath, AgentName) altrimenti potrebbe rigettare i create).
            var sync = _locks.GetOrAdd(normalized, _ => new object());
            lock (sync)
            {
                var discovered = new List<DiscoveredAgentCard>();
                discovered.AddRange(DiscoverLlmAgents(normalized));
                discovered.AddRange(DiscoverAlgorithmicAgents());

                IReadOnlyList<AgentRegistryEntry> catalog;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                    if (db == null)
                        throw new InvalidOperationException("IUserSettingsDB non risolvibile: impossibile riconciliare AgentIdentity.");

                    db.BeginTransaction();
                    var rows = db.GetDal<AgentIdentity>().GetList().ToList()
                        .Where(r => PathEquals(r.ProjectPath, normalized))
                        .ToList();

                    var existing = rows.Select(r => new ExistingIdentity
                    {
                        Id = r.Id,
                        Name = r.AgentName,
                        Trusted = r.Trusted,
                        Enabled = r.Enabled,
                        A2ABlockHash = r.A2ABlockHash,
                    });

                    catalog = _reconciler.Reconcile(discovered, existing);
                    PersistIdentities(db, normalized, rows, catalog);
                    db.Commit();
                }

                _cache[normalized] = catalog;
                return catalog;
            }
        }

        public AgentRegistryEntry TrustAgent(string projectPath, string agentName)
            => SetTrustByName(projectPath, agentName, trusted: true);

        public AgentRegistryEntry UntrustAgent(string projectPath, string agentName)
            => SetTrustByName(projectPath, agentName, trusted: false);

        public void OnProjectOpened(string projectPath) => QueueRefresh(projectPath, "projectOpen");

        public void OnAgentFileChanged(string projectPath) => QueueRefresh(projectPath, "fsw");

        // -------------------------------------------------------------------------

        /// <summary>
        /// Conferma/revoca il trust di un cittadino (§9, R3). Sul trust, memorizza
        /// l'<c>A2ABlockHash</c> CORRENTE (la conferma è ancorata al contenuto ora
        /// approvato); su revoca, azzera Trusted/Enabled. Fail-loud se l'agente non
        /// esiste o è escluso dal registry.
        /// </summary>
        private AgentRegistryEntry SetTrustByName(string projectPath, string agentName, bool trusted)
        {
            var normalized = Normalize(projectPath);
            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("projectPath mancante.", nameof(projectPath));
            var name = agentName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("agentName mancante.", nameof(agentName));

            var sync = _locks.GetOrAdd(normalized, _ => new object());
            lock (sync) // Monitor rientrante: RefreshCatalog riprende lo stesso lock senza deadlock.
            {
                var catalog = RefreshCatalog(projectPath);
                var entry = catalog.FirstOrDefault(e =>
                    string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                    throw new InvalidOperationException($"Agente '{name}' non trovato nel progetto.");
                if (entry.IsExcluded)
                    throw new InvalidOperationException(
                        $"Agente '{name}' escluso dal registry e non affidabile: {entry.RegistrationError}");
                if (entry.IdentityId == null)
                    throw new InvalidOperationException($"Agente '{name}' senza identità persistita.");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                    if (db == null)
                        throw new InvalidOperationException("IUserSettingsDB non risolvibile.");

                    db.BeginTransaction();
                    var dal = db.GetDal<AgentIdentity>();
                    var row = dal.GetList().ToList().FirstOrDefault(r => r.Id == entry.IdentityId.Value);
                    if (row == null)
                    {
                        db.Commit();
                        throw new InvalidOperationException($"Identità dell'agente '{name}' non più presente.");
                    }

                    row.Trusted = trusted;
                    row.Enabled = trusted; // Enabled ⇒ Trusted: trust abilita, revoca disabilita.
                    if (trusted)
                        row.A2ABlockHash = entry.CurrentA2ABlockHash; // àncora la conferma al contenuto attuale
                    row.UpdatedAt = DateTime.UtcNow;
                    dal.Save(row);
                    db.Commit();
                }

                // Ricostruisci dalle fonti: ora l'hash memorizzato combacia → niente decadenza.
                var refreshed = RefreshCatalog(projectPath);
                return refreshed.First(e => e.IdentityId == entry.IdentityId);
            }
        }

        private void QueueRefresh(string projectPath, string reason)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return;
            // Fire-and-forget con scope proprio (pattern AgentScheduleEventService): non
            // blocca l'evento (project-open / FSW) e non cattura servizi Scoped.
            Task.Run(() =>
            {
                try { RefreshCatalog(projectPath); }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Refresh del registry agenti fallito ({Reason}) per {Path}", reason, projectPath);
                }
            });
        }

        private IEnumerable<DiscoveredAgentCard> DiscoverLlmAgents(string projectPath)
        {
            var result = new List<DiscoveredAgentCard>();
            IEngineDB engineDb = null;
            try
            {
                engineDb = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);
                var files = engineDb.GetDal<MarkdownFile>().GetList().ToList()
                    .Where(f => f.Path != null &&
                                f.Path.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var f in files)
                {
                    // Autorità = filesystem: se l'indice è stale e il file non c'è più, salta.
                    if (!File.Exists(f.Path))
                        continue;

                    string content;
                    try
                    {
                        content = File.ReadAllText(f.Path);
                    }
                    catch (Exception ex)
                    {
                        result.Add(new DiscoveredAgentCard
                        {
                            Name = null,
                            Kind = AgentIdentity.KindEnum.Llm,
                            AgentFilePath = f.Path,
                            ParseError = $"Impossibile leggere il file: {ex.Message}",
                        });
                        continue;
                    }

                    var parsed = _cardParser.GetDescriptor(content);
                    if (!parsed.HasA2aBlock)
                        continue; // non cittadino: retrocompatibile, non entra nel registry

                    if (!parsed.IsValid)
                    {
                        result.Add(new DiscoveredAgentCard
                        {
                            Name = parsed.Card?.Name,
                            Kind = AgentIdentity.KindEnum.Llm,
                            AgentFilePath = f.Path,
                            ParseError = parsed.RegistrationError,
                        });
                        continue;
                    }

                    result.Add(new DiscoveredAgentCard
                    {
                        Name = parsed.Card.Name,
                        Kind = AgentIdentity.KindEnum.Llm,
                        AgentFilePath = f.Path,
                        Role = parsed.Card.Role,
                        Skills = parsed.Card.Skills?
                            .Select(s => new AgentRegistrySkill { Id = s.Id, Description = s.Description })
                            .ToList() ?? new List<AgentRegistrySkill>(),
                        // R3: impronta del blocco a2a: + tools: per la decadenza del trust.
                        CurrentA2ABlockHash = AgentTrustHasher.ComputeHash(parsed.Card, parsed.Tools),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scansione .agent.md fallita per {Path}", projectPath);
            }
            finally
            {
                try { engineDb?.Dispose(); }
                catch (Exception ex) { _logger.LogWarning(ex, "Dispose Engine DB isolato fallito per {Path}", projectPath); }
            }
            return result;
        }

        private IEnumerable<DiscoveredAgentCard> DiscoverAlgorithmicAgents()
        {
            var result = new List<DiscoveredAgentCard>();
            foreach (var agent in _algorithmicAgents ?? Enumerable.Empty<IAlgorithmicAgent>())
            {
                AgentCardInfo card;
                try
                {
                    card = agent.GetCard();
                }
                catch (Exception ex)
                {
                    result.Add(new DiscoveredAgentCard
                    {
                        Name = null,
                        Kind = AgentIdentity.KindEnum.Algorithmic,
                        ParseError = $"{agent.GetType().Name}.GetCard() ha sollevato: {ex.Message}",
                    });
                    continue;
                }

                // Stesse regole di identità degli agenti LLM (unica fonte di verità nel parser).
                var nameError = YamlAgentCardParser.ValidateAgentName(card?.Name);
                result.Add(new DiscoveredAgentCard
                {
                    Name = nameError == null ? card.Name.Trim() : card?.Name,
                    Kind = AgentIdentity.KindEnum.Algorithmic,
                    AgentFilePath = null,
                    Role = card?.Role,
                    Skills = card?.Skills?
                        .Select(s => new AgentRegistrySkill { Id = s.Id, Description = s.Description })
                        .ToList() ?? new List<AgentRegistrySkill>(),
                    ParseError = nameError,
                    // Gli algoritmici non hanno tools: dichiarati; l'hash copre la sola card.
                    CurrentA2ABlockHash = nameError == null ? AgentTrustHasher.ComputeHash(card, null) : null,
                });
            }
            return result;
        }

        private void PersistIdentities(
            IUserSettingsDB db, string projectPath,
            List<AgentIdentity> rows, IReadOnlyList<AgentRegistryEntry> catalog)
        {
            var dal = db.GetDal<AgentIdentity>();
            var now = DateTime.UtcNow;

            foreach (var entry in catalog)
            {
                if (entry.IdentityId == null)
                {
                    // Voce esclusa mai persistita → solo in-memory: niente riga (rispetta
                    // l'unique index; per i duplicati sarebbe comunque proibito).
                    if (entry.IsExcluded)
                        continue;

                    var row = new AgentIdentity
                    {
                        // Id NON pre-assegnato: GuidComb (gotcha entità detached).
                        ProjectPath = projectPath,
                        AgentName = entry.Name,
                        AgentFilePath = entry.AgentFilePath,
                        Kind = entry.Kind,
                        Trusted = false,
                        Enabled = false,
                        RegistrationError = null,
                        CreatedAt = now,
                        UpdatedAt = now,
                    };
                    dal.Save(row);
                    entry.IdentityId = row.Id;
                }
                else
                {
                    var row = rows.FirstOrDefault(r => r.Id == entry.IdentityId.Value);
                    if (row == null)
                        continue;

                    var changed = false;
                    if (row.Kind != entry.Kind) { row.Kind = entry.Kind; changed = true; }
                    if (row.AgentFilePath != entry.AgentFilePath) { row.AgentFilePath = entry.AgentFilePath; changed = true; }
                    if (row.RegistrationError != entry.RegistrationError) { row.RegistrationError = entry.RegistrationError; changed = true; }

                    // Decadenza del trust (R3): il reconciler l'ha già rilevata sull'entry;
                    // qui la rendiamo persistente (Trusted/Enabled → false, riconferma umana).
                    if (entry.TrustDecayed && (row.Trusted || row.Enabled))
                    {
                        row.Trusted = false;
                        row.Enabled = false;
                        changed = true;
                    }

                    if (changed)
                    {
                        row.UpdatedAt = now;
                        dal.Save(row);
                    }
                }
            }
        }

        private static string Normalize(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return projectPath;
            try { return Path.GetFullPath(projectPath).TrimEnd('/', '\\'); }
            catch { return projectPath.TrimEnd('/', '\\'); }
        }

        private static bool PathEquals(string a, string b)
            => string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
    }
}
