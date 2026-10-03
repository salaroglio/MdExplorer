using System;
using System.IO;
using System.Linq;
using MdExplorer.Service.Utilities;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La matita della toolbar che apre il CLI dell'ambiente agentico: quale comando, e in quale
    /// terminale.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md.</para>
    /// </summary>
    [TestClass]
    public class AgentCliTerminal_Should
    {
        [TestMethod]
        public void Name_the_command_of_every_engine()
        {
            Assert.AreEqual("copilot", MarkAgentEngines.CommandOf(MarkAgentEngine.Copilot));
            Assert.AreEqual("claude", MarkAgentEngines.CommandOf(MarkAgentEngine.Claude));
            Assert.AreEqual("opencode", MarkAgentEngines.CommandOf(MarkAgentEngine.OpenCode));
        }

        [TestMethod]
        public void Refuse_to_open_anything_when_there_is_no_environment()
        {
            // Senza ambiente non c'è un CLI da aprire: una stringa vuota farebbe lanciare un
            // processo senza nome.
            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => MarkAgentEngines.CommandOf(MarkAgentEngine.None));
            StringAssert.Contains(ex.Message, "ambiente agentico");
        }

        [TestMethod]
        public void Refuse_a_command_a_terminal_would_read_as_something_else()
        {
            var dir = Path.GetTempPath();
            foreach (var bad in new[] { "copilot; rm -rf /", "copilot && x", "$(whoami)", "co pilot", "" })
            {
                Assert.ThrowsException<ArgumentException>(
                    () => ProcessUtil.BuildAgentCliStartInfo(dir, bad), $"'{bad}'");
            }
        }

        /// <summary>
        /// ⚠️ Su Linux <c>UseShellExecute=true</c> NON apre un terminale: .NET lo traduce in
        /// <c>xdg-open</c>, che aprirebbe il file <c>pwsh</c> con l'applicazione predefinita. Serve
        /// un emulatore di terminale vero.
        /// </summary>
        [TestMethod]
        public void Open_a_real_terminal_on_linux()
        {
            if (!OperatingSystem.IsLinux())
            {
                Assert.Inconclusive("prova specifica di Linux");
            }

            var dir = Path.Combine(Path.GetTempPath(), "mde agent cli");   // con lo spazio, apposta
            Directory.CreateDirectory(dir);
            try
            {
                var psi = ProcessUtil.BuildAgentCliStartInfo(dir, "opencode");

                Assert.IsFalse(psi.UseShellExecute, "su Linux UseShellExecute finirebbe in xdg-open");
                StringAssert.Contains(psi.FileName, "terminal", "atteso un emulatore di terminale");
                Assert.AreEqual(dir, psi.WorkingDirectory);

                var script = psi.ArgumentList.LastOrDefault();
                StringAssert.Contains(script, "opencode");
                StringAssert.Contains(script, dir);
                // La finestra deve restare aperta quando il CLI esce, altrimenti un errore
                // all'avvio sparisce prima che si riesca a leggerlo.
                StringAssert.Contains(script, "exec bash");
                CollectionAssert.Contains(psi.ArgumentList.ToList(), "bash");
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch (IOException) { }
            }
        }

        /// <summary>
        /// ⚠️ Regressione vera del 21/09/2026: il controllo «installato» di Copilot risponde
        /// <c>true</c> fuori da Windows per scelta sua, e la matita apriva un terminale che diceva
        /// «command not found» invece del messaggio «non è installato». Qui la domanda si fa a una
        /// scansione vera del PATH, uguale per tutti e tre i CLI.
        /// </summary>
        [TestMethod]
        public void Tell_the_truth_about_what_is_installed()
        {
            // Un nome che non può esistere: deve rispondere di no su ogni piattaforma.
            Assert.IsFalse(ProcessUtil.IsCommandInPath("mde-comando-che-non-esiste-42"));
            Assert.IsFalse(ProcessUtil.IsCommandInPath(" "));
            Assert.IsFalse(ProcessUtil.IsCommandInPath(null));

            // E uno che c'è di sicuro: altrimenti la prova sopra passerebbe anche se il metodo
            // rispondesse sempre "no".
            var sicuro = OperatingSystem.IsWindows() ? "cmd" : "sh";
            Assert.IsTrue(ProcessUtil.IsCommandInPath(sicuro), $"'{sicuro}' dovrebbe essere nel PATH");
        }

        [TestMethod]
        public void Keep_powershell_on_windows()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Inconclusive("prova specifica di Windows");
            }

            var psi = ProcessUtil.BuildAgentCliStartInfo(@"C:\Progetti\Mio Progetto", "claude");

            StringAssert.Contains(psi.FileName, "powershell");
            StringAssert.Contains(psi.Arguments, "-NoExit");
            StringAssert.Contains(psi.Arguments, "claude");
            StringAssert.Contains(psi.Arguments, @"C:\Progetti\Mio Progetto");
            Assert.IsTrue(psi.UseShellExecute, "senza questo la finestra non ha vita propria");
        }
    }
}
