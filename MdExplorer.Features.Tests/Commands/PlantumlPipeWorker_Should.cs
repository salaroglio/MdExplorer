using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Commands
{
    /// <summary>
    /// Il PlantUML che resta acceso (23/11/2026). Le prove con Java vero girano se c'è un <c>java</c> e il jar del
    /// repository; senza, sono inconcludenti. Le regole su che cosa può rendere girano sempre.
    /// </summary>
    [TestClass]
    public class PlantumlPipeWorker_Should
    {
        private const string Good = "@startuml\nAlice -> Bob : ciao àè\n@enduml\n";
        private const string Broken = "@startuml\nAlice -> : \nfoo bar baz qux\n@enduml\n";

        private static string Java()
        {
            var candidates = new[]
            {
                Environment.GetEnvironmentVariable("JAVA_HOME") is string home && home.Length > 0 ? Path.Combine(home, "bin", OperatingSystem.IsWindows() ? "java.exe" : "java") : null,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sdkman", "candidates", "java", "current", "bin", "java"),
            }.Where(c => c != null && File.Exists(c)).ToList();
            if (candidates.Count > 0) return candidates[0];
            var onPath = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
                .Select(d => Path.Combine(d, OperatingSystem.IsWindows() ? "java.exe" : "java")).FirstOrDefault(File.Exists);
            if (onPath == null) Assert.Inconclusive("nessun java su questa macchina");
            return onPath;
        }

        private static string Jar()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var jar = Path.Combine(dir.FullName, "MdExplorer", "Binaries", "plantuml.jar");
                if (File.Exists(jar)) return jar;
            }
            Assert.Inconclusive("plantuml.jar non trovato");
            return null;
        }

        private static string Arguments() => $"-Dfile.encoding=UTF-8 -jar \"{Jar()}\" -pipe -charset UTF-8 -tsvg";

        /// <summary>Il lancio singolo di sempre: un Java per diagramma.</summary>
        private static async Task<byte[]> OneShot(string code)
        {
            using var process = Process.Start(new ProcessStartInfo(Java())
            {
                Arguments = Arguments(), RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
            });
            var output = new MemoryStream();
            var copying = process.StandardOutput.BaseStream.CopyToAsync(output);
            var errors = process.StandardError.ReadToEndAsync();
            var input = new UTF8Encoding(false).GetBytes(code);
            await process.StandardInput.BaseStream.WriteAsync(input);
            process.StandardInput.Close();
            await process.WaitForExitAsync();
            await copying; await errors;
            return output.ToArray();
        }

        [TestMethod]
        public void Take_only_a_diagram_it_can_answer_once_and_without_files_of_the_document()
        {
            Assert.IsTrue(PlantumlPipeWorker.CanRender(Good));
            Assert.IsTrue(PlantumlPipeWorker.CanRender("@startmindmap\n* a\n** b\n@endmindmap"));
            Assert.IsTrue(PlantumlPipeWorker.CanRender("  @startjson\n{\"a\":1}\n  @endjson\n"));
            Assert.IsFalse(PlantumlPipeWorker.CanRender(Good + Good), "due diagrammi darebbero due risposte");
            Assert.IsFalse(PlantumlPipeWorker.CanRender("Alice -> Bob"), "senza @start non si sa dove finisce");
            Assert.IsFalse(PlantumlPipeWorker.CanRender("@startuml\nAlice -> Bob\n"), "senza @end il processo aspetterebbe per sempre");
            Assert.IsFalse(PlantumlPipeWorker.CanRender("@startuml\n!include comune.puml\nAlice -> Bob\n@enduml"), "il file si cerca dalla cartella del documento");
            Assert.IsFalse(PlantumlPipeWorker.CanRender("@startuml\n  !includesub altro.puml!parte\n@enduml"));
            Assert.IsFalse(PlantumlPipeWorker.CanRender("@startuml\nnote: " + PlantumlPipeWorker.Delimiter + "\n@enduml"));
            Assert.IsFalse(PlantumlPipeWorker.CanRender(""));
        }

        [TestMethod]
        public async Task Render_many_diagrams_with_one_java_and_give_the_same_bytes_as_a_single_launch()
        {
            using var worker = new PlantumlPipeWorker();
            var first = await worker.RenderAsync(Java(), Arguments(), Good);
            var pid = worker.ProcessId;
            Assert.IsNotNull(pid);

            var classes = "@startuml\nclass Ordine\nclass Riga\nOrdine --> Riga\n@enduml";   // senza fine riga in fondo
            var second = await worker.RenderAsync(Java(), Arguments(), classes);
            var again = await worker.RenderAsync(Java(), Arguments(), Good);

            Assert.AreEqual(pid, worker.ProcessId, "lo stesso processo per tutti");
            CollectionAssert.AreEqual(await OneShot(Good), first, "gli stessi byte del lancio singolo");
            CollectionAssert.AreEqual(await OneShot(classes + "\n"), second);
            CollectionAssert.AreEqual(first, again, "niente resta in mezzo fra un diagramma e l'altro");
            StringAssert.Contains(Encoding.UTF8.GetString(first), "ciao");
        }

        [TestMethod]
        public async Task Say_a_wrong_diagram_as_the_single_launch_does_and_go_on_with_the_next()
        {
            using var worker = new PlantumlPipeWorker();
            await worker.RenderAsync(Java(), Arguments(), Good);
            var pid = worker.ProcessId;

            var error = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => worker.RenderAsync(Java(), Arguments(), Broken));
            StringAssert.StartsWith(error.Message, "PlantUML rendering failed (java exit code 200)");
            StringAssert.Contains(error.Message, "Syntax Error");

            var after = await worker.RenderAsync(Java(), Arguments(), Good);
            Assert.AreEqual(pid, worker.ProcessId, "un diagramma sbagliato non spegne il processo");
            StringAssert.Contains(Encoding.UTF8.GetString(after), "<svg");
            Assert.IsFalse(Encoding.UTF8.GetString(after).Contains("Syntax Error"), "l'errore di prima non sporca il diagramma dopo");
        }

        [TestMethod]
        public async Task Start_again_when_the_process_died_or_the_settings_changed()
        {
            using var worker = new PlantumlPipeWorker();
            await worker.RenderAsync(Java(), Arguments(), Good);
            var first = worker.ProcessId.Value;

            Process.GetProcessById(first).Kill();
            await Task.Delay(300);
            StringAssert.Contains(Encoding.UTF8.GetString(await worker.RenderAsync(Java(), Arguments(), Good)), "<svg");
            var second = worker.ProcessId.Value;
            Assert.AreNotEqual(first, second, "morto: riavviato");

            await worker.RenderAsync(Java(), Arguments() + " -nometadata", Good);
            Assert.AreNotEqual(second, worker.ProcessId.Value, "argomenti diversi: un processo nuovo");
        }

        [TestMethod]
        public async Task Close_its_java_when_disposed()
        {
            var worker = new PlantumlPipeWorker();
            await worker.RenderAsync(Java(), Arguments(), Good);
            var pid = worker.ProcessId.Value;
            worker.Dispose();
            await Task.Delay(300);
            Assert.IsFalse(Process.GetProcesses().Any(p => p.Id == pid && p.ProcessName.StartsWith("java", StringComparison.OrdinalIgnoreCase)), "nessun Java rimasto acceso");
        }
    }
}
