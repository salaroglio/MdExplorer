using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Radici di codice esterne, F1 (sprint 2026-10-12): collegare con git vero. Il primo che collega fissa l'identità
    /// (remote + sottocartella), un collega con il clone altrove si collega allo stesso alias, un clone di un altro
    /// repository è rifiutato col motivo, e gli stati dicono cosa non va.
    /// </summary>
    [TestClass]
    public class CodeRootService_Should
    {
        private sealed class MemoryStore : ICodeRootBindingStore
        {
            private readonly Dictionary<string, string> _map = new();
            public string Get(string p, string a) => _map.TryGetValue(p + "|" + a, out var v) ? v : null;
            public void Set(string p, string a, string l) => _map[p + "|" + a] = l;
            public void Remove(string p, string a) => _map.Remove(p + "|" + a);
        }

        private string _root, _project;
        private NativeGitRunner _git;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-coderoots", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "doc");
            Directory.CreateDirectory(_project);
            File.WriteAllText(Path.Combine(_project, "README.md"), "# doc");
            _git = new NativeGitRunner(NullLogger<NativeGitRunner>.Instance);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private async Task<string> CloneOf(string name, string remote)
        {
            var dir = Path.Combine(_root, name);
            Directory.CreateDirectory(Path.Combine(dir, "src", "Api"));
            File.WriteAllText(Path.Combine(dir, "src", "Api", "Foo.cs"), "class Foo {}");
            Assert.IsTrue((await _git.RunAsync(dir, new[] { "init", "-q" })).Ok);
            if (remote != null) Assert.IsTrue((await _git.RunAsync(dir, new[] { "remote", "add", "origin", remote })).Ok);
            return dir;
        }

        private CodeRootService Service(ICodeRootBindingStore store) => new(store, _git, NullLogger<CodeRootService>.Instance);

        [TestMethod]
        public async Task Declare_on_first_link_and_let_a_colleague_link_the_same_repository_elsewhere()
        {
            var mine = await CloneOf("io/backend", "https://github.com/acme/backend.git");
            var me = Service(new MemoryStore());
            var state = await me.LinkAsync(_project, "backend", Path.Combine(mine, "src"));
            Assert.AreEqual(CodeRootStatus.Linked, state.Status, state.Reason);
            Assert.AreEqual("src", state.Subpath, "l'identità è repository + sottocartella");
            Assert.AreEqual(Path.Combine(mine, "src"), state.RootPath);
            var yml = File.ReadAllText(Path.Combine(_project, ".development.yml"));
            StringAssert.Contains(yml, "alias: backend");
            StringAssert.Contains(yml, "repository: https://github.com/acme/backend.git");
            Assert.IsFalse(yml.Contains(mine), "nessun percorso della macchina nel file che va su Git");

            // il collega: clone altrove, via ssh, sceglie la radice del clone
            var theirs = await CloneOf("collega/lavoro/backend-clone", "git@github.com:acme/backend.git");
            var colleague = Service(new MemoryStore());
            var before = (await colleague.ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.NotLinked, before.Status);
            var after = await colleague.LinkAsync(_project, "backend", theirs);
            Assert.AreEqual(CodeRootStatus.Linked, after.Status, after.Reason);
            Assert.AreEqual(Path.Combine(theirs, "src"), after.RootPath, "la sottocartella dichiarata vale anche per lui");

            var resolver = await colleague.CreateResolverAsync(_project);
            Assert.AreEqual(Path.Combine(theirs, "src", "Api", "Foo.cs"), resolver.Resolve("@backend/Api/Foo.cs"));
            Assert.AreEqual("@backend/Api/Foo.cs", resolver.ToLogical(Path.Combine(theirs, "src", "Api", "Foo.cs")));
        }

        [TestMethod]
        public async Task Refuse_a_clone_of_another_repository_and_say_which_it_is()
        {
            var store = new MemoryStore();
            await Service(store).LinkAsync(_project, "backend", await CloneOf("backend", "https://github.com/acme/backend.git"));
            var wrong = await CloneOf("frontend", "https://github.com/acme/frontend.git");
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Service(new MemoryStore()).LinkAsync(_project, "backend", wrong));
            StringAssert.Contains(ex.Message, "acme/frontend");
            StringAssert.Contains(ex.Message, "acme/backend");
        }

        [TestMethod]
        public async Task Report_a_moved_folder_and_a_changed_remote()
        {
            var store = new MemoryStore();
            var service = Service(store);
            var clone = await CloneOf("backend", "https://github.com/acme/backend.git");
            await service.LinkAsync(_project, "backend", clone);

            await _git.RunAsync(clone, new[] { "remote", "set-url", "origin", "https://github.com/acme/altro.git" });
            var mismatch = (await service.ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.Mismatch, mismatch.Status);
            StringAssert.Contains(mismatch.Reason, "acme/altro");

            Directory.Move(clone, clone + "-spostato");
            var missing = (await service.ListAsync(_project)).Single();
            Assert.AreEqual(CodeRootStatus.NotFound, missing.Status);
            var resolver = await service.CreateResolverAsync(_project);
            StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => resolver.Resolve("@backend/x.cs")).Message, "non esiste più");
        }

        [TestMethod]
        public async Task Refuse_folders_inside_the_project_and_repositories_without_a_remote()
        {
            var inside = Path.Combine(_project, "program");
            Directory.CreateDirectory(inside);
            StringAssert.Contains((await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Service(new MemoryStore()).LinkAsync(_project, "x", inside))).Message, "dentro il progetto");
            var noRemote = await CloneOf("senza-remote", null);
            StringAssert.Contains((await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Service(new MemoryStore()).LinkAsync(_project, "x", noRemote))).Message, "senza remote");
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => Service(new MemoryStore()).LinkAsync(_project, "Back End", noRemote));
        }

        [TestMethod]
        public async Task Accept_a_folder_outside_git_and_say_it_has_no_identity()
        {
            var plain = Path.Combine(_root, "cartella-semplice");
            Directory.CreateDirectory(plain);
            var state = await Service(new MemoryStore()).LinkAsync(_project, "script", plain);
            Assert.AreEqual(CodeRootStatus.Linked, state.Status);
            Assert.IsNull(state.Repository);
            Assert.AreEqual(plain, state.RootPath);
        }
    }
}
