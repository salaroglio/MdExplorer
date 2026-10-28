using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;
using MdExplorer.Service.Models;
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
            var gitIgnore = new MdExplorer.Features.Services.GitIgnore.GitIgnoreRules();
            _mdIgnore = new MdIgnoreService(NullLogger<MdIgnoreService>.Instance, gitIgnore);
            _foldersIgnore = new FoldersIgnoreService(NullLogger<FoldersIgnoreService>.Instance, null, gitIgnore);
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

        private ProgramFolders Programs(params string[] relatives) =>
            ProgramFolders.From(_project, relatives.Select(r => new DevelopmentFolder { Path = r, Tags = new List<string> { "program" } }));

        /// <summary>
        /// 19/10/2026: a program folder with only source code shows in the tree, so it is not hidden content of
        /// the folder above; a program folder with no file at all is (the eye reveals it).
        /// </summary>
        [TestMethod]
        public void Not_count_a_program_folder_with_files_as_hidden()
        {
            var src = Folder("src");
            File.WriteAllText(Path.Combine(Folder("src/Api"), "Program.cs"), "// main\n");
            Assert.IsTrue(Hidden(src), "without the program tag Api has no markdown: hidden");

            var programs = Programs("src/Api");
            Assert.IsFalse(FolderRevealableContent.HasHidden(src, _project, _mdIgnore, _foldersIgnore, programs),
                "Api is a program folder with files: the tree shows it");

            Folder("src/Vuota");
            Assert.IsTrue(FolderRevealableContent.HasHidden(src, _project, _mdIgnore, _foldersIgnore, Programs("src/Vuota")),
                "a program folder with no file shows nothing: it stays hidden, Api (not tagged here) too");
        }

        /// <summary>The exception is the program folder's alone: the folder above it follows the markdown rule.</summary>
        [TestMethod]
        public void Keep_the_folder_above_a_program_folder_hidden_without_markdown()
        {
            Folder("src");
            File.WriteAllText(Path.Combine(Folder("src/Api"), "Program.cs"), "// main\n");
            var programs = Programs("src/Api");

            Assert.IsTrue(Hidden(_project), "src has no markdown: hidden at the root even if a program folder is inside");
            Assert.IsFalse(programs.ShowsWithoutMarkdown(Path.Combine(_project, "src"), _project, _foldersIgnore));
            Assert.IsTrue(programs.ShowsWithoutMarkdown(Path.Combine(_project, "src", "Api"), _project, _foldersIgnore));
        }

        [TestMethod]
        public void Find_a_file_deep_down_and_not_in_empty_folders()
        {
            var api = Folder("api");
            Folder("api/a/b");
            Assert.IsFalse(FolderRevealableContent.HasAnyFile(api, _project, _foldersIgnore), "only empty folders");
            File.WriteAllText(Path.Combine(Folder("api/a/b/c"), "x.py"), "pass\n");
            Assert.IsTrue(FolderRevealableContent.HasAnyFile(api, _project, _foldersIgnore));
        }

        [TestMethod]
        public void Read_program_paths_with_either_separator_and_leave_out_the_root_and_other_tags()
        {
            var folders = new[]
            {
                new DevelopmentFolder { Path = "src\\Api", Tags = new List<string> { "Program" } },
                new DevelopmentFolder { Path = "tools/", Tags = new List<string> { "program" } },
                new DevelopmentFolder { Path = "", Tags = new List<string> { "program" } },
                new DevelopmentFolder { Path = "docs", Tags = new List<string> { "docs" } },
            };
            var programs = ProgramFolders.From(_project, folders);

            Assert.AreEqual(2, programs.Paths.Count);
            Assert.IsTrue(programs.IsProgram(Path.Combine(_project, "src", "Api")));
            Assert.IsTrue(programs.IsProgram(Path.Combine(_project, "tools") + Path.DirectorySeparatorChar));
            Assert.IsFalse(programs.IsProgram(Path.Combine(_project, "docs")));
            Assert.IsFalse(programs.IsProgram(_project));
        }

        [TestMethod]
        public void Say_no_for_a_folder_that_is_gone()
        {
            Assert.IsFalse(Hidden(Path.Combine(_project, "sparita")));
            Assert.IsFalse(Revealable(Path.Combine(_project, "sparita")));
        }
    }
}
