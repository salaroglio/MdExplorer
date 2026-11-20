using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Pacchetto 1 dell'efficienza (20/11/2026, dal cliente QTYB-EM): code_ontology scadeva dopo 15 secondi, la query restava
    /// in Fuseki, le copie dei tentativi si mettevano in coda e cadevano anche code_inventory e code_schema; MarkAgent leggeva
    /// «answered 500:» senza un motivo. Ora ogni query porta il suo tempo a Fuseki, chi chiama da MarkAgent ha 25 secondi in
    /// tutto, il tempo scaduto dice come restringere, la navigazione non si butta via.
    /// </summary>
    [TestClass]
    public class CodeQueryBudgetShould
    {
        // ------------------------------------------------------------------ il tempo della richiesta, senza Fuseki

        [TestMethod]
        public void Leave_every_query_its_own_time_when_nobody_set_one()
        {
            Assert.IsNull(CodeQueryBudget.Remaining);
            Assert.AreEqual(TimeSpan.FromMinutes(3), CodeQueryBudget.Clamp(TimeSpan.FromMinutes(3)), "interfaccia, analisi, report: i tempi di prima");
        }

        [TestMethod]
        public void Give_a_query_at_most_what_is_left_and_restore_after()
        {
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(5)))
            {
                Assert.IsTrue(CodeQueryBudget.Clamp(TimeSpan.FromMinutes(3)) <= TimeSpan.FromSeconds(5));
                Assert.AreEqual(TimeSpan.FromSeconds(2), CodeQueryBudget.Clamp(TimeSpan.FromSeconds(2)), "una query più breve tiene il suo tempo");
                using (CodeQueryBudget.Begin(TimeSpan.FromMinutes(10)))
                    Assert.IsTrue(CodeQueryBudget.Remaining <= TimeSpan.FromSeconds(5), "un tempo più largo dentro non allarga quello di fuori");
                Assert.IsNotNull(CodeQueryBudget.Remaining);
            }
            Assert.IsNull(CodeQueryBudget.Remaining, "finita la richiesta, nessun tempo");
        }

        [TestMethod]
        public void Give_at_least_one_second_when_the_time_is_over()
        {
            using (CodeQueryBudget.Begin(TimeSpan.Zero))
                Assert.AreEqual(TimeSpan.FromSeconds(1), CodeQueryBudget.Clamp(TimeSpan.FromSeconds(15)), "fallisce subito, con il suo messaggio");
        }

        [TestMethod]
        public async Task Flow_into_the_work_of_the_request_but_not_out_of_a_cleared_one()
        {
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(5)))
            {
                Assert.IsNotNull(await Task.Run(() => CodeQueryBudget.Remaining), "le query della richiesta lo vedono");
                Assert.IsNull(await Task.Run(() => { CodeQueryBudget.Clear(); return CodeQueryBudget.Remaining; }), "il lavoro in sottofondo se ne libera");
                Assert.IsNotNull(CodeQueryBudget.Remaining, "senza toccare la richiesta");
            }
        }

        // ------------------------------------------------------------------ lo store, su un Fuseki vero

        /// <summary>Il client vero, che ricorda i tempi delle query e può farle scadere o rallentare.</summary>
        private sealed class WatchingClient : IFusekiClient
        {
            private readonly IFusekiClient _inner = FusekiFixture.NewClient();
            public List<TimeSpan> QueryTimeouts { get; } = new();
            public int UntimedQueries { get; private set; }
            /// <summary>Fuseki fermato dal suo limite: 503.</summary>
            public bool QueriesExpire { get; set; }
            /// <summary>La lettura di tutti gli archi della navigazione, rallentata.</summary>
            public TimeSpan NavigationDelay { get; set; }
            public int NavigationReads { get; private set; }

            public Task<FusekiTestResult> TestAsync(string baseUri, string dataset, string username, string passwordPlain) => _inner.TestAsync(baseUri, dataset, username, passwordPlain);
            public string SanitizeDatasetName(string projectName) => _inner.SanitizeDatasetName(projectName);
            public Task<bool> EnsureDatasetAsync(string baseUri, string dataset, string username, string passwordPlain) => _inner.EnsureDatasetAsync(baseUri, dataset, username, passwordPlain);
            public Task<string> QueryAsync(string baseUri, string dataset, string sparql, string username, string passwordPlain)
            {
                UntimedQueries++;
                return _inner.QueryAsync(baseUri, dataset, sparql, username, passwordPlain);
            }
            public async Task<string> QueryAsync(string baseUri, string dataset, string sparql, TimeSpan timeout, CancellationToken ct, string accept = null)
            {
                lock (QueryTimeouts) QueryTimeouts.Add(timeout);
                if (QueriesExpire) throw new FusekiRequestException("query", 503, "Query timed out");
                if (sparql.Contains("VALUES ?rel", StringComparison.Ordinal))
                {
                    NavigationReads++;
                    await Task.Delay(NavigationDelay, CancellationToken.None);   // come Fuseki: il lavoro va avanti anche se chi chiede se ne va
                }
                return await _inner.QueryAsync(baseUri, dataset, sparql, timeout, ct, accept);
            }
            public Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, string username, string passwordPlain) => _inner.UpdateAsync(baseUri, dataset, sparqlUpdate, username, passwordPlain);
            public Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, TimeSpan timeout, CancellationToken ct) => _inner.UpdateAsync(baseUri, dataset, sparqlUpdate, timeout, ct);
            public Task LoadGraphAsync(string baseUri, string dataset, string graphUri, string turtle, string username, string passwordPlain) => _inner.LoadGraphAsync(baseUri, dataset, graphUri, turtle, username, passwordPlain);
        }

        private string _root;
        private string _project;
        private WatchingClient _client;
        private CodeGraphStore _store;

        private async Task LoadedAsync()
        {
            var manager = FusekiFixture.ManagerOrInconclusive();
            _client = new WatchingClient();
            _store = new CodeGraphStore(manager, _client, NullLogger<CodeGraphStore>.Instance);
            _root = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Directory.CreateDirectory(Path.Combine(_project, ".mde-code", "src"));
            const string script =
                "{\"schema\":1,\"kind\":\"ast\",\"source\":\"src/Foo.toy\",\"language\":\"toy\",\"sourceHash\":\"abc\",\"extractor\":\"declarative\",\"extractorVersion\":\"2\",\"generatedAt\":\"2026-11-20T10:00:00Z\"}\n" +
                "{\"type\":\"symbol\",\"id\":\"s-main\",\"kind\":\"function\",\"name\":\"MAIN\",\"qualifiedName\":\"Foo.MAIN\",\"lineStart\":1,\"lineEnd\":9,\"signature\":\"PROC MAIN\"}\n" +
                "{\"type\":\"symbol\",\"id\":\"s-sub\",\"kind\":\"function\",\"name\":\"SUB\",\"qualifiedName\":\"Foo.SUB\",\"lineStart\":10,\"lineEnd\":12,\"signature\":\"PROC SUB\"}\n" +
                "{\"type\":\"edge\",\"sourceId\":\"s-main\",\"targetId\":\"s-sub\",\"relation\":\"calls\",\"confidence\":\"extracted\",\"line\":3}\n";
            File.WriteAllText(Path.Combine(_project, ".mde-code", "src", "Foo.toy.ast.ttl"), CodeScriptWriter.Serialize(CodeScriptParser.Parse(script, "fixture.jsonl")));
            var outcome = await _store.LoadProjectAsync(_project, compact: false);
            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            lock (_client.QueryTimeouts) _client.QueryTimeouts.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_root != null) try { Directory.Delete(_root, true); } catch { /* best effort */ }
        }

        [TestMethod]
        public async Task Send_every_short_read_with_its_time_so_fuseki_stops_too()
        {
            await LoadedAsync();
            await _store.ParserProfilesAsync(_project);
            Assert.AreEqual(0, _client.UntimedQueries, "nessuna query senza tempo: Fuseki non saprebbe quando fermarsi");
            Assert.IsTrue(_client.QueryTimeouts.Count > 0);
            Assert.IsTrue(_client.QueryTimeouts.All(t => t == TimeSpan.FromSeconds(15)), "senza un tempo della richiesta: i 15 secondi di sempre");
        }

        [TestMethod]
        public async Task Give_the_queries_of_an_agent_call_no_more_than_its_time()
        {
            await LoadedAsync();
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(4)))
            {
                await _store.ParserProfilesAsync(_project);
                await _store.OverviewAsync(_project);   // le letture lunghe (3 minuti) anche
            }
            Assert.IsTrue(_client.QueryTimeouts.Count > 0);
            Assert.IsTrue(_client.QueryTimeouts.All(t => t <= TimeSpan.FromSeconds(4)), string.Join(", ", _client.QueryTimeouts));
        }

        [TestMethod]
        public async Task Say_how_to_narrow_a_question_fuseki_stopped()
        {
            await LoadedAsync();
            _client.QueriesExpire = true;
            var ex = await Assert.ThrowsExceptionAsync<TimeoutException>(() => _store.ParserProfilesAsync(_project));
            StringAssert.Contains(ex.Message, "VALUES ?path", "il modo che usa l'indice");
            StringAssert.Contains(ex.Message, "STRSTARTS", "e quello da non usare, detto come tale");

            var sparql = await Assert.ThrowsExceptionAsync<CodeSparqlException>(() => _store.SparqlAsync(_project, "SELECT ?s WHERE { ?s ?p ?o }", 10));
            StringAssert.Contains(sparql.Message, "VALUES ?path");
            Assert.IsFalse(sparql.Message.Contains("(un FILTER sul path, un LIMIT)", StringComparison.Ordinal) || sparql.Message.Contains("(a FILTER on path, a LIMIT)", StringComparison.Ordinal),
                "non più il consiglio di restringere con un filtro sul percorso");
        }

        [TestMethod]
        public async Task Keep_preparing_the_navigation_when_an_agent_call_runs_out_of_time()
        {
            await LoadedAsync();
            _client.NavigationDelay = TimeSpan.FromSeconds(3);
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(1)))
            {
                var ex = await Assert.ThrowsExceptionAsync<TimeoutException>(() => _store.NavigationEdgesAsync(_project));
                Assert.IsTrue(ex.Message.Contains("riprova", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("again", StringComparison.OrdinalIgnoreCase), ex.Message);
            }
            await Task.Delay(TimeSpan.FromSeconds(4));   // la lettura finisce da sola
            var clock = Stopwatch.StartNew();
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(1)))
            {
                var edges = await _store.NavigationEdgesAsync(_project);
                Assert.IsTrue(edges.Any(e => e.Relation == "calls"), "gli archi letti in sottofondo");
            }
            Assert.IsTrue(clock.Elapsed < TimeSpan.FromSeconds(1), $"al secondo tentativo sono pronti ({clock.Elapsed})");
            Assert.AreEqual(1, _client.NavigationReads, "una lettura sola: il secondo tentativo non ricomincia da capo");
        }
    
        [TestMethod]
        public async Task Not_join_a_navigation_read_started_before_a_write()
        {
            // 20/11/2026: chi chiede dopo una scrittura si agganciava alla lettura in sottofondo partita prima, e riceveva gli
            // archi di prima della scrittura
            await LoadedAsync();
            _client.NavigationDelay = TimeSpan.FromSeconds(3);
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(1)))
                await Assert.ThrowsExceptionAsync<TimeoutException>(() => _store.NavigationEdgesAsync(_project));   // la lettura A parte
            var script = Path.Combine(_project, ".mde-code", "src", "Foo.toy.ast.ttl");
            File.WriteAllText(script, File.ReadAllText(script).Replace("\"Foo.SUB\"", "\"Foo.SUB2\""));
            Assert.IsTrue((await _store.LoadScriptAsync(_project, script)).Loaded, "la scrittura mentre A legge");
            using (CodeQueryBudget.Begin(TimeSpan.FromSeconds(1)))
                await Assert.ThrowsExceptionAsync<TimeoutException>(() => _store.NavigationEdgesAsync(_project));   // B, non A
            await Task.Delay(TimeSpan.FromSeconds(8));   // A finisce, poi B (aspetta il suo turno) legge e finisce
            Assert.AreEqual(2, _client.NavigationReads, "una lettura nuova dopo la scrittura, non quella di prima");
            var edges = await _store.NavigationEdgesAsync(_project);
            Assert.AreEqual(2, _client.NavigationReads, "e la copia tenuta è quella nuova");
            Assert.IsTrue(edges.Any(e => e.TargetName == "Foo.SUB2" || e.TargetName == "Foo.SUB"), "gli archi ci sono");
            Assert.IsFalse(edges.Any(e => e.TargetName == "Foo.SUB"), "e sono quelli DOPO la scrittura");
        }
}
}
