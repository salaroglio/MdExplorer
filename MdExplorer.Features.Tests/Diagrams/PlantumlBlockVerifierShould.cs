using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Diagrams
{
    /// <summary>
    /// The diagrams MarkAgent writes, checked by MdExplorer. The jar is replaced by a fake with
    /// the jar's own answers (exit 0; exit 200 with "ERROR / line / message"): the structural
    /// checks run for real.
    /// </summary>
    [TestClass]
    public class PlantumlBlockVerifierShould
    {
        private sealed class FakeJar
        {
            public readonly List<string> Checked = new List<string>();
            public PlantumlCheckOutcome Unavailable;

            public Task<PlantumlCheckOutcome> Check(string source)
            {
                Checked.Add(source);
                if (Unavailable != null) return Task.FromResult(Unavailable);
                // Like the jar: it rejects what it cannot read, and points at a line.
                return Task.FromResult(source.Contains("sbagliato")
                    ? PlantumlCheckOutcome.FromProcess(200, "ERROR\n3\nSyntax Error?")
                    : PlantumlCheckOutcome.FromProcess(0, ""));
            }
        }

        private static (PlantumlBlockVerifier Verifier, FakeJar Jar) Create()
        {
            var jar = new FakeJar();
            return (new PlantumlBlockVerifier(jar.Check), jar);
        }

        private const string Valid = "```plantuml\n@startuml\n!theme plain\nA -> B\n@enduml\n```\n";

        [TestMethod]
        public void FindTheBlocksThePageDraws()
        {
            var markdown =
                "# Titolo\n\n" +                                                   // 1-2
                "```plantuml\n@startuml\nA -> B\n@enduml\n```\n\n" +                // 3-7
                "- voce\n\n  ```plantuml\n  @startuml\n  C -> D\n  @enduml\n  ```\n\n" + // 9-15
                "````markdown\n```plantuml\n@startuml\nesempio\n@enduml\n```\n````\n\n" + // example, not a block
                "~~~plantuml\n@startuml\nX -> Y\n@enduml\n~~~\n\n" +                // ~~~ is not drawn
                "```plantuml(@json, ./dati.json)\n```\n\n" +                         // data file, not written by the model
                "```PlantUML\r\n@startuml\r\nE -> F\r\n@enduml\r\n```\r\n";

            var blocks = PlantumlBlockVerifier.FindBlocks(markdown);

            Assert.AreEqual(3, blocks.Count, string.Join(" | ", blocks.Select(b => b.Source)));
            Assert.AreEqual("@startuml\nA -> B\n@enduml", blocks[0].Source);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, blocks[0].FileLines);
            Assert.AreEqual(3, blocks[0].FenceLine);
            Assert.AreEqual("@startuml\nC -> D\n@enduml", blocks[1].Source, "indentation of the list item removed");
            CollectionAssert.AreEqual(new[] { 12, 13, 14 }, blocks[1].FileLines);
            Assert.AreEqual("@startuml\nE -> F\n@enduml", blocks[2].Source);
        }

        [TestMethod]
        public async Task PutEachProblemAtItsFileLine()
        {
            var (verifier, _) = Create();
            var markdown =
                "# Ordini\n\nTesto.\n\n" +                                          // 1-4
                "```plantuml\n@startuml\n!theme plain\nclass Ordine {\n  id\n@enduml\n```\n"; // 5-11, brace at 8

            var result = await verifier.VerifyAsync(new[] { ("docs/ordini.md", markdown) });

            Assert.AreEqual(1, result.Diagrams);
            var brace = result.Problems.Single(p => p.Source == "class Ordine {");
            Assert.AreEqual("docs/ordini.md", brace.Where);
            Assert.AreEqual(8, brace.FileLine, "the line of the file, not of the diagram");
            Assert.AreEqual(PlantumlCheckAnalyzer.Error, brace.Severity);
            StringAssert.Contains(brace.Fix, "}");
        }

        [TestMethod]
        public async Task ReportTheBacktickThatMakesTheDiagramVanish()
        {
            var (verifier, _) = Create();

            var result = await verifier.VerifyAsync(new[] { ("a.md", "```plantuml\n@startuml\n!theme plain\nA -> B : usa `x`\n@enduml\n```\n") });

            var p = result.Problems.Single();
            Assert.AreEqual(4, p.FileLine);
            StringAssert.Contains(p.Meaning, "troncato");
        }

        [TestMethod]
        public async Task PlaceTheJarVerdictToo()
        {
            var (verifier, _) = Create();

            // the fake jar says "line 3" of the diagram ("sbagliato"): that is line 6 of the file
            var result = await verifier.VerifyAsync(new[] { ("a.md", "Intro.\n\n```plantuml\n@startuml\n!theme plain\nsbagliato\n@enduml\n```\n") });

            var p = result.Problems.Single();
            Assert.AreEqual(PlantumlCheckAnalyzer.Error, p.Severity);
            Assert.AreEqual(6, p.FileLine);
        }

        [TestMethod]
        public async Task KeepWarningsAndLeaveOutStyleHints()
        {
            var (verifier, _) = Create();

            var withWhite = await verifier.VerifyAsync(new[] { ("a.md", "```plantuml\n@startuml\n!theme plain\nrectangle R #FFFFFF\n@enduml\n```\n") });
            var withoutTheme = await verifier.VerifyAsync(new[] { ("b.md", "```plantuml\n@startuml\nA -> B\n@enduml\n```\n") });

            Assert.AreEqual(PlantumlCheckAnalyzer.Warning, withWhite.Problems.Single().Severity, "white vanishes in dark theme");
            Assert.IsFalse(withoutTheme.HasProblems, "a missing !theme is only a hint: not worth a round of corrections");
        }

        [TestMethod]
        public async Task CheckSeveralTextsAndNameEach()
        {
            var (verifier, _) = Create();

            var result = await verifier.VerifyAsync(new[]
            {
                ("docs/uno.md", Valid),
                ("la tua risposta", "Ecco:\n\n```plantuml\n@startuml\n!theme plain\nA -> B : `x`\n@enduml\n```\n"),
            });

            Assert.AreEqual(2, result.Diagrams);
            Assert.AreEqual("la tua risposta", result.Problems.Single().Where);
            var text = result.ForModel();
            StringAssert.Contains(text, "la tua risposta, riga 6 (errore");
            StringAssert.Contains(text, "Correzione:");
        }

        [TestMethod]
        public async Task NotCheckTheSameDiagramTwice()
        {
            var (verifier, jar) = Create();

            await verifier.VerifyAsync(new[] { ("a.md", Valid) });
            await verifier.VerifyAsync(new[] { ("a.md", "Testo cambiato sopra.\n\n" + Valid) });

            Assert.AreEqual(1, jar.Checked.Count, "java costs about a second: an unchanged diagram is not checked again");
        }

        [TestMethod]
        public async Task SayTheCheckDidNotRunInsteadOfJudging()
        {
            var (verifier, jar) = Create();
            jar.Unavailable = PlantumlCheckOutcome.Unavailable("java non trovato");

            var result = await verifier.VerifyAsync(new[] { ("a.md", Valid) });

            Assert.AreEqual("java non trovato", result.ToolUnavailable);
            Assert.IsFalse(result.HasProblems, "not judged: no correction asked");

            jar.Unavailable = null;
            await verifier.VerifyAsync(new[] { ("a.md", Valid) });
            Assert.AreEqual(2, jar.Checked.Count, "'could not run' is not remembered: the next check tries again");
        }
    }
}
