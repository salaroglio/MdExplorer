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
