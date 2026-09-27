using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2ePostRun_Should
    {
        private string _root;
        private string _test;
        private string _scripts;

        private const string Script =
            "// stato: valido\n// sorgente: login.e2e.md, T1 — Uno\n// impronta-sorgente: da calcolare\n// generatore: mde-e2e v1\n// data: 2026-09-27 10:30\nnamespace MdeE2e.Login { }\n";

        private static readonly DateTime LongAgo = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-post-" + Guid.NewGuid().ToString("N"));
            _scripts = Path.Combine(_root, "test-e2e", "login.e2e", "scripts");
            Directory.CreateDirectory(_scripts);
            _test = Path.Combine(_root, "test-e2e", "login.e2e.md");
            WriteTest("Compare il testo \"ok\"");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt"), "x.password=Segreta!42\nx.pin=12\n");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "mappa.md"), "# mappa\n");
            File.WriteAllText(Path.Combine(_scripts, "login.T1.spec.cs"), Script);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_root, true);

        private void WriteTest(string check) => File.WriteAllText(_test,
            "---\ne2e:\n  baseUrl: https://example.org\n  siteMap: mappa.md\n  credentials: credenziali-x.txt\n  artifacts: login.e2e/\n---\n" +
            $"## T1 — Uno\n1. Scrivi {{{{x.password}}}} nel campo \"Password\"\n2. ✔ {check}\n\n## Artefatti\n\n## Esiti\n");

        private E2eRunItem Item()
        {
            var item = E2eRunPlanner.Plan(_test, _root, new DateTime(2026, 9, 27, 10, 30, 0)).Items.Single();
            Directory.CreateDirectory(item.RunFolder);
            return item;
        }

        private static readonly IReadOnlyDictionary<string, string> Secrets =
            new Dictionary<string, string> { ["x.password"] = "Segreta!42", ["x.pin"] = "12" };

        [TestMethod]
        public void Write_the_fingerprint_only_in_scripts_just_written_and_know_when_the_test_changes()
        {
            Assert.IsTrue(E2ePostRun.Scripts(Item()).Single().Stale, "'da calcolare' was never checked");

            var result = E2ePostRun.Process(Item(), Secrets, LongAgo);

            Assert.AreEqual(1, result.Fingerprinted.Count);
            var script = E2ePostRun.Scripts(Item()).Single();
            Assert.AreEqual("valido", script.State);
            Assert.IsFalse(script.Stale);

            // The test changes and the next run does not rewrite the script: it must stay stale (D17).
            WriteTest("Compare il testo \"OK\"");
            Assert.AreEqual(0, E2ePostRun.Process(Item(), Secrets, LongAgo).Fingerprinted.Count, "an old fingerprint is never overwritten");
            Assert.IsTrue(E2ePostRun.Scripts(Item()).Single().Stale, "the expected text changed");
        }

        [TestMethod]
        public void Treat_as_stale_a_script_written_by_another_version_of_the_skill()
        {
            E2ePostRun.Process(Item(), Secrets, LongAgo);
            Assert.IsFalse(E2ePostRun.Scripts(Item(), "mde-e2e v1").Single().Stale);
            Assert.IsTrue(E2ePostRun.Scripts(Item(), "mde-e2e v2").Single().Stale);
        }

        [TestMethod]
        public void Replace_a_credential_value_in_the_run_folder_and_only_report_it_elsewhere()
        {
            var item = Item();
            File.WriteAllText(Path.Combine(item.RunFolder, "report.md"), "2. Scrivi Segreta!42 nel campo \"Password\"\n");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "mappa.md"), "# mappa\nla password di prova è Segreta!42\n");

            var result = E2ePostRun.Process(item, Secrets, LongAgo);

            var report = result.Leaks.Single(l => Path.GetFileName(l.File) == "report.md");
            Assert.IsTrue(report.Replaced);
            Assert.AreEqual("2. Scrivi {{x.password}} nel campo \"Password\"\n", File.ReadAllText(Path.Combine(item.RunFolder, "report.md")));
            var map = result.Leaks.Single(l => Path.GetFileName(l.File) == "mappa.md");
            Assert.IsFalse(map.Replaced, "outside the run folder a value can be an ordinary word: reported, not rewritten");
            StringAssert.Contains(File.ReadAllText(Path.Combine(_root, "test-e2e", "mappa.md")), "Segreta!42");
            StringAssert.Contains(File.ReadAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt")), "Segreta!42", "the credentials file is left alone");
        }

        [TestMethod]
        public void Look_only_at_files_written_during_the_run()
        {
            File.WriteAllText(Path.Combine(_root, "test-e2e", "mappa.md"), "# mappa\nSegreta!42\n");
            File.SetLastWriteTimeUtc(Path.Combine(_root, "test-e2e", "mappa.md"), DateTime.UtcNow.AddHours(-1));
            Assert.AreEqual(0, E2ePostRun.Process(Item(), Secrets, DateTime.UtcNow.AddMinutes(-1)).Leaks.Count);
        }

        [TestMethod]
        public void Not_hunt_values_too_short_to_mean_anything()
        {
            var item = Item();
            File.WriteAllText(Path.Combine(item.RunFolder, "report.md"), "T12 superato in 12 secondi\n");
            Assert.AreEqual(0, E2ePostRun.Process(item, Secrets, LongAgo).Leaks.Count);
        }

        [TestMethod]
        public void Take_credential_values_out_of_replay_messages()
        {
            Assert.AreEqual("atteso '{{x.password}}', trovato ''", E2ePostRun.Redact("atteso 'Segreta!42', trovato ''", Secrets));
        }

        [TestMethod]
        public void Say_when_a_script_has_no_fingerprint_line_or_no_test()
        {
            File.WriteAllText(Path.Combine(_scripts, "login.T1.spec.cs"), "namespace X { }\n");
            File.WriteAllText(Path.Combine(_scripts, "login.T7.spec.cs"), Script);

            var problems = E2ePostRun.Process(Item(), Secrets, LongAgo).Problems;

            Assert.IsTrue(problems.Any(p => p.Contains("login.T1.spec.cs") && p.Contains("impronta-sorgente")), string.Join("\n", problems));
            Assert.IsTrue(problems.Any(p => p.Contains("login.T7.spec.cs") && p.Contains("T7")));
        }
    }
}
