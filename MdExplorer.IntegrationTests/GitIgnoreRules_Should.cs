using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.GitIgnore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>Sprint 2026-10-21-Watcher-Selettivo-E-Gitignore, W0: che cosa git ignora, calcolato da git vero.</summary>
    [TestClass]
    public class GitIgnoreRules_Should
    {
        private string _base;

        [TestInitialize]
        public void Setup()
        {
            _base = Path.Combine(Path.GetTempPath(), "mde-gitignore", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_base);
        }

        [TestCleanup]
        public void Cleanup() { try { Directory.Delete(_base, true); } catch { } }

        private static void Git(string cwd, string args)
        {
            var p = Process.Start(new ProcessStartInfo("git", args) { WorkingDirectory = cwd, RedirectStandardOutput = true, RedirectStandardError = true });
            p!.WaitForExit();
            Assert.AreEqual(0, p.ExitCode, $"git {args}: {p.StandardError.ReadToEnd()}");
        }

        private static void Write(string path, string text = "x")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
        }

        private string Repo(string name)
        {
            var dir = Path.Combine(_base, name);
            Directory.CreateDirectory(dir);
            Git(dir, "init -q");
            Git(dir, "config user.email t@t");
            Git(dir, "config user.name t");
            return dir;
        }

        [TestMethod]
        public void Ignore_what_git_ignores_with_nested_gitignores_but_not_tracked_files()
        {
            var repo = Repo("doc");
            Write(Path.Combine(repo, ".gitignore"), "node_modules/\n*.log\nbuild/\nnote-private.md\n");
            Write(Path.Combine(repo, "docs", ".gitignore"), "bozze/\n");
            Write(Path.Combine(repo, "README.md"));
            Write(Path.Combine(repo, "docs", "a.md"));
            Write(Path.Combine(repo, "docs", "bozze", "b.md"));
            Write(Path.Combine(repo, "node_modules", "x", "y.js"));
            Write(Path.Combine(repo, "build", "out.md"));
            Write(Path.Combine(repo, "app.log"));
            Write(Path.Combine(repo, "note-private.md"));
            Write(Path.Combine(repo, "tracciato.log"));
            Git(repo, "add -f tracciato.log");
            Git(repo, "add README.md docs/a.md .gitignore docs/.gitignore");
            Git(repo, "commit -q -m init");

            var rules = new GitIgnoreRules();
            bool Ignored(string rel) => rules.IsIgnored(Path.Combine(repo, rel.Replace('/', Path.DirectorySeparatorChar)), repo);
            Assert.IsTrue(Ignored("node_modules"));
            Assert.IsTrue(Ignored("node_modules/x/y.js"), "dentro una cartella ignorata");
            Assert.IsTrue(Ignored("build/out.md"));
            Assert.IsTrue(Ignored("docs/bozze/b.md"), ".gitignore annidato");
            Assert.IsTrue(Ignored("app.log"));
            Assert.IsTrue(Ignored("note-private.md"), "un .md ignorato da git non è documentazione");
            Assert.IsFalse(Ignored("tracciato.log"), "tracciato: git non lo ignora, anche se corrisponde a una regola");
            Assert.IsFalse(Ignored("README.md"));
            Assert.IsFalse(Ignored("docs/a.md"));
            Assert.IsFalse(Ignored("docs"));
            Assert.IsFalse(Ignored("nuovo/non-ignorato.md"), "un file nuovo non ignorato");
            Assert.IsFalse(rules.IsIgnored(Path.Combine(_base, "altro", "x.md"), repo), "fuori dal progetto");
        }

        [TestMethod]
        public void Apply_the_own_rules_of_each_submodule()
        {
            var inner = Repo("libreria");
            Write(Path.Combine(inner, ".gitignore"), "dist/\n");
            Write(Path.Combine(inner, "lib.md"));
            Git(inner, "add .");
            Git(inner, "commit -q -m lib");
            var repo = Repo("doc");
            Write(Path.Combine(repo, "README.md"));
            Git(repo, "add README.md");
            Git(repo, "commit -q -m init");
            Git(repo, $"-c protocol.file.allow=always submodule add -q \"{inner}\" vendor/libreria");
            Git(repo, "commit -q -m sub");
            Write(Path.Combine(repo, "vendor", "libreria", "dist", "bundle.md"));

            var rules = new GitIgnoreRules();
            Assert.IsTrue(rules.IsIgnored(Path.Combine(repo, "vendor", "libreria", "dist", "bundle.md"), repo), "il .gitignore del submodule");
            Assert.IsFalse(rules.IsIgnored(Path.Combine(repo, "vendor", "libreria", "lib.md"), repo));
        }

        [TestMethod]
        public void Have_no_git_rules_outside_a_repository()
        {
            var plain = Path.Combine(_base, "senza-git");
            Write(Path.Combine(plain, ".gitignore"), "*.md\n");
            Write(Path.Combine(plain, "a.md"));
            var snapshot = new GitIgnoreRules().Snapshot(plain);
            Assert.IsFalse(snapshot.IsGit);
            Assert.IsFalse(snapshot.IsIgnored(Path.Combine(plain, "a.md")), "senza repository un .gitignore non vale");
        }

        [TestMethod]
        public void Work_on_a_project_that_is_a_subfolder_or_sits_in_an_ignored_folder_of_a_bigger_repository()
        {
            var big = Repo("casa");
            Write(Path.Combine(big, ".gitignore"), "progetti/\nsub/doc/cache/\n");
            Write(Path.Combine(big, "sub", "doc", "a.md"));
            Write(Path.Combine(big, "sub", "doc", "cache", "c.md"));
            var rules = new GitIgnoreRules();
            var sub = Path.Combine(big, "sub", "doc");
            Assert.IsTrue(rules.IsIgnored(Path.Combine(sub, "cache", "c.md"), sub));
            Assert.IsFalse(rules.IsIgnored(Path.Combine(sub, "a.md"), sub));

            // un progetto dentro una cartella che il repository di sopra ignora: le sue cartelle non sono «ignorate tutte»
            var inIgnored = Path.Combine(big, "progetti", "doc");
            Write(Path.Combine(inIgnored, "a.md"));
            Assert.IsFalse(rules.IsIgnored(Path.Combine(inIgnored, "a.md"), inIgnored), "la radice del progetto non è mai ignorata");
        }

        [TestMethod]
        public void Recompute_after_invalidate_when_the_rules_change()
        {
            var repo = Repo("doc");
            Write(Path.Combine(repo, "bozze", "a.md"));
            var rules = new GitIgnoreRules();
            Assert.IsFalse(rules.IsIgnored(Path.Combine(repo, "bozze", "a.md"), repo));
            Write(Path.Combine(repo, ".gitignore"), "bozze/\n");
            Assert.IsFalse(rules.IsIgnored(Path.Combine(repo, "bozze", "a.md"), repo), "in cache finché nessuno invalida");
            rules.Invalidate(repo);
            Assert.IsTrue(rules.IsIgnored(Path.Combine(repo, "bozze", "a.md"), repo));
        }
    }
}
