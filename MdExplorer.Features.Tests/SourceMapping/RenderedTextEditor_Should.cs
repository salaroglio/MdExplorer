using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Markdig;
using MdExplorer.Features.Services.SourceMapping;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.SourceMapping
{
    /// <summary>
    /// Corrections made on the rendered page. The page is simulated as the browser will see it:
    /// the HTML of the document view (<see cref="MarkdownSourceMapService"/>, with its
    /// <c>data-mde-line-*</c>), runs read from its DOM. After every applied correction the new
    /// file is rendered again and must show exactly what was typed — the promise to the user.
    /// </summary>
    [TestClass]
    public class RenderedTextEditor_Should
    {
        private static readonly MarkdownSourceMapService SourceMap = new MarkdownSourceMapService(NullLogger<MarkdownSourceMapService>.Instance);

        // The document view's own pipeline, without the optional Jira links.
        private static readonly MarkdownPipeline Pipeline = DocumentViewPipeline.Build(null);

        private static readonly HashSet<string> ObjectTags = new HashSet<string> { "img", "br", "input" };
        private static readonly HashSet<string> BlockTags = new HashSet<string> { "ul", "ol", "p", "blockquote", "pre", "table", "div" };
        private static readonly HashSet<string> FormattingTags = new HashSet<string> { "strong", "em", "a", "code", "del", "sub", "sup", "ins", "mark" };

        // ── The page, simulated ────────────────────────────────────────────────────────────

        private static XmlElement Element(string markdown, string xpath)
        {
            var html = SourceMap.RenderHtmlWithSourceMap(markdown, markdown, Pipeline);
            var dom = new XmlDocument { PreserveWhitespace = true };
            dom.LoadXml("<root>" + html + "</root>");
            var element = dom.SelectSingleNode(xpath) as XmlElement;
            Assert.IsNotNull(element, $"{xpath} not found in:\n{html}");
            return element;
        }

        /// <summary>The block's own text nodes and objects; nested blocks (a sub-list) are not its text.</summary>
        private static List<RenderedRun> Runs(XmlElement block)
        {
            var runs = new List<RenderedRun>();
            Collect(block, new List<string>(), runs);
            return runs;
        }

        private static void Collect(XmlNode node, List<string> path, List<RenderedRun> runs)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child)
                {
                    case XmlComment _:
                        break;
                    case XmlCharacterData data:
                        runs.Add(T(data.Value, path.ToArray()));
                        break;
                    case XmlElement element when ObjectTags.Contains(element.LocalName):
                        runs.Add(O(element.LocalName, path.ToArray()));
                        break;
                    case XmlElement element when BlockTags.Contains(element.LocalName):
                        break;
                    case XmlElement element:
                        path.Add(element.LocalName);
                        Collect(element, path, runs);
                        path.RemoveAt(path.Count - 1);
                        break;
                }
            }
        }

        private static RenderedTextTarget TargetOf(XmlElement element)
        {
            if (element.LocalName == "td" || element.LocalName == "th")
            {
                var row = (XmlElement)element.ParentNode;
                var section = (XmlElement)row.ParentNode;
                var table = (XmlElement)section.ParentNode;
                return new RenderedTextTarget
                {
                    Line = int.Parse(table.GetAttribute("data-mde-line-start")),
                    Row = section.LocalName == "thead" ? 0 : ElementIndex(row) + 1, // pipe tables: one header row
                    Column = ElementIndex(element)
                };
            }
            return new RenderedTextTarget { Line = int.Parse(element.GetAttribute("data-mde-line-start")) };
        }

        private static int ElementIndex(XmlElement element)
            => element.ParentNode.ChildNodes.OfType<XmlElement>().ToList().IndexOf(element);

        private static RenderedRun T(string text, params string[] path) => new RenderedRun { Text = text, Path = path };
        private static RenderedRun O(string tag, params string[] path) => new RenderedRun { Object = tag, Path = path };

        /// <summary>Types in the one text node holding <paramref name="find"/>: same node, same formatting, as the browser does.</summary>
        private static Func<List<RenderedRun>, List<RenderedRun>> Replace(string find, string replacement)
            => runs =>
            {
                var holders = runs.Where(r => r.Text != null && r.Text.Contains(find)).ToList();
                Assert.AreEqual(1, holders.Count, $"\"{find}\" must be in exactly one run");
                var holder = holders[0];
                var at = holder.Text.IndexOf(find, StringComparison.Ordinal);
                Assert.AreEqual(-1, holder.Text.IndexOf(find, at + 1, StringComparison.Ordinal), $"\"{find}\" must be in the run once");
                var changed = T(holder.Text.Substring(0, at) + replacement + holder.Text.Substring(at + find.Length), holder.Path);
                return runs.Select(r => r == holder ? changed : r).ToList();
            };

        /// <summary>The browser drops an element whose text is all deleted.</summary>
        private static Func<List<RenderedRun>, List<RenderedRun>> RemoveRunsUnder(string tag)
            => runs => runs.Where(r => !(r.Path ?? Array.Empty<string>()).Contains(tag)).ToList();

        private static RenderedTextEdit Correct(string markdown, string xpath, Func<List<RenderedRun>, List<RenderedRun>> change)
        {
            var element = Element(markdown, xpath);
            var before = Runs(element);
            var after = change(before);
            var edit = RenderedTextEditor.Apply(markdown, Pipeline, TargetOf(element), before, after);

            if (edit.Status == RenderedTextEditStatus.Applied)
            {
                var reloaded = Runs(Element(edit.NewContent, xpath));
                Assert.AreEqual(Visible(after), Visible(reloaded), "the corrected file must show what was typed");
            }
            return edit;
        }

        /// <summary>Characters with their formatting, edges trimmed: what the reader sees.</summary>
        private static string Visible(List<RenderedRun> runs)
        {
            var symbols = runs.SelectMany(r =>
            {
                var path = string.Join("/", (r.Path ?? Array.Empty<string>()).Where(FormattingTags.Contains));
                return r.Object != null
                    ? new[] { (Text: $"<{r.Object}>", Path: path) }
                    : r.Text.Replace("\r", string.Empty).Select(c => (Text: c.ToString(), Path: path)).ToArray();
            }).ToList();
            var first = symbols.FindIndex(s => !string.IsNullOrWhiteSpace(s.Text));
            if (first < 0) return string.Empty;
            var last = symbols.FindLastIndex(s => !string.IsNullOrWhiteSpace(s.Text));
            return string.Join("|", symbols.Skip(first).Take(last - first + 1).Select(s => s.Text + "@" + s.Path));
        }

        private static string Applied(RenderedTextEdit edit)
        {
            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            return edit.NewContent;
        }

        private static void Refused(RenderedTextEdit edit, RenderedTextRefusal refusal)
        {
            Assert.AreEqual(RenderedTextEditStatus.Refused, edit.Status, $"expected {refusal}, got content:\n{edit.NewContent}");
            Assert.AreEqual(refusal, edit.Refusal, edit.Detail);
        }

        // ── Text ───────────────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Start_a_setext_heading_on_its_text_and_correct_it()
        {
            // Markdig's Line of "Titolo" over "---" is the underline's; the page says where the text is.
            const string markdown = "Intro.\n\nTitolo del capitolo\n---\n\nTesto.\n";
            Assert.AreEqual("3", Element(markdown, "//h2").GetAttribute("data-mde-line-start"));

            Assert.AreEqual("Intro.\n\nTitolo della parte\n---\n\nTesto.\n",
                Applied(Correct(markdown, "//h2", Replace("del capitolo", "della parte"))));
        }


        [TestMethod]
        public void ChangeAWordOfAParagraph()
        {
            Assert.AreEqual("# T\n\nUna frase giusta qui.\n\nAltro.\n",
                Applied(Correct("# T\n\nUna frase sbagliata qui.\n\nAltro.\n", "(//p)[1]", Replace("sbagliata", "giusta"))));
        }

        [TestMethod]
        public void DeletePartOfASentence()
        {
            Assert.AreEqual("Questa frase è lunga.\n",
                Applied(Correct("Questa frase è davvero troppo lunga.\n", "(//p)[1]", Replace(" davvero troppo", ""))));
        }

        [TestMethod]
        public void LeaveTheRestOfTheFileByteForByte()
        {
            const string file = "Titolo\n======\n\n__grassetto__ e _corsivo_ e un errrore.\n\n1) primo\n* voce\n";
            Assert.AreEqual(file.Replace("errrore", "errore"),
                Applied(Correct(file, "(//p)[1]", Replace("errrore", "errore"))));
        }

        [TestMethod]
        public void MakeTwoCorrectionsInOneBlock()
        {
            Assert.AreEqual("Primo errore e secondo errore.\n",
                Applied(Correct("Primo erore e secondo erore.\n", "(//p)[1]", Replace("Primo erore e secondo erore.", "Primo errore e secondo errore."))));
        }

        [TestMethod]
        public void CorrectTextInsideNestedFormatting()
        {
            Assert.AreEqual("Testo **grassetto _nidificato_** fine.\n",
                Applied(Correct("Testo **grassetto _annidato_** fine.\n", "(//p)[1]", Replace("annidato", "nidificato"))));
        }

        [TestMethod]
        public void CorrectTheTextOfALinkNotItsAddress()
        {
            Assert.AreEqual("Vai a [la home](http://x.com/pagina \"titolo\") ora.\n",
                Applied(Correct("Vai a [la pagina](http://x.com/pagina \"titolo\") ora.\n", "(//p)[1]", Replace("la pagina", "la home"))));
        }

        [TestMethod]
        public void CorrectTheTextOfAReferenceLink()
        {
            Assert.AreEqual("Vedi [la fonte][r].\n\n[r]: http://example.com\n",
                Applied(Correct("Vedi [il riferimento][r].\n\n[r]: http://example.com\n", "(//p)[1]", Replace("il riferimento", "la fonte"))));
        }

        [TestMethod]
        public void CorrectInlineCode()
        {
            Assert.AreEqual("Chiama `funzione()` e `` a ` b `` qui.\n",
                Applied(Correct("Chiama `funzone()` e `` a ` b `` qui.\n", "(//p)[1]", Replace("funzone", "funzione"))));
        }

        [TestMethod]
        public void CorrectTheLinesOfAParagraphWithLineBreaks()
        {
            Assert.AreEqual("prima riga  \nseconda riga\\\nterza riga\nquarta\n",
                Applied(Correct("prima riga  \nseconda rga\\\nterza riga\nquarta\n", "(//p)[1]", Replace("rga", "riga"))));
        }

        [TestMethod]
        public void KeepCrlf()
        {
            Assert.AreEqual("# T\r\n\r\nriga giusta\r\naltra riga\r\n",
                Applied(Correct("# T\r\n\r\nriga sbagliata\r\naltra riga\r\n", "(//p)[1]", Replace("sbagliata", "giusta"))));
        }

        [TestMethod]
        public void CorrectABlockHoldingAnEmoji()
        {
            // No page command turns :smile: into a PNG: the page shows Markdig's character.
            Assert.AreEqual("Ciao :smile: mondo sbagliato.\n",
                Applied(Correct("Ciao :smile: mondo sbaglaito.\n", "(//p)[1]", Replace("sbaglaito", "sbagliato"))));
        }

        // ── Blocks ─────────────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CorrectAtxAndSetextHeadings()
        {
            Assert.AreEqual("# Titolo giusto ##\n\nSotto titolo\n------------\n",
                Applied(Correct("# Titolo sbagliato ##\n\nSotto titolo\n------------\n", "(//h1)[1]", Replace("sbagliato", "giusto"))));
            Assert.AreEqual("# Titolo ##\n\nIl titolo\n------------\n",
                Applied(Correct("# Titolo ##\n\nSotto titolo\n------------\n", "(//h2)[1]", Replace("Sotto", "Il"))));
        }

        [TestMethod]
        public void CorrectListItemsWithoutTouchingTheirSubLists()
        {
            const string file = "1. voce\n   - annidata **forte**\n   - altra\n";
            Assert.AreEqual("1. prima\n   - annidata **forte**\n   - altra\n",
                Applied(Correct(file, "(//ol/li)[1]", Replace("voce", "prima"))));
            Assert.AreEqual("1. voce\n   - interna **forte**\n   - altra\n",
                Applied(Correct(file, "(//ul/li)[1]", Replace("annidata", "interna"))));
        }

        [TestMethod]
        public void CorrectATaskListItem()
        {
            Assert.AreEqual("- [ ] compito giusto\n",
                Applied(Correct("- [ ] compito sbagliato\n", "(//li)[1]", Replace("sbagliato", "giusto"))));
        }

        [TestMethod]
        public void CorrectAQuotedParagraph()
        {
            Assert.AreEqual("> citazione giusta\n> seconda riga\n",
                Applied(Correct("> citazione sbagliata\n> seconda riga\n", "(//blockquote/p)[1]", Replace("sbagliata", "giusta"))));
        }

        [TestMethod]
        public void CorrectTheCellsOfATable()
        {
            const string file = "| Nome | Valore |\r\n|------|--------|\r\n| alfa | 1 \\| 2 |\r\n| beta | tre |\r\n";

            Assert.AreEqual(file.Replace("| tre |", "| quattro |"),
                Applied(Correct(file, "(//tbody/tr)[2]/td[2]", Replace("tre", "quattro"))));
            Assert.AreEqual(file.Replace("| Nome |", "| Chiave |"),
                Applied(Correct(file, "(//thead/tr)[1]/th[1]", Replace("Nome", "Chiave"))));
            Assert.AreEqual(file.Replace("1 \\| 2", "1 \\| 3"),
                Applied(Correct(file, "(//tbody/tr)[1]/td[2]", Replace("2", "3"))), "the escaped pipe stays escaped");
        }

        [TestMethod]
        public void ClearACell()
        {
            Assert.AreEqual("| a | b |\n|---|---|\n|  | y |\n",
                Applied(Correct("| a | b |\n|---|---|\n| x | y |\n", "(//tbody/tr)[1]/td[1]", _ => new List<RenderedRun>())));
        }

        // ── Formatting: follows the page ───────────────────────────────────────────────────

        [TestMethod]
        public void PutTypedTextInsideTheBoldWhenThePageShowsItBold()
        {
            Assert.AreEqual("Molto **forte** davvero.\n",
                Applied(Correct("Molto **fort** davvero.\n", "(//p)[1]", Replace("fort", "forte"))));
        }

        [TestMethod]
        public void PutTypedTextOutsideTheBoldWhenThePageShowsItPlain()
        {
            Assert.AreEqual("Molto **fort**e davvero.\n",
                Applied(Correct("Molto **fort** davvero.\n", "(//p)[1]", Replace(" davvero.", "e davvero."))));
        }

        [TestMethod]
        public void KeepTheBoldWhenAllItsTextIsRetyped()
        {
            Assert.AreEqual("Un **nuovo** testo.\n",
                Applied(Correct("Un **vecchio** testo.\n", "(//p)[1]", Replace("vecchio", "nuovo"))));
        }

        [TestMethod]
        public void RemoveTheMarkersOfABoldWhoseTextIsDeleted()
        {
            Assert.AreEqual("Un  testo.\n",
                Applied(Correct("Un **inutile** testo.\n", "(//p)[1]", RemoveRunsUnder("strong"))));
        }

        [TestMethod]
        public void RemoveALinkWhoseTextIsDeleted()
        {
            Assert.AreEqual("Vai  ora.\n",
                Applied(Correct("Vai [qui](http://x.com \"t\") ora.\n", "(//p)[1]", RemoveRunsUnder("a"))));
        }

        [TestMethod]
        public void RemoveAReferenceLinkButNotItsDefinition()
        {
            Assert.AreEqual("Vedi  ora.\n\n[r]: http://x.com\n",
                Applied(Correct("Vedi [qui][r] ora.\n\n[r]: http://x.com\n", "(//p)[1]", RemoveRunsUnder("a"))));
        }

        [TestMethod]
        public void EscapeTypedTextThatWouldBecomeMarkdown()
        {
            Assert.AreEqual("Costo 5 \\* 3 \\[euro\\] e \\_poco\\_.\n",
                Applied(Correct("Costo 5 euro.\n", "(//p)[1]", Replace("5 euro", "5 * 3 [euro] e _poco_"))));
            Assert.AreEqual("| a |\n|---|\n| x \\| y |\n",
                Applied(Correct("| a |\n|---|\n| x |\n", "(//tbody/tr)[1]/td[1]", Replace("x", "x | y"))));
        }

        // ── Refusals: nothing written, and why ─────────────────────────────────────────────

        [TestMethod]
        public void ReportNoChange()
        {
            Assert.AreEqual(RenderedTextEditStatus.NoChange, Correct("Testo.\n", "(//p)[1]", runs => runs).Status);
        }

        [TestMethod]
        public void RefuseToAddOrRemoveALineBreak()
        {
            Refused(Correct("riga uno\nriga due\n", "(//p)[1]", Replace("uno\nriga", "uno riga")), RenderedTextRefusal.LineBreakNotAllowed);
            Refused(Correct("riga uno\nriga due\n", "(//p)[1]", Replace("uno", "uno\naltra")), RenderedTextRefusal.LineBreakNotAllowed);
        }

        [TestMethod]
        public void RefuseToTouchAnEntityOrAnEmoji()
        {
            Refused(Correct("A &amp; B.\n", "(//p)[1]", Replace("A & B", "A B")), RenderedTextRefusal.ProtectedContentTouched);
            Refused(Correct("Ciao :smile: mondo.\n", "(//p)[1]", Replace("😄 ", "")), RenderedTextRefusal.ProtectedContentTouched);
        }

        [TestMethod]
        public void RefuseToChangeTheTextOfAnAutolink()
        {
            Refused(Correct("Sito https://example.com qui.\n", "(//p)[1]", Replace("example", "esempio")), RenderedTextRefusal.ProtectedContentTouched);
        }

        [TestMethod]
        public void RefuseFormattingThatIsNotThere()
        {
            var element = Element("Testo normale.\n", "(//p)[1]");
            var before = Runs(element);
            var after = new List<RenderedRun> { T("Testo "), T("nuovo", "strong"), T(" normale.") };

            Refused(RenderedTextEditor.Apply("Testo normale.\n", Pipeline, TargetOf(element), before, after),
                RenderedTextRefusal.FormattingNotAllowed);
        }

        [TestMethod]
        public void RefuseWhenThePageDoesNotShowTheFile()
        {
            var edit = RenderedTextEditor.Apply("Testo del file.\n", Pipeline, new RenderedTextTarget { Line = 1 },
                new List<RenderedRun> { T("Testo della pagina.") },
                new List<RenderedRun> { T("Testo della pagina!") });

            Refused(edit, RenderedTextRefusal.RenderedTextMismatch);
        }

        [TestMethod]
        public void RefuseTypedTextThatWouldRenderAsSomethingElse()
        {
            Refused(Correct("Vedi sito.\n", "(//p)[1]", Replace("sito", "https://x.com")), RenderedTextRefusal.VerificationFailed);
            Refused(Correct("Ciao mondo.\n", "(//p)[1]", Replace("mondo", ":smile:")), RenderedTextRefusal.VerificationFailed);
        }

        /// <summary>
        /// Emptying a paragraph removes it with its lines (user's request, 25/09/2026 — it used to be
        /// refused). The deletion rules and their refusals: SlideTextCorrection_Should.
        /// </summary>
        [TestMethod]
        public void RemoveAnEmptiedParagraph()
        {
            var markdown = "Primo.\n\nSecondo.\n";
            var element = Element(markdown, "(//p)[1]");

            var edit = RenderedTextEditor.Apply(markdown, Pipeline, TargetOf(element), Runs(element), new List<RenderedRun>());

            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            Assert.IsTrue(edit.BlockDeleted);
            Assert.AreEqual("Secondo.\n", edit.NewContent);
        }

        [TestMethod]
        public void RefuseATargetWithNoBlock()
        {
            var edit = RenderedTextEditor.Apply("Testo.\n", Pipeline, new RenderedTextTarget { Line = 9 },
                new List<RenderedRun> { T("Testo.") }, new List<RenderedRun> { T("Testi.") });

            Refused(edit, RenderedTextRefusal.BlockNotFound);
        }
    }
}
