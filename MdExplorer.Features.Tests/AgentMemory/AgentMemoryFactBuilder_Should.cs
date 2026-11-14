using System;
using System.Collections.Generic;
using MdExplorer.Features.AgentMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace MdExplorer.Features.Tests.AgentMemory
{
    /// <summary>
    /// Fase 5b — i builder SPARQL della memoria: le triple asserite sono PROV-O ben formate
    /// (parse-back con dotNetRDF) e la query è ristretta ai soli grafi ammessi.
    /// </summary>
    [TestClass]
    public class AgentMemoryFactBuilder_Should
    {
        private static readonly Guid AgentId = Guid.Parse("aaaaaaaa-1111-2222-3333-444444444444");
        private static string AgentGraph => AgentMemoryGraphs.ForAgent(AgentId);

        private static LearnedFactInput Fact(string statement, params string[] tags) => new LearnedFactInput
        {
            Statement = statement,
            Confidence = 0.8,
            AboutTags = tags,
            RunId = Guid.Parse("bbbbbbbb-1111-2222-3333-444444444444"),
            ConversationId = "cccccccc-1111-2222-3333-444444444444",
            CreatedAtUtc = new DateTime(2026, 7, 17, 12, 0, 0, DateTimeKind.Utc),
        };

        [TestMethod]
        public void Produce_a_parseable_prov_update()
        {
            var (update, factUri) = AgentMemoryFactBuilder.BuildAssertUpdate(AgentGraph, AgentId, Fact("il batch gira alle 02:00", "pagamenti"));
            StringAssert.StartsWith(factUri, "urn:mde:mem:fact:");

            // dotNetRDF deve accettare l'update come SPARQL valido: se una literal fosse
            // spezzata (injection) il parse fallirebbe qui.
            var parsed = new SparqlUpdateParser().ParseFromString(update);
            Assert.AreEqual(1, parsed.CommandCount);
        }

        [TestMethod]
        public void Assert_the_fact_into_the_agent_graph_only()
        {
            var (update, _) = AgentMemoryFactBuilder.BuildAssertUpdate(AgentGraph, AgentId, Fact("x", "t1", "t2"));
            var store = new TripleStore();
            store.ExecuteUpdate(update);

            var g = store[new Uri(AgentGraph)];
            Assert.IsTrue(g.Triples.Count >= 6, "fatto + provenance");
            // Niente triple fuori dal grafo dell'agente: shared mai toccato.
            Assert.IsFalse(store.HasGraph(new Uri(AgentMemoryGraphs.Shared)), "nulla scritto in shared");
            // Ogni grafo materializzato oltre a quello dell'agente non deve contenere triple.
            foreach (var graph in store.Graphs)
                if (graph.Name?.ToString() != AgentGraph)
                    Assert.AreEqual(0, graph.Triples.Count, $"grafo inatteso non vuoto: {graph.Name}");
        }

        [TestMethod]
        public void Survive_a_hostile_statement()
        {
            // Tentativo di chiudere la literal e iniettare un DROP.
            var hostile = "y\" } } ; DROP GRAPH <" + AgentMemoryGraphs.Shared + "> ; INSERT DATA { GRAPH <x> { <a> <b> \"z";
            var (update, _) = AgentMemoryFactBuilder.BuildAssertUpdate(AgentGraph, AgentId, Fact(hostile, "t"));
            var store = new TripleStore();
            store.ExecuteUpdate(update);  // non deve eseguire il DROP

            // Il grafo shared non è mai stato toccato; lo statement è tornato intatto.
            var g = store[new Uri(AgentGraph)];
            var found = false;
            foreach (var t in g.Triples)
                if (t.Object is ILiteralNode l && l.Value == hostile) found = true;
            Assert.IsTrue(found, "lo statement ostile deve essere UNA sola literal intatta");
        }

        [TestMethod]
        public void Restrict_query_to_agent_and_shared_graphs()
        {
            var q = AgentMemoryFactBuilder.BuildQuery(AgentGraph, new[] { "pagamenti" }, 20);
            // Deve nominare ESATTAMENTE i due grafi ammessi nel FILTER di scope.
            StringAssert.Contains(q, AgentGraph);
            StringAssert.Contains(q, AgentMemoryGraphs.Shared);
            StringAssert.Contains(q, "FILTER(?g IN");
            StringAssert.Contains(q, "\"pagamenti\"");
            // Parse-back: query SPARQL valida.
            new VDS.RDF.Parsing.SparqlQueryParser().ParseFromString(q);
        }

        [TestMethod]
        public void Query_without_topics_has_no_tag_filter()
        {
            var q = AgentMemoryFactBuilder.BuildQuery(AgentGraph, Array.Empty<string>(), 20);
            StringAssert.Contains(q, "FILTER(?g IN");
            Assert.IsFalse(q.Contains("?matchTag"), "senza topics niente filtro per tag");
            new VDS.RDF.Parsing.SparqlQueryParser().ParseFromString(q);
        }

        [TestMethod]
        public void Clamp_confidence_and_fail_loud_on_empty_statement()
        {
            var f = Fact("ok");
            f.Confidence = 5.0;   // fuori range
            var (update, _) = AgentMemoryFactBuilder.BuildAssertUpdate(AgentGraph, AgentId, f);
            StringAssert.Contains(update, "mdeag:confidence 1.0");

            Assert.ThrowsException<ArgumentException>(() =>
                AgentMemoryFactBuilder.BuildAssertUpdate(AgentGraph, AgentId, Fact("   ")));
        }
    }
}
