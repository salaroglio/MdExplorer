using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.MarkPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Tests.MarkPoint
{
    /// <summary>
    /// "Chiedi a MarkAgent" on a point of a slide: MarkAgent chooses the keywords (Mark Search's
    /// grammar), MDE runs Mark Search's searches, and this turns their results into the files and
    /// passages MarkAgent explains from.
    /// </summary>
    [TestClass]
    public class ProjectSearchDigest_Should
    {
        private const string Root = "/proj";

        private static SearchResult Found(params string[] paths) => new()
        {
            Contents = paths.Select(p => new ContentSearchResult { Path = p, FileName = System.IO.Path.GetFileName(p) }).ToList()
        };

        [TestMethod]
        public void Read_the_keywords_as_Mark_Search_asks_them()
        {
            CollectionAssert.AreEqual(new[] { "breadcrumb", "slide" },
                MarkSearchKeywords.Parse("```json\n{\"keywords\": [\" breadcrumb \", \"slide\", \"\"]}\n```").ToList());
            CollectionAssert.AreEqual(new[] { "a" }, MarkSearchKeywords.Parse("{\"keywords\": [\"a\"]}").ToList());
            Assert.AreEqual(0, MarkSearchKeywords.Parse("{\"keywords\": []}").Count, "none chosen: no search");
            Assert.AreEqual(MarkSearchKeywords.MaxKeywords,
                MarkSearchKeywords.Parse("{\"keywords\": [\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]}").Count);
        }

        [TestMethod]
        public void Refuse_an_answer_without_the_keywords()
        {
            Assert.ThrowsException<FormatException>(() => MarkSearchKeywords.Parse("Cercherei breadcrumb e slide."));
            Assert.ThrowsException<FormatException>(() => MarkSearchKeywords.Parse("{\"words\": [\"a\"]}"));
            Assert.ThrowsException<FormatException>(() => MarkSearchKeywords.Parse("{\"keywords\": [1]}"));
            Assert.ThrowsException<FormatException>(() => MarkSearchKeywords.Parse("{\"keywords\": ["));
        }

        [TestMethod]
        public void Rank_first_the_file_found_by_the_rarer_keywords()
        {
            // «slide» is everywhere, «breadcrumb» in one file: that file comes first even though
            // another file is found by as many keywords.
            var searches = new List<(string, SearchResult)>
            {
                ("slide", Found("/proj/a.md", "/proj/b.md", "/proj/c.md", "/proj/d.md")),
                ("breadcrumb", Found("/proj/b.md")),
                ("partenza", Found("/proj/a.md", "/proj/c.md", "/proj/d.md")),
            };

            var sources = ProjectSearchDigest.Build(searches, Root, null, _ => "");

            Assert.AreEqual("b.md", sources[0].Path);
            CollectionAssert.AreEqual(new[] { "breadcrumb", "slide" }, sources[0].Keywords.ToList());
        }

        [TestMethod]
        public void Leave_out_the_document_asked_about_and_what_is_not_a_markdown_document()
        {
            var searches = new List<(string, SearchResult)>
            {
                ("logo", new SearchResult
                {
                    Contents = new() { new ContentSearchResult { Path = "/proj/deck.md" }, new ContentSearchResult { Path = "/proj/doc/guida.md" } },
                    Files = new() { new FileSearchResult { Path = "/proj/img/logo.png" } },
                    Links = new() { new LinkSearchResult { FullPath = "https://example.com/logo" } },
                }),
            };

            var sources = ProjectSearchDigest.Build(searches, Root, new[] { "/proj/deck.md" }, _ => "");

            CollectionAssert.AreEqual(new[] { "doc/guida.md" }, sources.Select(s => s.Path).ToList());
        }

        [TestMethod]
        public void Leave_out_the_skills_of_the_agentic_environments()
        {
            var searches = new List<(string, SearchResult)>
            {
                ("slide", Found("/proj/.claude/skills/mde-slide/SKILL.md", "/proj/.github/skills/x/SKILL.md", "/proj/.claude/notes.md", "/proj/doc.md")),
            };

            var sources = ProjectSearchDigest.Build(searches, Root, null, _ => "", new[] { ".claude/skills", ".github/skills/" });

            CollectionAssert.AreEquivalent(new[] { ".claude/notes.md", "doc.md" }, sources.Select(s => s.Path).ToList());
        }

        [TestMethod]
        public void Give_the_paragraphs_holding_the_keywords_with_their_line()
        {
            var text = "---\ntitle: breadcrumb\n---\n\n# Titolo\n\nNiente qui.\n\nIl breadcrumb riporta\nalla slide di partenza.\n\nAltro.\n\nUna slide sola.\n";

            var passages = ProjectSearchDigest.Passages(text, new[] { "breadcrumb", "slide" }, 1500);

            Assert.AreEqual(2, passages.Count, "the front matter is not a paragraph");
            Assert.AreEqual(9, passages[0].Line);
            Assert.AreEqual("Il breadcrumb riporta\nalla slide di partenza.", passages[0].Text);
            Assert.AreEqual(14, passages[1].Line);
        }

        [TestMethod]
        public void Leave_out_the_documents_the_point_links_to()
        {
            var searches = new List<(string, SearchResult)> { ("vendite", Found("/proj/sezioni/vendite.md", "/proj/doc.md")) };

            var sources = ProjectSearchDigest.Build(searches, Root, new[] { "/proj/deck.md", "sezioni/vendite.md" }, _ => "");

            CollectionAssert.AreEqual(new[] { "doc.md" }, sources.Select(s => s.Path).ToList());
        }

        [TestMethod]
        public void Match_whole_words_only()
        {
            var text = "Annota la domanda.\n\nI numeri dell'anno.\n";

            var passages = ProjectSearchDigest.Passages(text, new[] { "anno" }, 1500);

            Assert.AreEqual("I numeri dell'anno.", passages.Single().Text);
        }

        [TestMethod]
        public void Give_a_table_row_and_a_list_item_as_passages_of_their_own()
        {
            var text = "| # | Decisione |\n|---|---|\n| D1 | niente |\n| D2 | il breadcrumb in sessionStorage |\n\n- uno\n- il breadcrumb si azzera\n  dall'albero\n";

            var passages = ProjectSearchDigest.Passages(text, new[] { "breadcrumb" }, 1500);

            Assert.AreEqual(2, passages.Count);
            Assert.AreEqual((4, "| D2 | il breadcrumb in sessionStorage |"), (passages[0].Line, passages[0].Text));
            Assert.AreEqual((7, "- il breadcrumb si azzera\n  dall'albero"), (passages[1].Line, passages[1].Text));
        }

        [TestMethod]
        public void Keep_the_best_paragraphs_within_the_budget_in_file_order()
        {
            var text = "slide uno\n\n" + new string('x', 700) + " slide\n\nslide e breadcrumb\n";

            var passages = ProjectSearchDigest.Passages(text, new[] { "breadcrumb", "slide" }, 300);

            Assert.AreEqual(2, passages.Count);
            Assert.AreEqual("slide uno", passages[0].Text);
            Assert.AreEqual("slide e breadcrumb", passages[1].Text, "the paragraph with both keywords is kept first");
        }

        [TestMethod]
        public void Read_the_passages_of_each_source_from_its_file()
        {
            var searches = new List<(string, SearchResult)> { ("breadcrumb", Found("/proj/b.md")) };
            string asked = null;

            var sources = ProjectSearchDigest.Build(searches, Root, null, p => { asked = p; return "Il breadcrumb.\n"; });

            Assert.AreEqual("b.md", asked);
            Assert.AreEqual("Il breadcrumb.", sources[0].Passages.Single().Text);
        }
    }
}
