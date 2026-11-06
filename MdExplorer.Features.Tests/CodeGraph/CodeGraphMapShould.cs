using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Vista guidata, passo 2 del 14/10/2026: la mappa per livelli con le frecce sommate (Fuseki vero).</summary>
    [TestClass]
    public class CodeGraphMapShould
    {
        private string _project;
        private CodeGraphStore _store;

        [TestInitialize]
        public void Setup()
        {
            _store = FusekiFixture.NewStore();
            _project = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"), "project");
            Directory.CreateDirectory(_project);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(Path.GetDirectoryName(_project), true); } catch { } }

        private void Script(string source, IEnumerable<(string Id, string Kind, string Qn)> symbols, IEnumerable<(string From, string To, string Rel, string Conf)> edges = null)
        {
            var s = new CodeScript { Header = new CodeScriptHeader
            {
                Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = source, Language = "java", SourceHash = "h", Extractor = "test", ExtractorVersion = "1", GeneratedAt = "2026-10-14T00:00:00Z",
            } };
            foreach (var (id, kind, qn) in symbols) s.Symbols.Add(new ScriptSymbol { Id = id, Kind = kind, Name = qn.Split('.').Last(), QualifiedName = qn, LineStart = 1 });
            foreach (var (from, to, rel, conf) in edges ?? Enumerable.Empty<(string, string, string, string)>())
                s.Edges.Add(new ScriptEdge { SourceId = from, TargetId = to, Relation = rel, Confidence = conf, Line = 3 });
            var path = CodeScriptWriter.ScriptPathFor(_project, source, CodeScriptKind.Ast);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, CodeScriptWriter.Serialize(s));
        }

        private async Task Build()
        {
            const string J = "app/src/main/java/com/acme/";
            Script(J + "order/OrderService.java", new[] { ("os", "class", "com.acme.order.OrderService"), ("save", "method", "com.acme.order.OrderService.save()") },
                new[] { ("save", "charge", "calls", "extracted") });
            Script(J + "order/OrderRepo.java", new[] { ("repo", "class", "com.acme.order.OrderRepo") });
            Script(J + "pay/Payment.java", new[] { ("pay", "class", "com.acme.pay.Payment"), ("charge", "method", "com.acme.pay.Payment.charge()"), ("ext", "external", "org.lib.X") },
                new[] { ("pay", "repo", "imports", "extracted"), ("charge", "ext", "calls", "extracted") });
            Script(J + "App.java", new[] { ("app", "class", "com.acme.App"), ("main", "method", "com.acme.App.main()") },
                new[] { ("main", "save", "calls", "inferred"), ("main", "util", "calls", "extracted") });
            Script("lib/Util.java", new[] { ("utilc", "class", "Util"), ("util", "method", "Util.run()") });
            var load = await _store.LoadProjectAsync(_project);
            Assert.AreEqual(0, load.Failed, load.FirstError);
        }

        [TestMethod]
        public async Task Compact_the_folder_chain_and_sum_the_arrows_between_the_parts()
        {
            await Build();
            var map = await _store.MapAsync(_project, "app", new[] { "app", "lib" });

            Assert.AreEqual("folders", map.Level);
            Assert.AreEqual("app/src/main/java/com/acme", map.Folder, "src/main/java/com/acme si attraversa da sola");
            Assert.AreEqual(4, map.Files);
            var order = map.Groups.Single(g => g.Label == "order");
            Assert.AreEqual("app/src/main/java/com/acme/order", order.Folder);
            Assert.AreEqual(2, order.Files);
            Assert.AreEqual(1, map.Groups.Single(g => g.Kind == "files").Files, "App.java sta sciolto nella cartella");

            var orderToPay = map.Links.Single(l => l.From == order.Id && l.To == "app/src/main/java/com/acme/pay");
            Assert.AreEqual(1, orderToPay.Count);
            Assert.AreEqual(1, orderToPay.ByRelation["calls"]);
            Assert.IsFalse(map.Links.Any(l => l.ByRelation.ContainsKey("imports")), "gli imports non fanno frecce");
            var looseToOrder = map.Links.Single(l => l.From == "__files" && l.To == order.Id);
            Assert.AreEqual(1, looseToOrder.ByConfidence["inferred"], "la certezza resta visibile");
            Assert.AreEqual(1, map.Links.Single(l => l.From == "__files" && l.To == "__out").Count, "la chiamata verso lib esce di qui");
            Assert.AreEqual("app/src/main/java/com", map.Groups.Single(g => g.Kind == "outside").Folder);
        }

        [TestMethod]
        public async Task Show_the_programs_with_the_arrows_between_them()
        {
            await Build();
            var map = await _store.MapAsync(_project, "", new[] { "app", "lib" });

            Assert.AreEqual("programs", map.Level);
            CollectionAssert.AreEquivalent(new[] { "app", "lib" }, map.Groups.Select(g => g.Id).ToArray());
            Assert.AreEqual(4, map.Groups.Single(g => g.Id == "app").Files);
            var link = map.Links.Single();
            Assert.AreEqual(("app", "lib", 1), (link.From, link.To, link.Count));
        }

        [TestMethod]
        public async Task Gather_the_smaller_folders_beyond_the_limit()
        {
            for (var i = 0; i < CodeGraphStore.MapMaxGroups + 3; i++)
                Script($"big/m{i:00}/F.java", new[] { ($"c{i}", "class", $"m{i}.F") }, i > 0 ? new[] { ($"c{i}", "c0", "calls", "extracted") } : null);
            await _store.LoadProjectAsync(_project);

            var map = await _store.MapAsync(_project, "big", new[] { "big" });

            Assert.AreEqual(3, map.HiddenGroups);
            Assert.AreEqual(CodeGraphStore.MapMaxGroups + 1, map.Groups.Count);
            Assert.IsTrue(map.Links.Any(l => l.From == "__more"), "le frecce delle cartelle raccolte restano, sul gruppo «+N»");
        }
    }
}
