using System;
using System.IO;
using MdExplorer.Features.Services;
using MdExplorer.Service.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// The two rules of the tree's eye, shared by the folder load and the live update of the watcher (28/09/2026:
    /// files written by an e2e run never reached the tree). A folder of the tree hides only what is not markdown
    /// and folders without markdown; a revealed (green) folder shows everything.
    /// </summary>
    [TestClass]
    public class FolderRevealableContent_Should
    {
        private string _project;
        private MdIgnoreService _mdIgnore;
        private FoldersIgnoreService _foldersIgnore;

        [TestInitialize]
        public void Setup()
        {
            _project = Path.Combine(Path.GetTempPath(), "eye-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_project);
            _mdIgnore = new MdIgnoreService(NullLogger<MdIgnoreService>.Instance);
            _foldersIgnore = new FoldersIgnoreService(NullLogger<FoldersIgnoreService>.Instance, null);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_project, true);

        private string Folder(string relative)
        {
            var path = Path.Combine(_project, relative);
            Directory.CreateDirectory(path);
            return path;
        }

        private bool Hidden(string folder) => FolderRevealableContent.HasHidden(folder, _project, _mdIgnore, _foldersIgnore);
        private bool Revealable(string folder) => FolderRevealableContent.Has(folder, _project, _mdIgnore, _foldersIgnore);

        [TestMethod]
        public void Not_count_markdown_as_hidden_in_a_folder_of_the_tree()
        {
            var note = Folder("note");
            File.WriteAllText(Path.Combine(note, "a.md"), "# a\n");
            File.WriteAllText(Path.Combine(note, "note.md.directory"), "");

            Assert.IsFalse(Hidden(note), "a markdown and the TOC sidecar are not hidden content");
            Assert.IsTrue(Revealable(note), "a revealed folder shows its markdown too");

            File.WriteAllText(Path.Combine(note, "b.txt"), "x");
            Assert.IsTrue(Hidden(note));
        }

        [TestMethod]
        public void Count_a_subfolder_without_markdown_below_it_and_not_one_with_markdown_deep_down()
        {
            var tests = Folder("test-e2e/login.e2e");
            File.WriteAllText(Path.Combine(Folder("test-e2e/login.e2e/esecuzioni/2026-09-28_08-45"), "report.md"), "# r\n");
            Assert.IsFalse(Hidden(tests), "esecuzioni has a report.md two levels down: it is in the tree");

            File.WriteAllText(Path.Combine(Folder("test-e2e/login.e2e/scripts"), "login.T1.spec.cs"), "// T1\n");
            Assert.IsTrue(Hidden(tests), "scripts has no markdown: the tree leaves it out, the eye must show it");
        }

        [TestMethod]
        public void Say_no_for_a_folder_that_is_gone()
        {
            Assert.IsFalse(Hidden(Path.Combine(_project, "sparita")));
            Assert.IsFalse(Revealable(Path.Combine(_project, "sparita")));
        }
    }
}
