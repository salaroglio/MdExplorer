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

        public void OnProjectOpened(string projectPath) => QueueRefresh(projectPath, "projectOpen");

        public void OnAgentFileChanged(string projectPath) => QueueRefresh(projectPath, "fsw");

        // -------------------------------------------------------------------------

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
