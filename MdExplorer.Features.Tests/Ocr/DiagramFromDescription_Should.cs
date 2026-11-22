using System;
using System.Linq;
using MdExplorer.Features.Services.Ocr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Ocr
{
    [TestClass]
    public class DiagramFromDescription_Should
    {
        private const string Answer = "DESCRIPTION:\nIl cliente manda l'ordine al negozio.\nELEMENTS:\n- Cliente\n- `Gestione ordini`\nRELATIONS:\n- Cliente -> Gestione ordini\nDOUBTS:\n- none\n```plantuml\n@startuml\nactor Cliente\n@enduml\n```";

        [TestMethod]
        public void ReadTheAnswerOfTheSecondAttempt()
        {
            var verdict = RedrawVerdict.Parse(Answer.Replace("\n", "\r\n"));

            Assert.AreEqual("Il cliente manda l'ordine al negozio.", verdict.Description);
            CollectionAssert.AreEqual(new[] { "Cliente", "Gestione ordini" }, verdict.Elements);
            Assert.AreEqual(("Cliente", "Gestione ordini"), verdict.Relations.Single());
            Assert.AreEqual(0, verdict.Doubts.Count);
            Assert.AreEqual("@startuml\nactor Cliente\n@enduml", verdict.PlantUml);
        }

        [TestMethod]
        public void AcceptAnAnswerWithoutDiagramAndWithoutRelations()
        {
            var verdict = RedrawVerdict.Parse("DESCRIPTION:\nUna piantina.\nELEMENTS:\n- Sala\nRELATIONS:\n- none\nDOUBTS:\n- la scala non si capisce");

            Assert.IsNull(verdict.PlantUml);
            Assert.AreEqual(0, verdict.Relations.Count);
            Assert.AreEqual("la scala non si capisce", verdict.Doubts.Single());
        }

        [TestMethod]
        public void SayWhatIsMissingFromAnAnswerOutOfFormat()
        {
            StringAssert.Contains(Assert.ThrowsExactly<FormatException>(() => RedrawVerdict.Parse("Ecco lo schema.")).Message, "DESCRIPTION");
            StringAssert.Contains(Assert.ThrowsExactly<FormatException>(() => RedrawVerdict.Parse("DESCRIPTION:\nTesto.")).Message, "ELEMENTS");
            StringAssert.Contains(Assert.ThrowsExactly<FormatException>(
                () => RedrawVerdict.Parse("DESCRIPTION:\nTesto.\nELEMENTS:\n- A\nRELATIONS:\n- A verso B")).Message, "A verso B");
            StringAssert.Contains(Assert.ThrowsExactly<FormatException>(
                () => RedrawVerdict.Parse("DESCRIPTION:\nTesto.\nELEMENTS:\n- A\n```plantuml\nA -> B\n```")).Message, "@startuml");
        }

        [TestMethod]
        public void FindNothingToCheckInAFaithfulDiagram_AlsoWithAliasesAndLineBreaks()
        {
            var diagram = "@startuml\n!theme plain\nactor Cliente\nrectangle \"Gestione\\nordini\" as GO\n[Magazzino centrale] as M\nCliente --> GO : \"ordine\"\nGO -down-> M\n@enduml";

            var problems = DiagramFidelity.Check(diagram,
                new[] { "Cliente", "Gestione ordini", "Magazzino centrale" },
                new[] { ("Cliente", "Gestione ordini"), ("Gestione ordini", "Magazzino centrale") },
                "Il cliente si rivolge alla gestione ordini.", "Nel capitolo si parla del magazzino centrale.");

            Assert.AreEqual(0, problems.Count, string.Join(" | ", problems));
        }

        [TestMethod]
        public void ReportInventedNamesMissingElementsAndMissingConnections()
        {
            var diagram = "@startuml\nCliente -> Negozio : ordine\nNegozio -> Corriere : pacco\nparticipant Banca\n@enduml";

            var problems = DiagramFidelity.Check(diagram,
                new[] { "Cliente", "Negozio", "Banca", "Fornitore" },
                new[] { ("Cliente", "Negozio"), ("Negozio", "Banca"), ("Fornitore", "Negozio") },
                "Il cliente ordina al negozio, che paga la banca e chiama il fornitore.", "# Ordini");

            Assert.AreEqual(3, problems.Count, string.Join(" | ", problems));
            StringAssert.Contains(problems[0], "«Corriere»");
            Assert.IsFalse(problems[0].Contains("Banca") || problems[0].Contains("ordine"), problems[0]);
            StringAssert.Contains(problems[1], "«Fornitore»");
            StringAssert.Contains(problems[2], "«Negozio → Banca»");
            Assert.IsFalse(problems[2].Contains("Fornitore"), problems[2]);
        }

        [TestMethod]
        public void NotCheckConnectionsWhereTheyAreNotOnALine_ButStillCheckTheNames()
        {
            var activity = "@startuml\nstart\n:Ricevi ordine;\n:Prepara pacco;\nstop\n@enduml";

            var none = DiagramFidelity.Check(activity, new[] { "Ricevi ordine", "Prepara pacco" }, new[] { ("Ricevi ordine", "Prepara pacco") },
                "Si riceve l'ordine (Ricevi ordine) e poi: Prepara pacco.", string.Empty);
            var invented = DiagramFidelity.Check(activity, new[] { "Ricevi ordine" }, new (string, string)[0], "Ricevi ordine.", string.Empty);

            Assert.AreEqual(0, none.Count, string.Join(" | ", none));
            StringAssert.Contains(invented.Single(), "«Prepara pacco»");
        }

        [TestMethod]
        public void CheckTheNodesOfAMindMap()
        {
            var problems = DiagramFidelity.Check("@startmindmap\n* Progetto\n** Analisi\n** Collaudo\n@endmindmap",
                new[] { "Progetto", "Analisi" }, new[] { ("Progetto", "Analisi") }, "Il progetto ha l'analisi.", string.Empty);

            StringAssert.Contains(problems.Single(), "«Collaudo»");
        }

        [TestMethod]
        public void GiveTheHeadingsOfTheDocumentAndTheSectionOfTheImage()
        {
            var markdown = "# Uno\n\nintro\n\n## Due\n\nprima\n\n![](a.png)\n\n### Tre\n\ndopo\n\n```\n# non un titolo\n```\n\n## Quattro\n\nfuori\n";
            var image = DocumentImageTidy.Find(markdown).Single();

            var (outline, section) = DocumentContext.Of(markdown, image);

            Assert.AreEqual("# Uno\n## Due\n### Tre\n## Quattro", outline);
            Assert.IsTrue(section.StartsWith("## Due\n\nprima\n\n[THE IMAGE IS HERE]"), section);
            StringAssert.Contains(section, "dopo");
            Assert.IsFalse(section.Contains("fuori") || section.Contains("intro"), section);
        }

        [TestMethod]
        public void GiveOnlyThePartAroundTheImageOfAVeryLongSection_AndSaySo()
        {
            var markdown = "# Uno\n\n" + new string('a', 20000) + "\n\n![](a.png)\n\n" + new string('b', 20000) + "\n";
            var image = DocumentImageTidy.Find(markdown).Single();

            var (_, section) = DocumentContext.Of(markdown, image);

            Assert.IsTrue(section.StartsWith("(the section is long"), section.Substring(0, 60));
            Assert.IsTrue(section.Length < DocumentContext.MaxSectionChars + 200);
            StringAssert.Contains(section, "[THE IMAGE IS HERE]");
        }
    }
}
