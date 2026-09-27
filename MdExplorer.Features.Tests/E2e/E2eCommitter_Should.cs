using LibGit2Sharp;
using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eCommitter_Should
    {
        private string _root;
        private string _test;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-commit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "test-e2e", "login.e2e", "esecuzioni", "2026-09-27_10-30"));
            Repository.Init(_root);
            using (var repo = new Repository(_root))
            {
                repo.Config.Set("user.name", "Prova", ConfigurationLevel.Local);
                repo.Config.Set("user.email", "prova@example.org", ConfigurationLevel.Local);
            }
            File.WriteAllText(Path.Combine(_root, ".gitignore"), "credenziali-*.txt\n");
            _test = Path.Combine(_root, "test-e2e", "login.e2e.md");
            File.WriteAllText(_test,
                "---\ne2e:\n  baseUrl: https://example.org\n  siteMap: mappa.md\n  credentials: credenziali-x.txt\n  artifacts: login.e2e/\n  run:\n    commitAfterRun: true\n---\n" +
                "## T1 — Uno\n1. Scrivi {{x.password}} nel campo \"Password\"\n2. ✔ Compare il testo \"ok\"\n\n## Artefatti\n\n## Esiti\n");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt"), "x.password=segreta\n");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "mappa.md"), "# mappa\n");
            var run = Path.Combine(_root, "test-e2e", "login.e2e", "esecuzioni", "2026-09-27_10-30");
            File.WriteAllText(Path.Combine(run, "report.md"), "# Esecuzione\n\n## T1 — Uno — ✅ superato\n\n## T2 — Due — ❌ l'applicazione sbaglia\n");
            File.WriteAllBytes(Path.Combine(run, "T1-02-ok.png"), new byte[] { 1, 2, 3 });
            File.WriteAllText(Path.Combine(_root, "appunti-utente.md"), "non c'entra\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var f in Directory.GetFiles(_root, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(_root, true);
        }

        private E2eRunItem Item() =>
            E2eRunPlanner.Plan(_test, _root, new DateTime(2026, 9, 27, 10, 30, 0)).Items.Single();

        [TestMethod]
        public void Commit_only_what_the_run_produced_with_the_outcomes_in_the_message()
        {
            var result = E2eCommitter.Commit(Item());

            Assert.IsTrue(result.Committed, result.Reason);
            Assert.AreEqual("test e2e: test-e2e/login.e2e.md — 1 ✅, 1 ❌ (esecuzione 2026-09-27_10-30)", result.Message);
            using var repo = new Repository(_root);
            var files = repo.Head.Tip.Tree.SelectMany(Flatten).ToList();
            CollectionAssert.IsSubsetOf(new[]
            {
                ".gitignore", "test-e2e/login.e2e.md", "test-e2e/mappa.md",
                "test-e2e/login.e2e/esecuzioni/2026-09-27_10-30/report.md", "test-e2e/login.e2e/esecuzioni/2026-09-27_10-30/T1-02-ok.png",
            }, files);
            Assert.IsFalse(files.Contains("test-e2e/credenziali-x.txt"), "credentials are ignored by git and never staged");
            Assert.IsFalse(files.Contains("appunti-utente.md"), "the user's own changes stay out");
        }

        [TestMethod]
        public void Not_commit_over_what_the_user_already_staged()
        {
            using (var repo = new Repository(_root)) LibGit2Sharp.Commands.Stage(repo, "appunti-utente.md");

            var result = E2eCommitter.Commit(Item());

            Assert.IsFalse(result.Committed);
            StringAssert.Contains(result.Reason, "appunti-utente.md");
        }

        [TestMethod]
        public void Say_when_there_is_nothing_new_to_commit()
        {
            Assert.IsTrue(E2eCommitter.Commit(Item()).Committed);
            var again = E2eCommitter.Commit(Item());
            Assert.IsFalse(again.Committed);
            StringAssert.Contains(again.Reason, "nessuna modifica");
        }

        private static System.Collections.Generic.IEnumerable<string> Flatten(TreeEntry entry) =>
            entry.TargetType == TreeEntryTargetType.Tree
                ? ((Tree)entry.Target).SelectMany(Flatten)
                : new[] { entry.Path.Replace('\\', '/') };
    }
}
