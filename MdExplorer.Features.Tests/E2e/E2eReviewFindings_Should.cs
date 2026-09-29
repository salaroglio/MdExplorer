using MdExplorer.Features.E2e;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.Tests.E2e
{
    /// <summary>
    /// One test per defect found by the review of 27/09/2026 (sprint 2026-09-26-Test-E2E-Da-Markdown): each
    /// failed before its fix.
    /// </summary>
    [TestClass]
    public class E2eReviewFindings_Should
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "e2e-review-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_root, "test-e2e"));
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_root, true);

        private string Test(string frontMatterExtra = "", string credentials = "credenziali-x.txt", string key = "x.password")
        {
            var path = Path.Combine(_root, "test-e2e", "login.e2e.md");
            File.WriteAllText(path,
                $"---\ne2e:\n  baseUrl: https://example.org\n  credentials: {credentials}\n  artifacts: login.e2e/\n{frontMatterExtra}---\n" +
                $"## T1 — Uno\n1. Scrivi {{{{{key}}}}} nel campo \"Password\"\n2. ✔ Compare il testo \"ok\"\n\n## Artefatti\n\n## Esiti\n");
            return path;
        }

        [TestMethod]
        public void Keep_reading_the_e2e_block_after_a_comment_at_column_zero()
        {
            var markdown = "---\ne2e:\n  baseUrl: https://example.org\n# una nota\n  credentials: c.txt\n  run:\n    headless: false\n---\n";
            var mapping = E2eFrontMatter.ReadMapping(markdown, "t.e2e.md");
            Assert.IsTrue(mapping.Children.Keys.Select(k => k.ToString()).Contains("credentials"));
            Assert.AreEqual(false, E2eFrontMatter.ReadRunSettings(markdown, "t.e2e.md").Headless);

            var written = E2eFrontMatter.WriteRunSettings(markdown, new E2eRunSettings(null, null, true, false), "t.e2e.md");
            Assert.AreEqual(1, written.Split('\n').Count(l => l.Trim() == "run:"), "no second run: block");
        }

        [TestMethod]
        public void Read_the_emoji_check_mark_and_typographic_quotes_as_a_check()
        {
            var step = E2eTestParser.ReadStep(1, "✔️ Compare il testo “Benvenuto”");
            Assert.AreEqual(E2eCheckKind.TextVisible, step.Check);
            Assert.AreEqual("Benvenuto", step.Expected);
        }

        [TestMethod]
        public void Report_numbers_that_do_not_fit_instead_of_crashing()
        {
            var markdown = "---\ne2e:\n  baseUrl: https://example.org\n---\n## T99999999999 — troppo\n1. a\n\n## Artefatti\n\n## Esiti\n";
            var document = E2eTestParser.Parse(markdown, "t.e2e.md");
            Assert.IsTrue(document.Problems.Any(p => p.Contains("nessun test")), string.Join("\n", document.Problems));
        }

        [TestMethod]
        public void Refuse_credential_keys_the_playwright_server_would_drop()
        {
            var test = Test(key: "utente@sito");
            File.WriteAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt"), "utente@sito=u\n");
            var result = E2ePreflight.Check(test);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("'utente@sito'") && e.Contains("non accetta")), string.Join("\n", result.Errors));
        }

        [TestMethod]
        public void Refuse_a_credentials_file_outside_the_project()
        {
            var outside = Path.Combine(Path.GetTempPath(), "e2e-review-fuori-" + Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllText(outside, "x.password=segreta\n");
            try
            {
                var plan = E2eRunPlanner.Plan(Test(credentials: outside), _root, DateTime.Now);
                Assert.IsFalse(plan.CanRun);
                Assert.IsTrue(plan.Errors.Any(e => e.Contains("porta fuori dal progetto")), string.Join("\n", plan.Errors));
                Assert.AreEqual(0, plan.Secrets.Count, "values of a file outside the project are never read");
            }
            finally
            {
                File.Delete(outside);
            }
        }

        [TestMethod]
        public void Give_a_second_launch_of_the_same_minute_its_own_run_folder()
        {
            var test = Test();
            File.WriteAllText(Path.Combine(_root, "test-e2e", "credenziali-x.txt"), "x.password=segreta\n");
            var now = new DateTime(2026, 9, 27, 10, 30, 0);
            var first = E2eRunPlanner.Plan(test, _root, now).Items.Single().RunFolder;
            Directory.CreateDirectory(first);
            var second = E2eRunPlanner.Plan(test, _root, now).Items.Single().RunFolder;
            Assert.AreEqual(first + "-2", second);
        }

        [TestMethod]
        public void Accept_a_project_on_a_filesystem_root()
        {
            var root = Path.GetPathRoot(_root)!;
            Assert.IsTrue(E2eRunPlanner.IsInside(_root, root));
            Assert.IsTrue(E2eRunPlanner.IsInside(root, root));
            Assert.IsFalse(E2eRunPlanner.IsInside("/elsewhere", _root));
        }
    }
}
