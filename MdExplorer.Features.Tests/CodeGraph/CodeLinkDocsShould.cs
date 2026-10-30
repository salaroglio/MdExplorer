using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>D43: le descrizioni dei collegamenti delle regole, la parte deterministica (punti, impronte, domanda, risposta).</summary>
    [TestClass]
    public class CodeLinkDocsShould
    {
        private static ScriptEdge E(string s, string t, int line, string path = "p/AppModule.java") => new() { SourceId = s, TargetId = t, Relation = "resolvesTo", Confidence = "rule", Path = path, Line = line };

        [TestMethod]
        public void Ask_once_per_point_and_redo_when_the_line_the_links_or_the_question_change()
        {
            var script = new CodeScript();
            script.Edges.AddRange(new[] { E("I", "P", 9), E("I.export", "P.export", 9), E("S", "F", 12) });
            var points = CodeLinkDocs.Points(script);
            CollectionAssert.AreEqual(new[] { 9, 12 }, points.Select(p => p.Line).ToArray(), "metodo → metodo nasce dallo stesso punto della classe: una domanda");
            Assert.AreEqual(2, points[0].Edges.Count);

            var hash = CodeLinkDocs.FactHash("  bind(A.class).to(B.class);", points[0].Edges);
            Assert.AreEqual(hash, CodeLinkDocs.FactHash("bind(A.class).to(B.class);   ", points[0].Edges), "gli spazi non contano");
            Assert.AreNotEqual(hash, CodeLinkDocs.FactHash("bind(A.class).to(C.class);", points[0].Edges), "la riga sì");
            Assert.AreNotEqual(hash, CodeLinkDocs.FactHash("bind(A.class).to(B.class);", new[] { E("I", "Q", 9) }), "e gli archi");
            var v = CodeLinkDocs.PromptVersion("Con quale durata?");
            Assert.AreEqual("missing", CodeLinkDocs.Why(null, hash, v));
            Assert.IsNull(CodeLinkDocs.Why(new ScriptLinkDoc { FactHash = hash, PromptVersion = v }, hash, v));
            Assert.AreEqual("changed", CodeLinkDocs.Why(new ScriptLinkDoc { FactHash = hash, PromptVersion = CodeLinkDocs.PromptVersion("Altra domanda?") }, hash, v));
        }

        [TestMethod]
        public void Send_the_question_with_the_lines_and_read_back_only_the_points_asked()
        {
            var rule = new CodeEnrichmentRule { Id = "guice-bind", Kind = CodeRuleKind.Registration, Description = "bind(X).to(Y)", Describe = "Con quale qualificatore?" };
            var lines = Enumerable.Range(1, 20).Select(i => i == 9 ? "bind(Exporter.class).annotatedWith(Names.named(\"main\")).to(PdfExporter.class);" : "// " + i).ToList();
            var excerpt = CodeLinkDocs.Excerpt(lines, 9);
            StringAssert.Contains(excerpt, "9 > bind(Exporter.class)");
            StringAssert.Contains(excerpt, "5   // 5");
            Assert.IsFalse(excerpt.Contains("// 14"), "solo le righe intorno");
            var points = new List<CodeLinkPoint> { new() { RuleId = rule.Id, Path = "p/AppModule.java", Line = 9, Excerpt = excerpt, Links = new[] { "com.acme.Exporter → com.acme.PdfExporter" } } };
            var prompt = CodeLinkDocs.BuildPrompt(rule, points, "it");
            StringAssert.Contains(prompt, "Con quale qualificatore?");
            StringAssert.Contains(prompt, "=== PUNTO p/AppModule.java:9 ===");
            StringAssert.Contains(prompt, "collegamento: com.acme.Exporter → com.acme.PdfExporter");

            var answer = "Ecco:\n{\"point\": \"p/AppModule.java:9\", \"summary\": \"Qualificatore @Named(\\\"main\\\").\"}\n{\"point\": \"altro:1\", \"summary\": \"x\"}\n{\"point\": \"p/AppModule.java:9\", \"summary\": \"\"}\nnon json";
            var read = CodeLinkDocs.ParseAnswer(answer, points, out var rejected);
            Assert.AreEqual(("p/AppModule.java:9", "Qualificatore @Named(\"main\")."), read.Single());
            Assert.AreEqual(2, rejected.Count, "il punto non chiesto e la risposta vuota si scartano e si dicono");
        }
    }
}
