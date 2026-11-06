using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il gestore di sorgenti si riconosce da solo (30/10/2026): git o SVN, con la revisione, il ramo e i file modificati in
    /// locale; fuori da tutti e due si dice. Sui comandi veri: SVN serve installato (<c>svn</c>, <c>svnadmin</c>, <c>svnversion</c>).
    /// </summary>
    [TestClass]
    public class SourceControlProbe_Should
    {
        private string _root;
        private NativeGitRunner _git;
        private SourceControlProbe _probe;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "mde-scm-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _git = new NativeGitRunner(NullLogger<NativeGitRunner>.Instance);
            _probe = new SourceControlProbe(_git, NullLogger<SourceControlProbe>.Instance);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_root, true); } catch { } }

        [TestMethod]
        public async Task Read_commit_branch_and_local_changes_from_git()
        {
            var project = Path.Combine(_root, "git"); var src = Path.Combine(project, "src");
            Directory.CreateDirectory(src);
            File.WriteAllText(Path.Combine(src, "A.java"), "class A {}");
            File.WriteAllText(Path.Combine(project, "Other.java"), "class O {}");
            foreach (var args in new[] { new[] { "init", "-q", "-b", "trunk" }, new[] { "add", "-A" }, new[] { "-c", "user.name=t", "-c", "user.email=t@t", "commit", "-q", "-m", "a" } })
                Assert.IsTrue((await _git.RunAsync(project, args)).Ok, string.Join(' ', args));
            File.AppendAllText(Path.Combine(src, "A.java"), "// x");
            File.AppendAllText(Path.Combine(project, "Other.java"), "// fuori dalla cartella");
            File.WriteAllText(Path.Combine(src, "New.java"), "class N {}");   // non nel gestore: non è una modifica locale

            var info = await _probe.ProbeAsync(src);
            Assert.AreEqual(SourceControlInfo.Git, info.Kind);
            Assert.AreEqual((await _git.RunAsync(project, new[] { "rev-parse", "HEAD" })).Stdout.Trim(), info.Revision);
            Assert.AreEqual("trunk", info.Branch);
            Assert.AreEqual(1, info.LocalChanges, "solo i file della cartella già nel gestore");
            Assert.IsNull(info.Note);
        }

        [TestMethod]
        public async Task Read_revision_url_and_local_changes_from_svn()
        {
            if (!Runs("svnadmin", "--version", "--quiet")) Assert.Inconclusive("SVN non installato su questa macchina");
            var repo = Path.Combine(_root, "repo"); var wc = Path.Combine(_root, "wc");
            var url = new Uri(repo).AbsoluteUri;
            Svn("svnadmin", "create", repo);
            Svn("svn", "mkdir", "-q", "-m", "init", url + "/trunk", url + "/trunk/prog");
            Svn("svn", "checkout", "-q", url + "/trunk", wc);
            var prog = Path.Combine(wc, "prog");
            File.WriteAllText(Path.Combine(prog, "A.java"), "class A {}");
            Svn("svn", "add", "-q", Path.Combine(prog, "A.java"));
            Svn("svn", "commit", "-q", "-m", "a", wc);
            Svn("svn", "update", "-q", wc);
            File.AppendAllText(Path.Combine(prog, "A.java"), "// x");
            File.WriteAllText(Path.Combine(prog, "New.java"), "class N {}");

            var info = await _probe.ProbeAsync(prog);
            Assert.AreEqual(SourceControlInfo.Svn, info.Kind);
            Assert.AreEqual("2", info.Revision);
            Assert.AreEqual("^/trunk/prog", info.Branch);
            Assert.AreEqual(1, info.LocalChanges, "il file non aggiunto non conta");
            Assert.IsNull(info.Note);
        }

        [TestMethod]
        public async Task Say_when_there_is_no_source_control()
        {
            var info = await _probe.ProbeAsync(_root);
            Assert.AreEqual(SourceControlInfo.None, info.Kind);
            Assert.IsNull(info.Revision);
            Assert.IsNotNull(info.Note, "si dice, non si tace");
        }

        [TestMethod]
        public void Read_svnversion_and_svn_status_as_svn_writes_them()
        {
            Assert.AreEqual(("1240", false), SourceControlProbe.ParseSvnVersion("1240"));
            Assert.AreEqual(("1230:1240", true), SourceControlProbe.ParseSvnVersion("1230:1240MS\n"), "copia mista, modificata e switched");
            Assert.AreEqual(((string)null, false), SourceControlProbe.ParseSvnVersion("Unversioned directory"));
            Assert.AreEqual(((string)null, false), SourceControlProbe.ParseSvnVersion("exported"));
            Assert.AreEqual(2, SourceControlProbe.CountSvnStatus("M       prog/A.java\nA       prog/B.java\n\nPerforming status on external item at 'lib':\nX       lib\n"));
            Assert.AreEqual(2, SourceControlProbe.CountPorcelain(" M a.java\0R  new.java\0old.java\0"), "una rinomina è un file");
        }

        private static bool Runs(string command, params string[] args)
        {
            try { using var p = Start(command, args); p.WaitForExit(); return p.ExitCode == 0; }
            catch (System.ComponentModel.Win32Exception) { return false; }
        }

        private static void Svn(string command, params string[] args)
        {
            using var p = Start(command, args);
            var err = p.StandardError.ReadToEnd();
            p.WaitForExit();
            Assert.AreEqual(0, p.ExitCode, $"{command} {string.Join(' ', args)}: {err}");
        }

        private static Process Start(string command, string[] args)
        {
            var psi = new ProcessStartInfo(command) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            foreach (var a in args) psi.ArgumentList.Add(a);
            return Process.Start(psi);
        }
    }
}
