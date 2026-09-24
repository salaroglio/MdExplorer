using HtmlAgilityPack;
using Markdig;
using Markdig.Syntax;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>
    /// Every example of the mde-slide skill is rendered by the slide engine and checked against
    /// what its recipe promises. The skill is what an AI writes from: a syntax it teaches and the
    /// engine does not translate would be a presentation that silently comes out wrong.
    /// An example is a <c>````markdown esempio=&lt;id&gt;</c> block of <c>MdExplorer/skills/mde-slide/SKILL.md</c>;
    /// an example without a check here, or a check without an example, fails.
    /// </summary>
    [TestClass]
    public class MdeSlideSkill_Should
    {
        private const string FrontMatter = "---\ntitle: Esempio\ndocument_type: slides\n---\n";

        private static readonly Dictionary<string, Action<string>> Checks = new()
        {
            ["scheletro"] = page =>
            {
                var sections = SlidePage.Sections(page);
                Assert.AreEqual(3, sections.Length);
                Assert.IsNotNull(sections[0].SelectSingleNode("h1"));
            },
            ["frammenti"] = page =>
            {
                var items = SlidePage.Sections(page)[0].SelectNodes(".//li");
                Assert.AreEqual(3, items.Count);
                Assert.IsTrue(items.All(li => li.GetAttributeValue("class", null) == "fragment"));
            },
            ["frammenti-ordine"] = page =>
            {
                var paragraphs = SlidePage.Sections(page)[0].SelectNodes("p");
                CollectionAssert.AreEqual(new[] { "2", "1", "3" },
                    paragraphs.Select(p => p.GetAttributeValue("data-fragment-index", null)).ToArray());
                Assert.AreEqual("fragment fade-up", paragraphs[0].GetAttributeValue("class", null));
                Assert.AreEqual("fragment highlight-red", paragraphs[2].GetAttributeValue("class", null));
            },
            ["sfondo"] = page =>
            {
                var sections = SlidePage.Sections(page);
                Assert.AreEqual("#1b2a3a", sections[0].GetAttributeValue("data-background-color", null));
                Assert.AreEqual("img/cantiere.jpg", sections[1].GetAttributeValue("data-background-image", null));
                Assert.AreEqual("0.4", sections[1].GetAttributeValue("data-background-opacity", null));
            },
            ["transizione"] = page =>
                Assert.AreEqual("zoom", SlidePage.Sections(page)[0].GetAttributeValue("data-transition", null)),
            ["verticali"] = page =>
            {
                var sections = SlidePage.Sections(page);
                Assert.AreEqual(3, sections.Length);
                var stack = sections[1].ChildNodes.Where(n => n.Name == "section").ToArray();
                Assert.AreEqual(3, stack.Length);
                Assert.AreEqual("Italia: i rischi", stack[2].SelectSingleNode("h2").InnerText);
            },
            ["note"] = page =>
            {
                var section = SlidePage.Sections(page)[0];
                var notes = section.SelectSingleNode("aside[@class='notes']");
                StringAssert.Contains(notes.InnerText, "provvisorio");
                Assert.IsFalse(section.SelectSingleNode("ul").InnerText.Contains("provvisorio"));
            },
            ["codice"] = page =>
            {
                var code = SlidePage.Sections(page)[0].SelectSingleNode(".//code");
                Assert.AreEqual("1-2|4|5-7", code.GetAttributeValue("data-line-numbers", null));
                StringAssert.Contains(code.GetAttributeValue("class", ""), "language-js");
            },
            // The engine alone keeps the block: MdExplorer's PlantUML command, which the viewer
            // runs on slides, turns it into an <svg> (tried in the app, sprint F3).
            ["diagramma"] = page =>
                Assert.IsNotNull(SlidePage.Sections(page)[0].SelectSingleNode(".//code[contains(@class,'language-plantuml')]")),
            ["formula"] = page =>
            {
                var section = SlidePage.Sections(page)[0];
                Assert.IsNotNull(section.SelectSingleNode(".//span[@class='math']"));
                StringAssert.Contains(section.SelectSingleNode(".//div[@class='math']").InnerText, "C_n");
            },
            ["auto-animate"] = page =>
            {
                var sections = SlidePage.Sections(page);
                Assert.AreEqual(2, sections.Length);
                foreach (var section in sections)
                {
                    Assert.IsTrue(section.Attributes.Contains("data-auto-animate"));
                    Assert.IsNotNull(section.SelectSingleNode(".//div[@data-id='barra']"));
                }
            },
            ["pila"] = page =>
            {
                var images = SlidePage.Sections(page)[0].SelectNodes(".//div[@class='r-stack']/img");
                Assert.AreEqual(3, images.Count);
                Assert.AreEqual("fragment current-visible", images[1].GetAttributeValue("class", null));
            },
            ["colonne"] = page =>
            {
                // The markdown inside the <div>s is markdown, not text.
                var lists = SlidePage.Sections(page)[0].SelectNodes(".//div[@class='r-hstack']/div/ul");
                Assert.AreEqual(2, lists.Count);
            },
            ["config"] = page =>
            {
                var config = SlidePage.Config(page);
                Assert.AreEqual("fade", config["transition"].GetValue<string>());
                Assert.AreEqual("c/t", config["slideNumber"].GetValue<string>());
                Assert.AreEqual(false, config["controls"].GetValue<bool>());
                Assert.AreEqual(8000L, config["autoSlide"].GetValue<long>());
                Assert.AreEqual(true, config["loop"].GetValue<bool>());
                Assert.AreEqual(false, config["pdfSeparateFragments"].GetValue<bool>());
            },
            ["tema"] = page =>
            {
                StringAssert.Contains(page, "/reveal/dist/theme/dracula.css");
                StringAssert.Contains(page, "/reveal/dist/plugin/highlight/zenburn.css");
            },
        };

        /// <summary>Examples of what MdExplorer refuses, with a word of the message the skill quotes.</summary>
        private static readonly Dictionary<string, string> Refusals = new()
        {
            ["errore-opzione-fuori-posto"] = "Unknown key 'reveal.transition'",
            ["errore-mermaid"] = "Mermaid diagrams are not shown in slides",
        };

        private static Dictionary<string, string> Examples()
        {
            var path = Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "skills", "mde-slide", "SKILL.md");
            var document = Markdown.Parse(File.ReadAllText(path), new MarkdownPipelineBuilder().UseYamlFrontMatter().Build());
            return document.Descendants<FencedCodeBlock>()
                .Where(b => b.Info == "markdown" && (b.Arguments ?? "").StartsWith("esempio="))
                .ToDictionary(b => b.Arguments.Substring("esempio=".Length).Trim(), b => b.Lines.ToString());
        }

        private static string Render(string example)
        {
            // An example that shows the front matter is a whole file; the others are slides.
            var markdown = example.StartsWith("---") ? example : FrontMatter + "\n" + example;
            return SlideDeckRenderer.Render(markdown + "\n", new SlideDeckRenderOptions { Pipeline = DocumentViewPipeline.Build(null) });
        }

        [TestMethod]
        public void Check_every_example_it_teaches_and_nothing_else()
        {
            var inSkill = Examples().Keys.OrderBy(k => k).ToArray();
            var checkedHere = Checks.Keys.Concat(Refusals.Keys).OrderBy(k => k).ToArray();

            CollectionAssert.AreEqual(checkedHere, inSkill,
                $"skill: {string.Join(", ", inSkill)} — tests: {string.Join(", ", checkedHere)}");
        }

        [TestMethod]
        public void Render_each_example_as_its_recipe_promises()
        {
            foreach (var (id, example) in Examples().Where(e => Checks.ContainsKey(e.Key)))
            {
                try
                {
                    Checks[id](Render(example));
                }
                catch (Exception ex)
                {
                    throw new AssertFailedException($"example '{id}': {ex.Message}", ex);
                }
            }
        }

        [TestMethod]
        public void Refuse_each_wrong_example_with_the_message_the_skill_quotes()
        {
            foreach (var (id, example) in Examples().Where(e => Refusals.ContainsKey(e.Key)))
            {
                var ex = Assert.ThrowsException<SlideDeckException>(() => Render(example), $"example '{id}'");
                StringAssert.Contains(ex.Message, Refusals[id], $"example '{id}'");
            }
        }
    }
}
