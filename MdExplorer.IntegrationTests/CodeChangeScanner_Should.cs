using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// D14 del grafo del codice: git dice quali file guardare. Prima analisi = tutti; senza modifiche = nessun
    /// candidato; un commit che tocca due file = due candidati; un file non committato conta; fuori da git = modo
    /// «hash» dichiarato con tutti i file.
    /// </summary>
    [TestClass]
    public class CodeChangeScanner_Should
    {
        private string _root, _project, _src;
        private NativeGitRunner _git;
        private CodeChangeScanner _scanner;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-scanner-tests", Guid.NewGuid().ToString("N"));
            _project = Path.Combine(_root, "project");
            _src = Path.Combine(_project, "src");
            Directory.CreateDirectory(Path.Combine(_src, "bin"));
            File.WriteAllText(Path.Combine(_src, "A.cs"), "class A {}");
            File.WriteAllText(Path.Combine(_src, "B.cs"), "class B {}");
            File.WriteAllText(Path.Combine(_src, "README.md"), "# no");
            File.WriteAllText(Path.Combine(_src, "bin", "Gen.cs"), "class Gen {}");   // ignorato da .gitignore
            File.WriteAllText(Path.Combine(_project, ".gitignore"), "bin/\n");
            _git = new NativeGitRunner(NullLogger<NativeGitRunner>.Instance);
            _scanner = new CodeChangeScanner(_git, NullLogger<CodeChangeScanner>.Instance);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        private async Task Git(params string[] args)
        {
            var r = await _git.RunAsync(_project, args);
            Assert.IsTrue(r.Ok, $"git {string.Join(' ', args)}: {r.Describe()}");
        }

        private async Task<string> Head() => (await _git.RunAsync(_project, new[] { "rev-parse", "HEAD" })).Stdout.Trim();

        [TestMethod]
        public async Task Use_git_to_name_the_changed_files_and_confirm_nothing_changed_otherwise()
        {
            await Git("init", "-q");
            await Git("-c", "user.name=t", "-c", "user.email=t@t", "add", "-A");
            await Git("-c", "user.name=t", "-c", "user.email=t@t", "commit", "-q", "-m", "first");

            var first = await _scanner.ScanAsync(_project, _src, previousCommit: null);
            Assert.AreEqual("git", first.Mode);
            Assert.AreEqual("prima analisi", first.Reason);
            CollectionAssert.AreEquivalent(new[] { "src/A.cs", "src/B.cs" }, first.AllFiles.Select(f => f.LogicalPath).ToArray(), "README e bin/ restano fuori");
            Assert.AreEqual(2, first.Candidates.Count);
            var analyzed = await Head();
            Assert.AreEqual(analyzed, first.HeadCommit);

            var unchanged = await _scanner.ScanAsync(_project, _src, analyzed);
            Assert.AreEqual(0, unchanged.Candidates.Count, "secondo giro senza modifiche: nessun candidato");
            Assert.AreEqual(2, unchanged.AllFiles.Count);

            File.WriteAllText(Path.Combine(_src, "B.cs"), "class B { void M() {} }");
            File.WriteAllText(Path.Combine(_src, "C.cs"), "class C {}");
            await Git("-c", "user.name=t", "-c", "user.email=t@t", "add", "-A");
            await Git("-c", "user.name=t", "-c", "user.email=t@t", "commit", "-q", "-m", "second");
            File.WriteAllText(Path.Combine(_src, "D.cs"), "class D {}");   // non committato: conta lo stesso

            var after = await _scanner.ScanAsync(_project, _src, analyzed);
            CollectionAssert.AreEquivalent(new[] { "src/B.cs", "src/C.cs", "src/D.cs" }, after.Candidates.Select(f => f.LogicalPath).ToArray());
            Assert.AreEqual(4, after.AllFiles.Count);
            Assert.AreNotEqual(analyzed, after.HeadCommit);
        }

        [TestMethod]
        public async Task Fall_back_to_a_declared_hash_mode_outside_git()
        {
            var r = await _scanner.ScanAsync(_project, _src, previousCommit: null);
            Assert.AreEqual("hash", r.Mode);
            StringAssert.Contains(r.Reason, "repository");
            CollectionAssert.AreEquivalent(new[] { "src/A.cs", "src/B.cs" }, r.AllFiles.Select(f => f.LogicalPath).ToArray(), "bin/ è nella lista di esclusione");
            Assert.AreEqual(2, r.Candidates.Count);
        }
    }
}
