using LibGit2Sharp;
using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2ePreflight_Should
    {
        private string _root;
        private string _test;

        private const string TestFile =
            "---\ne2e:\n  baseUrl: https://example.org\n  siteMap: mappa-sito-x.md\n  credentials: credenziali-x.txt\n  artifacts: login.e2e/\n---\n" +
            "## T1 — Login\n1. Scrivi {{x.utente}} nel campo \"Username\"\n2. Scrivi {{x.password}} nel campo \"Password\"\n3. ✔ Compare il testo \"ok\"\n" +
            "\n## Artefatti\n\n## Esiti\n";

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-preflight-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "test-e2e"));
            _test = Path.Combine(_root, "test-e2e", "login.e2e.md");
            File.WriteAllText(_test, TestFile);
            File.WriteAllText(Path.Combine(_root, "test-e2e", "mappa-sito-x.md"), "# mappa\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var f in Directory.GetFiles(_root, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(_root, true);
        }

        private void Credentials(string content) => File.WriteAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt"), content);

        [TestMethod]
        public void Let_a_complete_test_run()
        {
            Credentials("# prova\nx.utente=u\nx.password=p\n");
            var result = E2ePreflight.Check(_test);
            Assert.IsTrue(result.CanRun, string.Join("\n", result.Errors));
            Assert.AreEqual(0, result.Warnings.Count);
        }

        [TestMethod]
        public void Warn_about_a_test_the_script_cannot_check()
        {
            // P7 (28/09/2026): the user's T1 had no check, and its replay passed while the page undid the filters.
            Credentials("x.utente=u\nx.password=p\n");
            File.WriteAllText(_test, TestFile.Replace("\n## Artefatti", "\n## T2 — Senza verifiche\n1. Apri `/`\n2. ✔ La pagina sembra in ordine\n\n## Artefatti"));
            var result = E2ePreflight.Check(_test);
            Assert.IsTrue(result.CanRun, "a warning, not an error");
            Assert.AreEqual(1, result.Warnings.Count, string.Join("\n", result.Warnings));
            StringAssert.Contains(result.Warnings[0], "T2 non ha verifiche ✔");
        }

        [TestMethod]
        public void Stop_on_a_key_the_credentials_file_does_not_have()
        {
            // The Playwright server would type the key's name, in silence (verified 27/09/2026).
            Credentials("x.utente=u\n");
            var result = E2ePreflight.Check(_test);
            Assert.IsFalse(result.CanRun);
            Assert.AreEqual(1, result.Errors.Count);
            StringAssert.Contains(result.Errors[0], "la chiave 'x.password' non c'è in 'credenziali-x.txt'");
        }

        [TestMethod]
        public void Stop_when_the_credentials_file_is_missing_listing_the_keys_it_needs()
        {
            var result = E2ePreflight.Check(_test);
            Assert.IsFalse(result.CanRun);
            StringAssert.Contains(result.Errors.Single(), "x.utente, x.password");
        }

        [TestMethod]
        public void Only_warn_when_the_site_map_does_not_exist_yet()
        {
            Credentials("x.utente=u\nx.password=p\n");
            File.Delete(Path.Combine(_root, "test-e2e", "mappa-sito-x.md"));
            var result = E2ePreflight.Check(_test);
            Assert.IsTrue(result.CanRun);
            StringAssert.Contains(result.Warnings.Single(), "non esiste ancora");
        }

        [TestMethod]
        public void Stop_when_git_would_commit_the_credentials()
        {
            Credentials("x.utente=u\nx.password=p\n");
            Repository.Init(_root);

            var notIgnored = E2ePreflight.Check(_test);
            StringAssert.Contains(notIgnored.Errors.Single(), "non è escluso da git");

            File.WriteAllText(Path.Combine(_root, ".gitignore"), "credenziali-*.txt\n");
            Assert.IsTrue(E2ePreflight.Check(_test).CanRun, "ignored by .gitignore");

            using (var repo = new Repository(_root))
            {
                LibGit2Sharp.Commands.Stage(repo, "test-e2e/credenziali-x.txt", new StageOptions { IncludeIgnored = true });
            }
            var tracked = E2ePreflight.Check(_test);
            StringAssert.Contains(tracked.Errors.Single(), "è già nel repository git");
        }

        [TestMethod]
        public void Refuse_a_file_that_is_not_a_test()
        {
            var other = Path.Combine(_root, "test-e2e", "note.md");
            File.WriteAllText(other, "# note\n");
            var result = E2ePreflight.Check(other);
            Assert.IsFalse(result.CanRun);
            Assert.IsNull(result.Document);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("deve finire in '.e2e.md'")));
        }
    }
}
