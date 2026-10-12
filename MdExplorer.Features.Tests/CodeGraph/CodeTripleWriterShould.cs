using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>D19: da uno script alle triple, senza Fuseki. IRI stabili, literal con gli escape, una richiesta per script.</summary>
    [TestClass]
    public class CodeTripleWriterShould
    {
        [TestMethod]
        public void Build_stable_iris_under_urn_mde_and_escape_literals()
        {
            Assert.AreEqual("urn:mde:sym:abc", CodeGraphIris.Symbol("abc"));
            Assert.AreEqual("abc", CodeGraphIris.SymbolId("urn:mde:sym:abc"));
            Assert.AreEqual("urn:mde:sym:abc:p2", CodeGraphIris.Parameter("abc", 2));
            Assert.AreEqual("urn:mde:file:src%2FDemo%2FFoo%20Bar.cs", CodeGraphIris.File("src/Demo/Foo Bar.cs"), "senza hash del progetto: va negli script su Git");
            Assert.AreEqual("urn:mde:file:%40backend%2Fsrc%2FFoo.cs", CodeGraphIris.File("@backend/src/Foo.cs"));
            Assert.AreEqual("urn:mde:edge:a:calls:b:12", CodeGraphIris.Edge("a", "calls", "b", 12));
            Assert.AreEqual("urn:mde:code:h1:data", CodeGraphIris.DataGraph("h1"));
            Assert.AreEqual("\"a \\\"quoted\\\" line\\nnext \\\\ back\"", CodeGraphIris.Lit("a \"quoted\" line\nnext \\ back"));
            Assert.AreEqual("code:implements", CodeGraphIris.RelationProperty("implements"));
        }

        [TestMethod]
        public void Write_one_update_per_ast_script_that_removes_the_old_and_inserts_the_new()
        {
            var script = CodeScriptParser.Parse(
                "{\"schema\":1,\"kind\":\"ast\",\"source\":\"src/Foo.cs\",\"language\":\"csharp\",\"sourceHash\":\"abc\",\"extractor\":\"roslyn\",\"extractorVersion\":\"1\"}\n" +
                "{\"type\":\"symbol\",\"id\":\"s-foo\",\"kind\":\"class\",\"name\":\"Foo\",\"qualifiedName\":\"Demo.Foo\",\"lineStart\":2,\"lineEnd\":9,\"signature\":\"public class Foo\",\"visibility\":\"public\"}\n" +
                "{\"type\":\"symbol\",\"id\":\"s-bar\",\"kind\":\"method\",\"name\":\"Bar\",\"qualifiedName\":\"Demo.Foo.Bar(string)\",\"lineStart\":3,\"lineEnd\":5,\"signature\":\"public void Bar(string s)\",\"returnType\":\"void\",\"parentId\":\"s-foo\"}\n" +
                "{\"type\":\"symbol\",\"id\":\"x-console\",\"kind\":\"external\",\"name\":\"Console\",\"qualifiedName\":\"System.Console\"}\n" +
                "{\"type\":\"parameter\",\"symbolId\":\"s-bar\",\"position\":0,\"name\":\"s\",\"parameterType\":\"string\"}\n" +
                "{\"type\":\"edge\",\"sourceId\":\"s-bar\",\"targetId\":\"x-console\",\"relation\":\"calls\",\"confidence\":\"extracted\",\"line\":4}\n",
                ".mde-code/src/Foo.cs.ast.jsonl");

            var update = CodeTripleWriter.LoadAstUpdate(script, ".mde-code/src/Foo.cs.ast.jsonl", "urn:mde:code:h1:data");

            StringAssert.Contains(update, "DELETE { GRAPH <urn:mde:code:h1:data> { ?a ?p ?b } }", "prima via gli archi diretti dello script");
            StringAssert.Contains(update, "?s code:fromScript \".mde-code/src/Foo.cs.ast.jsonl\" ; ?p ?o", "poi ogni nodo dello script");
            StringAssert.Contains(update, "INSERT DATA { GRAPH <urn:mde:code:h1:data> {");
            StringAssert.Contains(update, "<urn:mde:file:src%2FFoo.cs> a code:File ; code:path \"src/Foo.cs\"");
            StringAssert.Contains(update, "<urn:mde:sym:s-bar> a code:Symbol ; code:kind \"method\"");
            StringAssert.Contains(update, "code:parent <urn:mde:sym:s-foo>");
            StringAssert.Contains(update, "code:signatureHash \"", "l'hash della firma lo mette il caricatore, per il «da rivedere»");
            StringAssert.Contains(update, "<urn:mde:sym:s-bar:p0> a code:Parameter ; code:parameterOf <urn:mde:sym:s-bar> ; code:position 0 ; code:name \"s\"");
            StringAssert.Contains(update, "<urn:mde:sym:s-bar> code:calls <urn:mde:sym:x-console> .", "la proprietà diretta per i property path");
            StringAssert.Contains(update, "<urn:mde:edge:s-bar:calls:x-console:4> a code:Edge ; code:source <urn:mde:sym:s-bar> ; code:target <urn:mde:sym:x-console> ; code:relation \"calls\" ; code:relationIri <https://mdexplorer.net/ns/code#calls> ; code:confidence \"extracted\"");
            // L'external è condiviso: niente fromScript, così nessuno script lo cancella.
            var external = update.Substring(update.IndexOf("<urn:mde:sym:x-console> a code:Symbol"));
            external = external.Substring(0, external.IndexOf(" .\n"));
            Assert.IsFalse(external.Contains("fromScript"), external);
            Assert.IsFalse(external.Contains("inFile"), external);
        }

        [TestMethod]
        public void Write_descriptions_as_nodes_attached_to_the_symbol_by_id()
        {
            var doc = CodeScriptParser.Parse(
                "{\"schema\":1,\"kind\":\"doc\",\"source\":\"src/Foo.cs\",\"writtenBy\":\"mde-code-doc 1 / test\",\"writtenAt\":\"2026-10-10T11:00:00Z\"}\n" +
                "{\"type\":\"symbolDoc\",\"symbolId\":\"s-bar\",\"summary\":\"Scrive \\\"s\\\" sulla console.\",\"signatureHash\":\"sig1\"}\n" +
                "{\"type\":\"parameterDoc\",\"symbolId\":\"s-bar\",\"parameterName\":\"s\",\"description\":\"Il testo.\"}\n",
                ".mde-code/src/Foo.cs.doc.jsonl");
            var update = CodeTripleWriter.LoadDocUpdate(doc, ".mde-code/src/Foo.cs.doc.jsonl", "urn:mde:code:h1:data");
            StringAssert.Contains(update, "<urn:mde:doc:s-bar> a code:Description ; code:describes <urn:mde:sym:s-bar> ; code:summary \"Scrive \\\"s\\\" sulla console.\"");
            StringAssert.Contains(update, "code:signatureHash \"sig1\"");
            StringAssert.Contains(update, "<urn:mde:pdoc:s-bar:s> a code:ParameterDescription ; code:describes <urn:mde:sym:s-bar> ; code:parameterName \"s\" ; code:description \"Il testo.\"");
            StringAssert.Contains(update, "code:fromScript \".mde-code/src/Foo.cs.doc.jsonl\"");
        }
    }
}
