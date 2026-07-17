using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.AgentMemory;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentMemory
{
    /// <summary>Coordinate di connessione a un dataset Fuseki (risolte dai settings di progetto).</summary>
    public sealed class FusekiConnection
    {
        public string BaseUri { get; set; }
        public string Dataset { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>Un fatto recuperato dalla memoria (proiezione della query di risveglio).</summary>
    public sealed class MemoryFact
    {
        public string Statement { get; set; }
        public double Confidence { get; set; }
        public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
        public bool Shared { get; set; }
    }

    /// <summary>
    /// La memoria semantica degli agenti (Fase 5b, §11) sopra Fuseki. <b>Non conosce il RunToken</b>:
    /// riceve dal controller il <c>agentIdentityId</c> GIÀ risolto e ne deriva il named graph con
    /// <see cref="AgentMemoryGraphs"/>. Il grafo NON è mai un parametro del chiamante esterno —
    /// l'enforcement è nel controller, questo servizio è il braccio verso Fuseki.
    /// </summary>
    public interface IAgentMemoryService
    {
        /// <summary>Crea il dataset se manca e carica l'ontologia mde-agent v0 (idempotente).</summary>
        Task EnsureReadyAsync(FusekiConnection conn);

        /// <summary>Assere un fatto nel grafo dell'agente; ritorna l'URI del fatto coniato.</summary>
        Task<string> AssertFactAsync(FusekiConnection conn, Guid agentIdentityId, LearnedFactInput fact);

        /// <summary>Recupera i fatti dal grafo dell'agente + shared, filtrati per topic.</summary>
        Task<IReadOnlyList<MemoryFact>> QueryAsync(FusekiConnection conn, Guid agentIdentityId, IReadOnlyList<string> topics, int limit);
    }

    public class AgentMemoryService : IAgentMemoryService
    {
        private readonly IFusekiClient _fuseki;
        private readonly ILogger<AgentMemoryService> _logger;

        // Dataset già preparati (ensure+ontologia) in questa esecuzione: evita il round-trip
        // GSP a ogni chiamata. La ricarica dell'ontologia sarebbe comunque idempotente (RDF set).
        private static readonly ConcurrentDictionary<string, bool> _ready = new();

        public AgentMemoryService(IFusekiClient fuseki, ILogger<AgentMemoryService> logger)
        {
            _fuseki = fuseki;
            _logger = logger;
        }

        public async Task EnsureReadyAsync(FusekiConnection conn)
        {
            Validate(conn);
            var key = conn.BaseUri + "|" + conn.Dataset;
            if (_ready.ContainsKey(key)) return;

            var created = await _fuseki.EnsureDatasetAsync(conn.BaseUri, conn.Dataset, conn.Username, conn.Password);
            if (!created)
                throw new InvalidOperationException($"Dataset Fuseki '{conn.Dataset}' non disponibile su '{conn.BaseUri}'.");

            // Carica la TBox nel suo grafo dedicato (append idempotente via GSP).
            await _fuseki.LoadGraphAsync(conn.BaseUri, conn.Dataset, AgentMemoryOntology.OntologyGraph,
                AgentMemoryOntology.Turtle(), conn.Username, conn.Password);

            _ready[key] = true;
            _logger.LogInformation("[AgentMemory] dataset '{Dataset}' pronto + ontologia mde-agent caricata", conn.Dataset);
        }

        public async Task<string> AssertFactAsync(FusekiConnection conn, Guid agentIdentityId, LearnedFactInput fact)
        {
            Validate(conn);
            await EnsureReadyAsync(conn);

            var graph = AgentMemoryGraphs.ForAgent(agentIdentityId);   // grafo forzato dall'identità
            var (update, factUri) = AgentMemoryFactBuilder.BuildAssertUpdate(graph, agentIdentityId, fact);
            await _fuseki.UpdateAsync(conn.BaseUri, conn.Dataset, update, conn.Username, conn.Password);
            _logger.LogInformation("[AgentMemory] fatto asserito nel grafo dell'agente {Id}: {Uri}", agentIdentityId, factUri);
            return factUri;
        }

        public async Task<IReadOnlyList<MemoryFact>> QueryAsync(FusekiConnection conn, Guid agentIdentityId, IReadOnlyList<string> topics, int limit)
        {
            Validate(conn);
            await EnsureReadyAsync(conn);

            var graph = AgentMemoryGraphs.ForAgent(agentIdentityId);
            var sparql = AgentMemoryFactBuilder.BuildQuery(graph, topics, limit);
            var json = await _fuseki.QueryAsync(conn.BaseUri, conn.Dataset, sparql, conn.Username, conn.Password);
            return ParseResults(json, graph);
        }

        private static IReadOnlyList<MemoryFact> ParseResults(string sparqlResultsJson, string agentGraph)
        {
            var list = new List<MemoryFact>();
            using var doc = JsonDocument.Parse(sparqlResultsJson);
            if (!doc.RootElement.TryGetProperty("results", out var results)) return list;
            if (!results.TryGetProperty("bindings", out var bindings)) return list;

            foreach (var b in bindings.EnumerateArray())
            {
                var statement = Str(b, "statement");
                if (string.IsNullOrEmpty(statement)) continue;
                double.TryParse(Str(b, "confidence"), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var conf);
                var tagsRaw = Str(b, "tags");
                var tags = string.IsNullOrWhiteSpace(tagsRaw)
                    ? Array.Empty<string>()
                    : tagsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var g = Str(b, "g");
                list.Add(new MemoryFact
                {
                    Statement = statement,
                    Confidence = conf,
                    Tags = tags,
                    Shared = g == AgentMemoryGraphs.Shared,
                });
            }
            return list;
        }

        private static string Str(JsonElement binding, string name)
            => binding.TryGetProperty(name, out var v) && v.TryGetProperty("value", out var val)
                ? val.GetString()
                : null;

        private static void Validate(FusekiConnection conn)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (string.IsNullOrWhiteSpace(conn.BaseUri)) throw new ArgumentException("BaseUri Fuseki mancante.", nameof(conn));
            if (string.IsNullOrWhiteSpace(conn.Dataset)) throw new ArgumentException("Dataset Fuseki mancante.", nameof(conn));
        }
    }
}
