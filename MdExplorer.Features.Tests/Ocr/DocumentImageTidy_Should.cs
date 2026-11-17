using System;
using System.Linq;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class DocumentImageTidy_Should
    {
        private const string Document =
            "# Architettura\n\nIl sistema è fatto così.\n\n![](assets/doc_img/img1.png)\n\nPoi c'è il logo ![](assets/doc_img/img2.png) in mezzo.\n\n" +
            "```markdown\n![](assets/doc_img/finta.png)\n```\n\n![Planimetria](assets/doc_img/img3.png)\nTesto attaccato.\n";

        private static ImageVerdict Verdict(string answer) => ImageVerdict.Parse(answer);

        [TestMethod]
        public void FindTheImagesOutsideCode_AndSayWhichStandAlone()
        {
            var images = DocumentImageTidy.Find(Document);

            CollectionAssert.AreEqual(new[] { "assets/doc_img/img1.png", "assets/doc_img/img2.png", "assets/doc_img/img3.png" },
                images.Select(i => i.Path).ToList());
            CollectionAssert.AreEqual(new[] { true, false, true }, images.Select(i => i.Standalone).ToList());
            Assert.IsTrue(images.All(i => !i.Treated));
        }

        [TestMethod]
        public void PutTheDiagramInPlaceOfTheImage_KeepingTheOriginalFolded()
        {
            var image = DocumentImageTidy.Find(Document)[0];
            var verdict = Verdict("KIND: diagram\nTITLE: Architettura a tre livelli\n```plantuml\n@startuml\nA -> B\n@enduml\n```\nDESCRIPTION:\nTre livelli.");

            var result = DocumentImageTidy.WithDiagram(Document, image, verdict);

            StringAssert.Contains(result, "Il sistema è fatto così.\n\n```plantuml\n@startuml\nA -> B\n@enduml\n```\n\n<details>\n<summary>");
            StringAssert.Contains(result, "</summary>\n\n![Architettura a tre livelli](assets/doc_img/img1.png)\n\n</details>\n\nPoi c'è il logo");
            // Il testo intorno è lo stesso, carattere per carattere.
            Assert.AreEqual(Document.Substring(0, image.Start), result.Substring(0, image.Start));
            Assert.IsTrue(result.EndsWith(Document.Substring(image.Start + image.Length)));
            // Rifatto il giro, quell'immagine risulta già sistemata.
            Assert.IsTrue(DocumentImageTidy.Find(result).Single(i => i.Path.EndsWith("img1.png")).Treated);
        }

        [TestMethod]
        public void PutTheDescriptionUnderTheImage_AndCloseTheBlockBeforeTheTextThatFollows()
        {
            var image = DocumentImageTidy.Find(Document)[2];
            var verdict = Verdict("KIND: photo\nTITLE: Planimetria\nDESCRIPTION:\nTre capannoni affiancati.\n\nIl terzo è il più grande.");

            var result = DocumentImageTidy.WithDescription(Document, image, verdict);

            StringAssert.Contains(result, "![Planimetria](assets/doc_img/img3.png)\n\n<details>\n<summary>");
            StringAssert.Contains(result, "</summary>\n\nTre capannoni affiancati.\n\nIl terzo è il più grande.\n\n</details>\n\nTesto attaccato.\n");
            Assert.IsTrue(DocumentImageTidy.Find(result).Single(i => i.Path.EndsWith("img3.png")).Treated);
        }

        [TestMethod]
        public void GiveOnlyTheAlternativeTextToAnImageInsideTheText()
        {
            var image = DocumentImageTidy.Find(Document)[1];
            var verdict = Verdict("KIND: screenshot\nTITLE: Logo [aziendale]\nDESCRIPTION:\nUn logo.");

            var result = DocumentImageTidy.WithAlternativeText(Document, image, verdict);

            StringAssert.Contains(result, "Poi c'è il logo ![Logo aziendale](assets/doc_img/img2.png) in mezzo.");
            Assert.IsTrue(DocumentImageTidy.Find(result)[1].Treated);
            Assert.ThrowsException<InvalidOperationException>(() => DocumentImageTidy.WithDescription(Document, image, verdict));
        }

        [TestMethod]
        public void ReadADecorativeVerdictWithoutAnythingElse()
        {
            Assert.AreEqual(ImageVerdict.Decorative, Verdict("KIND: decorative").Kind);
        }

        [TestMethod]
        public void RefuseAnAnswerOutOfFormat_SayingWhatIsMissing()
        {
            StringAssert.Contains(Assert.ThrowsException<FormatException>(() => Verdict("È uno schema.")).Message, "KIND");
            StringAssert.Contains(Assert.ThrowsException<FormatException>(() => Verdict("KIND: diagram\nDESCRIPTION:\nx")).Message, "TITLE");
            StringAssert.Contains(Assert.ThrowsException<FormatException>(() => Verdict("KIND: photo\nTITLE: x")).Message, "DESCRIPTION");
            StringAssert.Contains(Assert.ThrowsException<FormatException>(
                () => Verdict("KIND: diagram\nTITLE: x\n```plantuml\nA -> B\n```\nDESCRIPTION:\ny")).Message, "@startuml");
            StringAssert.Contains(Assert.ThrowsException<FormatException>(
                () => Verdict("KIND: photo\nTITLE: x\nDESCRIPTION:\ny\n</details>")).Message, "details");
        }
    }
}
