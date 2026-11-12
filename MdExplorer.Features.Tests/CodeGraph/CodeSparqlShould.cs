using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Il controllo delle query SPARQL di MarkAgent (<c>code_sparql</c>, 12/11/2026), prima che arrivino a Fuseki.</summary>
    [TestClass]
    public class CodeSparqlShould
    {
        private const string Graph = "urn:mde:code:data:abc";

        [TestMethod]
        public void Put_the_where_block_inside_the_project_graph_with_the_prefixes()
        {
            var (q, ask) = CodeSparql.Prepare("SELECT ?rel (COUNT(?e) AS ?n) WHERE { ?e a code:Edge ; code:relation ?rel . { SELECT ?e WHERE { ?e code:path ?p } } } GROUP BY ?rel ORDER BY DESC(?n)", Graph);
            Assert.IsFalse(ask);
            StringAssert.StartsWith(q, CodeSparql.Prefixes);
            StringAssert.Contains(q, "WHERE { GRAPH <" + Graph + "> {  ?e a code:Edge");
            StringAssert.EndsWith(q, "} } GROUP BY ?rel ORDER BY DESC(?n)");
        }

        [TestMethod]
        public void Accept_ask_and_keywords_inside_strings_iris_comments_and_names()
        {
            var (q, ask) = CodeSparql.Prepare("# FROM nella nota\nASK { ?s code:qualifiedName ?qn . ?s <urn:x:graph> ?from . FILTER(CONTAINS(?qn, \"DELETE FROM X\")) }", Graph);
            Assert.IsTrue(ask);
            StringAssert.Contains(q, "GRAPH <" + Graph + ">");
        }

        [DataTestMethod]
        [DataRow("INSERT DATA { <urn:a> <urn:b> <urn:c> }", "SELECT")]
        [DataRow("SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }", "GRAPH")]
        [DataRow("SELECT * FROM <urn:altro> WHERE { ?s ?p ?o }", "FROM")]
        [DataRow("SELECT * WHERE { SERVICE <http://x/sparql> { ?s ?p ?o } }", "SERVICE")]
        [DataRow("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }", "CONSTRUCT")]
        [DataRow("SELECT * WHERE { ?s ?p ?o ", "braces")]
        [DataRow("   ", "empty")]
        public void Refuse_what_is_not_a_read_of_the_project_graph(string query, string reason)
        {
            var ex = Assert.ThrowsException<CodeSparqlException>(() => CodeSparql.Prepare(query, Graph));
            StringAssert.Contains(ex.Message, reason);
        }
    }
}
