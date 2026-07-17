using System;
using System.Linq;
using MdExplorer.Features.AgentMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace MdExplorer.Features.Tests.AgentMemory
{
    /// <summary>
    /// Fase 5a — l'escaping delle literal è il cuore di sicurezza del path SPARQL:
    /// tutto ciò che un LLM asserisce passa da qui prima di toccare Fuseki. La proprietà
    /// verificata è il <b>round-trip</b>: il termine formattato, ri-parsato dentro una
    /// tripla Turtle, torna a essere ESATTAMENTE una literal col valore originale — quindi
    /// nessun input può spezzare la literal e iniettare altri statement.
    /// </summary>
    [TestClass]
    public class SparqlText_Should
    {
        // Inserisce il termine come oggetto di una tripla, la parsa, e ritorna il valore
        // della literal — fallisce se il termine ha prodotto qualcosa di diverso da 1 literal.
        private static string RoundTrip(string formattedTerm)
        {
            var g = new Graph();
            new TurtleParser().Load(g, new System.IO.StringReader($"<urn:s> <urn:p> {formattedTerm} ."));
            var triples = g.Triples.ToList();
            Assert.AreEqual(1, triples.Count, "il termine deve produrre esattamente UNA tripla");
            var obj = triples[0].Object as ILiteralNode;
            Assert.IsNotNull(obj, "l'oggetto deve essere una literal");
            return obj.Value;
        }

        [TestMethod]
        public void Round_trip_quotes_backslashes_and_newlines()
        {
            var original = "riga1\nriga2 \"quoted\" back\\slash";
            Assert.AreEqual(original, RoundTrip(SparqlText.Literal(original)));
        }

        [TestMethod]
        public void Block_sparql_injection_via_literal()
        {
            // Un "fatto" ostile che prova a chiudere la literal e iniettare un update.
            var hostile = "x\" . } ; DROP GRAPH <urn:mde:mem:shared> ; INSERT DATA { <a> <b> \"y";
            // Se torna intatto come UN solo valore, l'iniezione è impossibile.
            Assert.AreEqual(hostile, RoundTrip(SparqlText.Literal(hostile)));
        }

        [TestMethod]
        public void Round_trip_unicode()
        {
            Assert.AreEqual("città ⚙️ 中文", RoundTrip(SparqlText.Literal("città ⚙️ 中文")));
        }

        [TestMethod]
        public void Format_double_invariant()
        {
            Assert.AreEqual("0.85", SparqlText.Double(0.85));
            Assert.AreEqual("1.0", SparqlText.Double(1));
        }

        [TestMethod]
        public void Format_datetime_as_utc_xsd()
        {
            var s = SparqlText.DateTime(new DateTime(2026, 7, 17, 10, 30, 0, DateTimeKind.Utc));
            StringAssert.StartsWith(s, "\"2026-07-17T10:30:00.000Z\"");
            StringAssert.Contains(s, "XMLSchema#dateTime");
        }

        [TestMethod]
        public void Fail_loud_on_invalid_iri()
        {
            Assert.ThrowsException<ArgumentException>(() => SparqlText.Iri("non un uri"));
            Assert.ThrowsException<ArgumentException>(() => SparqlText.Iri(""));
            Assert.AreEqual("<urn:mde:mem:shared>", SparqlText.Iri("urn:mde:mem:shared"));
        }

        [TestMethod]
        public void Name_agent_graphs_deterministically()
        {
            var id = Guid.Parse("11111111-2222-3333-4444-555555555555");
            Assert.AreEqual("urn:mde:mem:agent:11111111-2222-3333-4444-555555555555", AgentMemoryGraphs.ForAgent(id));
            Assert.AreEqual("urn:mde:mem:shared", AgentMemoryGraphs.Shared);
            Assert.ThrowsException<ArgumentException>(() => AgentMemoryGraphs.ForAgent(Guid.Empty));
        }
    }
}
