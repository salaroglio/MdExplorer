using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.AgentMemory;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentMemory
{
    /// <summary>Un fatto scelto dall'umano per la promozione (§5.1.d): il curatore è l'umano.</summary>
    public sealed class PromoteFactInput
    {
        public string FactUri { get; set; }
        public string Graph { get; set; }
        public string Statement { get; set; }
    }

    /// <summary>Esito del consolidamento di una conversazione.</summary>
    public sealed class ConsolidationResult
    {
        public bool MemoryDisabled { get; set; }
        public int Promoted { get; set; }
        public int Decayed { get; set; }
        public int Deleted { get; set; }
        public List<string> Agents { get; set; } = new();
    }

    /// <summary>
    /// Il <b>consolidamento</b> (Fase 7f, §5.1): un gesto umano per-conversazione che promuove i
    /// fatti durevoli scelti dalla memoria Fuseki nel <c>.agent.md</c> (il "diploma", versionato e
    /// condiviso via git) e <b>decade</b> il resto (abbassa la confidence, cancella sotto il
    /// pavimento). Un solo atto = promozione + decadimento.
    /// </summary>
    public interface IMemoryConsolidationService
    {
        Task<ConsolidationResult> ConsolidateAsync(
            string projectPath, Guid conversationId, IReadOnlyList<PromoteFactInput> promote, CancellationToken ct = default);
    }

    public class MemoryConsolidationService : IMemoryConsolidationService
    {
        // Taratura del decadimento (§5.1): fattore moltiplicativo, pavimento sotto cui si cancella,
        // e soglia-alta = proxy di confirmedBy (⚠️ non cablato in v1) esclusa dal decadimento.
        private const double DecayFactor = 0.5;
        private const double DecayFloor = 0.15;
        private const double ConfirmedProxyThreshold = 0.9;

        private const string BlockStart = "<!-- mde:consolidated:start -->";
        private const string BlockEnd = "<!-- mde:consolidated:end -->";
        private const string BlockHeading = "## Memoria consolidata";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMemoryService _memory;
        private readonly IFusekiConnectionResolver _fusekiResolver;
        private readonly ILogger<MemoryConsolidationService> _logger;

        public MemoryConsolidationService(
            IServiceScopeFactory scopeFactory,
            IAgentRegistryService registry,
            IAgentMemoryService memory,
            IFusekiConnectionResolver fusekiResolver,
            ILogger<MemoryConsolidationService> logger)
        {
            _scopeFactory = scopeFactory;
            _registry = registry;
            _memory = memory;
            _fusekiResolver = fusekiResolver;
            _logger = logger;
        }

        public async Task<ConsolidationResult> ConsolidateAsync(
            string projectPath, Guid conversationId, IReadOnlyList<PromoteFactInput> promote, CancellationToken ct = default)
        {
            var result = new ConsolidationResult();

            FusekiConnection conn;
            try { conn = await _fusekiResolver.ResolveAsync(projectPath); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Consolidate] risoluzione Fuseki fallita per '{Project}'", projectPath);
                result.MemoryDisabled = true;
                return result;
            }
            if (conn == null)
            {
                // Memoria non abilitata: niente da consolidare (non è un errore, §7f).
                result.MemoryDisabled = true;
                return result;
            }

            // Agenti coinvolti nella conversazione = distinct(From ∪ To) meno "user" (§7f.2: il
            // pattern del MailboxController NON esclude user → lo escludo qui).
            var participants = ResolveParticipants(conversationId);
            if (participants.Count == 0)
                return result;

            // Nome agente → grafo (inverso di ResolveGraphNames) + nome → AgentFilePath.
            var catalog = _registry.RefreshCatalog(projectPath)
                .Where(e => e.IsCitizen && e.IdentityId != null).ToList();
            var nameToGraph = catalog.ToDictionary(e => e.Name, e => AgentMemoryGraphs.ForAgent(e.IdentityId.Value), StringComparer.OrdinalIgnoreCase);
            var nameToFile = catalog.ToDictionary(e => e.Name, e => e.AgentFilePath, StringComparer.OrdinalIgnoreCase);

            var promoteByGraph = (promote ?? Array.Empty<PromoteFactInput>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Graph) && !string.IsNullOrWhiteSpace(p.FactUri))
                .GroupBy(p => p.Graph, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

            foreach (var agentName in participants)
            {
                ct.ThrowIfCancellationRequested();
                if (!nameToGraph.TryGetValue(agentName, out var graph))
                    continue;   // partecipante senza identità/grafo (es. agente non-cittadino)

                var promotedUris = new HashSet<string>(StringComparer.Ordinal);

                // 1) Promozione: scrivi i fatti scelti nel .agent.md, poi rimuovili da Fuseki.
                if (promoteByGraph.TryGetValue(graph, out var toPromote) && toPromote.Count > 0
                    && nameToFile.TryGetValue(agentName, out var agentFile) && !string.IsNullOrWhiteSpace(agentFile))
                {
                    var statements = toPromote
                        .Select(p => (p.Statement ?? string.Empty).Trim())
                        .Where(s => s.Length > 0)
                        .ToList();
                    try
                    {
                        WriteConsolidatedSection(agentFile, statements);
                        foreach (var p in toPromote)
                        {
                            await _memory.DeleteFactAsync(conn, graph, p.FactUri);
                            promotedUris.Add(p.FactUri);
                            result.Promoted++;
                        }
                        if (!result.Agents.Contains(agentName)) result.Agents.Add(agentName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[Consolidate] promozione fallita per '{Agent}'", agentName);
                    }
                }

                // 2) Decadimento: i fatti NON promossi dell'agente perdono confidence; sotto il
                // pavimento si cancellano. I fatti a confidence alta (proxy confirmedBy) sono esclusi.
                try
                {
                    var facts = await _memory.ListAsync(conn, new[] { graph }, 1000);
                    foreach (var f in facts)
                    {
                        if (promotedUris.Contains(f.FactUri)) continue;              // appena promosso
                        if (f.Confidence >= ConfirmedProxyThreshold) continue;       // proxy confirmedBy
                        var newConf = f.Confidence * DecayFactor;
                        if (newConf < DecayFloor)
                        {
                            await _memory.DeleteFactAsync(conn, graph, f.FactUri);
                            result.Deleted++;
                        }
                        else
                        {
                            await _memory.SetConfidenceAsync(conn, graph, f.FactUri, newConf);
                            result.Decayed++;
                        }
                    }
                    if (!result.Agents.Contains(agentName)) result.Agents.Add(agentName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Consolidate] decadimento fallito per '{Agent}'", agentName);
                }
            }

            _logger.LogInformation("[Consolidate] conversazione {Conv}: {P} promossi, {D} decaduti, {X} cancellati su {N} agenti.",
                conversationId, result.Promoted, result.Decayed, result.Deleted, result.Agents.Count);
            return result;
        }

        private List<string> ResolveParticipants(Guid conversationId)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var msgs = db.GetDal<AgentMessage>().GetList().Where(m => m.ConversationId == conversationId).ToList();
                db.Commit();
                foreach (var m in msgs)
                {
                    Add(set, m.FromAgent);
                    Add(set, m.ToAgent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Consolidate] risoluzione partecipanti fallita per {Conv}", conversationId);
            }
            return set.ToList();

            static void Add(HashSet<string> s, string name)
            {
                var n = (name ?? string.Empty).Trim();
                // §7f.2: escludi "user" (il pattern del MailboxController non lo fa).
                if (n.Length > 0 && !string.Equals(n, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                    s.Add(n);
            }
        }

        /// <summary>
        /// Scrive/aggiorna il blocco "## Memoria consolidata" nel <c>.agent.md</c> in modo
        /// idempotente e <b>accumulativo</b> (i fatti promossi in Fuseki vengono cancellati, quindi
        /// il markdown è l'unica sede durevole): fonde i bullet esistenti coi nuovi (dedup). Appende
        /// sotto il frontmatter → NON tocca l'hash del blocco a2a: (niente trust-decay).
        /// </summary>
        private static void WriteConsolidatedSection(string agentFilePath, IReadOnlyList<string> newStatements)
        {
            if (newStatements.Count == 0 || string.IsNullOrWhiteSpace(agentFilePath) || !File.Exists(agentFilePath))
                return;

            var text = File.ReadAllText(agentFilePath);
            var existing = ParseExistingBullets(text);
            var merged = new List<string>(existing);
            var seen = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
            foreach (var s in newStatements)
                if (seen.Add(s)) merged.Add(s);

            var block = BuildBlock(merged);

            int start = text.IndexOf(BlockStart, StringComparison.Ordinal);
            int end = text.IndexOf(BlockEnd, StringComparison.Ordinal);
            string updated;
            if (start >= 0 && end > start)
            {
                var before = text.Substring(0, start);
                var after = text.Substring(end + BlockEnd.Length);
                updated = before + block + after;
            }
            else
            {
                var sep = text.EndsWith("\n") ? "\n" : "\n\n";
                updated = text + sep + block + "\n";
            }
            File.WriteAllText(agentFilePath, updated, new UTF8Encoding(false));
        }

        private static List<string> ParseExistingBullets(string text)
        {
            var list = new List<string>();
            int start = text.IndexOf(BlockStart, StringComparison.Ordinal);
            int end = text.IndexOf(BlockEnd, StringComparison.Ordinal);
            if (start < 0 || end <= start) return list;
            var inner = text.Substring(start + BlockStart.Length, end - start - BlockStart.Length);
            foreach (var raw in inner.Split('\n'))
            {
                var t = raw.Trim();
                if (t.StartsWith("- ", StringComparison.Ordinal))
                    list.Add(t.Substring(2).Trim());
            }
            return list;
        }

        private static string BuildBlock(IReadOnlyList<string> statements)
        {
            var sb = new StringBuilder();
            sb.Append(BlockStart).Append('\n');
            sb.Append(BlockHeading).Append("\n\n");
            foreach (var s in statements)
                sb.Append("- ").Append(s).Append('\n');
            sb.Append(BlockEnd);
            return sb.ToString();
        }
    }
}
