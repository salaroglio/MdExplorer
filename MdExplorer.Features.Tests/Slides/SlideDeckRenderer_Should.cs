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

        private static HtmlNode[] Sections(string page) => SlidePage.Sections(page);

        private static JsonObject Config(string page) => SlidePage.Config(page);

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
            Assert.AreEqual(true, config["postMessageEvents"].GetValue<bool>());

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
        public void Give_element_comments_in_ordered_lists_and_inside_raw_html()
        {
            var section = Sections(Render("1. Uno <!-- .element: class=\"fragment\" -->\n2. Due\n\n<div class=\"r-hstack\">\n<div>\n\n- Dentro <!-- .element: class=\"fragment\" -->\n\n</div>\n</div>\n"))[0];

            Assert.AreEqual("fragment", section.SelectSingleNode("ol/li").GetAttributeValue("class", null));
            Assert.AreEqual("fragment", section.SelectSingleNode(".//div[@class='r-hstack']//li").GetAttributeValue("class", null));
        }

        [TestMethod]
        public void Give_a_slide_comment_anywhere_in_the_slide_to_the_section()
        {
            var section = Sections(Render("## Titolo\n\nTesto\n\n<!-- .slide: data-background-color=\"#000\" data-transition=\"zoom\" -->\n"))[0];

            Assert.AreEqual("#000", section.GetAttributeValue("data-background-color", null));
            Assert.AreEqual("zoom", section.GetAttributeValue("data-transition", null));
        }

        [TestMethod]
        public void Read_notes_that_start_on_the_note_line_and_end_at_the_next_slide()
        {
            var sections = Sections(Render("## A\n\nNote: subito qui\nseconda riga\n\n--\n\n## B\n"))[0]
                .ChildNodes.Where(n => n.Name == "section").ToArray();

            StringAssert.Contains(sections[0].SelectSingleNode("aside").InnerText, "subito qui");
            Assert.IsNull(sections[1].SelectSingleNode("aside"));
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
        public void Refuse_mermaid_and_point_to_plantuml()
        {
            var ex = Assert.ThrowsException<SlideDeckException>(() => Render("## A\n\n```mermaid\ngraph TD; A-->B;\n```\n"));

            StringAssert.Contains(ex.Message, "PlantUML");
        }

        [TestMethod]
        public void Shrink_diagrams_to_three_quarters_of_the_slide_height()
        {
            var page = Render("# A\n");
            StringAssert.Contains(page, "svg[data-diagram-type] { max-width: 100%; max-height: 525px; width: auto; height: auto; }");
            // No !important: the zoom of the diagram scripts writes the size on the style, and must win.
            Assert.IsFalse(page.Contains("!important"));
            StringAssert.Contains(page, "svg[data-diagram-type][style*=\"max-width: none\"] { max-height: none; }");

            var tall = SlideDeckRenderer.Render(
                "---\ndocument_type: slides\nreveal:\n  config:\n    height: 1080\n---\n# A\n", Options());
            StringAssert.Contains(tall, "max-height: 810px;");
        }

        [TestMethod]
        public void Take_plantumls_pixel_size_out_of_the_diagram_style_and_keep_the_rest()
        {
            var svg = Sections(Render("<svg data-diagram-type=\"CLASS\" width=\"837px\" height=\"895px\" style=\"width:837px;height:895px;background:#FFFFFF;\"><rect style=\"stroke-width:1.5;\"/></svg>\n"))[0]
                .SelectSingleNode(".//svg");

            Assert.AreEqual("background:#FFFFFF;", svg.GetAttributeValue("style", null));
            Assert.AreEqual("837px", svg.GetAttributeValue("width", null));
            Assert.AreEqual("stroke-width:1.5;", svg.SelectSingleNode("rect").GetAttributeValue("style", null));
        }

        [TestMethod]
        public void Load_the_diagram_scripts_of_the_document_view_before_reveal_starts()
        {
            var page = Render("# A\n");

            foreach (var name in new[] { "interactive-svg", "interactive-svg-sequence", "interactive-svg-yaml-links", "interactive-svg-yaml" })
            {
                StringAssert.Contains(page, $"<link rel=\"stylesheet\" href=\"/javascripts/jqueryForFirstPage/interactive-svg/{name}.css\">");
                StringAssert.Contains(page, $"<script src=\"/javascripts/jqueryForFirstPage/interactive-svg/{name}.js\"></script>");
            }
            // The colour legend: the document toolbar's row styles, its own corner, the shared IT/EN texts.
            StringAssert.Contains(page, "<link rel=\"stylesheet\" href=\"/javascripts/jqueryForFirstPage/images/image-toolbar.css\">");
            StringAssert.Contains(page, "<link rel=\"stylesheet\" href=\"/javascripts/slides/slide-diagrams.css\">");
            Assert.IsTrue(page.IndexOf("/images/toolbar-shared.js") < page.IndexOf("/javascripts/slides/slide-diagrams.js"));
            // Links between decks and the breadcrumb (slide-navigation.js), also registered on ready.
            StringAssert.Contains(page, "<link rel=\"stylesheet\" href=\"/javascripts/slides/slide-navigation.css\">");
            Assert.IsTrue(page.IndexOf("/javascripts/slides/slide-navigation.js") < page.IndexOf("Reveal.initialize("));
            // Text correction: the document's inline-edit.js, started by slide-edit.js, in the content the page declares.
            StringAssert.Contains(page, "<div class=\"slides\" data-mde-content>");
            Assert.IsTrue(page.IndexOf("/inline-edit/inline-edit.js") < page.IndexOf("/javascripts/slides/slide-edit.js"));
            StringAssert.Contains(page, "/javascripts/jqueryForFirstPage/clipboard/clipboard-paste.css");
            // slide-diagrams.js registers on Reveal's ready event: it must come before Reveal.initialize.
            Assert.IsTrue(page.IndexOf("/javascripts/slides/slide-diagrams.js") < page.IndexOf("Reveal.initialize("));
            Assert.IsFalse(page.Contains("jquery-3"), "the diagram scripts do not need jQuery");
            // "Ask to MarkAgent" is left out of the slides (user's decision, 24/09/2026).
            Assert.IsFalse(page.Contains("mark-diagram-context"));
        }

        [TestMethod]
        public void Write_the_document_project_and_connection_on_the_body_as_a_document_page_does()
        {
            var page = SlideDeckRenderer.Render(FrontMatter + "# A\n", new SlideDeckRenderOptions
            {
                Pipeline = DocumentViewPipeline.Build(null),
                DocumentPath = @"C:\progetto\R&S\deck.md",
                ProjectPath = @"C:\progetto",
                ConnectionId = "abc",
            });

            StringAssert.Contains(page, "<body ConnectionId=\"abc\" DocumentPath=\"C:\\progetto\\R&amp;S\\deck.md\" ProjectPath=\"C:\\progetto\">");
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

        // ---- files the slides load by themselves ----

        private static HtmlNode[] SectionsWithQuery(string body)
            => SlidePage.Sections(SlideDeckRenderer.Render(FrontMatter + body, new SlideDeckRenderOptions
            {
                Pipeline = DocumentViewPipeline.Build(null),
                ResourceQuery = "connectionId=abc",
            }));

        [TestMethod]
        public void Let_images_written_in_html_reach_the_projects_files()
        {
            var images = SectionsWithQuery("<div class=\"r-stack\">\n<img src=\"assets/a.png\">\n<img src=\"b.svg?v=2#x\">\n</div>\n")[0].SelectNodes(".//img");

            Assert.AreEqual("assets/a.png?connectionId=abc", images[0].GetAttributeValue("src", null));
            Assert.AreEqual("b.svg?v=2&connectionId=abc#x", images[1].GetAttributeValue("src", null));
        }

        [TestMethod]
        public void Let_slide_backgrounds_reach_the_projects_files()
        {
            var sections = SectionsWithQuery(
                "<!-- .slide: data-background-image=\"img/fondo.jpg\" -->\n## A\n\n---\n\n<!-- .slide: data-background-video=\"v.mp4, v.webm\" -->\n## B\n");

            Assert.AreEqual("img/fondo.jpg?connectionId=abc", sections[0].GetAttributeValue("data-background-image", null));
            Assert.AreEqual("v.mp4?connectionId=abc,v.webm?connectionId=abc", sections[1].GetAttributeValue("data-background-video", null));
        }

        [TestMethod]
        public void Leave_urls_that_are_not_relative_paths_as_written()
        {
            var section = SectionsWithQuery(
                "<img src=\"https://example.com/a.png\">\n<img src=\"/api/mdexplorer/b.png?connectionId=x\">\n<img src=\"data:image/png;base64,AA==\">\n<a href=\"#sopra\">su</a> <a href=\"mailto:a@b.it\">m</a>\n")[0];

            CollectionAssert.AreEqual(
                new[] { "https://example.com/a.png", "/api/mdexplorer/b.png?connectionId=x", "data:image/png;base64,AA==" },
                section.SelectNodes(".//img").Select(i => i.GetAttributeValue("src", null)).ToArray());
            CollectionAssert.AreEqual(new[] { "#sopra", "mailto:a@b.it" },
                section.SelectNodes(".//a").Select(a => a.GetAttributeValue("href", null)).ToArray());
        }

        [TestMethod]
        public void Leave_urls_as_written_without_a_page_query()
        {
            var image = Sections(Render("<img src=\"assets/a.png\">\n"))[0].SelectSingleNode(".//img");

            Assert.AreEqual("assets/a.png", image.GetAttributeValue("src", null));
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
            // How the viewer and the PDF export know the page is a deck.
            StringAssert.Contains(page, "<meta name=\"mdexplorer-view\" content=\"slides\">");
        }

        [TestMethod]
        public void Offer_exactly_the_themes_that_ship_in_wwwroot()
        {
            var wwwroot = Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "wwwroot");

            var themes = Directory.GetFiles(Path.Combine(wwwroot, "reveal", "dist", "theme"), "*.css")
                .Select(Path.GetFileNameWithoutExtension).OrderBy(t => t).ToArray();
            var highlight = Directory.GetFiles(Path.Combine(wwwroot, "reveal", "dist", "plugin", "highlight"), "*.css")
                .Select(Path.GetFileNameWithoutExtension).OrderBy(t => t).ToArray();

            CollectionAssert.AreEqual(themes, SlideDeckFrontMatter.Themes.OrderBy(t => t).ToArray());
            CollectionAssert.AreEqual(highlight, SlideDeckFrontMatter.HighlightThemes.OrderBy(t => t).ToArray());
        }
    }
}
