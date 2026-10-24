using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Services.CodeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>Radici di codice esterne F4: il grafo si aggiorna dopo il turno e il turno dopo aspetta.</summary>
    [TestClass]
    public class CodeGraphTurnRefresher_Should
    {
        private sealed class SlowAnalysis : ICodeAnalysisService
        {
            public int Runs; public int Running; public int MaxParallel; public bool Fail;
            public TaskCompletionSource Gate = new();
            public async Task<List<CodeFolderAnalysisOutcome>> RefreshAfterTurnAsync(string projectPath, CancellationToken ct = default)
            {
                var now = Interlocked.Increment(ref Running);
                MaxParallel = Math.Max(MaxParallel, now);
                await Gate.Task;
                Interlocked.Increment(ref Runs);
                Interlocked.Decrement(ref Running);
                if (Fail) throw new InvalidOperationException("Fuseki giù");
                return new List<CodeFolderAnalysisOutcome> { new() { Folder = "@backend", Reparsed = 1 } };
            }
            public Task<CodeFolderLanguages> GetLanguagesAsync(string p, string f, CancellationToken ct = default) => throw new NotImplementedException();
            public void SaveLanguages(string p, string f, IReadOnlyList<string> l, string d = null) => throw new NotImplementedException();
            public string DescriptionModeOf(string p, string f) => throw new NotImplementedException();
            public Task<CodeFolderAnalysisOutcome> AnalyzeFolderAsync(string p, string f, bool i, CancellationToken ct = default) => throw new NotImplementedException();
            public Task<List<CodeFolderAnalysisOutcome>> AnalyzeProjectAsync(string p, bool i, CancellationToken ct = default) => throw new NotImplementedException();
            public Task<List<CodeOutdatedScript>> ListOutdatedScriptsAsync(string p, string f, CancellationToken ct = default) => throw new NotImplementedException();
            public Task<List<CodeProgramFolder>> ListProgramsAsync(string p, CancellationToken ct = default) => throw new NotImplementedException();
            public IReadOnlyList<CodeScanInProgress> Scanning(string p) => throw new NotImplementedException();
            public Task<List<MdExplorer.Features.Services.CodeGraph.CodeLanguageFiles>> FilesOfLanguageAsync(string p, MdExplorer.Features.Services.CodeGraph.CodeLanguageCatalog c, string l, CancellationToken ct = default) => throw new NotImplementedException();
            public Task<(List<MdExplorer.Features.Services.CodeGraph.CodeBridge> Bridges, List<string> Warnings)> BridgesAsync(string p, CancellationToken ct = default) => throw new NotImplementedException();
        }

        [TestMethod]
        public async Task Make_the_next_turn_wait_for_the_refresh_left_by_the_previous_one()
        {
            var analysis = new SlowAnalysis();
            var refresher = new CodeGraphTurnRefresher(analysis, NullLogger<CodeGraphTurnRefresher>.Instance);
            refresher.AfterTurn("/doc");
            var next = refresher.BeforeTurnAsync("/doc");
            await Task.Delay(100);
            Assert.IsFalse(next.IsCompleted, "il turno dopo aspetta il grafo aggiornato");
            analysis.Gate.SetResult();
            await next;
            Assert.AreEqual(1, analysis.Runs);
        }

        [TestMethod]
        public async Task Queue_refreshes_one_at_a_time_and_survive_a_failure()
        {
            var analysis = new SlowAnalysis { Fail = true };
            var refresher = new CodeGraphTurnRefresher(analysis, NullLogger<CodeGraphTurnRefresher>.Instance);
            refresher.AfterTurn("/doc");
            refresher.AfterTurn("/doc/");
            analysis.Gate.SetResult();
            await refresher.BeforeTurnAsync("/doc");   // un errore dell'aggiornamento non arriva alla chat
            Assert.AreEqual(2, analysis.Runs, "due turni, due aggiornamenti");
            Assert.AreEqual(1, analysis.MaxParallel, "in coda, mai insieme");
        }

        [TestMethod]
        public async Task Not_wait_when_nothing_is_pending()
        {
            var refresher = new CodeGraphTurnRefresher(new SlowAnalysis(), NullLogger<CodeGraphTurnRefresher>.Instance);
            var t = refresher.BeforeTurnAsync("/altro");
            Assert.IsTrue(t.IsCompleted);
            await t;
        }
    }
}
