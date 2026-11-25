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
    }
}
