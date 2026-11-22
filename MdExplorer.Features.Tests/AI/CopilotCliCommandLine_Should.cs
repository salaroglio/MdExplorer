using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.AI
{
    /// <summary>
    /// R5 e R6 della valutazione del 07/11, con un <c>copilot</c> finto messo in testa al PATH (solo Linux/macOS).
    /// R5: il prompt non sta mai nella riga di comando, dove un <c>\"</c> lo faceva diventare opzioni del CLI.
    /// R6: annullare ferma anche i processi che il CLI ha lanciato (su Windows il CLI vero è figlio di cmd.exe).
    /// </summary>
    [TestClass]
    [DoNotParallelize] // cambia il PATH del processo
    public class CopilotCliCommandLine_Should
    {
        private string _dir;
        private string _oldPath;

        [TestInitialize]
        public void Setup()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("copilot finto in sh: solo Linux/macOS");
            _dir = Path.Combine(Path.GetTempPath(), "mde-copilot-finto-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _oldPath = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", _dir + Path.PathSeparator + _oldPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_oldPath != null) Environment.SetEnvironmentVariable("PATH", _oldPath);
            if (_dir != null) try { Directory.Delete(_dir, true); } catch { }
        }

        private void FakeCopilot(string body)
        {
            var path = Path.Combine(_dir, "copilot");
            File.WriteAllText(path, "#!/bin/sh\n" + body + "\n");
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        private static CopilotCliProvider Provider() => new(NullLogger<CopilotCliProvider>.Instance, null);

        [TestMethod]
        public void Never_put_the_prompt_on_the_command_line()
        {
            FakeCopilot("cat");
            const string hostile = "fammi un riassunto \\\" --allow-all-paths --allow-all-tools \\";

            var psi = Provider().CreateProcessStartInfo(hostile, null, streaming: false);

            Assert.IsTrue(psi.RedirectStandardInput, "il prompt va su stdin");
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(psi.Arguments, "allow-all-paths|riassunto|--prompt"), psi.Arguments);
        }

        [TestMethod]
        public async Task Give_the_prompt_to_the_cli_exactly_as_written()
        {
            var got = Path.Combine(_dir, "ricevuto.txt");
            FakeCopilot($"cat > '{got}'; echo fatto");
            const string prompt = "finisce con \\\" e con \\";

            var answer = await Provider().ChatAsync(prompt);

            Assert.AreEqual(prompt, File.ReadAllText(got));
            StringAssert.Contains(answer, "fatto");
        }

        [TestMethod]
        public async Task Stop_the_children_of_the_cli_when_the_turn_is_cancelled()
        {
            var pidFile = Path.Combine(_dir, "figlio.pid");
            // Il figlio non tiene le pipe: senza questo, prima della correzione il test resterebbe appeso invece di fallire.
            FakeCopilot($"cat > /dev/null; sleep 300 > /dev/null 2>&1 < /dev/null & echo $! > '{pidFile}'; wait");

            using var cts = new CancellationTokenSource();
            var turn = Task.Run(() => Provider().ChatAsync("lavora", ct: cts.Token)); // ChatAsync aspetta il processo in modo sincrono
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (!File.Exists(pidFile) || File.ReadAllText(pidFile).Trim().Length == 0)
            {
                Assert.IsTrue(DateTime.UtcNow < deadline, "il copilot finto non è partito");
                await Task.Delay(50);
            }
            var child = int.Parse(File.ReadAllText(pidFile).Trim());
            cts.Cancel();
            try { await turn; } catch { }

            var stopped = DateTime.UtcNow.AddSeconds(5);
            while (Alive(child) && DateTime.UtcNow < stopped) await Task.Delay(50);
            var alive = Alive(child);
            if (alive) try { Process.GetProcessById(child).Kill(); } catch { }
            Assert.IsFalse(alive, "il figlio del CLI è ancora vivo dopo l'annullamento");
        }

        // ── L'errore del CLI: su stderr (CLI di prima) o come evento session.error su stdout (CLI recenti, uscita JSON) ──

        /// <summary>La riga vera scritta dalla CLI 1.0.114 il 22/11/2026 per un modello che non esiste (stderr vuoto, uscita 1).</summary>
        private const string SessionErrorLine =
            "{\"type\":\"session.error\",\"data\":{\"errorType\":\"query\",\"message\":\"Execution failed: Model \\\"modello-che-non-esiste\\\" is not available.\"},\"id\":\"68e188db\",\"timestamp\":\"2026-11-22T08:30:44.872Z\"}";

        /// <summary>Quello che scriveva su stderr la CLI fino alla 1.0.107 (dal commento del codice, «verificato sul campo»).</summary>
        private const string OldStderr = "Error: Model \"modello-che-non-esiste\" from --model flag is not available.";

        private static CopilotInvocation Invocation(string dir) => new(dir, null);

        [TestMethod]
        public async Task Report_the_error_a_recent_cli_sends_as_a_session_error_event()
        {
            // stdout: gli eventi, fra cui session.error; stderr: niente; uscita 1.
            FakeCopilot("cat > /dev/null; printf '%s\\n' '{\"type\":\"session.start\",\"data\":{}}' '" + SessionErrorLine.Replace("'", "'\\''") + "' '{\"type\":\"result\"}'; exit 1");

            var run = await Provider().RunHeadlessDetailedAsync("ciao", Invocation(_dir), modelId: "modello-che-non-esiste");

            Assert.AreEqual(1, run.ExitCode);
            Assert.AreEqual("Execution failed: Model \"modello-che-non-esiste\" is not available.", run.Error);
            Assert.IsTrue(CopilotCliProvider.IsModelUnavailable(run.Error));
        }

        [TestMethod]
        public async Task Report_the_error_an_older_cli_writes_on_stderr()
        {
            FakeCopilot("cat > /dev/null; echo '" + OldStderr + "' >&2; exit 1");

            var run = await Provider().RunHeadlessDetailedAsync("ciao", Invocation(_dir), modelId: "modello-che-non-esiste");

            Assert.AreEqual(1, run.ExitCode);
            StringAssert.Contains(run.Error, OldStderr);
            Assert.IsTrue(CopilotCliProvider.IsModelUnavailable(run.Error));
        }

        [TestMethod]
        public async Task Prefer_stderr_when_the_cli_gives_both()
        {
            FakeCopilot("cat > /dev/null; printf '%s\\n' '" + SessionErrorLine.Replace("'", "'\\''") + "'; echo 'errore su stderr' >&2; exit 1");

            var run = await Provider().RunHeadlessDetailedAsync("ciao", Invocation(_dir));

            StringAssert.Contains(run.Error, "errore su stderr");
        }

        [TestMethod]
        public async Task Give_the_answer_and_no_error_when_the_turn_succeeds()
        {
            FakeCopilot("cat > /dev/null; printf '%s\\n' '{\"type\":\"assistant.message\",\"data\":{\"content\":\"risposta\"}}'; exit 0");

            var run = await Provider().RunHeadlessDetailedAsync("ciao", Invocation(_dir));

            Assert.AreEqual(0, run.ExitCode);
            Assert.AreEqual("risposta", run.Text);
            Assert.AreEqual("", run.Error);
        }

        [TestMethod]
        public async Task Say_why_the_raw_chat_failed_when_the_cli_sends_a_session_error()
        {
            // Prima: «produced no assistant.message event», senza il motivo.
            FakeCopilot("cat > /dev/null; printf '%s\\n' '" + SessionErrorLine.Replace("'", "'\\''") + "'; exit 1");

            var ex = await Assert.ThrowsAsync<Exception>(() => Provider().ChatRawAsync("ciao", "modello-che-non-esiste"));

            StringAssert.Contains(ex.Message, "is not available");
        }

        [TestMethod]
        public void Read_only_session_error_events_as_errors()
        {
            Assert.AreEqual("Execution failed: Model \"modello-che-non-esiste\" is not available.", CopilotCliProvider.SessionErrorOf(SessionErrorLine));
            Assert.IsNull(CopilotCliProvider.SessionErrorOf("{\"type\":\"assistant.message\",\"data\":{\"content\":\"session.error\"}}"));
            Assert.IsNull(CopilotCliProvider.SessionErrorOf("{\"type\":\"session.error\",\"data\":null}"));
            Assert.IsNull(CopilotCliProvider.SessionErrorOf("{\"type\":\"session.error\"}"));
            Assert.IsNull(CopilotCliProvider.SessionErrorOf("rumore del CLI, non un evento"));
            Assert.IsNull(CopilotCliProvider.SessionErrorOf("{rotto"));
            Assert.IsNull(CopilotCliProvider.SessionErrorOf(null));
        }

        [TestMethod]
        public void Recognise_a_refused_model_in_both_wordings_and_nothing_else()
        {
            Assert.IsTrue(CopilotCliProvider.IsModelUnavailable(OldStderr));
            Assert.IsTrue(CopilotCliProvider.IsModelUnavailable("Execution failed: Model \"x\" is not available."));
            Assert.IsFalse(CopilotCliProvider.IsModelUnavailable("Execution failed: rate limit exceeded"));
            Assert.IsFalse(CopilotCliProvider.IsModelUnavailable("The service is not available right now"));
            Assert.IsFalse(CopilotCliProvider.IsModelUnavailable(""));
            Assert.IsFalse(CopilotCliProvider.IsModelUnavailable(null));
        }

        // ── Il turno mentre gira: gli eventi del CLI nella forma comune ai tre motori (il diario del turno) ──

        [TestMethod]
        public void Translate_the_events_of_the_cli_that_matter_to_who_watches_the_turn()
        {
            static string Of(string line) => CopilotCliProvider.TurnEventOf(line) is { } e ? e.Kind + "|" + e.Text : null;
            // Le righe vere scritte dalla CLI 1.0.114 il 22/11/2026, accorciate nei campi che qui non servono.
            Assert.AreEqual("message|Ho contato 3 ", Of("{\"type\":\"assistant.message_delta\",\"data\":{\"messageId\":\"33413c76\",\"deltaContent\":\"Ho contato 3 \"}}"));
            Assert.AreEqual("thinking|I'm checking wh", Of("{\"type\":\"assistant.reasoning_delta\",\"data\":{\"reasoningId\":\"d39e7cec\",\"deltaContent\":\"I'm checking wh\"}}"));
            Assert.AreEqual("tool|bash · wc -l < nota.txt", Of("{\"type\":\"tool.execution_start\",\"data\":{\"toolCallId\":\"toolu_01\",\"toolName\":\"bash\",\"arguments\":{\"command\":\"wc -l < nota.txt\",\"description\":\"Count lines in nota.txt\"},\"turnId\":\"0\"}}"));
            Assert.AreEqual("tool|↳ ok", Of("{\"type\":\"tool.execution_complete\",\"data\":{\"toolCallId\":\"toolu_01\",\"success\":true,\"result\":{\"content\":\"3\\n\"}}}"));
            Assert.AreEqual("tool|↳ errore", Of("{\"type\":\"tool.execution_complete\",\"data\":{\"toolCallId\":\"toolu_02\",\"success\":false}}"));
            Assert.AreEqual("error|Execution failed: Model \"modello-che-non-esiste\" is not available.", Of(SessionErrorLine));
            // Ciò che a chi guarda non dice niente: la risposta intera (arriva già a pezzi), l'uscita della shell, l'avvio, il rumore.
            Assert.IsNull(Of("{\"type\":\"assistant.message\",\"data\":{\"content\":\"Ho contato 3 righe.\"}}"));
            Assert.IsNull(Of("{\"type\":\"tool.shell_output\",\"data\":{\"toolCallId\":\"toolu_01\",\"stream\":\"stdout\",\"text\":\"3\\n\"}}"));
            Assert.IsNull(Of("{\"type\":\"session.start\",\"data\":{\"sessionId\":\"6d1d016d\"}}"));
            Assert.IsNull(Of("{\"type\":\"assistant.message_delta\",\"data\":{\"deltaContent\":\"\"}}"));
            Assert.IsNull(Of("{\"type\":\"result\"}"));
            Assert.IsNull(Of("rumore del CLI"));
            Assert.IsNull(Of(null));
        }

        [TestMethod]
        public async Task Tell_who_watches_what_the_cli_does_while_the_turn_runs()
        {
            FakeCopilot("cat > /dev/null; printf '%s\\n' "
                + "'{\"type\":\"assistant.reasoning_delta\",\"data\":{\"deltaContent\":\"Leggo il file.\"}}' "
                + "'{\"type\":\"tool.execution_start\",\"data\":{\"toolName\":\"bash\",\"arguments\":{\"command\":\"wc -l nota.txt\"}}}' "
                + "'{\"type\":\"tool.execution_complete\",\"data\":{\"success\":true}}' "
                + "'{\"type\":\"assistant.message_delta\",\"data\":{\"deltaContent\":\"Ho contato \"}}' "
                + "'{\"type\":\"assistant.message_delta\",\"data\":{\"deltaContent\":\"3 righe.\"}}' "
                + "'{\"type\":\"assistant.message\",\"data\":{\"content\":\"Ho contato 3 righe.\"}}'; exit 0");
            var seen = new System.Collections.Generic.List<string>();

            var run = await Provider().RunHeadlessDetailedAsync("ciao", Invocation(_dir), onEvent: e => seen.Add(e.Kind + "|" + e.Text));

            Assert.AreEqual("thinking|Leggo il file.;tool|bash · wc -l nota.txt;tool|↳ ok;message|Ho contato ;message|3 righe.", string.Join(";", seen));
            Assert.AreEqual("Ho contato 3 righe.", run.Text, "la risposta del turno resta quella intera, una volta sola");
        }

        [TestMethod]
        public async Task Finish_the_turn_even_if_who_watches_fails()
        {
            FakeCopilot("cat > /dev/null; printf '%s\\n' '{\"type\":\"assistant.message_delta\",\"data\":{\"deltaContent\":\"x\"}}' '{\"type\":\"assistant.message\",\"data\":{\"content\":\"risposta\"}}'; exit 0");

            var run = await Provider().RunHeadlessDetailedAsync("ciao", Invocation(_dir), onEvent: _ => throw new InvalidOperationException("chi guarda è rotto"));

            Assert.AreEqual("risposta", run.Text);
        }

        private static bool Alive(int pid)
        {
            // Uno zombie (già finito, in attesa del padre) conta come fermo.
            var stat = $"/proc/{pid}/stat";
            if (!File.Exists(stat)) return OperatingSystem.IsLinux() ? false : ProcessAlive(pid);
            try { return !File.ReadAllText(stat).Contains(") Z "); } catch { return false; }
        }

        private static bool ProcessAlive(int pid)
        {
            try { return !Process.GetProcessById(pid).HasExited; } catch { return false; }
        }
    }
}
