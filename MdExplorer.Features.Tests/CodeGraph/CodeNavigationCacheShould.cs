using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// Pacchetto 3, punti 1 e 2 (20/11/2026). La lettura completa degli archi della navigazione si è alleggerita (archi, simboli,
    /// scelte e interfacce letti a parte in formato tabellare e ricomposti in memoria: prima 51-68 s e 290 MB su 350 mila archi)
    /// e deve dare lo stesso elenco della query di prima. La copia in memoria si butta solo per una scrittura che cambia gli archi
    /// DI QUEL progetto: non per lo stato riscritto a fine turno, non per una descrizione senza scelte, non per un altro progetto.
    /// </summary>
    [TestClass]
    public class CodeNavigationCacheShould
    {
        // ------------------------------------------------------------------ il formato tabellare

        [TestMethod]
        public void Read_the_tabular_format_like_the_json_one()
        {
            var t = SparqlTsv.Parse("?s\t?name\t?n\t?missing\n" +
                "<urn:mde:sym:a>\t\"Ordini \\\"aperti\\\"\\tbis\\\\\"\t42\t\n" +
                "<urn:mde:sym:b>\t\"città\"@it\t\"7\"^^<http://www.w3.org/2001/XMLSchema#integer>\t\"x\"\r\n");
            CollectionAssert.AreEqual(new[] { "s", "name", "n", "missing" }, t.Columns);
            Assert.AreEqual(2, t.Rows.Count);
            CollectionAssert.AreEqual(new[] { "urn:mde:sym:a", "Ordini \"aperti\"\tbis\\", "42", null }, t.Rows[0], "escape, numero nudo, valore non legato");
            CollectionAssert.AreEqual(new[] { "urn:mde:sym:b", "città", "7", "x" }, t.Rows[1], "lingua e tipo tolti, fine riga di Windows");
            Assert.AreEqual(0, SparqlTsv.Parse("?s\n").Rows.Count, "nessuna riga");
        }

        // ------------------------------------------------------------------ su Fuseki

        /// <summary>Il client vero, che conta le letture complete della navigazione.</summary>
        private sealed class CountingClient : IFusekiClient
        {
            private readonly IFusekiClient _inner = FusekiFixture.NewClient();
            public int FullReads;
            public Task<FusekiTestResult> TestAsync(string baseUri, string dataset, string username, string passwordPlain) => _inner.TestAsync(baseUri, dataset, username, passwordPlain);
            public string SanitizeDatasetName(string projectName) => _inner.SanitizeDatasetName(projectName);
            public Task<bool> EnsureDatasetAsync(string baseUri, string dataset, string username, string passwordPlain) => _inner.EnsureDatasetAsync(baseUri, dataset, username, passwordPlain);
            public Task<string> QueryAsync(string baseUri, string dataset, string sparql, string username, string passwordPlain) => _inner.QueryAsync(baseUri, dataset, sparql, username, passwordPlain);
            public Task<string> QueryAsync(string baseUri, string dataset, string sparql, TimeSpan timeout, CancellationToken ct, string accept = null)
            {
                if (accept != null && sparql.Contains("VALUES ?rel", StringComparison.Ordinal) && !sparql.Contains("VALUES ?s", StringComparison.Ordinal) && !sparql.Contains("VALUES ?t", StringComparison.Ordinal))
                    Interlocked.Increment(ref FullReads);
                return _inner.QueryAsync(baseUri, dataset, sparql, timeout, ct, accept);
            }
            public Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, string username, string passwordPlain) => _inner.UpdateAsync(baseUri, dataset, sparqlUpdate, username, passwordPlain);
            public Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, TimeSpan timeout, CancellationToken ct) => _inner.UpdateAsync(baseUri, dataset, sparqlUpdate, timeout, ct);
            public Task LoadGraphAsync(string baseUri, string dataset, string graphUri, string turtle, string username, string passwordPlain) => _inner.LoadGraphAsync(baseUri, dataset, graphUri, turtle, username, passwordPlain);
        }

        private string _root;
        private CountingClient _client;
        private CodeGraphStore _store;

        [TestCleanup]
        public void Cleanup()
        {
            if (_root != null) try { Directory.Delete(_root, true); } catch { /* best effort */ }
        }

        private const string Ast =
            "{\"schema\":1,\"kind\":\"ast\",\"source\":\"src/Foo.vb\",\"language\":\"toy\",\"sourceHash\":\"HASH\",\"extractor\":\"declarative\",\"extractorVersion\":\"2\",\"generatedAt\":\"2026-11-20T10:00:00Z\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"mod\",\"kind\":\"module\",\"name\":\"Foo\",\"qualifiedName\":\"src/Foo.vb\",\"lineStart\":1,\"lineEnd\":99,\"signature\":\"Foo\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"ctl\",\"kind\":\"control\",\"name\":\"btnSalva\",\"qualifiedName\":\"Foo.btnSalva\",\"lineStart\":2,\"lineEnd\":2,\"signature\":\"btnSalva\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"main\",\"kind\":\"method\",\"name\":\"Main\",\"qualifiedName\":\"Foo.Main\",\"lineStart\":3,\"lineEnd\":9,\"signature\":\"Main()\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"one\",\"kind\":\"method\",\"name\":\"One\",\"qualifiedName\":\"Foo.One\",\"lineStart\":10,\"lineEnd\":12,\"signature\":\"One()\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"two\",\"kind\":\"method\",\"name\":\"Two\",\"qualifiedName\":\"Foo.Two\",\"lineStart\":13,\"lineEnd\":15,\"signature\":\"Two()\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"ifc\",\"kind\":\"interface\",\"name\":\"IRepo\",\"qualifiedName\":\"Foo.IRepo\",\"lineStart\":16,\"lineEnd\":18,\"signature\":\"IRepo\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"save\",\"kind\":\"method\",\"name\":\"Save\",\"qualifiedName\":\"Foo.IRepo.Save\",\"lineStart\":17,\"lineEnd\":17,\"signature\":\"Save()\",\"parentId\":\"ifc\"}\n" +
            "{\"type\":\"symbol\",\"id\":\"tab\",\"kind\":\"external\",\"name\":\"ORDINI\",\"qualifiedName\":\"dbo.ORDINI\"}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"mod\",\"targetId\":\"ctl\",\"relation\":\"contains\",\"confidence\":\"extracted\",\"line\":2}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"mod\",\"targetId\":\"main\",\"relation\":\"contains\",\"confidence\":\"extracted\",\"line\":3}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"ctl\",\"targetId\":\"main\",\"relation\":\"calls\",\"confidence\":\"extracted\",\"line\":2}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"main\",\"targetId\":\"one\",\"relation\":\"calls\",\"confidence\":\"ambiguous\",\"line\":5}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"main\",\"targetId\":\"two\",\"relation\":\"calls\",\"confidence\":\"ambiguous\",\"line\":5}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"main\",\"targetId\":\"save\",\"relation\":\"calls\",\"confidence\":\"inferred\",\"line\":6}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"one\",\"targetId\":\"tab\",\"relation\":\"writes\",\"confidence\":\"extracted\",\"line\":11}\n" +
            "{\"type\":\"edge\",\"sourceId\":\"one\",\"targetId\":\"ifc\",\"relation\":\"references\",\"confidence\":\"extracted\",\"line\":11}\n";

        private static string Doc(string source, bool withChoice) =>
            $"{{\"schema\":1,\"kind\":\"doc\",\"source\":\"{source}\",\"writtenBy\":\"test\",\"writtenAt\":\"2026-11-20T10:00:00Z\"}}\n" +
            (withChoice ? "{\"type\":\"callResolution\",\"symbolId\":\"main\",\"line\":5,\"target\":\"one\"}\n" : "{\"type\":\"fileDoc\",\"summary\":\"Gli ordini.\"}\n");

        private static string Write(string project, string rel, string jsonl, Action<CodeScript> tweak = null)
        {
            var path = Path.Combine(project, ".mde-code", rel.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var script = CodeScriptParser.Parse(jsonl, "fixture.jsonl");
            tweak?.Invoke(script);
            File.WriteAllText(path, CodeScriptWriter.Serialize(script));
            return path;
        }

        private async Task<string> ProjectAsync(string name)
        {
            var project = Path.Combine(_root, name);
            Write(project, "src/Foo.vb.ast.ttl", Ast);
            // la scelta dell'LLM: main → one, scartata main → two (come la scrive CodeDocPlanner)
            Write(project, "src/Foo.vb.doc.ttl", Doc("src/Foo.vb", withChoice: true), s => s.CallResolutions[0].Rejected.Add("two"));
            Write(project, "src/Bar.vb.ast.ttl", Ast.Replace("src/Foo.vb", "src/Bar.vb").Replace("\"id\":\"", "\"id\":\"bar-").Replace("Id\":\"", "Id\":\"bar-"));
            var outcome = await _store.LoadProjectAsync(project, compact: false);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            return project;
        }

        private async Task SetupAsync()
        {
            var manager = FusekiFixture.ManagerOrInconclusive();
            _client = new CountingClient();
            _store = new CodeGraphStore(manager, _client, NullLogger<CodeGraphStore>.Instance);
            _root = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
        }

        /// <summary>La query di prima del 20/11/2026, tale e quale: il riferimento per l'equivalenza.</summary>
        private async Task<List<CodeNavEdge>> OldReadAsync(string project)
        {
            var g = CodeGraphIris.DataGraph(CodeGraphIris.ProjectHash(project));
            var relations = string.Join(" ", CodeNavigation.Relations.Select(CodeGraphIris.Lit));
            var baseUri = await FusekiFixture.ManagerOrInconclusive().EnsureRunningAsync();
            var json = await FusekiFixture.NewClient().QueryAsync(baseUri, CodeOntology.Dataset, CodeOntology.SparqlPrefixes +
                $"SELECT ?s ?sk ?sqn ?sp ?t ?tk ?tqn ?tp ?rel ?conf ?path ?line ?guard WHERE {{ GRAPH <{g}> {{ VALUES ?rel {{ {relations} }} " +
                "?e a code:Edge ; code:source ?s ; code:target ?t ; code:relation ?rel ; code:confidence ?conf0 . " +
                "FILTER NOT EXISTS { ?rejectedBy code:rejects ?e } BIND(IF(EXISTS { ?chosenBy code:chooses ?e }, \"" + CodeOntology.LlmResolved + "\", ?conf0) AS ?conf) " +
                "?s code:kind ?sk . ?t code:kind ?tk . FILTER(?rel != \"contains\" || ?tk = \"control\") " +
                "OPTIONAL { ?s code:qualifiedName ?sqn } OPTIONAL { ?s code:path ?sp } OPTIONAL { ?t code:qualifiedName ?tqn } OPTIONAL { ?t code:path ?tp } " +
                "OPTIONAL { ?e code:path ?path } OPTIONAL { ?e code:line ?line } OPTIONAL { ?e code:guard ?guard } } }", TimeSpan.FromSeconds(30), CancellationToken.None);
            var abstracts = SparqlResults.Parse(await FusekiFixture.NewClient().QueryAsync(baseUri, CodeOntology.Dataset, CodeOntology.SparqlPrefixes +
                $"SELECT ?m WHERE {{ GRAPH <{g}> {{ ?m code:parent ?p . ?p code:kind \"interface\" }} }}", TimeSpan.FromSeconds(30), CancellationToken.None))
                .Select(r => CodeGraphIris.SymbolId(r["m"])).ToHashSet();
            string S(Dictionary<string, string> r, string k) => r.TryGetValue(k, out var v) ? v : null;
            return SparqlResults.Parse(json).Select(r =>
            {
                var s = CodeGraphIris.SymbolId(r["s"]); var t = CodeGraphIris.SymbolId(r["t"]);
                return new CodeNavEdge(s, S(r, "sk"), S(r, "sqn") ?? s, S(r, "sp"), t, S(r, "tk"), S(r, "tqn") ?? t, S(r, "tp"), S(r, "rel"), S(r, "conf"), S(r, "path"),
                    int.TryParse(S(r, "line"), out var n) ? n : null, abstracts.Contains(s), abstracts.Contains(t), S(r, "guard"));
            }).ToList();
        }

        private static string[] Canonical(IEnumerable<CodeNavEdge> edges) => edges.Select(e => e.ToString()).OrderBy(x => x, StringComparer.Ordinal).ToArray();

        [TestMethod]
        public async Task Give_the_same_edges_as_the_old_heavy_query()
        {
            await SetupAsync();
            var project = await ProjectAsync("a");
            var lean = await _store.NavigationEdgesAsync(project);
            var old = await OldReadAsync(project);
            CollectionAssert.AreEqual(Canonical(old), Canonical(lean));
            // e il campione dice davvero qualcosa: scelta, scarto, interfaccia, contains verso un controllo sì e verso un metodo no
            Assert.IsTrue(lean.Any(e => e.Source == "main" && e.Target == "one" && e.Confidence == CodeOntology.LlmResolved), "la scelta dell'LLM");
            Assert.IsFalse(lean.Any(e => e.Source == "main" && e.Target == "two"), "la candidata scartata");
            Assert.IsTrue(lean.Any(e => e.Target == "save" && e.TargetAbstract), "il membro di un'interfaccia");
            Assert.IsTrue(lean.Any(e => e.Relation == "contains" && e.Target == "ctl"));
            Assert.IsFalse(lean.Any(e => e.Relation == "contains" && e.Target == "main"));
            Assert.IsFalse(lean.Any(e => e.Relation == "references"), "solo le relazioni della navigazione");
            Assert.IsTrue(lean.Any(e => e.Relation == "writes" && e.TargetName == "dbo.ORDINI"));
        }

        [TestMethod]
        public async Task Lose_no_character_of_names_paths_and_guards_in_the_tabular_format()
        {
            // 20/11/2026, utente: «cambiando il formato, non è che ci sono dati che non trova più?» — i testi peggiori che il
            // grafo può portare: accenti (BAL in CP-850), emoji, tabulazioni, a capo, virgolette, barre, un carattere di controllo
            await SetupAsync();
            var project = Path.Combine(_root, "brutti");
            const string ugly = "Città.Ordini\t\"aperti\"\\fine\nseconda riga 😀 \u0001 ü€";
            var source = "src/Città dei Fiori/Ordini vecchi.vb";
            Write(project, source + ".ast.ttl", Ast.Replace("src/Foo.vb", source), script =>
            {
                // i testi brutti sullo script, non nel JSON di prova (che non li ammetterebbe crudi)
                script.Symbols.Single(x => x.Id == "main").QualifiedName = ugly;
                script.Symbols.Single(x => x.Id == "tab").QualifiedName = "dbo.[Ordini \"già\" chiusi]";
                foreach (var e in script.Edges.Where(e => e.Relation == "calls")) e.Guard = "If x = \"A\" Then\n\tY = \"\\server\\share\" ' è così\r\nEnd If";
            });
            var outcome = await _store.LoadProjectAsync(project, compact: false);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            var lean = await _store.NavigationEdgesAsync(project);
            var old = await OldReadAsync(project);
            CollectionAssert.AreEqual(Canonical(old), Canonical(lean));
            Assert.IsTrue(lean.Any(e => e.TargetName == "Città.Ordini\t\"aperti\"\\fine\nseconda riga 😀 \u0001 ü€"), "il nome arriva intero, carattere per carattere");
            Assert.IsTrue(lean.Any(e => e.Guard != null && e.Guard.Contains("\\server\\share") && e.Guard.Contains("\n\tY")), "la condizione su più righe");
            Assert.IsTrue(lean.All(e => e.Path == null || e.Path == source), "il percorso con spazi e accenti");
            Assert.IsTrue(lean.Any(e => e.TargetName == "dbo.[Ordini \"già\" chiusi]"));
        }

        /// <summary>
        /// Il grafo di un progetto vero (cartella <c>.mde-code</c> indicata da MDE_NAV_COMPARE_PROJECT): lettura completa di prima
        /// e di adesso, arco per arco. Senza la variabile: Inconclusive (è una prova da lanciare a mano su un grafo vero).
        /// </summary>
        [TestMethod]
        public async Task Give_the_same_edges_as_the_old_query_on_a_real_project()
        {
            var real = Environment.GetEnvironmentVariable("MDE_NAV_COMPARE_PROJECT");
            if (string.IsNullOrEmpty(real)) Assert.Inconclusive("MDE_NAV_COMPARE_PROJECT non impostata");
            await SetupAsync();
            var project = Path.Combine(_root, "vero");
            foreach (var f in Directory.EnumerateFiles(Path.Combine(real, ".mde-code"), "*.ttl", SearchOption.AllDirectories))
            {
                var to = Path.Combine(project, Path.GetRelativePath(real, f));
                Directory.CreateDirectory(Path.GetDirectoryName(to));
                File.Copy(f, to);
            }
            var outcome = await _store.LoadProjectAsync(project, compact: false);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            var lean = Canonical(await _store.NavigationEdgesAsync(project));
            var old = Canonical(await OldReadAsync(project));
            Console.WriteLine($"archi della navigazione: prima {old.Length}, adesso {lean.Length}, con condizione {lean.Count(x => !x.EndsWith("Guard =  }", StringComparison.Ordinal))}");
            CollectionAssert.AreEqual(old, lean);
        }

        [TestMethod]
        public async Task Throw_the_copy_away_only_for_writes_that_change_the_edges_of_that_project()
        {
            await SetupAsync();
            var project = await ProjectAsync("a");
            var other = await ProjectAsync("b");
            async Task<int> ReadsAfter(Func<Task> write)
            {
                await _store.NavigationEdgesAsync(project);   // la copia c'è
                var before = _client.FullReads;
                await write();
                await _store.NavigationEdgesAsync(project);
                return _client.FullReads - before;
            }

            Assert.AreEqual(0, await ReadsAfter(() => Task.CompletedTask), "senza scritture: la copia");
            Assert.AreEqual(0, await ReadsAfter(() => _store.UpsertScanStateAsync(project, new CodeScanState { ProgramFolder = "src", ScannedAt = DateTime.UtcNow.ToString("o"), Mode = "hash" })),
                "lo stato a fine turno di MarkAgent non cambia gli archi");
            Assert.AreEqual(0, await ReadsAfter(() => _store.LoadScriptAsync(project, Write(project, "src/Bar.vb.doc.ttl", Doc("src/Bar.vb", withChoice: false)))),
                "una descrizione senza scelte di chiamata");
            Assert.AreEqual(0, await ReadsAfter(() => _store.LoadScriptAsync(other, Write(other, "src/Foo.vb.ast.ttl", Ast.Replace("HASH", "HASH2")))),
                "un altro progetto");
            Assert.AreEqual(1, await ReadsAfter(() => _store.LoadScriptAsync(project, Write(project, "src/Foo.vb.doc.ttl", Doc("src/Foo.vb", withChoice: false)))),
                "una descrizione che TOGLIE una scelta: la certezza degli archi cambia");
            Assert.AreEqual(1, await ReadsAfter(() => _store.LoadScriptAsync(project, Write(project, "src/Foo.vb.doc.ttl", Doc("src/Foo.vb", withChoice: true)))),
                "una descrizione che porta una scelta");
            Assert.AreEqual(1, await ReadsAfter(() => _store.LoadScriptAsync(project, Write(project, "src/Foo.vb.ast.ttl", Ast.Replace("HASH", "HASH3")))),
                "lo script del codice");
            Assert.AreEqual(1, await ReadsAfter(() => _store.RemoveScriptAsync(project, Path.Combine(project, ".mde-code", "src", "Bar.vb.ast.ttl"))),
                "uno script tolto");
        }
    }
}
