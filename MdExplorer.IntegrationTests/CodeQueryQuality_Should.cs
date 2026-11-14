using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I controlli di qualità aggregati di <c>code_query</c> (14/11/2026): <c>pages-to-data</c> e <c>unreached-code</c> come li
    /// chiama MarkAgent — nomi dei parametri, valori ammessi, forma della risposta. Il grafo è finto: del store servono solo
    /// gli archi della navigazione e i simboli.
    /// </summary>
    [TestClass]
    public class CodeQueryQuality_Should
    {
        /// <summary>Un store che risponde solo alle due letture della navigazione; il resto non deve servire.</summary>
        public class FakeStore : DispatchProxy
        {
            public IReadOnlyList<CodeNavEdge> Edges;
            public List<(string Id, string Kind, string Name, string Path)> Symbols;

            protected override object Invoke(MethodInfo method, object[] args) => method.Name switch
            {
                nameof(ICodeGraphStore.NavigationEdgesAsync) => Task.FromResult(Edges),
                nameof(ICodeGraphStore.SymbolsOfKindsAsync) => Task.FromResult(Symbols
                    .Where(s => ((IReadOnlyCollection<string>)args[1]).Contains(s.Kind)
                        && (args[2] is not IReadOnlyCollection<string> prefixes || prefixes.Any(p => s.Path.StartsWith(p, StringComparison.Ordinal)))).ToList()),
                _ => throw new NotSupportedException(method.Name),
            };
        }

        private static readonly Dictionary<string, (string Kind, string Name, string Path)> N = new()
        {
            ["p1"] = ("module", "web/Ordini.aspx", "web/Ordini.aspx"), ["h1"] = ("method", "Web.Ordini.Page_Load()", "web/Ordini.aspx.vb"),
            ["p2"] = ("module", "web/Login.aspx", "web/Login.aspx"), ["h2"] = ("method", "Web.Login.Page_Load()", "web/Login.aspx.vb"),
            ["dl"] = ("method", "Dl.Ordini.Leggi()", "dl/Ordini.vb"), ["mai"] = ("method", "Dl.Ordini.Mai()", "dl/Ordini.vb"),
            ["t"] = ("external", "ORDINI", null), ["x"] = ("external", "Sicurezza.Verifica", null),
        };

        private static CodeNavEdge E(string s, string t, string rel, string conf = "extracted")
            => new(s, N[s].Kind, N[s].Name, N[s].Path, t, N[t].Kind, N[t].Name, N[t].Path, rel, conf, N[s].Path, 1);

        private static CodeQueryService Service()
        {
            var store = DispatchProxy.Create<ICodeGraphStore, FakeStore>();
            var fake = (FakeStore)(object)store;
            fake.Edges = new[] { E("p1", "h1", "calls"), E("h1", "dl", "calls"), E("dl", "t", "reads"), E("p2", "h2", "calls"), E("h2", "x", "calls", "ambiguous") };
            fake.Symbols = N.Where(n => n.Value.Kind != "external").Select(n => (n.Key, n.Value.Kind, n.Value.Name, n.Value.Path)).ToList();
            return new CodeQueryService(store, null, null);
        }

        private static async Task<JsonElement> Ask(string name, params (string Key, string Value)[] parameters)
            => JsonSerializer.SerializeToElement(await Service().RunAsync("/progetto", name, parameters.ToDictionary(p => p.Key, p => p.Value), CancellationToken.None));

        [TestMethod]
        public async Task Answer_pages_to_data_with_counts_only_and_a_sample_on_request()
        {
            var counts = await Ask("pages-to-data", ("from", "web"));
            Assert.AreEqual(2, counts.GetProperty("total").GetInt32());
            Assert.AreEqual(1, counts.GetProperty("byOutcome").GetProperty("data").GetInt32());
            Assert.AreEqual(1, counts.GetProperty("byOutcome").GetProperty("stops-at-unresolved-name").GetInt32());
            Assert.AreEqual(JsonValueKind.Null, counts.GetProperty("samples").ValueKind, "senza sample: solo conti");
            Assert.AreEqual("Sicurezza.Verifica", counts.GetProperty("topExternalStops")[0].GetProperty("Symbol").GetString());

            var sampled = await Ask("pages-to-data", ("from", "web"), ("outcome", "stops-at-unresolved-name"), ("sample", "5"), ("seed", "verifica"));
            var cases = sampled.GetProperty("samples").GetProperty("stops-at-unresolved-name");
            Assert.AreEqual("web/Login.aspx", cases[0].GetProperty("Symbol").GetString());
            StringAssert.Contains(sampled.GetProperty("sampleNote").GetString(), "verifica");
        }

        [TestMethod]
        public async Task Answer_unreached_code_by_cause_and_folder()
        {
            var r = await Ask("unreached-code", ("from", "web"), ("to", "dl"), ("sample", "2"));
            Assert.AreEqual((2, 1, 1), (r.GetProperty("targets").GetInt32(), r.GetProperty("reached").GetInt32(), r.GetProperty("unreached").GetInt32()));
            Assert.AreEqual(1, r.GetProperty("byCause").GetProperty("no-callers").GetInt32());
            Assert.AreEqual("Dl.Ordini.Mai()", r.GetProperty("samples").GetProperty("no-callers")[0].GetProperty("Symbol").GetString());
            Assert.AreEqual("dl/", r.GetProperty("folders")[0].GetProperty("Folder").GetString());
        }

        [TestMethod]
        public async Task Refuse_a_wrong_value_or_a_missing_parameter_saying_what_is_allowed()
        {
            var wrong = await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Ask("pages-to-data", ("outcome", "boh")));
            StringAssert.Contains(wrong.Message, "stops-at-library");
            await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Ask("pages-to-data", ("unit", "form")));
            await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Ask("unreached-code", ("from", "web")));
            await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Ask("unreached-code", ("to", "dl"), ("sample", "51")));
        }
    }
}
