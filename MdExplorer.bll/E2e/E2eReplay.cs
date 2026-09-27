using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MdExplorer.Features.E2e
{
    public sealed record E2eReplayOutcome(int Test, string Script, bool Passed, string Message);

    public sealed class E2eReplayFileResult
    {
        public string File { get; init; }
        public string RunFolder { get; init; }
        public IReadOnlyList<E2eReplayOutcome> Outcomes { get; init; } = Array.Empty<E2eReplayOutcome>();

        /// <summary>Scripts not replayed because their test changed since they were written (D17).</summary>
        public IReadOnlyList<string> Stale { get; init; } = Array.Empty<string>();

        public string Problem { get; init; }
        public E2eCommitResult Commit { get; init; }
    }

    /// <summary>
    /// Replays the scripts of the tests without any LLM (F5, the seed of G3): <c>dotnet test</c> on the tests
    /// folder's project, only the scripts whose fingerprint matches their test, with the browser found by
    /// the prerequisites (Chrome or Edge) and the headless setting of the test. The outcomes are written by
    /// MdExplorer on top of the <c>## Esiti</c> table, marked «(script)».
    /// </summary>
    public static class E2eReplay
    {
        private static readonly XNamespace Trx = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(15);

        /// <param name="secrets">Credential values: removed from the messages before they are written or returned.</param>
        /// <param name="currentGenerator">"mde-e2e v&lt;n&gt;" of the installed skill: scripts of another version are stale.</param>
        public static async Task<E2eReplayFileResult> RunAsync(E2eRunItem item, string browserArgument, string dotnet, DateTime now,
            IReadOnlyDictionary<string, string> secrets, string currentGenerator, CancellationToken ct)
        {
            var folder = Path.GetDirectoryName(item.TestFile)!;
            var relative = item.RelativeTestFile;
            var project = Path.Combine(folder, "E2eTests.csproj");
            if (!File.Exists(project))
                return new E2eReplayFileResult { File = relative, Problem = "manca E2eTests.csproj accanto al test: esegui prima i test con MarkAgent, che crea i file di supporto." };
            if (browserArgument is not ("chrome" or "msedge"))
                return new E2eReplayFileResult { File = relative, Problem = "il rigioco degli script usa Chrome o Edge installati: il Chromium di Playwright non va bene per la libreria .NET." };

            var scripts = E2ePostRun.Scripts(item, currentGenerator);
            var stale = scripts.Where(s => s.Stale).Select(s => Path.GetFileName(s.Path)).ToList();
            var runnable = scripts.Where(s => !s.Stale).ToList();
            if (runnable.Count == 0)
                return new E2eReplayFileResult { File = relative, Stale = stale, Problem = scripts.Count == 0 ? "non ci sono script da rigiocare." : "tutti gli script sono scaduti: il test è cambiato, esegui di nuovo con MarkAgent." };

            var classes = new Dictionary<string, E2eScript>(StringComparer.Ordinal);
            foreach (var script in runnable)
            {
                var text = await File.ReadAllTextAsync(script.Path, ct);
                var ns = Regex.Match(text, @"^\s*namespace\s+([\w.]+)", RegexOptions.Multiline).Groups[1].Value;
                var cls = Regex.Match(text, @"\bclass\s+(\w+)").Groups[1].Value;
                if (ns.Length > 0 && cls.Length > 0) classes[ns + "." + cls] = script;
            }
            if (classes.Count == 0)
                return new E2eReplayFileResult { File = relative, Stale = stale, Problem = "negli script non trovo namespace e classe (vedi lo scheletro della skill mde-e2e)." };

            var runFolder = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(runnable[0].Path)!)!, "esecuzioni",
                now.ToString("yyyy-MM-dd_HH-mm", System.Globalization.CultureInfo.InvariantCulture) + "_script");
            Directory.CreateDirectory(runFolder);
            var trx = Path.Combine(Path.GetTempPath(), "mde-e2e-" + Guid.NewGuid().ToString("N") + ".trx");

            var start = new ProcessStartInfo(dotnet)
            {
                WorkingDirectory = folder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var a in new[] { "test", project, "--settings", Path.Combine(folder, "e2e.runsettings"),
                         "--logger", "trx;LogFileName=" + trx,
                         "--filter", string.Join("|", classes.Keys.Select(c => "FullyQualifiedName~" + c + ".")),
                         "--", "Playwright.LaunchOptions.Channel=" + browserArgument,
                         "Playwright.LaunchOptions.Headless=" + (item.Settings.Headless.Value ? "true" : "false") })
                start.ArgumentList.Add(a);
            start.Environment["E2E_RUN_DIR"] = runFolder;
            start.Environment["E2E_ROOT"] = folder;
            start.Environment.Remove("ELECTRON_RUN_AS_NODE");

            using var process = Process.Start(start)!;
            using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
            limit.CancelAfter(Timeout);
            var stdout = process.StandardOutput.ReadToEndAsync(limit.Token);
            var stderr = process.StandardError.ReadToEndAsync(limit.Token);
            try
            {
                await process.WaitForExitAsync(limit.Token);
            }
            catch (OperationCanceledException)
            {
                // dotnet test and the browser it started must not outlive the request.
                try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
                if (ct.IsCancellationRequested) throw;
                return new E2eReplayFileResult { File = relative, Stale = stale, RunFolder = runFolder,
                    Problem = $"dotnet test non ha finito entro {Timeout.TotalMinutes:0} minuti: interrotto." };
            }
            var output = E2ePostRun.Redact((await stdout) + (await stderr), secrets);

            if (!File.Exists(trx))
                return new E2eReplayFileResult { File = relative, Stale = stale, RunFolder = runFolder,
                    Problem = "dotnet test non ha prodotto risultati (compilazione fallita?): " + Tail(output) };

            var outcomes = ReadTrx(trx, classes)
                .Select(o => o with { Message = E2ePostRun.Redact(o.Message, secrets) }).ToList();
            File.Delete(trx);

            var date = now.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            var states = runnable.ToDictionary(r => r.TestNumber, r => r.State);
            var link = Path.GetRelativePath(folder, runFolder).Replace('\\', '/') + "/";
            var rows = outcomes.OrderByDescending(o => o.Test).Select(o => new E2eResultRow(date, o.Test,
                o.Passed ? "✅ superato (script)"
                    : states.TryGetValue(o.Test, out var state) && state == "incompleto" ? "⚠️ incompleto (script)" : "❌ fallito (script)",
                (o.Passed ? "" : FirstLine(o.Message) + " — ") + $"[screenshot]({link})")).ToList();
            var markdown = await File.ReadAllTextAsync(item.TestFile, ct);
            await File.WriteAllTextAsync(item.TestFile, E2eResultsTable.AddRows(markdown, rows), ct);

            return new E2eReplayFileResult { File = relative, RunFolder = runFolder, Outcomes = outcomes, Stale = stale };
        }

        public static IReadOnlyList<E2eReplayOutcome> ReadTrx(string trxPath, IReadOnlyDictionary<string, E2eScript> classes)
        {
            var doc = XDocument.Load(trxPath);
            var classOf = doc.Descendants(Trx + "UnitTest").ToDictionary(
                u => (string)u.Attribute("id"),
                u => (string)u.Element(Trx + "TestMethod")?.Attribute("className"));
            var outcomes = new List<E2eReplayOutcome>();
            foreach (var r in doc.Descendants(Trx + "UnitTestResult"))
            {
                var cls = classOf.TryGetValue((string)r.Attribute("testId") ?? "", out var c) ? c : null;
                if (cls == null || !classes.TryGetValue(cls, out var script)) continue;
                var passed = (string)r.Attribute("outcome") == "Passed";
                var message = (string)r.Descendants(Trx + "Message").FirstOrDefault();
                outcomes.Add(new E2eReplayOutcome(script.TestNumber, Path.GetFileName(script.Path), passed, message));
            }
            return outcomes;
        }

        private static string FirstLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "errore senza messaggio";
            var line = text.Trim().Split('\n')[0].Trim();
            return line.Length > 140 ? line.Substring(0, 140) + "…" : line;
        }

        private static string Tail(string text) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= 800 ? text : "…" + text.Substring(text.Length - 800);
    }
}
