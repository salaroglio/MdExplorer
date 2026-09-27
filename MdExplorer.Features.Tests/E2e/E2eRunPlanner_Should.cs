using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    [TestClass]
    public class E2eRunPlanner_Should
    {
        private static readonly DateTime Now = new(2026, 9, 27, 10, 30, 0);
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-plan-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_root, true);

        private string Test(string relative, string credentials = "credenziali-x.txt", string key = "x.password", string run = null)
        {
            var path = Path.Combine(_root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var name = Path.GetFileName(path).Replace(".e2e.md", "");
            File.WriteAllText(path,
                "---\ne2e:\n  baseUrl: https://example.org\n  siteMap: mappa.md\n" +
                $"  credentials: {credentials}\n  artifacts: {name}.e2e/\n" + (run == null ? "" : "  run:\n" + run) + "---\n" +
                $"## T1 — Uno\n1. Scrivi {{{{{key}}}}} nel campo \"Password\"\n2. ✔ Compare il testo \"ok\"\n\n## Artefatti\n\n## Esiti\n");
            return path;
        }

        private void Credentials(string relative, string content) =>
            File.WriteAllText(Path.Combine(_root, relative), content);

        [TestMethod]
        public void Plan_a_single_test_with_its_run_folder_and_the_two_line_prompt()
        {
            var test = Test("test-e2e/login.e2e.md");
            Credentials("test-e2e/credenziali-x.txt", "x.password=segreta\n");

            var plan = E2eRunPlanner.Plan(test, _root, Now);

            Assert.IsTrue(plan.CanRun, string.Join("\n", plan.Errors));
            var item = plan.Items.Single();
            Assert.AreEqual("test-e2e/login.e2e.md", item.RelativeTestFile);
            Assert.AreEqual("test-e2e/login.e2e/esecuzioni/2026-09-27_10-30", item.RelativeRunFolder);
            Assert.AreEqual("Esegui i test di test-e2e/login.e2e.md.\nCartella dell'esecuzione: test-e2e/login.e2e/esecuzioni/2026-09-27_10-30/\n", item.Prompt);
            Assert.AreEqual("segreta", plan.Secrets["x.password"]);
            CollectionAssert.AreEqual(new[] { Path.Combine(_root, "test-e2e", "credenziali-x.txt") }, plan.DeniedPaths.ToArray());
        }

        [TestMethod]
        public void Take_every_test_under_a_folder_but_not_hidden_or_build_folders()
        {
            Test("test-e2e/a.e2e.md");
            Test("test-e2e/sotto/b.e2e.md", credentials: "../credenziali-x.txt");
            Test("test-e2e/.nascosta/c.e2e.md");
            Test("test-e2e/bin/d.e2e.md");
            Credentials("test-e2e/credenziali-x.txt", "x.password=segreta\n");

            var plan = E2eRunPlanner.Plan(Path.Combine(_root, "test-e2e"), _root, Now);

            Assert.IsTrue(plan.CanRun, string.Join("\n", plan.Errors));
            CollectionAssert.AreEqual(new[] { "test-e2e/a.e2e.md", "test-e2e/sotto/b.e2e.md" }, plan.Items.Select(i => i.RelativeTestFile).ToArray());
        }

        [TestMethod]
        public void Refuse_the_same_key_with_two_values_in_one_launch()
        {
            Test("test-e2e/a.e2e.md", credentials: "credenziali-a.txt");
            Test("test-e2e/b.e2e.md", credentials: "credenziali-b.txt");
            Credentials("test-e2e/credenziali-a.txt", "x.password=uno\n");
            Credentials("test-e2e/credenziali-b.txt", "x.password=due\n");

            var plan = E2eRunPlanner.Plan(Path.Combine(_root, "test-e2e"), _root, Now);

            Assert.IsFalse(plan.CanRun);
            StringAssert.Contains(plan.Errors.Single(), "La chiave 'x.password' ha valori diversi");
        }

        [TestMethod]
        public void Refuse_mixing_hidden_and_visible_browser_in_the_markagent_tab()
        {
            Test("test-e2e/a.e2e.md", run: "    dedicatedSession: false\n    headless: true\n");
            Test("test-e2e/b.e2e.md", run: "    dedicatedSession: false\n    headless: false\n");
            Test("test-e2e/c.e2e.md", run: "    dedicatedSession: true\n    headless: false\n");
            Credentials("test-e2e/credenziali-x.txt", "x.password=segreta\n");

            var plan = E2eRunPlanner.Plan(Path.Combine(_root, "test-e2e"), _root, Now);

            var error = plan.Errors.Single();
            StringAssert.Contains(error, "test-e2e/a.e2e.md (nascosto)");
            StringAssert.Contains(error, "test-e2e/b.e2e.md (visibile)");
            Assert.IsFalse(error.Contains("c.e2e.md"), "a dedicated session has its own browser");
        }

        [TestMethod]
        public void Pass_on_the_preflight_errors_of_every_file()
        {
            Test("test-e2e/a.e2e.md");
            Credentials("test-e2e/credenziali-x.txt", "altra.chiave=1\n");

            var plan = E2eRunPlanner.Plan(Path.Combine(_root, "test-e2e"), _root, Now);

            Assert.IsFalse(plan.CanRun);
            StringAssert.Contains(plan.Errors.Single(), "la chiave 'x.password' non c'è");
        }

        [TestMethod]
        public void Say_when_a_folder_has_no_tests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "vuota"));
            var plan = E2eRunPlanner.Plan(Path.Combine(_root, "vuota"), _root, Now);
            StringAssert.Contains(plan.Errors.Single(), "non ci sono file .e2e.md");
        }

        [TestMethod]
        public void Write_secrets_the_way_the_playwright_server_reads_them_back()
        {
            // The quoting was verified against the real server on 27/09/2026: unquoted, dotenv cuts at '#'
            // and trims the spaces; quoted as below, the typed characters were exactly the values.
            var file = Path.Combine(_root, "segreti", "lancio.env");
            E2eRunPlanner.WriteSecretsFile(file, new Dictionary<string, string>
            {
                ["k1"] = "pa ss#word",
                ["k2"] = " lead",
                ["k3"] = "has\"dq#",
                ["k4"] = "O'Brien#1",
                ["k5"] = "plain.value=with=equals",
            });

            var lines = File.ReadAllLines(file).Skip(1).ToArray();
            CollectionAssert.AreEqual(new[]
            {
                "k1='pa ss#word'", "k2=' lead'", "k3='has\"dq#'", "k4=\"O'Brien#1\"", "k5=plain.value=with=equals",
            }, lines);
            if (!OperatingSystem.IsWindows())
                Assert.AreEqual(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(file));

            Assert.ThrowsException<InvalidOperationException>(() =>
                E2eRunPlanner.WriteSecretsFile(file, new Dictionary<string, string> { ["k"] = "a'b\"c #" }));
        }

        [TestMethod]
        public void Start_the_playwright_server_with_electron_as_node_and_always_an_output_folder()
        {
            var ready = new E2ePrerequisitesReport
            {
                Electron = new E2eRequirement("electron", true, "", null, false),
                Browser = new E2eRequirement("browser", true, "", null, false),
                PlaywrightMcp = new E2eRequirement("playwright-mcp", true, "", null, true),
                BrowserArgument = "msedge",
                ElectronPath = "/app/MdExplorer",
                PlaywrightMcpCli = "/tools/cli.js",
            };

            var server = E2ePlaywrightServer.For(ready, headless: false, secretsFile: "/s/lancio.env", outputDir: "/diag");

            Assert.AreEqual("/app/MdExplorer", server.Command);
            CollectionAssert.AreEqual(new[]
            {
                "/tools/cli.js", "--browser", "msedge", "--isolated", "--codegen", "csharp", "--output-dir", "/diag", "--secrets", "/s/lancio.env",
            }, server.Args.ToArray());
            Assert.AreEqual("1", server.Env["ELECTRON_RUN_AS_NODE"]);
            Assert.IsTrue(E2ePlaywrightServer.For(ready, true, null, "/diag").Args.Contains("--headless"));
            Assert.ThrowsException<ArgumentException>(() => E2ePlaywrightServer.For(ready, true, null, ""));
        }
    }
}
