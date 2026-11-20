using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>
    /// 17/11/2026, dal cliente: oltre 2.000 script grossi, il lotto non stava nei 15 secondi delle richieste brevi e il
    /// caricamento moriva con «The request was canceled due to the configured HttpClient.Timeout». Le scritture nel grafo
    /// hanno il loro tempo, e una scadenza si dice per quello che è, non come un annullamento.
    /// </summary>
    [TestClass]
    public class CodeGraphWriteTimeoutShould
    {
        /// <summary>Il client vero, che ricorda con quale tempo si scrive e può far scadere le scritture come fa HttpClient.</summary>
        private sealed class WatchingClient : IFusekiClient
        {
            private readonly IFusekiClient _inner = FusekiFixture.NewClient();
            public List<TimeSpan> WriteTimeouts { get; } = new();
            public int ShortWrites { get; private set; }
            public bool WritesExpire { get; set; }

            public Task<FusekiTestResult> TestAsync(string baseUri, string dataset, string username, string passwordPlain) => _inner.TestAsync(baseUri, dataset, username, passwordPlain);
            public string SanitizeDatasetName(string projectName) => _inner.SanitizeDatasetName(projectName);
            public Task<bool> EnsureDatasetAsync(string baseUri, string dataset, string username, string passwordPlain) => _inner.EnsureDatasetAsync(baseUri, dataset, username, passwordPlain);
            public Task<string> QueryAsync(string baseUri, string dataset, string sparql, string username, string passwordPlain) => _inner.QueryAsync(baseUri, dataset, sparql, username, passwordPlain);
            public Task<string> QueryAsync(string baseUri, string dataset, string sparql, TimeSpan timeout, CancellationToken ct, string accept = null) => _inner.QueryAsync(baseUri, dataset, sparql, timeout, ct, accept);
            public Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, string username, string passwordPlain) { ShortWrites++; return _inner.UpdateAsync(baseUri, dataset, sparqlUpdate, username, passwordPlain); }
            public Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, TimeSpan timeout, CancellationToken ct)
            {
                WriteTimeouts.Add(timeout);
                // così la dà HttpClient quando scade il suo Timeout: un annullamento che nessuno ha chiesto
                if (WritesExpire) throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout elapsing.", new TimeoutException());
                return _inner.UpdateAsync(baseUri, dataset, sparqlUpdate, timeout, ct);
            }
            public Task LoadGraphAsync(string baseUri, string dataset, string graphUri, string turtle, string username, string passwordPlain) => _inner.LoadGraphAsync(baseUri, dataset, graphUri, turtle, username, passwordPlain);
        }

        private string _root;
        private string _project;
        private WatchingClient _client;
        private CodeGraphStore _store;

        [TestInitialize]
        public void Setup()
        {
            var manager = FusekiFixture.ManagerOrInconclusive();
            _client = new WatchingClient();
            _store = new CodeGraphStore(manager, _client, NullLogger<CodeGraphStore>.Instance);
            _root = Path.Combine(Path.GetTempPath(), "mde-codegraph-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            Directory.CreateDirectory(Path.Combine(_project, ".mde-code", "src"));
            const string script =
                "{\"schema\":1,\"kind\":\"ast\",\"source\":\"src/Foo.toy\",\"language\":\"toy\",\"sourceHash\":\"abc\",\"extractor\":\"declarative\",\"extractorVersion\":\"2\",\"generatedAt\":\"2026-11-17T10:00:00Z\"}\n" +
                "{\"type\":\"symbol\",\"id\":\"s-main\",\"kind\":\"function\",\"name\":\"MAIN\",\"qualifiedName\":\"Foo.MAIN\",\"lineStart\":1,\"lineEnd\":9,\"signature\":\"PROC MAIN\"}\n";
            File.WriteAllText(Path.Combine(_project, ".mde-code", "src", "Foo.toy.ast.ttl"), CodeScriptWriter.Serialize(CodeScriptParser.Parse(script, "fixture.jsonl")));
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(_root, true); } catch { /* best effort */ }
        }

        [TestMethod]
        public async Task Write_to_the_graph_with_the_time_of_a_write_not_of_a_short_request()
        {
            var outcome = await _store.LoadProjectAsync(_project, compact: false);

            Assert.AreEqual(0, outcome.Failed, outcome.FirstError);
            Assert.AreEqual(1, outcome.Loaded);
            Assert.AreEqual(0, _client.ShortWrites, "nessuna scrittura nel grafo passa dai 15 secondi delle richieste brevi");
            Assert.IsTrue(_client.WriteTimeouts.Count > 0);
            Assert.IsTrue(_client.WriteTimeouts.TrueForAll(t => t == CodeGraphStore.WriteTimeout));
            Assert.IsTrue(CodeGraphStore.WriteTimeout >= TimeSpan.FromMinutes(5), "un lotto da 8 milioni di caratteri ha impiegato 12 secondi su una macchina veloce");
        }

        [TestMethod]
        public async Task Say_that_the_write_expired_and_stop_the_load_instead_of_passing_for_a_cancellation()
        {
            _client.WritesExpire = true;

            var ex = await Assert.ThrowsExceptionAsync<CodeGraphWriteTimeoutException>(() => _store.LoadProjectAsync(_project, compact: false));

            StringAssert.Contains(ex.Message, "Fuseki");
            Assert.AreEqual(CodeGraphStore.WriteTimeout, ex.Timeout);
            Assert.IsTrue(ex.Characters > 0);
            Assert.AreEqual(1, _client.WriteTimeouts.Count, "dopo la scadenza non si mandano altre scritture: Fuseki sta ancora lavorando");
        }
    }
}
