using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MdExplorer.Features.Services.SourceMapping;

namespace MdExplorer.Features.Tests.SourceMapping
{
    [TestClass]
    public class MarkdownChangeRegions_Should
    {
        private const string Base =
            "---\ntitle: Prova\n---\n\n" +                                   // 0-3
            "# Titolo\n\n" +                                                  // 4-5
            "Primo paragrafo.\nSeconda riga.\n\n" +                           // 6-8
            "- voce 1\n- voce 2\n- voce 3\n- voce 4\n\n" +                    // 9-13
            "| A | B |\n|---|---|\n| 1 | 2 |\n\n" +                           // 14-17
            "```plantuml\n@startuml\nAlice -> Bob\n@enduml\n```\n\n" +        // 18-23
            "Ultimo paragrafo con [un link][sito].\n\n" +                     // 24-25
            "[sito]: https://example.com\n";                                  // 26

        private static MarkdownChanges Compute(string oldText, string newText)
            => MarkdownChangeRegions.Compute(oldText, newText, DocumentViewPipeline.Build(null));

        /// <summary>«tipo vecchio:da-a nuovo:da-a», righe da 0; «-» dove la parte non c'è; «voci» per gli elenchi.</summary>
        private static string Describe(MarkdownChanges changes) => string.Join(" | ", changes.Regions.Select(r =>
            $"{r.Kind}{(r.ListItems ? " voci" : "")} vecchio:{(r.HasOld ? r.OldStart + "-" + r.OldEnd : "-")} nuovo:{(r.HasNew ? r.NewStart + "-" + r.NewEnd : "-")}"));

        [TestMethod]
        public void Find_nothing_when_the_two_versions_are_the_same()
        {
            var changes = Compute(Base, Base);
            Assert.AreEqual(0, changes.Regions.Count);
            Assert.IsFalse(changes.HiddenChanges);
        }

        [TestMethod]
        public void Treat_a_file_that_only_changed_its_line_endings_as_unchanged()
        {
            var changes = Compute(Base, Base.Replace("\n", "\r\n"));
            Assert.AreEqual(0, changes.Regions.Count);
        }

        [TestMethod]
        public void Widen_a_changed_line_to_its_whole_paragraph_and_to_nothing_more()
        {
            var changes = Compute(Base, Base.Replace("Seconda riga.", "Seconda riga, riscritta."));
            Assert.AreEqual("changed vecchio:6-7 nuovo:6-7", Describe(changes));
        }

        [TestMethod]
        public void Keep_a_changed_list_item_to_itself_when_its_neighbours_did_not_change()
        {
            var changes = Compute(Base, Base.Replace("- voce 3", "- voce 3 (rivista)"));
            Assert.AreEqual("changed voci vecchio:11-11 nuovo:11-11", Describe(changes));
            CollectionAssert.AreEqual(new[] { 11 }, changes.Regions[0].NewItemLines);
            CollectionAssert.AreEqual(new[] { 11 }, changes.Regions[0].OldItemLines);
        }

        [TestMethod]
        public void Say_where_a_removed_list_item_was_by_the_item_that_follows_or_precedes_it()
        {
            var middle = Compute(Base, Base.Replace("- voce 2\n", "")).Regions.Single();
            Assert.AreEqual("removed", middle.Kind);
            Assert.IsTrue(middle.ListItems);
            Assert.AreEqual(10, middle.NewBefore, "prima di «voce 3», che nel nuovo è alla riga 10");

            var last = Compute(Base, Base.Replace("- voce 4\n", "")).Regions.Single();
            Assert.AreEqual(-1, last.NewBefore);
            Assert.AreEqual(11, last.NewAfterItem, "dopo «voce 3», l'ultima rimasta");
        }

        [TestMethod]
        public void Take_the_whole_list_when_no_item_next_to_the_change_stayed_the_same()
        {
            var all = Base.Replace("- voce 1", "- uno").Replace("- voce 2", "- due").Replace("- voce 3", "- tre").Replace("- voce 4", "- quattro");
            Assert.AreEqual("changed vecchio:9-12 nuovo:9-12", Describe(Compute(Base, all)));
        }

        [TestMethod]
        public void Take_the_whole_list_when_the_change_also_touches_the_block_before_it()
        {
            var both = Base.Replace("Seconda riga.", "Seconda riga, riscritta.").Replace("- voce 1", "- voce uno");
            Assert.AreEqual("changed vecchio:6-12 nuovo:6-12", Describe(Compute(Base, both)));
        }

        [TestMethod]
        public void Take_the_whole_table_for_a_changed_cell()
        {
            var changes = Compute(Base, Base.Replace("| 1 | 2 |", "| 1 | 3 |"));
            Assert.AreEqual("changed vecchio:14-16 nuovo:14-16", Describe(changes));
        }

        [TestMethod]
        public void Take_the_whole_diagram_for_a_changed_line_inside_it()
        {
            var changes = Compute(Base, Base.Replace("Alice -> Bob", "Alice -> Carla"));
            Assert.AreEqual("changed vecchio:18-22 nuovo:18-22", Describe(changes));
        }

        [TestMethod]
        public void Report_a_block_added_in_the_middle_as_added()
        {
            var changes = Compute(Base, Base.Replace("| A | B |", "Paragrafo nuovo.\n\n| A | B |"));
            Assert.AreEqual("added vecchio:- nuovo:14-14", Describe(changes));
        }

        [TestMethod]
        public void Report_a_removed_block_with_the_place_where_it_was()
        {
            var changes = Compute(Base, Base.Replace("| A | B |\n|---|---|\n| 1 | 2 |\n\n", ""));
            Assert.AreEqual("removed vecchio:14-16 nuovo:-", Describe(changes));
            Assert.AreEqual(14, changes.Regions[0].NewBefore, "prima del diagramma, che nel nuovo inizia alla riga 14");
        }

        [TestMethod]
        public void Put_a_block_removed_at_the_end_after_the_last_line()
        {
            var text = "# Titolo\n\nUno.\n\nDue.\n";
            var changes = Compute(text, "# Titolo\n\nUno.\n");
            Assert.AreEqual("removed vecchio:4-4 nuovo:-", Describe(changes));
            Assert.AreEqual(changes.NewLines.Length, changes.Regions[0].NewBefore);
        }

        [TestMethod]
        public void Keep_two_changes_apart_when_an_unchanged_block_sits_between_them()
        {
            var changes = Compute(Base, Base.Replace("# Titolo", "# Titolo nuovo").Replace("| 1 | 2 |", "| 1 | 3 |"));
            Assert.AreEqual("changed vecchio:4-4 nuovo:4-4 | changed vecchio:14-16 nuovo:14-16", Describe(changes));
        }

        [TestMethod]
        public void Join_two_changes_in_blocks_that_touch()
        {
            var changes = Compute(Base, Base.Replace("# Titolo", "# Titolo nuovo").Replace("Primo paragrafo.", "Primo paragrafo, riscritto."));
            Assert.AreEqual("changed vecchio:4-7 nuovo:4-7", Describe(changes));
        }

        [TestMethod]
        public void See_two_paragraphs_joined_by_removing_the_empty_line_between_them()
        {
            var changes = Compute("Uno.\n\nDue.\n\nTre.\n", "Uno.\nDue.\n\nTre.\n");
            Assert.AreEqual("changed vecchio:0-2 nuovo:0-1", Describe(changes));
        }

        [TestMethod]
        public void Flag_changes_in_lines_that_have_no_block_on_the_page()
        {
            var yaml = Compute(Base, Base.Replace("title: Prova", "title: Altra"));
            Assert.AreEqual(0, yaml.Regions.Count);
            Assert.IsTrue(yaml.HiddenChanges, "intestazione YAML");

            var link = Compute(Base, Base.Replace("https://example.com", "https://example.org"));
            Assert.AreEqual(0, link.Regions.Count);
            Assert.IsTrue(link.HiddenChanges, "definizione di un link");
        }

        [TestMethod]
        public void Refuse_with_a_message_two_versions_too_different_to_compare()
        {
            var a = string.Join("\n\n", Enumerable.Range(0, 1500).Select(i => "riga a " + i));
            var b = string.Join("\n\n", Enumerable.Range(0, 1500).Select(i => "riga b " + i));
            var error = Assert.ThrowsExactly<InvalidOperationException>(() => Compute(a, b));
            StringAssert.Contains(error.Message, MarkdownSourceMapService.MaxDiffDistance.ToString());
        }

        [TestMethod]
        public void Put_two_markers_around_a_changed_block_in_both_texts()
        {
            var marked = MarkdownChangeMarkup.Build(Compute("Uno.\n\nDue.\n\nTre.\n", "Uno.\n\nDue, riscritto.\n\nTre.\n"));
            var expected = "Uno.\n\n\n<div class=\"mde-chg-s\" data-r=\"1\"></div>\n\n{0}\n\n<div class=\"mde-chg-e\" data-r=\"1\"></div>\n\n\nTre.\n";
            Assert.AreEqual(string.Format(expected, "Due, riscritto."), marked.NewText);
            Assert.AreEqual(string.Format(expected, "Due."), marked.OldText);
            Assert.AreEqual("changed", marked.Regions.Single().Kind);
        }

        [TestMethod]
        public void Put_an_empty_pair_of_markers_where_a_removed_block_was()
        {
            var marked = MarkdownChangeMarkup.Build(Compute("Uno.\n\nDue.\n\nTre.\n", "Uno.\n\nTre.\n"));
            Assert.AreEqual("Uno.\n\n\n<div class=\"mde-chg-s\" data-r=\"1\"></div>\n\n<div class=\"mde-chg-e\" data-r=\"1\"></div>\n\nTre.\n", marked.NewText);
        }

        [TestMethod]
        public void Give_list_items_by_their_line_in_the_text_with_the_markers()
        {
            // Una parte a blocchi PRIMA dell'elenco sposta le righe: la voce va cercata dove sta nel testo marcato.
            var marked = MarkdownChangeMarkup.Build(Compute(Base, Base.Replace("# Titolo", "# Titolo nuovo").Replace("- voce 3", "- voce 3 (rivista)")));
            var item = marked.Regions.Single(r => r.ListItems);
            var newLines = marked.NewText.Split('\n');
            Assert.AreEqual("- voce 3 (rivista)", newLines[item.NewItems.Single() - 1]);
            Assert.AreEqual("- voce 3", marked.OldText.Split('\n')[item.OldItems.Single() - 1]);
            Assert.IsFalse(marked.NewText.Split('\n').Skip(13).Take(6).Any(l => l.Contains("mde-chg")), "nessun marcatore dentro l'elenco");
        }

        [TestMethod]
        public void Render_the_markers_at_the_two_ends_of_the_changed_block()
        {
            var marked = MarkdownChangeMarkup.Build(Compute(Base, Base.Replace("| 1 | 2 |", "| 1 | 3 |")));
            var html = Markdig.Markdown.ToHtml(marked.NewText, DocumentViewPipeline.Build(null));
            var start = html.IndexOf("mde-chg-s", StringComparison.Ordinal);
            var table = html.IndexOf("<table", StringComparison.Ordinal);
            var end = html.IndexOf("mde-chg-e", StringComparison.Ordinal);
            Assert.IsTrue(start >= 0 && start < table && table < end, "marcatore, tabella, marcatore");
            Assert.AreEqual(1, System.Text.RegularExpressions.Regex.Matches(html, "<table").Count, "la tabella resta una tabella");
        }

        [TestMethod]
        public void Describe_a_new_document_as_one_added_part()
        {
            var changes = Compute("", "# Titolo\n\nTesto.\n");
            Assert.AreEqual("added vecchio:- nuovo:0-2", Describe(changes));
        }
    }
}
