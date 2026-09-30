using HtmlAgilityPack;
using MdExplorer.Features.StaticSite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Tests.StaticSite
{
    /// <summary>
    /// The static HTML export on a made-up project. The renderer is a stand-in: a markdown file's text IS
    /// the page MdExplorer would render (the addresses written as the service writes them), and "DECK" in
    /// it makes it a deck. So each test says exactly which addresses the page carries.
    /// </summary>
    [TestClass]
    public class StaticSiteExporter_Should
    {
        private string _root;
        private string _project;
        private string _web;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-static-site-" + Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _web = Path.Combine(_root, "wwwroot");
            Web("reveal/dist/reveal.js", "// reveal");
            Web("reveal/dist/theme/white.css", "/* see url(%22nowhere.png%22) */ @import url(./fonts/source.css); .reveal { background: url(\"img/bg.png\"); }");
            Web("reveal/dist/theme/fonts/source.css", "@font-face { src: url(source.woff); }");
            Web("reveal/dist/theme/fonts/source.woff", "font");
            Web("reveal/dist/theme/img/bg.png", "png");
            Web("javascripts/slides/slide-navigation.js", "// nav");
            Web("javascripts/jqueryForFirstPage/images/image-toolbar.css", "/* toolbar */");
            Web("javascripts/jqueryForFirstPage/images/toolbar-shared.js", "// texts");
            Web("javascripts/slides/slide-toolbar.css", "");
            Web("javascripts/slides/slide-ink.css", "");
            Web("javascripts/slides/slide-navigation.css", "");
            Web("javascripts/slides/html-page-trail.js", "");
            Web("javascripts/slides/slide-toolbar.js", "");
            Web("javascripts/slides/slide-ink.js", "");
            Web("katex/dist/katex.min.js", "// katex");
            Web("katex/dist/fonts/KaTeX_Main.woff2", "font");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }

        private void Web(string path, string content) => Write(_web, path, content);

        private void Project(string path, string content) => Write(_project, path, content);

        private static void Write(string root, string path, string content)
        {
            var file = Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllText(file, content);
        }

        private static string Page(string body, bool deck = false)
            => $"<!DOCTYPE html><html><head><title>T</title></head><body DocumentPath=\"x\" ConnectionId=\"abc\">{(deck ? "<!-- DECK -->" : "")}{body}</body></html>";

        private StaticSiteExport Export(string entry)
            => StaticSiteExporter.Export(new StaticSiteRequest
            {
                ProjectRoot = _project,
                WebRoot = _web,
                Entry = entry,
                Render = (path, katex) =>
                {
                    var text = File.ReadAllText(Path.Combine(_project, path.Replace('/', Path.DirectorySeparatorChar)));
                    if (text.Contains("FAIL")) throw new InvalidOperationException("broken on purpose");
                    return new RenderedMarkdown { Html = text.Replace("{katex}", katex), IsDeck = text.Contains("DECK") };
                },
            });

        private static string Text(StaticSiteExport export, string path)
        {
            Assert.IsTrue(export.Files.ContainsKey(path), $"'{path}' is not in the export: {string.Join(", ", export.Files.Keys)}");
            return Encoding.UTF8.GetString(export.Files[path]);
        }

        private static HtmlNode Node(StaticSiteExport export, string path, string xpath)
        {
            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(Text(export, path));
            var node = document.DocumentNode.SelectSingleNode(xpath);
            Assert.IsNotNull(node, $"{xpath} not in {path}");
            return node;
        }

        // ---- decks, sub-decks, documents ----

        [TestMethod]
        public void Turn_a_link_to_another_deck_into_its_page_keeping_the_pages_and_the_slide()
        {
            Project("vendite/master.md", Page("<a href=\"/api/mdexplorer/vendite/costi.md?connectionId=abc&pages=2,6-9#/1\">Costi</a>", deck: true));
            Project("vendite/costi.md", Page("costi", deck: true));

            var export = Export("vendite/master.md");

            var link = Node(export, "vendite/master.html", "//a");
            Assert.AreEqual("costi.html?pages=2%2C6-9#/1", link.GetAttributeValue("href", null));
            Assert.AreEqual("markdown", link.GetAttributeValue("data-mde-export-kind", null));
            Assert.IsTrue(export.Files.ContainsKey("vendite/costi.html"));
        }

        [TestMethod]
        public void Follow_links_recursively_to_documents_and_pages_once_each_even_in_a_cycle()
        {
            Project("master.md", Page("<a href=\"docs/analisi.md?connectionId=abc\">A</a>", deck: true));
            Project("docs/analisi.md", Page("<a href=\"../master.md?connectionId=abc\">back</a><a href=\"altro?connectionId=abc\">no extension</a>"));
            Project("docs/altro.md", Page("<a href=\"/api/mdexplorer/docs/analisi.md?connectionId=abc\">cycle</a>"));

            var export = Export("master.md");

            Assert.AreEqual("../master.html", Node(export, "docs/analisi.html", "//a[1]").GetAttributeValue("href", null));
            Assert.AreEqual("altro.html", Node(export, "docs/analisi.html", "//a[2]").GetAttributeValue("href", null));
            Assert.AreEqual("analisi.html", Node(export, "docs/altro.html", "//a").GetAttributeValue("href", null));
            Assert.AreEqual(3, export.Report.Pages);
        }

        [TestMethod]
        public void Name_the_page_x_md_html_when_the_project_already_has_an_x_html()
        {
            Project("master.md", Page("<a href=\"analisi.md?connectionId=abc\">A</a><a href=\"analisi.html?connectionId=abc\">H</a>", deck: true));
            Project("analisi.md", Page("markdown"));
            Project("analisi.html", "<html><body>page</body></html>");

            var export = Export("master.md");

            Assert.AreEqual("analisi.md.html", Node(export, "master.html", "//a[1]").GetAttributeValue("href", null));
            Assert.AreEqual("analisi.html", Node(export, "master.html", "//a[2]").GetAttributeValue("href", null));
            StringAssert.Contains(Text(export, "analisi.md.html"), "markdown");
            StringAssert.Contains(Text(export, "analisi.html"), "page");
        }

        [TestMethod]
        public void Report_a_document_that_cannot_be_rendered_and_go_on()
        {
            Project("master.md", Page("<a href=\"rotto.md?connectionId=abc\">R</a><a href=\"ok.md?connectionId=abc\">O</a>", deck: true));
            Project("rotto.md", "FAIL");
            Project("ok.md", Page("ok"));

            var export = Export("master.md");

            Assert.IsTrue(export.Files.ContainsKey("ok.html"));
            Assert.IsFalse(export.Files.ContainsKey("rotto.html"));
            Assert.IsTrue(export.Report.Issues.Any(i => i.Kind == StaticSiteIssueKind.RenderFailed && i.Address == "rotto.md"));
        }

        // ---- files the pages load ----

        [TestMethod]
        public void Copy_images_backgrounds_and_videos_next_to_the_page()
        {
            Project("vendite/master.md", Page(
                "<img src=\"/api/mdexplorer/vendite/img/grafico.png?connectionId=abc\">" +
                "<section data-background-image=\"img/sfondo.jpg\" data-background-video=\"v/a.mp4, v/a.webm\"></section>" +
                "<div style=\"background: url('img/sfondo.jpg')\"></div>", deck: true));
            Project("vendite/img/grafico.png", "png");
            Project("vendite/img/sfondo.jpg", "jpg");
            Project("vendite/v/a.mp4", "mp4");
            Project("vendite/v/a.webm", "webm");

            var export = Export("vendite/master.md");

            Assert.AreEqual("img/grafico.png", Node(export, "vendite/master.html", "//img").GetAttributeValue("src", null));
            Assert.AreEqual("v/a.mp4,v/a.webm", Node(export, "vendite/master.html", "//section").GetAttributeValue("data-background-video", null));
            foreach (var file in new[] { "vendite/img/grafico.png", "vendite/img/sfondo.jpg", "vendite/v/a.mp4", "vendite/v/a.webm" })
            {
                Assert.IsTrue(export.Files.ContainsKey(file), file);
            }
        }

        [TestMethod]
        public void Copy_MdExplorers_files_under_mde_with_what_their_styles_load()
        {
            Project("a/b/master.md", Page("<link rel=\"stylesheet\" href=\"/reveal/dist/theme/white.css\"><script src=\"/reveal/dist/reveal.js?v=123\"></script>", deck: true));

            var export = Export("a/b/master.md");

            Assert.AreEqual("../../_mde/reveal/dist/theme/white.css", Node(export, "a/b/master.html", "//link").GetAttributeValue("href", null));
            Assert.AreEqual("../../_mde/reveal/dist/reveal.js", Node(export, "a/b/master.html", "//script").GetAttributeValue("src", null));
            StringAssert.Contains(Text(export, "_mde/reveal/dist/theme/white.css"), "url(\"img/bg.png\")");
            Assert.IsTrue(export.Files.ContainsKey("_mde/reveal/dist/theme/img/bg.png"));
            Assert.IsTrue(export.Files.ContainsKey("_mde/reveal/dist/theme/fonts/source.woff"));
            Assert.AreEqual(0, export.Report.Issues.Count, "An address in a comment of a style sheet is not read.");
        }

        [TestMethod]
        public void Copy_KaTeX_for_a_deck_and_tell_the_deck_where_it_is()
        {
            Project("a/master.md", Page("<script>var katex = '{katex}';</script>", deck: true));

            var export = Export("a/master.md");

            StringAssert.Contains(Text(export, "a/master.html"), "var katex = '../_mde/katex';");
            Assert.IsTrue(export.Files.ContainsKey("_mde/katex/dist/katex.min.js"));
            Assert.IsTrue(export.Files.ContainsKey("_mde/katex/dist/fonts/KaTeX_Main.woff2"));
        }

        // ---- HTML pages of the project ----

        [TestMethod]
        public void Copy_an_HTML_page_with_what_it_loads_and_add_the_bar_and_the_breadcrumb()
        {
            Project("master.md", Page("<a href=\"pagine/demo.html?connectionId=abc\">demo</a>", deck: true));
            Project("pagine/demo.html", "<html><head><link rel=\"stylesheet\" href=\"demo.css\"><script src=\"js/demo.js\"></script></head><body><img src=\"/pagine/logo.png\"></body></html>");
            Project("pagine/demo.css", "body { background: url(img/bg.png); }");
            Project("pagine/img/bg.png", "png");
            Project("pagine/js/demo.js", "// demo");
            Project("pagine/logo.png", "png");

            var export = Export("master.md");

            Assert.AreEqual("page", Node(export, "master.html", "//a").GetAttributeValue("data-mde-export-kind", null));
            foreach (var file in new[] { "pagine/demo.css", "pagine/img/bg.png", "pagine/js/demo.js", "pagine/logo.png" })
            {
                Assert.IsTrue(export.Files.ContainsKey(file), file);
            }
            Assert.AreEqual("logo.png", Node(export, "pagine/demo.html", "//img").GetAttributeValue("src", null));
            var html = Node(export, "pagine/demo.html", "//html");
            Assert.IsNotNull(html.Attributes["data-mde-html-page"]);
            Assert.AreEqual("pagine/demo.html", html.GetAttributeValue("data-mde-export-path", null));
            Assert.IsNotNull(Node(export, "pagine/demo.html", "//script[@src='../_mde/javascripts/slides/html-page-trail.js']"));
            Assert.IsTrue(export.Files.ContainsKey("_mde/javascripts/slides/slide-ink.js"));
        }

        // ---- what cannot be carried ----

        [TestMethod]
        public void Report_missing_files_files_outside_the_project_and_resources_on_other_sites()
        {
            Project("master.md", Page(
                "<img src=\"/api/mdexplorer/manca.png?connectionId=abc\">" +
                "<img src=\"../../fuori.png\">" +
                "<img src=\"https://cdn.example.com/x.png\">" +
                "<a href=\"https://example.com\">sito</a>", deck: true));

            var export = Export("master.md");

            var issues = export.Report.Issues;
            Assert.IsTrue(issues.Any(i => i.Kind == StaticSiteIssueKind.Missing && i.Address == "/api/mdexplorer/manca.png"));
            Assert.IsTrue(issues.Any(i => i.Kind == StaticSiteIssueKind.OutsideProject && i.Address == "../../fuori.png"));
            Assert.IsTrue(issues.Any(i => i.Kind == StaticSiteIssueKind.NeedsNetwork && i.Address == "https://cdn.example.com/x.png"));
            Assert.IsFalse(issues.Any(i => i.Address == "https://example.com"), "A link to a site is a link, not a missing resource.");
            Assert.AreEqual("https://example.com", Node(export, "master.html", "//a").GetAttributeValue("href", null));
        }

        [TestMethod]
        public void Report_a_diagram_not_generated_yet_and_a_mermaid_block()
        {
            Project("master.md", Page("<pre data-mde-missing-diagram=\"true\">x</pre><pre class=\"mermaid\">graph TD</pre>", deck: true));

            var export = Export("master.md");

            Assert.IsTrue(export.Report.Issues.Any(i => i.Kind == StaticSiteIssueKind.DiagramNotGenerated));
            Assert.IsTrue(export.Report.Issues.Any(i => i.Kind == StaticSiteIssueKind.Mermaid));
        }

        [TestMethod]
        public void Take_the_paths_of_this_computer_off_the_page_and_report_any_left()
        {
            Project("master.md", Page($"<div mdeFullPathDocument=\"{_project}/x.md\"></div><p>{_project}</p>", deck: true));

            var export = Export("master.md");

            var body = Node(export, "master.html", "//body");
            Assert.IsNull(body.Attributes["DocumentPath"]);
            Assert.IsNull(body.Attributes["ConnectionId"]);
            Assert.IsNull(Node(export, "master.html", "//div").Attributes["mdeFullPathDocument"]);
            Assert.IsTrue(export.Report.Issues.Any(i => i.Kind == StaticSiteIssueKind.LocalPath), "The path left in the text is reported.");
        }

        // ---- the zip ----

        [TestMethod]
        public void Start_from_index_html_mark_every_page_and_write_the_report()
        {
            Project("vendite/master.md", Page("master", deck: true));

            var export = Export("vendite/master.md");

            StringAssert.Contains(Text(export, "index.html"), "url=vendite/master.html");
            var html = Node(export, "vendite/master.html", "//html");
            Assert.AreEqual("../", html.GetAttributeValue("data-mde-export-root", null));
            Assert.AreEqual("vendite/master.html", html.GetAttributeValue("data-mde-export-path", null));
            Assert.IsNotNull(html.Attributes["data-mde-export"]);
            StringAssert.Contains(Text(export, "_mde/resoconto.html"), "vendite/master.html");
        }

        [TestMethod]
        public void Keep_the_projects_own_index_html_and_start_from_another_page()
        {
            Project("master.md", Page("<a href=\"index.html?connectionId=abc\">home</a>", deck: true));
            Project("index.html", "<html><body>home</body></html>");

            var export = Export("master.md");

            StringAssert.Contains(Text(export, "index.html"), "home");
            StringAssert.Contains(Text(export, "avvio-mdexplorer.html"), "url=master.html");
            Assert.IsTrue(export.Report.Issues.Any(i => i.Kind == StaticSiteIssueKind.StartPageRenamed));
        }

        [TestMethod]
        public void Write_a_zip_that_holds_every_file()
        {
            Project("master.md", Page("<img src=\"a b.png\">", deck: true));
            Project("a b.png", "png");

            var export = Export("master.md");
            using var stream = new MemoryStream();
            export.WriteZip(stream);
            stream.Position = 0;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

            CollectionAssert.AreEquivalent(export.Files.Keys.ToList(), zip.Entries.Select(e => e.FullName).ToList());
            Assert.AreEqual("a%20b.png", Node(export, "master.html", "//img").GetAttributeValue("src", null));
        }

        [TestMethod]
        public void Refuse_to_start_from_a_file_that_is_not_there()
        {
            var error = Assert.ThrowsException<StaticSiteException>(() => Export("manca.md"));
            StringAssert.Contains(error.Message, "manca.md");
        }
    }
}
