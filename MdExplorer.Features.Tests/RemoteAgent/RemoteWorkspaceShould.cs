using System;
using System.IO;
using System.Linq;
using System.Text;
using MdExplorer.Remote.Protocol;
using MdExplorer.RemoteAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.RemoteAgent
{
    /// <summary>Sprint Cartella-Remota-Via-SSH, F1: le operazioni del serverino dentro una radice.</summary>
    [TestClass]
    public class RemoteWorkspaceShould
    {
        private string _base;
        private string _root;
        private RemoteWorkspace _ws;

        [TestInitialize]
        public void Setup()
        {
            _base = Path.Combine(Path.GetTempPath(), "mde-remote-" + Guid.NewGuid().ToString("N"));
            _root = Path.Combine(_base, "proj");
            Directory.CreateDirectory(Path.Combine(_root, "src", "api"));
            Directory.CreateDirectory(Path.Combine(_root, "Zeta"));
            Directory.CreateDirectory(Path.Combine(_root, ".git"));
            File.WriteAllText(Path.Combine(_root, "src", "caller.js"), "SaveOrder(42);\nnothing\n");
            File.WriteAllText(Path.Combine(_root, "src", "api", "Order.cs"), "class Order { void SaveOrder() {} }\n");
            File.WriteAllBytes(Path.Combine(_root, "logo.png"), new byte[] { 0x89, 0x50, 0x00, 0x00, 0x01 });
            File.WriteAllText(Path.Combine(_root, "b.txt"), "b");
            Directory.CreateDirectory(Path.Combine(_base, "proj2"));
            File.WriteAllText(Path.Combine(_base, "fuori.txt"), "segreto SaveOrder");
            _ws = new RemoteWorkspace(_root);
        }

        [TestCleanup]
        public void Cleanup() { if (Directory.Exists(_base)) Directory.Delete(_base, true); }

        private static string Code(Action action)
        {
            try { action(); }
            catch (RemoteFailure f) { return f.Code; }
            Assert.Fail("doveva essere rifiutato");
            return null;
        }

        [TestMethod]
        public void List_folders_first_then_files_without_git_with_text_flag()
        {
            var entries = _ws.List("");
            CollectionAssert.AreEqual(new[] { "src", "Zeta", "b.txt", "logo.png" }, entries.Select(e => e.Name).ToArray());
            Assert.IsTrue(entries[0].IsFolder);
            Assert.IsTrue(entries.Single(e => e.Name == "b.txt").IsText);
            Assert.IsFalse(entries.Single(e => e.Name == "logo.png").IsText, "un binario non si apre come testo");
            Assert.AreEqual("src/api", _ws.List("src").Single(e => e.IsFolder).Path, "percorsi relativi con /");
        }

        [TestMethod]
        public void Refuse_paths_outside_the_root_and_absolute_paths()
        {
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, Code(() => _ws.Read("../fuori.txt")));
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, Code(() => _ws.List("../proj2")), "/proj2 non è dentro /proj");
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, Code(() => _ws.Read("src/../../fuori.txt")));
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Read(Path.Combine(_base, "fuori.txt"))));
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Read(null)));
        }

        [TestMethod]
        public void Not_follow_symbolic_links()
        {
            if (OperatingSystem.IsWindows()) Assert.Inconclusive("i link simbolici su Windows chiedono privilegi");
            File.CreateSymbolicLink(Path.Combine(_root, "link.txt"), Path.Combine(_base, "fuori.txt"));
            Directory.CreateSymbolicLink(Path.Combine(_root, "linkdir"), _base);
            Assert.IsFalse(_ws.List("").Any(e => e.Name.StartsWith("link")), "i link non si mostrano");
            Assert.AreEqual(RemoteErrorCodes.Link, Code(() => _ws.Read("link.txt")));
            Assert.AreEqual(RemoteErrorCodes.Link, Code(() => _ws.Read("linkdir/fuori.txt")), "neanche attraversando una cartella-link");
            Assert.AreEqual(RemoteErrorCodes.Link, Code(() => _ws.Create("linkdir/nuovo.txt", "x")));
            Assert.IsFalse(_ws.Grep("", "segreto", false, 0, null).Hits.Any(), "la ricerca non entra nei link");
        }

        [TestMethod]
        public void Read_and_write_with_the_stamp_of_the_read()
        {
            var (text, stamp, bom) = _ws.Read("src/caller.js");
            Assert.AreEqual("SaveOrder(42);\nnothing\n", text);
            Assert.IsFalse(bom);
            var after = _ws.Write("src/caller.js", "SaveOrder(7); // àè\n", stamp);
            Assert.AreEqual("SaveOrder(7); // àè\n", File.ReadAllText(Path.Combine(_root, "src", "caller.js")));
            Assert.AreNotEqual(stamp, after);
            Assert.AreEqual(RemoteErrorCodes.Changed, Code(() => _ws.Write("src/caller.js", "x", stamp)), "impronta vecchia → rifiuto");
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Write("src/caller.js", "x", null)), "senza impronta → prima leggi");
            Assert.AreEqual(RemoteErrorCodes.NotFound, Code(() => _ws.Write("src/nuovo.js", "x", after)), "write non crea");
            Assert.IsFalse(Directory.GetFiles(Path.Combine(_root, "src")).Any(f => f.EndsWith(".tmp")), "niente file temporanei rimasti");
        }

        [TestMethod]
        public void Keep_the_bom_and_the_permissions_when_writing()
        {
            var file = Path.Combine(_root, "bom.txt");
            File.WriteAllBytes(file, new byte[] { 0xEF, 0xBB, 0xBF, (byte)'a' });
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            var (text, stamp, bom) = _ws.Read("bom.txt");
            Assert.AreEqual("a", text, "il BOM non entra nel testo");
            Assert.IsTrue(bom);
            _ws.Write("bom.txt", "b", stamp);
            CollectionAssert.AreEqual(new byte[] { 0xEF, 0xBB, 0xBF, (byte)'b' }, File.ReadAllBytes(file));
            if (!OperatingSystem.IsWindows())
                Assert.AreEqual(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(file));
        }

        [TestMethod]
        public void Refuse_to_read_binary_or_non_utf8_files()
        {
            Assert.AreEqual(RemoteErrorCodes.Binary, Code(() => _ws.Read("logo.png")));
            File.WriteAllBytes(Path.Combine(_root, "latin1.txt"), new byte[] { (byte)'c', 0xE0 });
            Assert.AreEqual(RemoteErrorCodes.NotUtf8, Code(() => _ws.Read("latin1.txt")), "riscriverlo lo rovinerebbe");
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Read("src")), "una cartella non si legge");
            Assert.AreEqual(RemoteErrorCodes.NotFound, Code(() => _ws.Read("manca.txt")));
        }

        [TestMethod]
        public void Create_files_and_folders_with_missing_parents_but_never_over_existing_ones()
        {
            _ws.Create("nuova/cartella/file.md", "# Ciao");
            Assert.AreEqual("# Ciao", File.ReadAllText(Path.Combine(_root, "nuova", "cartella", "file.md")));
            Assert.AreEqual(RemoteErrorCodes.Exists, Code(() => _ws.Create("b.txt", "x")));
            Assert.AreEqual("b", File.ReadAllText(Path.Combine(_root, "b.txt")), "il file esistente non si tocca");
            _ws.Mkdir("altra/sotto");
            Assert.IsTrue(Directory.Exists(Path.Combine(_root, "altra", "sotto")));
            Assert.AreEqual(RemoteErrorCodes.Exists, Code(() => _ws.Mkdir("src")));
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Mkdir("")), "la radice non si tocca");
        }

        [TestMethod]
        public void Delete_files_and_folders_but_a_full_folder_only_when_asked()
        {
            _ws.Delete("b.txt", recursive: false);
            Assert.IsFalse(File.Exists(Path.Combine(_root, "b.txt")));
            Assert.AreEqual(RemoteErrorCodes.NotEmpty, Code(() => _ws.Delete("src", recursive: false)));
            _ws.Delete("Zeta", recursive: false);
            _ws.Delete("src", recursive: true);
            Assert.IsFalse(Directory.Exists(Path.Combine(_root, "src")));
            Assert.AreEqual(RemoteErrorCodes.NotFound, Code(() => _ws.Delete("src", recursive: true)));
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Delete("", recursive: true)), "mai la radice");
        }

        [TestMethod]
        public void Move_and_rename_files_and_folders()
        {
            _ws.Move("b.txt", "docs/b2.txt");
            Assert.AreEqual("b", File.ReadAllText(Path.Combine(_root, "docs", "b2.txt")));
            _ws.Move("src", "lib/src");
            Assert.IsTrue(File.Exists(Path.Combine(_root, "lib", "src", "api", "Order.cs")));
            Assert.AreEqual(RemoteErrorCodes.Exists, Code(() => _ws.Move("docs/b2.txt", "lib/src/caller.js")));
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Move("lib", "lib/dentro")));
            Assert.AreEqual(RemoteErrorCodes.OutsideRoot, Code(() => _ws.Move("docs/b2.txt", "../b2.txt")));
            Assert.AreEqual(RemoteErrorCodes.NotFound, Code(() => _ws.Move("manca", "altro")));
        }

        [TestMethod]
        public void Grep_on_the_server_skipping_binaries_and_excluded_folders()
        {
            File.WriteAllText(Path.Combine(_root, ".git", "config"), "SaveOrder in git");
            var (hits, truncated) = _ws.Grep("", "SaveOrder", false, 0, new[] { ".git" });
            Assert.IsFalse(truncated);
            CollectionAssert.AreEqual(new[] { "src/api/Order.cs:1", "src/caller.js:1" }, hits.Select(h => $"{h.Path}:{h.Line}").ToArray());
            Assert.AreEqual("SaveOrder(42);", hits[1].Text);
            Assert.AreEqual(2, _ws.Grep("src/caller.js", "nothing", false, 0, null).Hits.Single().Line, "anche su un file solo");
            Assert.AreEqual(2, _ws.Grep("", "saveorder", true, 0, new[] { ".git" }).Hits.Count, "maiuscole indifferenti");
            var limited = _ws.Grep("", "SaveOrder", false, 1, new[] { ".git" });
            Assert.AreEqual(1, limited.Hits.Count);
            Assert.IsTrue(limited.Truncated);
            Assert.AreEqual(RemoteErrorCodes.BadRequest, Code(() => _ws.Grep("", "(", false, 0, null)));
        }

        [TestMethod]
        public void Read_the_git_remotes_from_the_files_without_git()
        {
            Assert.IsNull(new RemoteWorkspace(Path.Combine(_base, "proj2")).GitInfo());
            File.WriteAllText(Path.Combine(_root, ".git", "config"),
                "[core]\n\tbare = false\n[remote \"upstream\"]\n\turl = https://example.org/up.git\n[remote \"origin\"]\n\turl = git@example.org:acme/backend.git\n\tfetch = +refs/heads/*:refs/remotes/origin/*\n");
            var git = new RemoteWorkspace(Path.Combine(_root, "src")).GitInfo();
            Assert.AreEqual(_root, git.Top, "il repository sopra la radice");
            CollectionAssert.AreEqual(new[] { "origin=git@example.org:acme/backend.git", "upstream=https://example.org/up.git" },
                git.Remotes.Select(r => $"{r.Name}={r.Url}").ToArray());
        }

        [TestMethod]
        public void Follow_a_dot_git_file_to_the_common_config_of_a_worktree()
        {
            var main = Path.Combine(_base, "main.git");
            Directory.CreateDirectory(Path.Combine(main, "worktrees", "wt"));
            File.WriteAllText(Path.Combine(main, "config"), "[remote \"origin\"]\n\turl = https://example.org/wt.git\n");
            File.WriteAllText(Path.Combine(main, "worktrees", "wt", "commondir"), "../..\n");
            var wt = Path.Combine(_base, "wt");
            Directory.CreateDirectory(wt);
            File.WriteAllText(Path.Combine(wt, ".git"), "gitdir: " + Path.Combine(main, "worktrees", "wt") + "\n");
            Assert.AreEqual("https://example.org/wt.git", new RemoteWorkspace(wt).GitInfo().Remotes.Single().Url);
        }
    }
}
