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
            public List<string> WithoutClass = new();

            protected override object Invoke(MethodInfo method, object[] args) => method.Name switch
            {
                nameof(ICodeGraphStore.NavigationEdgesAsync) => Task.FromResult(Edges),
                // pacchetto 3: gli archi che escono da (o entrano in) quei simboli, come lo store vero
                nameof(ICodeGraphStore.NavigationEdgesOfAsync) => Task.FromResult((IReadOnlyList<CodeNavEdge>)Edges
                    .Where(e => ((IReadOnlyCollection<string>)args[1]).Contains((bool)args[2] ? e.Source : e.Target)).ToList()),
                nameof(ICodeGraphStore.SymbolsOfKindsAsync) => Task.FromResult(Symbols
                    .Where(s => ((IReadOnlyCollection<string>)args[1]).Contains(s.Kind)
                        && (args[2] is not IReadOnlyCollection<string> prefixes || prefixes.Any(p => s.Path.StartsWith(p, StringComparison.Ordinal)))).ToList()),
                nameof(ICodeGraphStore.PagesWithoutClassAsync) => Task.FromResult(WithoutClass),
                // per nome o per id, sotto la cartella chiesta; «Doppio» è il nome di due simboli
                nameof(ICodeGraphStore.ResolveAsync) when args.Length == 6 => Task.FromResult(Symbols
                    .Where(s => (s.Name == (string)args[1] || s.Id == (string)args[1] || (string)args[1] == "Doppio" && s.Name.StartsWith("Dl.Ordini.", StringComparison.Ordinal))
                        && (args[2] is not string prefix || s.Path.StartsWith(prefix, StringComparison.Ordinal)))
                    .Select(s => new CodeSymbolView { Id = s.Id, Kind = s.Kind, QualifiedName = s.Name, Name = s.Name, Path = s.Path, LineStart = 3 }).ToList()),
                nameof(ICodeGraphStore.FilesUnderAsync) => Task.FromResult(Symbols.Where(s => s.Path.StartsWith((string)args[1], StringComparison.Ordinal))
                    .GroupBy(s => s.Path).Select(x => (Path: x.Key, Symbols: x.Key.EndsWith("Login.aspx.vb", StringComparison.Ordinal) ? 0 : x.Count(), Summary: (string)null)).ToList()),
                nameof(ICodeGraphStore.FileProjectsAsync) => Task.FromResult(Symbols.Select(x => x.Path).Distinct()
                    .Select(p => (Path: p, Project: p.StartsWith("web/", StringComparison.Ordinal) ? "web/Web.vbproj" : "dl/Dl.vbproj")).ToList()),
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

        private static CodeQueryService Service(params CodeNavEdge[] more)
        {
            var store = DispatchProxy.Create<ICodeGraphStore, FakeStore>();
            var fake = (FakeStore)(object)store;
            fake.Edges = new[] { E("p1", "h1", "calls"), E("h1", "dl", "calls"), E("dl", "t", "reads"), E("p2", "h2", "calls"), E("h2", "x", "calls", "ambiguous") }.Concat(more).ToList();
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
            Assert.AreEqual(0, counts.GetProperty("byOutcome").GetProperty("page-class-not-in-graph").GetInt32());
            Assert.AreEqual(0, counts.GetProperty("byOutcome").GetProperty("data-only-through-deduced-starts").GetInt32());
            Assert.AreEqual("ORDINI", counts.GetProperty("topTables")[0].GetProperty("Table").GetString());
            var supported = await Ask("pages-to-data", ("from", "web"), ("support", "ordini"));
            Assert.AreEqual((0, 1), (supported.GetProperty("byOutcome").GetProperty("data").GetInt32(), supported.GetProperty("byOutcome").GetProperty("data-only-support-tables").GetInt32()));
            Assert.AreEqual(JsonValueKind.Null, counts.GetProperty("samples").ValueKind, "senza sample: solo conti");
            Assert.AreEqual("Sicurezza.Verifica", counts.GetProperty("topUnresolvedNames")[0].GetProperty("Symbol").GetString());
            Assert.AreEqual((1, 0), (counts.GetProperty("unresolvedNames").GetInt32(), counts.GetProperty("libraryStops").GetInt32()));

            var sampled = await Ask("pages-to-data", ("from", "web"), ("outcome", "stops-at-unresolved-name"), ("sample", "5"), ("seed", "verifica"));
            var cases = sampled.GetProperty("samples").GetProperty("stops-at-unresolved-name");
            Assert.AreEqual("web/Login.aspx", cases[0].GetProperty("Symbol").GetString());
            StringAssert.Contains(sampled.GetProperty("sampleNote").GetString(), "verifica");
        }

        [TestMethod]
        public async Task Check_many_claims_of_a_document_in_one_call_saying_true_false_or_undetermined()
        {
            // 16/11/2026, dal cliente: un documento con cinquanta affermazioni, una chiamata per ciascuna, e il contesto di
            // MarkAgent finiva prima. Qui tutte insieme, con un esito certo per riga
            var claims = string.Join("\n",
                "exists: Web.Ordini.Page_Load()", "exists: Non.Esiste", "exists: Doppio",
                "calls: web/Ordini.aspx -> Dl.Ordini.Leggi()", "calls: web/Login.aspx -> Dl.Ordini.Leggi()", "calls: web/Login.aspx -> Dl.Ordini.Mai()",
                "reads: web/Ordini.aspx -> ordini", "writes: web/Ordini.aspx -> ORDINI", "touches: Web.Ordini.Page_Load() -> ORDINI",
                "file: dl/Ordini.vb; file: web/Login.aspx.vb; file: dl/Manca.vb");
            var r = JsonSerializer.SerializeToElement(await Service(E("h2", "mai", "calls", "ambiguous"))
                .RunAsync("/progetto", "check-claims", new Dictionary<string, string> { ["claims"] = claims }, CancellationToken.None));

            string Verdict(int n) => r.GetProperty("results")[n - 1].GetProperty("verdict").GetString();
            string Why(int n) => r.GetProperty("results")[n - 1].GetProperty("why").GetString();
            Assert.AreEqual(12, r.GetProperty("checked").GetInt32());
            Assert.AreEqual("true false undetermined | true false undetermined | true false true | true true false",
                $"{Verdict(1)} {Verdict(2)} {Verdict(3)} | {Verdict(4)} {Verdict(5)} {Verdict(6)} | {Verdict(7)} {Verdict(8)} {Verdict(9)} | {Verdict(10)} {Verdict(11)} {Verdict(12)}");
            StringAssert.Contains(Why(3), "more than one symbol", "due simboli con quel nome: non si sa di quale parla il documento");
            var chain = r.GetProperty("results")[3].GetProperty("chain");
            Assert.AreEqual("Web.Ordini.Page_Load() > Dl.Ordini.Leggi()", string.Join(" > ", chain.EnumerateArray().Select(s => s.GetProperty("Symbol").GetString())));
            StringAssert.Contains(r.GetProperty("results")[4].GetProperty("caveat").GetString(), "unresolved names", "falso, ma da Login parte una chiamata che il grafo non ha risolto");
            StringAssert.Contains(Why(6), "ambiguous edge", "il cammino c'è solo per un arco ambiguo: non è né vero né falso");
            Assert.AreEqual("ORDINI", r.GetProperty("results")[6].GetProperty("chain").EnumerateArray().Last().GetProperty("Symbol").GetString());
            StringAssert.Contains(Why(11), "WITHOUT symbols", "il file c'è ma il progetto non lo compila");
            Assert.AreEqual((6, 4, 2), (r.GetProperty("counts").GetProperty("true").GetInt32(), r.GetProperty("counts").GetProperty("false").GetInt32(), r.GetProperty("counts").GetProperty("undetermined").GetInt32()));

            // con la cartella, il nome doppio non lo è più se uno solo sta lì; una forma sconosciuta si rifiuta dicendo quali ci sono
            await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Service().RunAsync("/progetto", "check-claims", new Dictionary<string, string> { ["claims"] = "usa: A -> B" }, CancellationToken.None));
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
        public async Task Answer_unit_map_with_rows_between_projects_and_a_sample_of_one_row()
        {
            var map = await Ask("unit-map");
            Assert.AreEqual("project", map.GetProperty("unit").GetString());
            var row = map.GetProperty("rows")[0];
            Assert.AreEqual(("web/Web.vbproj", "dl/Dl.vbproj", 1), (row.GetProperty("From").GetString(), row.GetProperty("To").GetString(), row.GetProperty("Edges").GetInt32()));
            Assert.AreEqual(5, map.GetProperty("totals").GetProperty("Edges").GetInt32());
            Assert.AreEqual(JsonValueKind.Null, map.GetProperty("samples").ValueKind);

            var sampled = await Ask("unit-map", ("pair", "web/Web.vbproj -> dl/Dl.vbproj"), ("sample", "5"));
            Assert.AreEqual("Dl.Ordini.Leggi()", sampled.GetProperty("samples")[0].GetProperty("Target").GetString());

            var folders = await Ask("unit-map", ("unit", "folder"));
            Assert.AreEqual("web/", folders.GetProperty("rows")[0].GetProperty("From").GetString());

            var none = await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Ask("unit-map", ("pair", "dl/Dl.vbproj -> web/Web.vbproj")));
            StringAssert.Contains(none.Message, "rows");
            await Assert.ThrowsExceptionAsync<CodeQueryException>(() => Ask("unit-map", ("pair", "solo-una")));
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
