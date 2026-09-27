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
        private string _run;

        private const string Script =
            "// stato: valido\n// sorgente: login.e2e.md, T1 — Uno\n// impronta-sorgente: da calcolare\n// generatore: mde-e2e v1\n// data: 2026-09-27 10:30\nnamespace MdeE2e.Login { }\n";

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-post-" + Guid.NewGuid().ToString("N"));
            _scripts = Path.Combine(_root, "test-e2e", "login.e2e", "scripts");
            _run = Path.Combine(_root, "test-e2e", "login.e2e", "esecuzioni", "2026-09-27_10-30");
            Directory.CreateDirectory(_scripts);
            Directory.CreateDirectory(_run);
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

        private E2eRunItem Item() => E2eRunPlanner.Plan(_test, _root, new DateTime(2026, 9, 27, 10, 30, 0)).Items.Single();

        private static readonly IReadOnlyDictionary<string, string> Secrets =
            new Dictionary<string, string> { ["x.password"] = "Segreta!42", ["x.pin"] = "12" };

        [TestMethod]
        public void Write_the_test_fingerprint_in_its_script_and_know_when_the_test_changes()
        {
            Assert.IsTrue(E2ePostRun.Scripts(Item()).Single().Stale, "'da calcolare' was never checked");

            var result = E2ePostRun.Process(Item(), Secrets);

            Assert.AreEqual(1, result.Fingerprinted.Count);
            var script = E2ePostRun.Scripts(Item()).Single();
            Assert.AreEqual(1, script.TestNumber);
            Assert.AreEqual("valido", script.State);
            Assert.AreEqual("mde-e2e v1", script.Generator);
            Assert.IsFalse(script.Stale);
            StringAssert.Contains(File.ReadAllText(script.Path), "// impronta-sorgente: " + Item().Preflight.Document.Tests[0].Fingerprint);

            WriteTest("Compare il testo \"OK\"");
            Assert.IsTrue(E2ePostRun.Scripts(Item()).Single().Stale, "the expected text changed");
        }

        [TestMethod]
        public void Put_the_key_back_where_a_credential_value_ended_up()
        {
            File.WriteAllText(Path.Combine(_run, "report.md"), "2. Scrivi Segreta!42 nel campo \"Password\"\n");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "mappa.md"), "# mappa\nla password di prova è Segreta!42\n");

            var result = E2ePostRun.Process(Item(), Secrets);

            CollectionAssert.AreEquivalent(new[] { "report.md", "mappa.md" }, result.Leaks.Select(l => Path.GetFileName(l.File)).ToArray());
            Assert.IsTrue(result.Leaks.All(l => l.Key == "x.password"));
            Assert.AreEqual("2. Scrivi {{x.password}} nel campo \"Password\"\n", File.ReadAllText(Path.Combine(_run, "report.md")));
            StringAssert.Contains(File.ReadAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt")), "Segreta!42", "the credentials file is left alone");
        }

        [TestMethod]
        public void Not_hunt_values_too_short_to_mean_anything()
        {
            File.WriteAllText(Path.Combine(_run, "report.md"), "T12 superato in 12 secondi\n");
            Assert.AreEqual(0, E2ePostRun.Process(Item(), Secrets).Leaks.Count);
        }

        [TestMethod]
        public void Say_when_a_script_has_no_fingerprint_line_or_no_test()
        {
            File.WriteAllText(Path.Combine(_scripts, "login.T1.spec.cs"), "namespace X { }\n");
            File.WriteAllText(Path.Combine(_scripts, "login.T7.spec.cs"), Script);

            var problems = E2ePostRun.Process(Item(), Secrets).Problems;

            Assert.IsTrue(problems.Any(p => p.Contains("login.T1.spec.cs") && p.Contains("impronta-sorgente")), string.Join("\n", problems));
            Assert.IsTrue(problems.Any(p => p.Contains("login.T7.spec.cs") && p.Contains("T7")));
        }
    }
}
