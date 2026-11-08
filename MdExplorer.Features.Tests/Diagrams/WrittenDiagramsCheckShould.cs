using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Diagrams
{
    /// <summary>
    /// C4 della valutazione del 07/11: la verifica dei diagrammi a fine turno per Claude e opencode. Gli argomenti delle
    /// chiamate sono quelli veri dei due motori (Claude: Write/Edit con <c>file_path</c>; opencode: write/edit con
    /// <c>filePath</c>); il jar è finto, come negli altri test del verificatore.
    /// </summary>
    [TestClass]
    public class WrittenDiagramsCheckShould
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-diagrammi-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "docs"));
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private static Task<PlantumlCheckOutcome> Jar(string source)
            => Task.FromResult(source.Contains("sbagliato")
                ? PlantumlCheckOutcome.FromProcess(200, "ERROR\n3\nSyntax Error?")
                : PlantumlCheckOutcome.FromProcess(0, ""));

        private WrittenDiagramsCheck Check() => new WrittenDiagramsCheck(new PlantumlBlockVerifier(Jar), _root, "Claude");

        // I messaggi seguono la lingua dell'interfaccia (UiText.Say).
        private static void Says(string text, string it, string en)
            => Assert.IsTrue(text != null && (text.Contains(it) || text.Contains(en)), $"atteso «{it}» o «{en}» in: {text}");

        private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement;

        [TestMethod]
        public async Task Hand_back_the_broken_diagram_of_a_file_claude_wrote()
        {
            File.WriteAllText(Path.Combine(_root, "docs", "rotto.md"), "# T\n\n```plantuml\n@startuml\n!theme plain\nsbagliato\n@enduml\n```\n");
            var check = Check();
            check.ToolInput(Json("{\"file_path\":\"" + Path.Combine(_root, "docs", "rotto.md").Replace("\\", "\\\\") + "\",\"content\":\"…\"}"));

            var (notice, forModel) = await check.TakeAsync(lastRound: false, CancellationToken.None);

            Says(notice, "rimandati a Claude", "handed back to Claude");
            StringAssert.Contains(notice, Path.Combine("docs", "rotto.md"));
            Assert.IsNotNull(forModel);
            StringAssert.Contains(forModel, "da correggere");
        }

        [TestMethod]
        public async Task Read_the_relative_path_of_an_opencode_edit_and_say_when_all_is_well()
        {
            File.WriteAllText(Path.Combine(_root, "docs", "buono.md"), "```plantuml\n@startuml\n!theme plain\nA -> B : usa `x`\n@enduml\n```\n");
            var check = Check();
            check.ToolInput(Json("{\"filePath\":\"docs/buono.md\",\"oldString\":\"a\",\"newString\":\"b\"}"));

            var (notice, forModel) = await check.TakeAsync(lastRound: false, CancellationToken.None);

            Says(notice, "verificati ✓", "checked ✓");
            Assert.IsNull(forModel, "niente da correggere: nessun turno in più");
        }

        [TestMethod]
        public async Task Stop_after_the_correction_round_and_tell_the_user()
        {
            File.WriteAllText(Path.Combine(_root, "docs", "rotto.md"), "```plantuml\n@startuml\n!theme plain\nsbagliato\n@enduml\n```\n");
            var check = Check();
            check.ToolInput(Json("{\"file_path\":\"docs/rotto.md\"}"));

            var (notice, forModel) = await check.TakeAsync(lastRound: true, CancellationToken.None);

            Says(notice, "restano", "still in");
            Assert.IsNull(forModel, "dopo il turno di correzione non si rimanda più");
        }

        [TestMethod]
        public async Task Check_the_diagram_written_in_the_answer_and_stay_quiet_without_diagrams()
        {
            var check = Check();
            check.ToolInput(Json("{\"command\":\"ls docs\"}"));
            Assert.AreEqual((null, null), await check.TakeAsync(false, CancellationToken.None), "nessun file .md, nessun diagramma: niente da dire");

            check.AnswerText("Ecco:\n\n```plantuml\n@startuml\n!theme plain\n");
            check.AnswerText("sbagliato\n@enduml\n```\n");
            var (notice, forModel) = await check.TakeAsync(false, CancellationToken.None);
            Says(notice, "rimandati", "handed back");
            Says(forModel, "la tua risposta", "your answer");
        }
    }
}
