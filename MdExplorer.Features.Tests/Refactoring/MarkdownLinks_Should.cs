using System.IO;
using System.Linq;
using MdExplorer.Features.Refactoring;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Refactoring
{
    /// <summary>I link letti con Markdig e riscritti per posizione (sprint 2026-11-25-Spostamento-File-E-Link).</summary>
    [TestClass]
    public class MarkdownLinks_Should
    {
        /// <summary>
        /// Le immagini ridimensionate o spostate nella pagina portano dopo il link una classe (<c>{.img1}</c>) e il suo
        /// CSS; da quella classe nasce il «linkHash» dei bottoni. Riscrivendo il percorso, classe e CSS restano.
        /// </summary>
        [TestMethod]
        public void Keep_the_image_class_that_names_its_css_when_the_path_changes()
        {
            var md = "![schema](assets/a.png){.img1 md-plantuml=\"dynamic:true\"}\n\n<style>\n.img1 { width: 300px; }\n</style>\n";
            var (text, changed) = MarkdownLinks.Rewrite(md, l => "../assets/a.png");
            Assert.AreEqual(1, changed);
            Assert.AreEqual(md.Replace("(assets/a.png)", "(../assets/a.png)"), text);
        }

        [TestMethod]
        public void See_a_link_on_the_first_line_and_never_inside_code()
        {
            var links = MarkdownLinks.Read("[A](a.md)\n\n```\n[B](b.md)\n```\n`[C](c.md)` e ![i](i.png)\n");
            CollectionAssert.AreEqual(new[] { "a.md", "i.png" }, links.Select(l => l.Target).ToArray());
            Assert.IsTrue(links[1].IsImage);
        }

        [TestMethod]
        public void Keep_title_anchor_and_angle_brackets_out_of_the_target()
        {
            var links = MarkdownLinks.Read("[A](a.md#s \"titolo\") [B](<b c.md>) [D](d%20e.md)\n");
            CollectionAssert.AreEqual(new[] { "a.md", "b c.md", "d e.md" }, links.Select(l => l.Target).ToArray());
            Assert.AreEqual("#s", links[0].Suffix);
        }

        [TestMethod]
        public void Read_a_reference_style_link_once_on_its_definition()
        {
            var links = MarkdownLinks.Read("Vedi [A][r] e ancora [A][r].\n\n[r]: docs/a.md\n");
            Assert.AreEqual(1, links.Count);
            Assert.IsTrue(links[0].IsDefinition);
            Assert.AreEqual("docs/a.md", links[0].Target);
        }

        [TestMethod]
        public void Rewrite_only_the_destination_keeping_everything_else()
        {
            var (text, changed) = MarkdownLinks.Rewrite("[docs/a.md](docs/a.md#s \"t\") [x](<docs/a.md>)\n",
                l => l.Target == "docs/a.md" ? "docs/sub/a b.md" : null);
            Assert.AreEqual(2, changed);
            Assert.AreEqual("[docs/a.md](docs/sub/a%20b.md#s \"t\") [x](<docs/sub/a b.md>)\n", text);
        }

        [TestMethod]
        public void Tell_external_links_from_files()
        {
            Assert.IsTrue(MarkdownLinks.IsExternal("https://example.com/a.md"));
            Assert.IsTrue(MarkdownLinks.IsExternal("mailto:mario@example.com"));
            Assert.IsTrue(MarkdownLinks.IsExternal(""));
            Assert.IsFalse(MarkdownLinks.IsExternal("docs/a.md"));
            Assert.IsFalse(MarkdownLinks.IsExternal("/docs/a.md"));
        }

        [TestMethod]
        public void Resolve_from_the_project_root_or_from_the_document_folder()
        {
            var root = Path.Combine(Path.GetTempPath(), "proj");
            var doc = Path.Combine(root, "docs", "x.md");
            Assert.AreEqual(Path.Combine(root, "index.md"), MarkdownLinks.ResolveTarget(root, doc, "/index.md"));
            Assert.AreEqual(Path.Combine(root, "index.md"), MarkdownLinks.ResolveTarget(root, doc, "../index.md#s"));
            Assert.AreEqual(Path.Combine(root, "docs", "a.md"), MarkdownLinks.ResolveTarget(root, doc, "./a.md"));
            Assert.IsNull(MarkdownLinks.ResolveTarget(root, doc, "https://example.com"));
        }

        // ---- Gli altri link: comandi che includono un file, PlantUML, HTML, mdShow ----

        private const string Mixed =
            "```text(src/a.cs)\ncodice\n```\n\n" +
            "```plantuml(@json, dati/b.json)\n```\n\n" +
            "```plantuml\n@startuml\nA : [[docs/c.md#s{suggerimento} Vai]]\nB : <img:assets/d.png{scale=0.5}>\n@enduml\n```\n\n" +
            "<a href=\"docs/e%20f.md#t\">E</a> <img src='assets/g.png'>\n\n" +
            "mdShowMd(docs/h.md)\n\n" +
            "```\n<a href=\"no.md\">esempio</a> mdShowMd(no.md)\n```\n";

        [TestMethod]
        public void Read_every_kind_of_link_and_none_written_as_an_example_in_code()
        {
            var links = MarkdownLinks.ReadAll(Mixed);
            CollectionAssert.AreEqual(
                new[] { "src/a.cs", "dati/b.json", "docs/c.md", "assets/d.png", "docs/e f.md", "assets/g.png", "docs/h.md" },
                links.Select(l => l.Target).ToArray());
            CollectionAssert.AreEqual(new[]
            {
                MarkdownLinkKind.IncludeCommand, MarkdownLinkKind.IncludeCommand, MarkdownLinkKind.PlantumlLink,
                MarkdownLinkKind.PlantumlImage, MarkdownLinkKind.Html, MarkdownLinkKind.Html, MarkdownLinkKind.MdShow,
            }, links.Select(l => l.Kind).ToArray());
        }

        [TestMethod]
        public void Relink_every_kind_of_link_when_the_document_goes_one_folder_down()
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-links");
            var oldDoc = Path.Combine(root, "nota.md");
            var newDoc = Path.Combine(root, "archivio", "nota.md");
            var text = MarkdownLinks.RelinkForMoves(Mixed, newDoc, oldDoc, root, _ => null, out var changed);
            Assert.AreEqual(7, changed);
            StringAssert.Contains(text, "```text(../src/a.cs)");
            StringAssert.Contains(text, "```plantuml(@json, ../dati/b.json)");
            StringAssert.Contains(text, "[[../docs/c.md#s{suggerimento} Vai]]");
            StringAssert.Contains(text, "<img:../assets/d.png{scale=0.5}>");
            StringAssert.Contains(text, "<a href=\"../docs/e%20f.md#t\">E</a> <img src='../assets/g.png'>");
            StringAssert.Contains(text, "mdShowMd(../docs/h.md)");
            StringAssert.Contains(text, "```\n<a href=\"no.md\">esempio</a> mdShowMd(no.md)\n```");
        }

        [TestMethod]
        public void Keep_root_links_from_the_root_but_a_plantuml_image_from_the_document_folder()
        {
            var root = Path.Combine(Path.GetTempPath(), "mde-links");
            var md = "```text(/src/a.cs)\n```\n\n```plantuml\n@startuml\nA : [[/docs/c.md]]\nB : <img:d.png>\n@enduml\n```\n";
            var text = MarkdownLinks.RelinkForMoves(md, Path.Combine(root, "x", "nota.md"), Path.Combine(root, "nota.md"), root, _ => null, out var changed);
            Assert.AreEqual(1, changed);
            StringAssert.Contains(text, "```text(/src/a.cs)");
            StringAssert.Contains(text, "[[/docs/c.md]]");
            StringAssert.Contains(text, "<img:../d.png>");
        }
    }
}
