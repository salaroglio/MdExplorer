using HtmlAgilityPack;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace MdExplorer.Features.Tests.Slides
{
    [TestClass]
    public class SlideDeckRenderer_Should
    {
        private const string FrontMatter = "---\ntitle: Prova\ndocument_type: slides\n---\n";

        private static string Render(string body, bool dark = false, Func<string, string> before = null, Func<string, string> after = null)
            => SlideDeckRenderer.Render(FrontMatter + body, Options(dark, before, after));

        private static SlideDeckRenderOptions Options(bool dark = false, Func<string, string> before = null, Func<string, string> after = null)
            => new SlideDeckRenderOptions
            {
                Pipeline = DocumentViewPipeline.Build(null),
                DarkTheme = dark,
                BeforeMarkdown = before ?? (s => s),
                AfterMarkdown = after ?? (s => s),
            };

        private static HtmlNode Slides(string page)
        {
            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(page);
            return document.DocumentNode.SelectSingleNode("//div[@class='slides']");
        }

        private static HtmlNode[] Sections(string page)
            => Slides(page).ChildNodes.Where(n => n.Name == "section").ToArray();

        private static JsonObject Config(string page)
        {
            const string start = "Reveal.initialize(Object.assign(";
            var from = page.IndexOf(start, StringComparison.Ordinal) + start.Length;
            var to = page.IndexOf(", { plugins:", from, StringComparison.Ordinal);
            return (JsonObject)JsonNode.Parse(page.Substring(from, to - from));
        }

        // ---- slides ----

        [TestMethod]
        public void Cut_the_deck_into_slides_on_a_dashes_line()
        {
            var sections = Sections(Render("# Uno\n\n---\n\n## Due\n\n---\n\n## Tre\n"));

            Assert.AreEqual(3, sections.Length);
            Assert.AreEqual("Due", sections[1].SelectSingleNode("h2").InnerText);
        }

        [TestMethod]
        public void Stack_vertical_slides_on_a_double_dash_line()
        {
            var sections = Sections(Render("# Uno\n\n---\n\n## Due\n\n--\n\n## Due bis\n"));

            Assert.AreEqual(2, sections.Length);
            var stack = sections[1].ChildNodes.Where(n => n.Name == "section").ToArray();
            Assert.AreEqual(2, stack.Length);
            Assert.AreEqual("Due bis", stack[1].SelectSingleNode("h2").InnerText);
        }

        [TestMethod]
        public void Keep_a_dashes_line_inside_code_as_code()
        {
            var sections = Sections(Render("# Uno\n\n```yaml\nkey: a\n---\nkey: b\n```\n"));

            Assert.AreEqual(1, sections.Length);
            StringAssert.Contains(sections[0].SelectSingleNode(".//code").InnerText, "---");
        }

        [TestMethod]
        public void Separate_on_a_dashes_line_right_under_text_as_reveal_does()
        {
            // In a document this would be a setext heading; in a deck it is a separator.
            var sections = Sections(Render("Titolo\n---\nSeconda\n"));

            Assert.AreEqual(2, sections.Length);
            Assert.IsNull(sections[0].SelectSingleNode("h2"));
        }

        [TestMethod]
        public void Turn_what_follows_Note_into_speaker_notes()
        {
            var section = Sections(Render("## Risultati\n\nTesto\n\nNote:\nSoffermarsi sul **fatturato**.\n"))[0];

            var notes = section.SelectSingleNode("aside[@class='notes']");
            Assert.IsNotNull(notes);
            Assert.AreEqual("fatturato", notes.SelectSingleNode(".//strong").InnerText);
            Assert.IsFalse(section.InnerText.Replace(notes.InnerText, "").Contains("Soffermarsi"));
        }

        // ---- front matter ----

        [TestMethod]
        public void Follow_MdExplorers_theme_when_the_deck_names_none()
        {
            StringAssert.Contains(Render("# A\n", dark: true), "/reveal/dist/theme/black.css");
            StringAssert.Contains(Render("# A\n", dark: false), "/reveal/dist/theme/white.css");
        }

        [TestMethod]
        public void Use_the_theme_the_deck_names()
        {
            var page = SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  theme: dracula\n  highlight_theme: zenburn\n---\n# A\n", Options(dark: true));

            StringAssert.Contains(page, "/reveal/dist/theme/dracula.css");
            StringAssert.Contains(page, "/reveal/dist/plugin/highlight/zenburn.css");
        }

        [TestMethod]
        public void Refuse_a_theme_that_does_not_exist_and_list_the_valid_ones()
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  theme: rosso\n---\n# A\n", Options()));

            StringAssert.Contains(ex.Message, "rosso");
            StringAssert.Contains(ex.Message, "white-contrast");
        }

        [TestMethod]
        public void Refuse_a_reveal_option_written_outside_config_and_say_where_it_goes()
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  transition: fade\n---\n# A\n", Options()));

            StringAssert.Contains(ex.Message, "reveal.config.transition");
        }

        [TestMethod]
        public void Refuse_invalid_yaml_with_the_line_of_the_file()
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config: [unclosed\n---\n# A\n", Options()));

            StringAssert.Contains(ex.Message, "line");
        }

        [TestMethod]
        public void Refuse_a_deck_without_front_matter()
        {
            Assert.ThrowsException<SlideDeckException>(() => SlideDeckRenderer.Render("# A\n", Options()));
        }

        [TestMethod]
        public void Pass_the_config_to_reveal_with_yaml_types()
        {
            var page = SlideDeckRenderer.Render(@"---
document_type: slides
reveal:
  config:
    controls: false
    progress: 'false'
    slideNumber: c/t
    width: 1280
    margin: 0.04
    autoSlide: ~
    keyboard:
      '13': next
    dependencies: [a, b]
---
# A
", Options());

            var config = Config(page);
            Assert.AreEqual(false, config["controls"].GetValue<bool>());
            Assert.AreEqual("false", config["progress"].GetValue<string>());
            Assert.AreEqual("c/t", config["slideNumber"].GetValue<string>());
            Assert.AreEqual(1280L, config["width"].GetValue<long>());
            Assert.AreEqual(0.04, config["margin"].GetValue<double>());
            Assert.IsTrue(config.ContainsKey("autoSlide"));
            Assert.IsNull(config["autoSlide"]);
            Assert.AreEqual("next", config["keyboard"]["13"].GetValue<string>());
            Assert.AreEqual(2, config["dependencies"].AsArray().Count);
        }

        // ---- configuration ----

        [TestMethod]
        public void Start_from_MdExplorers_defaults_and_let_the_deck_override_them()
        {
            var config = Config(Render("# A\n"));
            Assert.AreEqual(true, config["hash"].GetValue<bool>());
            Assert.IsTrue(config.ContainsKey("scrollActivationWidth"));
            Assert.IsNull(config["scrollActivationWidth"]);

            var overridden = Config(SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config:\n    hash: false\n    scrollActivationWidth: 435\n---\n# A\n", Options()));
            Assert.AreEqual(false, overridden["hash"].GetValue<bool>());
            Assert.AreEqual(435L, overridden["scrollActivationWidth"].GetValue<long>());
        }

        [TestMethod]
        public void Always_load_KaTeX_from_MdExplorer_and_keep_the_decks_katex_options()
        {
            var config = Config(SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config:\n    katex:\n      extensions: [mhchem]\n---\n# A\n", Options()));

            Assert.AreEqual("/katex", config["katex"]["local"].GetValue<string>());
            Assert.AreEqual("mhchem", config["katex"]["extensions"][0].GetValue<string>());
            Assert.AreEqual(2, config["katex"]["delimiters"].AsArray().Count);
        }

        [TestMethod]
        public void Refuse_settings_MdExplorer_owns()
        {
            Assert.ThrowsException<SlideDeckException>(() => SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config:\n    plugins: [RevealZoom]\n---\n# A\n", Options()));
            Assert.ThrowsException<SlideDeckException>(() => SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config:\n    katex:\n      local: https://cdn.example\n---\n# A\n", Options()));
        }

        [TestMethod]
        public void Not_let_a_config_value_close_the_script()
        {
            var page = SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config:\n    title: '</script><script>alert(1)</script>'\n---\n# A\n", Options());

            Assert.IsFalse(page.Contains("</script><script>alert(1)"));
        }

        // ---- attribute comments (reveal.js's markdown plugin rules) ----

        [TestMethod]
        public void Give_an_element_comment_at_the_end_of_a_list_item_to_the_item()
        {
            var items = Sections(Render("- Uno <!-- .element: class=\"fragment\" -->\n- Due\n"))[0].SelectNodes(".//li");

            Assert.AreEqual("fragment", items[0].GetAttributeValue("class", null));
            Assert.IsNull(items[1].GetAttributeValue("class", null));
        }

        [TestMethod]
        public void Give_an_element_comment_under_a_list_to_the_last_item_without_a_blank_line()
        {
            var section = Sections(Render("- Uno\n- Due\n<!-- .element: class=\"fragment\" -->\n"))[0];

            Assert.AreEqual("fragment", section.SelectNodes(".//li")[1].GetAttributeValue("class", null));
            Assert.IsNull(section.SelectSingleNode(".//ul").GetAttributeValue("class", null));
        }

        [TestMethod]
        public void Give_an_element_comment_under_a_list_to_the_list_after_a_blank_line()
        {
            var section = Sections(Render("- Uno\n- Due\n\n<!-- .element: class=\"fragment\" -->\n"))[0];

            Assert.AreEqual("fragment", section.SelectSingleNode(".//ul").GetAttributeValue("class", null));
            Assert.IsNull(section.SelectNodes(".//li")[1].GetAttributeValue("class", null));
        }

        [TestMethod]
        public void Give_an_element_comment_in_a_paragraph_to_the_paragraph()
        {
            var p = Sections(Render("Testo <!-- .element: class=\"fragment fade-up\" data-fragment-index=\"2\" -->\n"))[0].SelectSingleNode("p");

            Assert.AreEqual("fragment fade-up", p.GetAttributeValue("class", null));
            Assert.AreEqual("2", p.GetAttributeValue("data-fragment-index", null));
        }

        [TestMethod]
        public void Give_a_slide_comment_to_the_section()
        {
            var section = Sections(Render("<!-- .slide: data-background-color=\"#1b2a3a\" data-auto-animate -->\n\n## Titolo\n"))[0];

            Assert.AreEqual("#1b2a3a", section.GetAttributeValue("data-background-color", null));
            Assert.IsTrue(section.Attributes.Contains("data-auto-animate"));
        }

        [TestMethod]
        public void Give_a_slide_comment_to_its_own_slide_in_a_vertical_stack()
        {
            var stack = Sections(Render("## A\n\n--\n\n<!-- .slide: data-transition=\"zoom\" -->\n## B\n"))[0];
            var slides = stack.ChildNodes.Where(n => n.Name == "section").ToArray();

            Assert.IsNull(stack.GetAttributeValue("data-transition", null));
            Assert.IsNull(slides[0].GetAttributeValue("data-transition", null));
            Assert.AreEqual("zoom", slides[1].GetAttributeValue("data-transition", null));
        }

        [TestMethod]
        public void Leave_the_html_of_a_slide_without_comments_as_markdig_wrote_it()
        {
            var page = Render("<svg viewBox=\"0 0 10 10\"><rect width=\"4\" height=\"4\"/></svg>\n");

            StringAssert.Contains(page, "viewBox=\"0 0 10 10\"");
        }

        [TestMethod]
        public void Keep_the_case_of_svg_attributes_when_comments_are_applied()
        {
            var page = Render("<svg viewBox=\"0 0 10 10\"><rect width=\"4\" height=\"4\"/></svg>\n\nTesto <!-- .element: class=\"fragment\" -->\n");

            StringAssert.Contains(page, "viewBox=\"0 0 10 10\"");
        }

        // ---- code, math, escaping, raw html ----

        [TestMethod]
        public void Step_through_code_lines_as_reveal_does()
        {
            var code = Sections(Render("```js [1-2|3]\nlet a = 1;\nlet b = 2;\nlog(a + b);\n```\n"))[0].SelectSingleNode(".//code");

            Assert.AreEqual("1-2|3", code.GetAttributeValue("data-line-numbers", null));
            StringAssert.Contains(code.GetAttributeValue("class", ""), "language-js");
        }

        [TestMethod]
        public void Start_the_line_numbers_where_the_fence_says()
        {
            var code = Sections(Render("```js [5: 1-2]\nlet a = 1;\n```\n"))[0].SelectSingleNode(".//code");

            Assert.AreEqual("1-2", code.GetAttributeValue("data-line-numbers", null));
            Assert.AreEqual("5", code.GetAttributeValue("data-ln-start-from", null));
        }

        [TestMethod]
        public void Write_math_with_the_delimiters_KaTeX_is_told_to_read()
        {
            var page = Render("Area $\\pi r^2$\n\n$$\n\\sum_i i\n$$\n");

            StringAssert.Contains(page, "\\(\\pi r^2\\)");
            StringAssert.Contains(page, "\\[");
        }

        [TestMethod]
        public void Render_characters_that_broke_the_old_xml_page()
        {
            var section = Sections(Render("## R&S\n\nriga<br>due & < tre\n"))[0];

            Assert.AreEqual("R&amp;S", section.SelectSingleNode("h2").InnerHtml);
        }

        [TestMethod]
        public void Let_raw_html_through_for_what_markdown_cannot_say()
        {
            var section = Sections(Render("<div class=\"r-stack\">\n<img class=\"fragment\" src=\"a.png\">\n</div>\n"))[0];

            Assert.IsNotNull(section.SelectSingleNode("div[@class='r-stack']/img[@class='fragment']"));
        }

        // ---- MdExplorer's commands ----

        [TestMethod]
        public void Run_the_markdown_commands_once_on_the_whole_body_before_cutting_it()
        {
            var calls = 0;
            // A command that includes text may bring separators with it.
            var sections = Sections(Render("# A\n\n@@include@@\n", before: body => { calls++; return body.Replace("@@include@@", "---\n\n## B"); }));

            Assert.AreEqual(1, calls);
            Assert.AreEqual(2, sections.Length);
        }

        [TestMethod]
        public void Run_the_html_commands_on_each_slide_and_on_the_notes()
        {
            var calls = 0;
            Render("# A\n\n---\n\n## B\n\nNote:\nnota\n", after: html => { calls++; return html; });

            Assert.AreEqual(3, calls);
        }

        // ---- the page ----

        [TestMethod]
        public void Load_reveal_and_its_plugins_from_the_service()
        {
            var page = Render("# A\n");

            foreach (var script in new[] { "/reveal/dist/reveal.js", "/reveal/dist/plugin/highlight.js", "/reveal/dist/plugin/notes.js",
                                           "/reveal/dist/plugin/math.js", "/reveal/dist/plugin/search.js", "/reveal/dist/plugin/zoom.js" })
            {
                StringAssert.Contains(page, $"src=\"{script}\"");
            }
            StringAssert.Contains(page, "RevealMath.KaTeX");
            StringAssert.Contains(page, "<title>Prova</title>");
        }

        [TestMethod]
        public void Offer_exactly_the_themes_that_ship_in_wwwroot()
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "MdExplorer.sln")))
            {
                root = root.Parent;
            }
            Assert.IsNotNull(root, "repository root not found");
            var wwwroot = Path.Combine(root.FullName, "MdExplorer", "wwwroot");

            var themes = Directory.GetFiles(Path.Combine(wwwroot, "reveal", "dist", "theme"), "*.css")
                .Select(Path.GetFileNameWithoutExtension).OrderBy(t => t).ToArray();
            var highlight = Directory.GetFiles(Path.Combine(wwwroot, "reveal", "dist", "plugin", "highlight"), "*.css")
                .Select(Path.GetFileNameWithoutExtension).OrderBy(t => t).ToArray();

            CollectionAssert.AreEqual(themes, SlideDeckFrontMatter.Themes.OrderBy(t => t).ToArray());
            CollectionAssert.AreEqual(highlight, SlideDeckFrontMatter.HighlightThemes.OrderBy(t => t).ToArray());
        }
    }
}
