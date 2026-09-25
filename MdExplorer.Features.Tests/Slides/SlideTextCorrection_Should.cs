using HtmlAgilityPack;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>
    /// A text correction made on a slide page ("Modifica testo", as in documents): the page's blocks
    /// carry the file lines they come from, and the correction lands on the right characters of the
    /// file. The page is the slide page itself — the HTML SlideDeckRenderer writes — and the runs are
    /// read from its DOM as inline-edit.js reads them.
    /// </summary>
    [TestClass]
    public class SlideTextCorrection_Should
    {
        private static readonly HashSet<string> ObjectTags = new() { "img", "br", "input" };
        private static readonly HashSet<string> BlockTags = new() { "ul", "ol", "p", "blockquote", "pre", "table", "div" };

        private static string Page(string markdown, Func<string, string> before = null)
            => SlideDeckRenderer.Render(markdown, new SlideDeckRenderOptions
            {
                Pipeline = DocumentViewPipeline.Build(null),
                SourceHash = MarkdownFileEditor.SourceHash(markdown),
                BeforeMarkdown = before ?? (s => s),
            });

        private static HtmlNode Node(string page, string xpath)
        {
            var node = SlidePage.Slides(page).SelectSingleNode(xpath);
            Assert.IsNotNull(node, $"{xpath} not found");
            return node;
        }

        /// <summary>The 1-based line of the file holding <paramref name="text"/>.</summary>
        private static int LineOf(string markdown, string text)
            => markdown.Split('\n').Select((line, i) => (line, i)).Single(l => l.line.Contains(text)).i + 1;

        private static int StartLine(HtmlNode node) => int.Parse(node.GetAttributeValue("data-mde-line-start", "0"));

        private static List<RenderedRun> Runs(HtmlNode block)
        {
            var runs = new List<RenderedRun>();
            void Collect(HtmlNode node, List<string> path)
            {
                foreach (var child in node.ChildNodes)
                {
                    switch (child.NodeType)
                    {
                        case HtmlNodeType.Comment:
                            break;
                        case HtmlNodeType.Text:
                            runs.Add(new RenderedRun { Text = HtmlEntity.DeEntitize(child.InnerText), Path = path.ToArray() });
                            break;
                        case HtmlNodeType.Element when ObjectTags.Contains(child.Name):
                            runs.Add(new RenderedRun { Object = child.Name, Path = path.ToArray() });
                            break;
                        case HtmlNodeType.Element when BlockTags.Contains(child.Name):
                            break;
                        case HtmlNodeType.Element:
                            path.Add(child.Name);
                            Collect(child, path);
                            path.RemoveAt(path.Count - 1);
                            break;
                    }
                }
            }
            Collect(block, new List<string>());
            return runs;
        }

        /// <summary>Types <paramref name="replacement"/> over <paramref name="find"/> in the one text node holding it.</summary>
        private static string Correct(string markdown, string xpath, string find, string replacement, RenderedTextTarget target = null)
        {
            var block = Node(Page(markdown), xpath);
            var before = Runs(block);
            var after = before.Select(r => r.Text != null && r.Text.Contains(find)
                ? new RenderedRun { Text = r.Text.Replace(find, replacement), Path = r.Path }
                : r).ToList();
            var edit = RenderedTextEditor.Apply(markdown, DocumentViewPipeline.Build(null),
                target ?? new RenderedTextTarget { Line = StartLine(block) }, before, after);
            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            return edit.NewContent;
        }

        /// <summary>Deletes all the text of the block at <paramref name="xpath"/>, as the page sends it: runs after = none.</summary>
        private static RenderedTextEdit DeleteAll(string markdown, string xpath)
        {
            var block = Node(Page(markdown), xpath);
            return RenderedTextEditor.Apply(markdown, DocumentViewPipeline.Build(null),
                new RenderedTextTarget { Line = StartLine(block) }, Runs(block), new List<RenderedRun>());
        }

        private const string Deck = @"---
title: Prova
document_type: slides
---

# Prova

---

## Punti

- Costi in crescita <!-- .element: class=""fragment"" -->
- Clienti nuovi <!-- .element: class=""fragment"" -->

Note:
Appunti del relatore.

--

Verticale
---
Testo della verticale

---

| a | b |
|---|---|
| x | yy |
";

        [TestMethod]
        public void Mark_the_page_with_the_files_fingerprint()
        {
            StringAssert.Contains(Page(Deck), $"data-mde-source-hash=\"{MarkdownFileEditor.SourceHash(Deck)}\"");
        }

        [TestMethod]
        public void Give_each_block_the_file_line_it_comes_from()
        {
            var page = Page(Deck);

            Assert.AreEqual(LineOf(Deck, "# Prova"), StartLine(Node(page, "//h1")));
            Assert.AreEqual(LineOf(Deck, "## Punti"), StartLine(Node(page, "//h2")));
            Assert.AreEqual(LineOf(Deck, "Costi in crescita"), StartLine(Node(page, "(//li)[1]")));
            Assert.AreEqual(LineOf(Deck, "Clienti nuovi"), StartLine(Node(page, "(//li)[2]")));
            Assert.AreEqual(LineOf(Deck, "Verticale"), StartLine(Node(page, "//p[contains(.,'Verticale') and not(contains(.,'della'))]")));
            Assert.AreEqual(LineOf(Deck, "Testo della verticale"), StartLine(Node(page, "//p[contains(.,'Testo della verticale')]")));
            Assert.AreEqual(LineOf(Deck, "| a | b |"), StartLine(Node(page, "//table")));
        }

        [TestMethod]
        public void Give_no_line_to_speaker_notes()
        {
            var notes = Node(Page(Deck), "//aside[@class='notes']");

            Assert.IsNull(notes.SelectSingleNode(".//*[@data-mde-line-start]"));
        }

        [TestMethod]
        public void Give_no_lines_without_the_fingerprint()
        {
            var page = SlideDeckRenderer.Render(Deck, new SlideDeckRenderOptions { Pipeline = DocumentViewPipeline.Build(null) });

            Assert.IsFalse(page.Contains("data-mde-line-start"));
            Assert.IsFalse(page.Contains("data-mde-source-hash"));
        }

        [TestMethod]
        public void Keep_the_lines_right_when_a_command_adds_lines_and_leave_generated_blocks_without_one()
        {
            var deck = "---\ndocument_type: slides\n---\n\n## Uno\n\n@@include@@\n\n---\n\n## Due\n\nTesto della due\n";
            // A command that writes three paragraphs in place of one line, as an inclusion does.
            var page = Page(deck, body => body.Replace("@@include@@", "Incluso uno\n\nIncluso due\n\nIncluso tre"));

            Assert.AreEqual(LineOf(deck, "## Due"), StartLine(Node(page, "(//h2)[2]")));
            Assert.AreEqual(LineOf(deck, "Testo della due"), StartLine(Node(page, "//p[contains(.,'Testo della due')]")));
            Assert.AreEqual(0, StartLine(Node(page, "//p[contains(.,'Incluso due')]")), "generated: no line");
        }

        [TestMethod]
        public void Correct_a_fragment_item_and_keep_its_comment()
        {
            var corrected = Correct(Deck, "(//li)[1]", "Costi", "Spese");

            StringAssert.Contains(corrected, "- Spese in crescita <!-- .element: class=\"fragment\" -->");
            Assert.AreEqual(Deck.Replace("Costi in crescita", "Spese in crescita"), corrected);
        }

        [TestMethod]
        public void Correct_a_title_written_right_above_a_separator()
        {
            // On the slide page «Verticale» is a paragraph (the --- separates slides); Markdig reading
            // the file sees a setext heading on the same line with the same text.
            var corrected = Correct(Deck, "//p[contains(.,'Verticale') and not(contains(.,'della'))]", "Verticale", "Approfondimento");

            Assert.AreEqual(Deck.Replace("\nVerticale\n---", "\nApprofondimento\n---"), corrected);
        }

        [TestMethod]
        public void Correct_a_table_cell()
        {
            var page = Page(Deck);
            var table = Node(page, "//table");
            var corrected = Correct(Deck, "(//tbody/tr)[1]/td[2]", "yy", "zz",
                new RenderedTextTarget { Line = StartLine(table), Row = 1, Column = 1 });

            Assert.AreEqual(Deck.Replace("| x | yy |", "| x | zz |"), corrected);
        }
    
        // ── Deleting all the text: the block goes away with its lines ──

        [TestMethod]
        public void Delete_a_fragment_item_with_its_bullet_and_its_comment()
        {
            var edit = DeleteAll(Deck, "(//li)[1]");

            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            Assert.IsTrue(edit.BlockDeleted);
            Assert.AreEqual(Deck.Replace("- Costi in crescita <!-- .element: class=\"fragment\" -->\n", ""), edit.NewContent);
        }

        [TestMethod]
        public void Delete_the_only_item_of_a_list()
        {
            var deck = "---\ndocument_type: slides\n---\n\n## Uno\n\n- Solo\n\n---\n\n## Due\n";

            var edit = DeleteAll(deck, "//li");

            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            Assert.AreEqual(deck.Replace("- Solo\n", ""), edit.NewContent);
        }

        [TestMethod]
        public void Delete_a_paragraph_with_one_blank_line()
        {
            var edit = DeleteAll(Deck, "//p[contains(.,'Testo della verticale')]");

            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            Assert.AreEqual(Deck.Replace("Testo della verticale\n\n", ""), edit.NewContent);
        }

        [TestMethod]
        public void Refuse_to_delete_an_item_with_sub_items()
        {
            var deck = "---\ndocument_type: slides\n---\n\n## Uno\n\n- Padre\n  - Figlio\n- Altro\n";

            var edit = DeleteAll(deck, "(//li)[1]");

            Assert.AreEqual(RenderedTextRefusal.BlockDeletionNotAllowed, edit.Refusal);
        }

        [TestMethod]
        public void Refuse_to_delete_a_title_written_above_a_separator()
        {
            // In the file Markdig reads «Verticale» + «---» as a setext heading: removing it would take
            // the slide separator away.
            var edit = DeleteAll(Deck, "//p[contains(.,'Verticale') and not(contains(.,'della'))]");

            Assert.AreEqual(RenderedTextRefusal.BlockDeletionNotAllowed, edit.Refusal);
        }

        [TestMethod]
        public void Refuse_a_deletion_that_would_join_two_lists()
        {
            var deck = "---\ndocument_type: slides\n---\n\n## Uno\n\n- a\n\nIn mezzo\n\n- b\n";

            var edit = DeleteAll(deck, "//p[contains(.,'In mezzo')]");

            Assert.AreEqual(RenderedTextRefusal.BlockDeletionNotAllowed, edit.Refusal);
        }

        [TestMethod]
        public void Keep_an_emptied_cell_in_its_table()
        {
            var table = Node(Page(Deck), "//table");
            var cell = Node(Page(Deck), "(//tbody/tr)[1]/td[2]");

            var edit = RenderedTextEditor.Apply(Deck, DocumentViewPipeline.Build(null),
                new RenderedTextTarget { Line = StartLine(table), Row = 1, Column = 1 }, Runs(cell), new List<RenderedRun>());

            Assert.AreEqual(RenderedTextEditStatus.Applied, edit.Status, $"{edit.Refusal}: {edit.Detail}");
            Assert.IsFalse(edit.BlockDeleted);
            StringAssert.Contains(edit.NewContent, "| x |  |");
        }
    }
}
