using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using MdExplorer.Features.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Diagrams
{
    /// <summary>
    /// C4 della valutazione del 07/11: il corpo di un blocco ```plantuml si fermava al primo backtick. Si è cambiata solo la
    /// regola che trova il corpo (<see cref="PlantumlFence"/>); questi test fermano chi perde qualcosa delle copie di prima:
    /// sui documenti veri del repository ogni blocco senza backtick deve dare gli STESSI gruppi di prima (il gruppo 1 dà il
    /// nome ai file .svg/.png), e i blocchi che la pagina disegna sono quelli che il verificatore controlla.
    /// </summary>
    [TestClass]
    public class PlantumlFenceShould
    {
        private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        // Le regole di prima, copiate com'erano, una per chi legge i blocchi.
        private static readonly (string Who, string Old, string New)[] Readers =
        {
            ("svg (pagina, html, pdf)", @"```plantuml([^```]*)`{3}(?:(?={){([^{]*)}|)", PlantumlFence.Body + @"(?:(?={){([^{]*)}|)"),
            ("png, github, collegamenti", @"```plantuml([^```]*)```", PlantumlFence.Body),
            ("barra delle immagini", @"```plantuml([^```]+)```[{]?([^}]*)[}]?", PlantumlFence.Body + @"[{]?([^}]*)[}]?"),
        };

        private static IEnumerable<string> Corpus()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MdExplorer.sln"))) dir = dir.Parent;
            Assert.IsNotNull(dir, "radice del repository non trovata");
            foreach (var folder in new[] { "docs-internal", Path.Combine("MdExplorer", "skills"), "mdexplorer-demo" })
            {
                var root = Path.Combine(dir.FullName, folder);
                if (!Directory.Exists(root)) continue;
                foreach (var f in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(f);
                    if (text.IndexOf("```plantuml", StringComparison.OrdinalIgnoreCase) >= 0) yield return text;
                }
            }
        }

        [TestMethod]
        public void Give_every_reader_the_same_groups_as_before_for_blocks_without_backticks()
        {
            var corpus = Corpus().ToList();
            Assert.IsTrue(corpus.Count >= 10, $"pochi documenti con diagrammi: {corpus.Count}");
            var compared = 0;
            foreach (var (who, oldPattern, newPattern) in Readers)
            {
                foreach (var text in corpus)
                {
                    var olds = Regex.Matches(text, oldPattern, Options).Cast<Match>().ToList();
                    var news = Regex.Matches(text, newPattern, Options).Cast<Match>().ToDictionary(m => m.Index);
                    foreach (var old in olds)
                    {
                        // Un'apertura in mezzo a una frase non è un blocco (come per Markdig): lì cambia apposta.
                        var openLine = text.LastIndexOf('\n', Math.Max(0, old.Index - 1));
                        if (old.Index > 0 && text.Substring(openLine + 1, old.Index - openLine - 1).Trim(' ', '\t').Length != 0) continue;
                        // Un blocco di prima che finiva su un backtick singolo o doppio del corpo: lì cambia apposta.
                        if (!text.Substring(old.Groups[1].Index + old.Groups[1].Length).StartsWith("```")) continue;

                        Assert.IsTrue(news.TryGetValue(old.Index, out var now), $"{who}: un blocco di prima non si trova più:\n{old.Value}");
                        for (var g = 0; g < old.Groups.Count; g++)
                            Assert.AreEqual(old.Groups[g].Value, now.Groups[g].Value, $"{who}: gruppo {g} cambiato per:\n{old.Value}");
                        compared++;
                    }
                }
            }
            Assert.IsTrue(compared >= 30, $"confronti troppo pochi: {compared}");
        }

        [TestMethod]
        public void Keep_a_backtick_inside_the_diagram()
        {
            const string md = "Prima\n\n```plantuml\n@startuml\nrectangle \"`mvn compile`\" as A\nA -> A : ``doppio``\n@enduml\n```{.grande}\n\nDopo\n";

            foreach (var (who, _, newPattern) in Readers)
            {
                var m = Regex.Matches(md, newPattern, Options);
                Assert.AreEqual(1, m.Count, who);
                Assert.AreEqual("\n@startuml\nrectangle \"`mvn compile`\" as A\nA -> A : ``doppio``\n@enduml\n", m[0].Groups[1].Value, who);
            }
            var svg = Regex.Match(md, Readers[0].New, Options);
            Assert.AreEqual(".grande", svg.Groups[2].Value, "le opzioni dopo la chiusura restano");
        }

        [TestMethod]
        public void Draw_the_blocks_that_the_verifier_checks()
        {
            foreach (var text in Corpus().Append("Testo `codice` e\n\n```plantuml\n@startuml\nA -> B : `x`\n@enduml\n```\n\n````markdown\n```plantuml\n@startuml\nesempio\n@enduml\n```\n````\n"))
            {
                var regions = MarkdownCodeRegions.Of(text);
                var drawn = Regex.Matches(text, Readers[0].New, Options).Cast<Match>()
                    .Where(m => !regions.IsCode(m.Index))
                    .Where(m => !m.Groups[1].Value.StartsWith("(")) // plantuml(@json, …): espanso prima, il verificatore lo salta apposta
                    .Select(m => Normalize(m.Groups[1].Value)).ToList();
                var checkedByVerifier = PlantumlBlockVerifier.FindBlocks(text).Select(b => Normalize(b.Source)).ToList();
                var onlyDrawn = drawn.Except(checkedByVerifier).ToList();
                var onlyChecked = checkedByVerifier.Except(drawn).ToList();
                Assert.IsTrue(onlyDrawn.Count == 0 && onlyChecked.Count == 0 && drawn.Count == checkedByVerifier.Count,
                    $"pagina e verificatore vedono blocchi diversi ({drawn.Count} contro {checkedByVerifier.Count}).\nSolo la pagina:\n{string.Join("\n---\n", onlyDrawn.Select(Head))}\nSolo il verificatore:\n{string.Join("\n---\n", onlyChecked.Select(Head))}");
            }
        }

        // Il verificatore toglie la riga d'intestazione e l'ultima andata a capo; la pagina no (l'hash resta quello di prima).
        private static string Head(string s) => s.Length > 300 ? s.Substring(0, 300) + "…" : s;

        private static string Normalize(string body) => body.Replace("\r\n", "\n").Trim('\n', ' ', '\t');
    }
}
