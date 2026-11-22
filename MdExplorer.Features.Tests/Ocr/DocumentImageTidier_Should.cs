using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class DocumentImageTidier_Should
    {
        private const string Good = "KIND: diagram\nTITLE: Flusso degli ordini\n```plantuml\n@startuml\nCliente -> Negozio : ordine\n@enduml\n```\nDESCRIPTION:\nIl cliente manda l'ordine al negozio.";
        private const string Broken = "KIND: diagram\nTITLE: Flusso degli ordini\n```plantuml\n@startuml\nROTTO\n@enduml\n```\nDESCRIPTION:\nIl cliente manda l'ordine al negozio.";

        private const string Undrawn = "KIND: diagram\nTITLE: Flusso degli ordini\nDESCRIPTION:\nQualcuno manda qualcosa a un negozio.";

        private static string Redraw(string body) => "DESCRIPTION:\nIl cliente manda l'ordine al negozio.\nELEMENTS:\n- Cliente\n- Negozio\nRELATIONS:\n- Cliente -> Negozio\nDOUBTS:\n- none\n```plantuml\n@startuml\n" + body + "\n@enduml\n```";

        private string _project;
        private string _document;
        private List<string> _prompts;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "mde-tidy-" + Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(_project, "assets", "doc_img"));
            foreach (var name in new[] { "img1.png", "img2.png", "img3.emf" })
            {
                File.WriteAllBytes(Path.Combine(_project, "assets", "doc_img", name), new byte[] { 1, 2, 3 });
            }
            _document = Path.Combine(_project, "doc.md");
            File.WriteAllText(_document, "# Ordini\n\nCome funziona.\n\n![](/assets/doc_img/img1.png)\n\nIl logo ![](/assets/doc_img/img2.png) qui.\n\n![](/assets/doc_img/img3.emf)\n\n![](/assets/doc_img/manca.png)\n");
            _prompts = new List<string>();
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_project, true);

        // Il motore finto risponde in sequenza; la verifica finta boccia i diagrammi che contengono «ROTTO».
        private DocumentImageTidier Tidier(params string[] answers)
        {
            var queue = new Queue<string>(answers);
            return new DocumentImageTidier(_document, _project, "ISTRUZIONI", "RIDISEGNA",
                prompt => { _prompts.Add(prompt); return Task.FromResult(queue.Dequeue()); },
                new PlantumlBlockVerifier(source => Task.FromResult(source.Contains("ROTTO")
                    ? PlantumlCheckOutcome.FromProcess(200, "ERROR\n2\nSyntax Error?\n")
                    : PlantumlCheckOutcome.FromProcess(0, string.Empty))));
        }

        [TestMethod]
        public void ListTheImagesStillToDo()
        {
            CollectionAssert.AreEqual(
                new[] { "/assets/doc_img/img1.png", "/assets/doc_img/img2.png", "/assets/doc_img/img3.emf", "/assets/doc_img/manca.png" },
                DocumentImageTidier.PendingImages(File.ReadAllText(_document)));
        }

        [TestMethod]
        public async Task RedrawADiagramThatPassesTheCheck_AndTellTheModelWhereTheImageIs()
        {
            var (action, comment) = await Tidier(Good).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Redrawn, action);
            StringAssert.Contains(comment, "Flusso degli ordini");
            var text = File.ReadAllText(_document);
            StringAssert.Contains(text, "```plantuml\n@startuml\nCliente -> Negozio : ordine\n@enduml\n```");
            StringAssert.Contains(text, "![Flusso degli ordini](/assets/doc_img/img1.png)\n\n</details>");
            StringAssert.Contains(_prompts[0], "ISTRUZIONI");
            StringAssert.Contains(_prompts[0], Path.Combine(_project, "assets", "doc_img", "img1.png"));
            StringAssert.Contains(_prompts[0], "Come funziona.\n\n[THE IMAGE IS HERE]");
        }

        [TestMethod]
        public async Task AskOnceMoreWhenTheDiagramFailsTheCheck_AndUseTheCorrectedOne()
        {
            var (action, _) = await Tidier(Broken, Good).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Redrawn, action);
            Assert.AreEqual(2, _prompts.Count);
            StringAssert.Contains(_prompts[1], "ROTTO");
            StringAssert.Contains(_prompts[1], "---- CHECK ----");
            Assert.IsFalse(File.ReadAllText(_document).Contains("ROTTO"));
        }

        [TestMethod]
        public async Task RedrawFromTheDescriptionAndTheDocument_WhenTheImageGaveNoDiagram_WithoutShowingTheImageAgain()
        {
            var (action, comment) = await Tidier(Undrawn, Redraw("Cliente -> Negozio : ordine")).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Redrawn, action, comment);
            Assert.AreEqual(2, _prompts.Count);
            StringAssert.Contains(_prompts[1], "RIDISEGNA");
            StringAssert.Contains(_prompts[1], "Qualcuno manda qualcosa a un negozio.");
            StringAssert.Contains(_prompts[1], "---- DOCUMENT OUTLINE ----\n# Ordini");
            StringAssert.Contains(_prompts[1], "Come funziona.\n\n[THE IMAGE IS HERE]");
            Assert.IsFalse(_prompts[1].Contains("img1.png"), "il secondo tentativo non deve guardare l'immagine");

            var text = File.ReadAllText(_document);
            StringAssert.Contains(text, "Come funziona.\n\n```plantuml\n@startuml\nCliente -> Negozio : ordine\n@enduml\n```");
            StringAssert.Contains(text, "![Flusso degli ordini](/assets/doc_img/img1.png)\n\nIl cliente manda l'ordine al negozio.\n\n</details>");
            Assert.IsFalse(text.Contains("> **"), text);
            Assert.AreEqual(0, DocumentImageTidy.Find(text).Count(image => image.Path.EndsWith("img1.png") && !image.Treated));
        }

        [TestMethod]
        public async Task RedrawFromTheDescription_WhenTheDiagramFromTheImageNeverPasses()
        {
            var (action, _) = await Tidier(Broken, Broken, Redraw("Cliente -> Negozio : ordine")).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Redrawn, action);
            Assert.AreEqual(3, _prompts.Count);
            Assert.IsFalse(File.ReadAllText(_document).Contains("ROTTO"));
        }

        [TestMethod]
        public async Task SayAtWhichStageItIs_SoThatWhoWatchesKnowsItIsWorking()
        {
            var stages = new List<string>();
            var tidier = Tidier(Broken, Broken, Redraw("Cliente -> Negozio : ordine"));
            tidier.OnStage = stages.Add;

            await tidier.TidyAsync("/assets/doc_img/img1.png");

            // immagine, verifica, correzione, dalla descrizione, verifica, confronto dei nomi
            Assert.AreEqual(6, stages.Count, string.Join(" | ", stages));
            Assert.IsTrue(stages.TrueForAll(stage => stage.Trim().Length > 0));
        }

        [TestMethod]
        public async Task KeepTheDiagramWithAWarning_WhenItDoesNotMatchTheDescription()
        {
            // «Magazzino» non è né nella descrizione né nel documento; «Negozio» è dichiarato ma non disegnato.
            var (action, comment) = await Tidier(Undrawn, Redraw("Cliente -> Magazzino : ordine")).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.RedrawnToCheck, action);
            StringAssert.Contains(comment, "Magazzino");
            StringAssert.Contains(comment, "Negozio");
            var text = File.ReadAllText(_document);
            StringAssert.Contains(text, "> **");
            StringAssert.Contains(text, "«Magazzino»");
            StringAssert.Contains(text, "```plantuml\n@startuml\nCliente -> Magazzino : ordine\n@enduml\n```");
        }

        [TestMethod]
        public async Task KeepTheDiagramWithAWarning_WhenTheModelDeclaresADoubt()
        {
            var answer = Redraw("Cliente -> Negozio : ordine").Replace("- none", "- il documento parla di un fornitore, la descrizione no");

            var (action, _) = await Tidier(Undrawn, answer).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.RedrawnToCheck, action);
            StringAssert.Contains(File.ReadAllText(_document), "il documento parla di un fornitore, la descrizione no.\n\n```plantuml");
        }

        [TestMethod]
        public async Task KeepTheSourceFoldedAsText_WhenTheDiagramFromTheDescriptionDoesNotDraw()
        {
            var (action, comment) = await Tidier(Undrawn, Redraw("ROTTO"), Redraw("ROTTO ancora")).TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Described, action, comment);
            Assert.AreEqual(3, _prompts.Count);
            StringAssert.Contains(_prompts[2], "---- CHECK ----");
            var text = File.ReadAllText(_document);
            Assert.IsFalse(text.Contains("```plantuml"), text);
            StringAssert.Contains(text, "![Flusso degli ordini](/assets/doc_img/img1.png)\n\n<details>");
            StringAssert.Contains(text, "Il cliente manda l'ordine al negozio.");
            StringAssert.Contains(text, "```text\n@startuml\nROTTO ancora\n@enduml\n```\n\n</details>");
        }

        [TestMethod]
        public async Task KeepTheFirstDescription_WhenTheSecondAttemptAnswersOutOfFormat_AndSaySo()
        {
            var (action, comment) = await Tidier(Undrawn, "Non saprei.", "Davvero non saprei.").TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Described, action);
            StringAssert.Contains(comment, "DESCRIPTION");
            StringAssert.Contains(File.ReadAllText(_document), "Qualcuno manda qualcosa a un negozio.\n\n</details>");
        }

        [TestMethod]
        public async Task NotAskForADiagramFromTheDescription_ForAScreenshot()
        {
            var (action, _) = await Tidier("KIND: screenshot\nTITLE: Finestra\nDESCRIPTION:\nUna finestra.").TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Described, action);
            Assert.AreEqual(1, _prompts.Count);
        }

        [TestMethod]
        public async Task AskAgainWhenTheAnswerIsOutOfFormat_ThenGiveUpWithoutTouchingTheDocument()
        {
            var before = File.ReadAllText(_document);

            var error = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => Tidier("È uno schema.", "Te l'ho detto, è uno schema.").TidyAsync("/assets/doc_img/img1.png"));

            StringAssert.Contains(error.Message, "KIND");
            StringAssert.Contains(_prompts[1], "not in the required format");
            Assert.AreEqual(before, File.ReadAllText(_document));
        }

        [TestMethod]
        public async Task LeaveDecorativeUnreadableAndMissingImagesAlone_WithoutWastingAQuestionOnTheLastTwo()
        {
            var before = File.ReadAllText(_document);
            var tidier = Tidier("KIND: decorative");

            Assert.AreEqual(DocumentImageTidier.Skipped, (await tidier.TidyAsync("/assets/doc_img/img1.png")).Action);
            var unreadable = await tidier.TidyAsync("/assets/doc_img/img3.emf");
            Assert.AreEqual(DocumentImageTidier.Skipped, unreadable.Action);
            StringAssert.Contains(unreadable.Comment, ".emf");
            Assert.AreEqual(DocumentImageTidier.Failed, (await tidier.TidyAsync("/assets/doc_img/manca.png")).Action);

            Assert.AreEqual(1, _prompts.Count);
            Assert.AreEqual(before, File.ReadAllText(_document));
        }

        [TestMethod]
        public async Task GiveAnImageInsideTheTextOnlyItsAlternativeText()
        {
            var (action, _) = await Tidier("KIND: screenshot\nTITLE: Logo del negozio\nDESCRIPTION:\nUn logo.").TidyAsync("/assets/doc_img/img2.png");

            Assert.AreEqual(DocumentImageTidier.Captioned, action);
            StringAssert.Contains(File.ReadAllText(_document), "Il logo ![Logo del negozio](/assets/doc_img/img2.png) qui.");
        }

        [TestMethod]
        public async Task TranscribeAnImageOfText()
        {
            var (action, _) = await Tidier("KIND: text\nTITLE: Avviso\nDESCRIPTION:\nIl negozio resta chiuso lunedì.").TidyAsync("/assets/doc_img/img1.png");

            Assert.AreEqual(DocumentImageTidier.Transcribed, action);
            StringAssert.Contains(File.ReadAllText(_document), "![Avviso](/assets/doc_img/img1.png)\n\n<details>");
            StringAssert.Contains(File.ReadAllText(_document), "Il negozio resta chiuso lunedì.\n\n</details>");
        }

        [TestMethod]
        public async Task RefuseAnImageOutsideTheProject()
        {
            File.WriteAllText(_document, "![](../../fuori.png)\n");

            Assert.AreEqual(DocumentImageTidier.Failed, (await Tidier().TidyAsync("../../fuori.png")).Action);
            Assert.AreEqual(0, _prompts.Count);
        }
    }
}
