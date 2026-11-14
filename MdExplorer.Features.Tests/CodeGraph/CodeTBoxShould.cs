using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Il TBox che MarkAgent legge prima di usare il grafo (11/11/2026, MCP <c>code_ontology</c>): dice tutto ciò che gli
    /// script possono contenere, e resta allineato da solo — una classe, un predicato, un tipo o una relazione nuovi senza
    /// la loro riga fanno fallire questo test.
    /// </summary>
    [TestClass]
    public class CodeTBoxShould
    {
        [TestMethod]
        public void Describe_every_class_property_kind_and_relation_the_scripts_can_hold()
        {
            CollectionAssert.AreEquivalent(CodeScriptTurtle.Allowed.Keys.ToList(), CodeTBox.Classes.Keys.ToList(), "una classe degli script senza descrizione, o viceversa");
            var missing = CodeTBox.Rows().Where(r => r.Range == null).Select(r => r.Class + "." + r.Property).ToList();
            Assert.AreEqual(0, missing.Count, "predicati senza codominio e senso nel TBox: " + string.Join(", ", missing));
            foreach (var r in CodeScriptParser.Relations) Assert.IsTrue(CodeGraphSchema.Relations.ContainsKey(r), $"relazione '{r}' senza senso");
            foreach (var k in CodeScriptParser.SymbolKinds) Assert.IsTrue(CodeGraphSchema.SymbolKinds.ContainsKey(k), $"tipo '{k}' senza senso");
        }

        [TestMethod]
        public void Be_valid_turtle_with_domains_and_ranges()
        {
            var turtle = CodeTBox.Turtle();
            var g = new Graph();
            new TurtleParser().Load(g, new System.IO.StringReader(turtle));
            Assert.IsTrue(g.Triples.Count > 200, "classi, proprietà, relazioni");
            StringAssert.Contains(turtle, "code:writes a owl:ObjectProperty ; rdfs:subPropertyOf code:Relation ; rdfs:domain code:Symbol ; rdfs:range code:Symbol");
            StringAssert.Contains(turtle, "code:argumentOf a owl:ObjectProperty ; rdfs:domain [ owl:unionOf ( code:StringArgument code:TypeArgument code:AnnotationArgument ) ] ; rdfs:range [ owl:unionOf ( code:CallSite code:Annotation ) ]");
            StringAssert.Contains(turtle, "code:lineStart a owl:DatatypeProperty ; rdfs:domain code:Symbol ; rdfs:range xsd:integer");
            StringAssert.Contains(turtle, "\"external\"");
        }
    }
}
