using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.CodeGraph
{
    /// <summary>Radici di codice esterne, R9: i figli di una cartella di una radice, per l'albero in sola lettura.</summary>
    [TestClass]
    public class CodeRootBrowserShould
    {
        private string _base;
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _base = Path.Combine(Path.GetTempPath(), "mde-browser-" + Guid.NewGuid().ToString("N"));
            _root = Path.Combine(_base, "clone", "src");
            Directory.CreateDirectory(Path.Combine(_root, "Zeta"));
            Directory.CreateDirectory(Path.Combine(_root, "api"));
            Directory.CreateDirectory(Path.Combine(_root, ".git"));
            File.WriteAllText(Path.Combine(_root, "b.cs"), "class B {}");
            File.WriteAllText(Path.Combine(_root, "A.md"), "# A");
            File.WriteAllBytes(Path.Combine(_root, "logo.png"), new byte[] { 0x89, 0x50, 0x00, 0x00, 0x01 });
            File.WriteAllText(Path.Combine(_root, "api", "Foo.cs"), "class Foo {}");
            Directory.CreateDirectory(Path.Combine(_base, "fuori"));
        }

        [TestCleanup]
        public void Cleanup() { if (Directory.Exists(_base)) Directory.Delete(_base, true); }

        private CodeRootResolver Resolver() => new(Path.Combine(_base, "doc"), new[]
        {
            new CodeRootState { Alias = "backend", Status = CodeRootStatus.Linked, RootPath = _root },
            new CodeRootState { Alias = "frontend", Status = CodeRootStatus.NotLinked, Reason = "su questa macchina non è collegata" },
        });

        [TestMethod]
        public void List_folders_first_then_files_with_logical_paths_and_without_git()
        {
            var entries = CodeRootBrowser.List(Resolver(), "@backend");
            CollectionAssert.AreEqual(new[] { "api", "Zeta", "A.md", "b.cs", "logo.png" }, entries.Select(e => e.Name).ToArray());
            Assert.IsTrue(entries[0].IsFolder);
            Assert.AreEqual("@backend/api", entries[0].LogicalPath);
            Assert.AreEqual(Path.Combine(_root, "api"), entries[0].AbsolutePath);
            Assert.IsTrue(entries.Single(e => e.Name == "b.cs").IsText);
            Assert.IsFalse(entries.Single(e => e.Name == "logo.png").IsText, "un binario non si apre");
        }

        [TestMethod]
        public void List_a_subfolder()
        {
            var entries = CodeRootBrowser.List(Resolver(), "@backend/api");
            Assert.AreEqual("@backend/api/Foo.cs", entries.Single().LogicalPath);
        }

        [TestMethod]
        public void Hide_symbolic_links_that_could_lead_outside_the_root()
        {
            var link = Path.Combine(_root, "scorciatoia");
            try { Directory.CreateSymbolicLink(link, Path.Combine(_base, "fuori")); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { Assert.Inconclusive("link simbolici non creabili qui: " + ex.Message); }
            Assert.IsFalse(CodeRootBrowser.List(Resolver(), "@backend").Any(e => e.Name == "scorciatoia"));
        }

        [TestMethod]
        public void Refuse_escapes_unlinked_roots_and_project_paths_with_the_reason()
        {
            var r = Resolver();
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => CodeRootBrowser.List(r, "@backend/../..")).Message, "esce dalla radice");
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => CodeRootBrowser.List(r, "@frontend")).Message, "non è collegata");
            Assert.ThrowsException<ArgumentException>(() => CodeRootBrowser.List(r, "program"));
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => CodeRootBrowser.List(r, "@backend/manca")).Message, "non esiste");
        }
    }
}
