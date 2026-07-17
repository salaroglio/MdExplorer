using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.AgentMemory
{
    /// <summary>Un fatto appreso pronto da asserire (input strutturato — R8, mai SPARQL grezza).</summary>
    public sealed class LearnedFactInput
    {
        public string Statement { get; set; }
        public double Confidence { get; set; }
        /// <summary>Tag testuali dell'argomento (allineati ai topics del messaggio). Possono essere vuoti.</summary>
        public IReadOnlyList<string> AboutTags { get; set; } = Array.Empty<string>();
        /// <summary>Run in cui è stato appreso (provenance). Guid del RunToken.</summary>
        public Guid RunId { get; set; }
        /// <summary>Conversazione del run (provenance). Vuoto se non correlato.</summary>
        public string ConversationId { get; set; }
        /// <summary>Istante di apprendimento (UTC).</summary>
        public DateTime CreatedAtUtc { get; set; }
    }

    /// <summary>
    /// Compone il testo SPARQL per la memoria (Fase 5b). <b>Nessun grafo arriva dal chiamante</b>:
    /// il grafo dell'agente è passato qui già risolto server-side dal RunToken (§11, come <c>$pid</c>
    /// per Neo4j). Tutte le literal passano da <see cref="SparqlText"/> — anti-injection.
    /// </summary>
    public static class AgentMemoryFactBuilder
    {
        private const string P = "PREFIX mdeag: <" + AgentMemoryOntology.Prefix + ">\n" +
                                 "PREFIX prov: <" + AgentMemoryOntology.ProvPrefix + ">\n" +
                                 "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>\n";

        /// <summary>
        /// INSERT del fatto + provenance PROV-O nel grafo <paramref name="agentGraph"/>: il fatto
        /// (<c>mdeag:LearnedFact</c> con statement/confidence/aboutTag/createdAt),
        /// <c>mdeag:learnedDuring</c> un <c>mdeag:AgentRun</c> associato all'agente e agganciato
        /// alla conversazione. Ritorna il fatto-URI coniato, per il caller.
        /// </summary>
        public static (string Update, string FactUri) BuildAssertUpdate(string agentGraph, Guid agentIdentityId, LearnedFactInput fact)
        {
            if (string.IsNullOrWhiteSpace(agentGraph)) throw new ArgumentException("agentGraph richiesto.", nameof(agentGraph));
            if (fact == null) throw new ArgumentNullException(nameof(fact));
            if (string.IsNullOrWhiteSpace(fact.Statement)) throw new ArgumentException("statement richiesto.", nameof(fact));
            if (fact.RunId == Guid.Empty) throw new ArgumentException("runId richiesto per la provenance.", nameof(fact));

            var factUri = $"urn:mde:mem:fact:{Guid.NewGuid():D}";
            var runUri = $"urn:mde:mem:run:{fact.RunId:D}";
            var agentUri = $"urn:mde:mem:agentself:{agentIdentityId:D}";

            var sb = new StringBuilder();
            sb.Append(P);
            sb.Append("INSERT DATA {\n  GRAPH ").Append(SparqlText.Iri(agentGraph)).Append(" {\n");
            sb.Append("    ").Append(SparqlText.Iri(factUri)).Append(" a mdeag:LearnedFact ;\n");
            sb.Append("      mdeag:statement ").Append(SparqlText.Literal(fact.Statement)).Append(" ;\n");
            sb.Append("      mdeag:confidence ").Append(SparqlText.Double(ClampConfidence(fact.Confidence))).Append(" ;\n");
            sb.Append("      mdeag:createdAt ").Append(SparqlText.DateTime(fact.CreatedAtUtc)).Append(" ;\n");
            foreach (var tag in NormalizeTags(fact.AboutTags))
                sb.Append("      mdeag:aboutTag ").Append(SparqlText.Literal(tag)).Append(" ;\n");
            sb.Append("      mdeag:learnedDuring ").Append(SparqlText.Iri(runUri)).Append(" .\n");
            sb.Append("    ").Append(SparqlText.Iri(runUri)).Append(" a mdeag:AgentRun ;\n");
            sb.Append("      prov:wasAssociatedWith ").Append(SparqlText.Iri(agentUri));
            if (!string.IsNullOrWhiteSpace(fact.ConversationId) && Guid.TryParse(fact.ConversationId, out var conv))
                sb.Append(" ;\n      mdeag:inConversation ").Append(SparqlText.Iri($"urn:mde:mem:conv:{conv:D}"));
            sb.Append(" .\n  }\n}");
            return (sb.ToString(), factUri);
        }

        /// <summary>
        /// SELECT dei fatti dai due soli grafi ammessi (<paramref name="agentGraph"/> + shared),
        /// opzionalmente filtrati per tag. Il vincolo <c>FILTER(?g IN (...))</c> è la barriera:
        /// non si vedono MAI i grafi di altri agenti.
        /// </summary>
        public static string BuildQuery(string agentGraph, IReadOnlyList<string> topics, int limit)
        {
            if (string.IsNullOrWhiteSpace(agentGraph)) throw new ArgumentException("agentGraph richiesto.", nameof(agentGraph));
            var lim = Math.Clamp(limit <= 0 ? 20 : limit, 1, 200);

            var tags = NormalizeTags(topics);

            var sb = new StringBuilder();
            sb.Append(P);
            sb.Append("SELECT ?statement ?confidence (GROUP_CONCAT(DISTINCT ?tag; separator=\",\") AS ?tags) ?g\n");
            sb.Append("WHERE {\n");
            sb.Append("  GRAPH ?g {\n");
            sb.Append("    ?f a mdeag:LearnedFact ;\n");
            sb.Append("       mdeag:statement ?statement ;\n");
            sb.Append("       mdeag:confidence ?confidence .\n");
            sb.Append("    OPTIONAL { ?f mdeag:aboutTag ?tag }\n");
            if (tags.Count > 0)
            {
                // Il vincolo dei tag deve stare DENTRO il GRAPH ?g: i tag vivono nel named
                // graph, non nel default graph. Un fatto è rilevante se ALMENO un suo tag è
                // tra i topics richiesti (il GROUP BY collassa i duplicati).
                sb.Append("    ?f mdeag:aboutTag ?matchTag .\n");
                sb.Append("    FILTER(?matchTag IN (")
                  .Append(string.Join(", ", tags.Select(SparqlText.Literal)))
                  .Append("))\n");
            }
            sb.Append("  }\n");
            // Barriera di scope: solo il grafo dell'agente + shared.
            sb.Append("  FILTER(?g IN (").Append(SparqlText.Iri(agentGraph)).Append(", ")
              .Append(SparqlText.Iri(AgentMemoryGraphs.Shared)).Append("))\n");
            sb.Append("}\n");
            sb.Append("GROUP BY ?statement ?confidence ?g\n");
            sb.Append("ORDER BY DESC(?confidence)\n");
            sb.Append("LIMIT ").Append(lim);
            return sb.ToString();
        }

        private static double ClampConfidence(double c) => Math.Clamp(c, 0.0, 1.0);

        private static List<string> NormalizeTags(IReadOnlyList<string> tags)
            => (tags ?? Array.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
