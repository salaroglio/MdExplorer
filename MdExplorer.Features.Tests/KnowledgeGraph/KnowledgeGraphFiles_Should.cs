using System;
using System.IO;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.KnowledgeGraph
{
    /// <summary>
    /// Paths and kinds of the Knowledge Graph file nodes, on real files in a temporary project.
    /// "Foreign" separators are the other system's: "\" on Linux (what LinkInsideMarkdown stores
    /// there), "/" on Windows.
    /// </summary>
    [TestClass]
    public class KnowledgeGraphFiles_Should
    {
        private static string _root;

        private static readonly Func<string, bool> DefaultApplications =
            ext => ext == "xlsx" || ext == "pdf" || ext == "bmpr" || ext == "docx" || ext == "pptx" || ext == "xls" || ext == "ppt";

        [ClassInitialize]
        public static void CreateProject(TestContext _)
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-kg-files-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "docs", "risorse"));
            File.WriteAllText(Path.Combine(_root, "docs", "alfa.md"), "# Alfa\n");
            File.WriteAllText(Path.Combine(_root, "docs", "risorse", "config.json"), "{ \"a\": 1 }\n");
            File.WriteAllText(Path.Combine(_root, "docs", "risorse", "impostazioni.yaml"), "a: 1\n");
            File.WriteAllText(Path.Combine(_root, "docs", "risorse", "Dockerfile"), "FROM alpine\n");
            File.WriteAllText(Path.Combine(_root, "docs", "risorse", "pagina.html"), "<p>ciao</p>\n");
            File.WriteAllBytes(Path.Combine(_root, "docs", "risorse", "presentazione.pptx"), new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x01 });
            File.WriteAllBytes(Path.Combine(_root, "docs", "risorse", "relazione.docx"), new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00, 0x02 });
            File.WriteAllBytes(Path.Combine(_root, "docs", "risorse", "foto.png"), new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00 });
            File.WriteAllBytes(Path.Combine(_root, "docs", "risorse", "dati.bin"), new byte[] { 0x00, 0x01, 0x02, 0x03 });
        }

        [ClassCleanup]
        public static void DeleteProject()
        {
            if (_root != null && Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        }

        private static string P(params string[] parts) => Path.Combine(_root, Path.Combine(parts));

        private static string Foreign(string path)
            => Path.DirectorySeparatorChar == '/' ? path.Replace('/', '\\') : path.Replace('\\', '/');

        // ── Paths ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void NormalizeForeignSeparatorsAndParentSegments()
        {
            var stored = Foreign(Path.Combine(_root, "docs", "risorse", "..", "alfa.md"));
            Assert.AreEqual(P("docs", "alfa.md"), KnowledgeGraphFiles.NormalizePath(stored));
        }

        [TestMethod]
        public void GiveTheRelativePathOfAFileStoredWithForeignSeparators()
        {
            Assert.AreEqual("docs/risorse/config.json",
                KnowledgeGraphFiles.ToProjectRelative(Foreign(P("docs", "risorse", "config.json")), _root));
            Assert.AreEqual("docs/alfa.md",
                KnowledgeGraphFiles.ToProjectRelative(P("docs", "risorse", "..", "alfa.md"), _root + Path.DirectorySeparatorChar));
        }

        [TestMethod]
        public void GiveNoRelativePathOutsideTheProject()
        {
            Assert.IsNull(KnowledgeGraphFiles.ToProjectRelative(Path.Combine(Path.GetTempPath(), "altro", "x.md"), _root));
            // A sibling folder whose name starts like the project is outside too.
            Assert.IsNull(KnowledgeGraphFiles.ToProjectRelative(_root + "-copia" + Path.DirectorySeparatorChar + "x.md", _root));
            Assert.IsNull(KnowledgeGraphFiles.ToProjectRelative(P("docs", "alfa.md"), ""));
        }

        // ── Kind and how it opens ──────────────────────────────────────────────

        private static void AssertDescribed(string path, string kind, string openWith, bool exists = true)
        {
            var info = KnowledgeGraphFiles.Describe(path, DefaultApplications);
            Assert.AreEqual(kind, info.Kind, "kind of " + path);
            Assert.AreEqual(openWith, info.OpenWith, "openWith of " + path);
            Assert.AreEqual(exists, info.Exists, "exists of " + path);
        }

        [TestMethod]
        public void OpenMarkdownInThePage()
            => AssertDescribed(P("docs", "alfa.md"), "markdown", KnowledgeGraphFiles.OpenInPage);

        [TestMethod]
        public void OpenTextFilesInThePageAsColoredSource()
        {
            AssertDescribed(P("docs", "risorse", "config.json"), "json", KnowledgeGraphFiles.OpenInPage);
            AssertDescribed(P("docs", "risorse", "impostazioni.yaml"), "text", KnowledgeGraphFiles.OpenInPage);
            AssertDescribed(P("docs", "risorse", "Dockerfile"), "text", KnowledgeGraphFiles.OpenInPage);   // text by its content, not its name
        }

        [TestMethod]
        public void OpenTheProjectApplicationExtensionsWithTheirApplication()
        {
            AssertDescribed(P("docs", "risorse", "presentazione.pptx"), "powerpoint", KnowledgeGraphFiles.OpenWithApplication);
            AssertDescribed(P("docs", "risorse", "relazione.docx"), "word", KnowledgeGraphFiles.OpenWithApplication);
        }

        [TestMethod]
        public void FollowTheProjectConfigurationForATextFile()
        {
            // A project that opens .json with its application: the configuration wins over the colored view.
            var info = KnowledgeGraphFiles.Describe(P("docs", "risorse", "config.json"), ext => ext == "json");
            Assert.AreEqual(KnowledgeGraphFiles.OpenWithApplication, info.OpenWith);
        }

        [TestMethod]
        public void OpenOtherExistingFilesWithTheSystemApplication()
        {
            AssertDescribed(P("docs", "risorse", "foto.png"), "image", KnowledgeGraphFiles.OpenWithApplication);
            AssertDescribed(P("docs", "risorse", "dati.bin"), "other", KnowledgeGraphFiles.OpenWithApplication);
            // HTML is text, but the page renders it instead of coloring it.
            AssertDescribed(P("docs", "risorse", "pagina.html"), "text", KnowledgeGraphFiles.OpenWithApplication);
        }

        [TestMethod]
        public void NotOpenAMissingFile()
        {
            AssertDescribed(P("docs", "manca.md"), "markdown", KnowledgeGraphFiles.CannotOpen, exists: false);
            AssertDescribed(P("docs", "risorse", "mancante.pptx"), "powerpoint", KnowledgeGraphFiles.CannotOpen, exists: false);
        }

        [TestMethod]
        public void DescribeAFileStoredWithForeignSeparators()
            => AssertDescribed(Foreign(P("docs", "risorse", "config.json")), "json", KnowledgeGraphFiles.OpenInPage);
    }
}
