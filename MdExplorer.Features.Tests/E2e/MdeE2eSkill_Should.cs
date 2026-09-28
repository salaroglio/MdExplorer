using Markdig;
using Markdig.Syntax;
using MdExplorer.Features.Tests.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Tests.E2e
{
    /// <summary>
    /// The examples of the mde-e2e skill must agree with each other: an AI copies them, so a script that
    /// comments a different step, a credential key the credentials file does not have, or a value that leaks
    /// into another file would be copied as well. An example is a fenced block <c>esempio=&lt;id&gt;</c> of
    /// <c>MdExplorer/skills/mde-e2e/SKILL.md</c>. That the C# examples compile and pass against the real site
    /// was verified by hand (sprint 2026-09-26-Test-E2E-Da-Markdown): this project has no Playwright.
    /// </summary>
    [TestClass]
    public class MdeE2eSkill_Should
    {
        private static readonly string[] ExpectedExamples =
            { "credenziali", "csproj", "esiti", "mappa", "report", "runsettings", "scheda", "script", "supporto", "test" };

        private static string SkillPath() =>
            Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "skills", "mde-e2e", "SKILL.md");

        private static Dictionary<string, string> Examples()
        {
            var document = Markdown.Parse(File.ReadAllText(SkillPath()), new MarkdownPipelineBuilder().UseYamlFrontMatter().Build());
            return document.Descendants<FencedCodeBlock>()
                .Where(b => (b.Arguments ?? "").StartsWith("esempio="))
                .ToDictionary(b => b.Arguments.Substring("esempio=".Length).Trim(), b => b.Lines.ToString());
        }

        private static Dictionary<string, string> Credentials() =>
            Examples()["credenziali"].Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#"))
                .Select(l => l.Split('=', 2))
                .ToDictionary(p => p[0], p => p[1]);

        /// <summary>The lines of test T1 in the <c>test</c> example, without the number.</summary>
        private static string[] StepsOfT1()
        {
            var test = Examples()["test"];
            var t1 = Regex.Match(test, @"^## T1 — .*?\n(.*?)(?=^## )", RegexOptions.Singleline | RegexOptions.Multiline).Groups[1].Value;
            return Regex.Matches(t1, @"^\d+\. (.+)$", RegexOptions.Multiline).Select(m => m.Groups[1].Value.Trim()).ToArray();
        }

        [TestMethod]
        public void Teach_exactly_the_examples_checked_here()
        {
            CollectionAssert.AreEqual(ExpectedExamples, Examples().Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray());
        }

        [TestMethod]
        public void Give_the_test_example_the_front_matter_and_sections_the_skill_describes()
        {
            var test = Examples()["test"];
            foreach (var key in new[] { "e2e:", "baseUrl:", "siteMap:", "credentials:", "artifacts:" })
                StringAssert.Contains(test, key);
            Assert.IsTrue(Regex.IsMatch(test, @"^## Artefatti$", RegexOptions.Multiline), "missing section ## Artefatti");
            Assert.IsTrue(Regex.IsMatch(test, @"^## Esiti$", RegexOptions.Multiline), "missing section ## Esiti");
            Assert.IsTrue(Regex.IsMatch(test, @"^## T1 — ", RegexOptions.Multiline));
        }

        [TestMethod]
        public void Use_only_credential_keys_that_the_credentials_file_has()
        {
            var keys = Credentials().Keys.ToHashSet();
            foreach (var id in new[] { "test", "report", "esiti", "mappa" })
            {
                foreach (Match m in Regex.Matches(Examples()[id], @"\{\{([^}]+)\}\}"))
                    Assert.IsTrue(keys.Contains(m.Groups[1].Value), $"example '{id}': key '{m.Groups[1].Value}' is not in the credentials example");
            }
            foreach (Match m in Regex.Matches(Examples()["script"], @"E2e\.Credential\(Credentials, ""([^""]+)""\)"))
                Assert.IsTrue(keys.Contains(m.Groups[1].Value), $"script: key '{m.Groups[1].Value}' is not in the credentials example");
        }

        [TestMethod]
        public void Never_write_a_credential_value_outside_the_credentials_file()
        {
            var values = Credentials().Values.ToArray();
            foreach (var (id, text) in Examples().Where(e => e.Key != "credenziali"))
                foreach (var value in values)
                    Assert.IsFalse(text.Contains(value), $"example '{id}' contains the value of a credential");
        }

        [TestMethod]
        public void Comment_in_the_script_exactly_the_steps_of_the_test()
        {
            var script = Examples()["script"];
            var comments = Regex.Matches(script, @"^\s*// \d+\. (?:✔ )?(.+)$", RegexOptions.Multiline)
                .Select(m => m.Groups[1].Value.Trim()).ToArray();
            var steps = StepsOfT1().Select(s => s.StartsWith("✔ ") ? s.Substring(2) : s).ToArray();
            CollectionAssert.AreEqual(steps, comments);
        }

        [TestMethod]
        public void Follow_in_the_script_the_rules_the_skill_gives()
        {
            var script = Examples()["script"];
            StringAssert.Contains(script, "public class Test_T1 : PageTest");
            StringAssert.Contains(script, "public async Task T1_");
            StringAssert.Contains(script, "E2e.Screenshot(Page, Artifacts, ");
            Assert.IsFalse(script.Contains("GetEnvironmentVariable"), "credentials come from E2e.Credential, not from environment variables");
            Assert.IsFalse(script.Contains("ScreenshotAsync"), "screenshots go through E2e.Screenshot");
            foreach (var header in new[] { "// stato: ", "// sorgente: ", "// impronta-sorgente: ", "// generatore: ", "// data: " })
                StringAssert.Contains(script, header);

            // The generator version (what makes old scripts stale) is the skeleton's own line, apart from the skill's
            // `mde: version`, which rises with any change of text so that projects receive the new skill.
            Assert.AreEqual(1, Regex.Matches(File.ReadAllText(SkillPath()), @"^// generatore: mde-e2e v\d+$", RegexOptions.Multiline).Count,
                "exactly one generator line, in the script skeleton");
        }

        [TestMethod]
        public void Give_the_support_files_the_contract_the_script_relies_on()
        {
            var support = Examples()["supporto"];
            StringAssert.Contains(support, "namespace MdeE2e");
            StringAssert.Contains(support, "public static string Credential(string file, string key");
            StringAssert.Contains(support, "public static Task Screenshot(IPage page, string artifacts, string name");
            StringAssert.Contains(support, "public static Task EnableSignals(IBrowserContext context)");
            StringAssert.Contains(support, "public static async Task Signal(IPage page, string name, string key, TimeSpan max)");
            StringAssert.Contains(Examples()["csproj"], "<ImplicitUsings>disable</ImplicitUsings>");
            StringAssert.Contains(Examples()["csproj"], "\"Microsoft.Playwright.NUnit\"");
            StringAssert.Contains(Examples()["runsettings"], "<Channel>chrome</Channel>");
        }

        [TestMethod]
        public void Put_the_most_recent_run_first_in_the_results_table()
        {
            var esiti = Examples()["esiti"];
            StringAssert.Contains(esiti, "| Data | Test | Esito | Dettagli |");
            var dates = Regex.Matches(esiti, @"^\| (\d{4}-\d{2}-\d{2} \d{2}:\d{2}) \|", RegexOptions.Multiline)
                .Select(m => m.Groups[1].Value).ToArray();
            Assert.IsTrue(dates.Length >= 2, "the example should show more than one row");
            CollectionAssert.AreEqual(dates.OrderByDescending(d => d, StringComparer.Ordinal).ToArray(), dates);
        }
    

        [TestMethod]
        public void Replace_an_old_support_file_by_the_version_line_the_skill_names()
        {
            // An E2eSupport.cs written by an older skill lacks E2e.Signal: the skill tells the agent to replace a
            // support file without this exact line, so the line in the text and in the example must be the same.
            var line = Regex.Match(Examples()["supporto"], @"^// versione-supporto: \d+$", RegexOptions.Multiline).Value;
            Assert.AreNotEqual("", line, "the support example has no version line");
            StringAssert.Contains(File.ReadAllText(SkillPath()), "`" + line + "`");
        }

        [TestMethod]
        public void Wait_for_an_event_with_a_maximum_never_for_a_fixed_pause()
        {
            foreach (var (id, text) in Examples())
                foreach (var pause in new[] { "Task.Delay", "WaitForTimeoutAsync", "Thread.Sleep" })
                    Assert.IsFalse(text.Contains(pause), $"example '{id}' uses a fixed pause ({pause})");
            StringAssert.Contains(Examples()["script"], "// attesa: ");
        }

        [TestMethod]
        public void Look_for_a_text_only_among_the_visible_elements()
        {
            // F0 of 2026-09-28: the same text in hidden views (a select's options, inactive tabs) made the first
            // match invisible, and without .First Playwright stops on the ambiguity.
            var script = Examples()["script"];
            var byText = Regex.Matches(script, @"GetByText\(""[^""]*""\)[^;]*").Select(m => m.Value).ToArray();
            Assert.IsTrue(byText.Length > 0);
            foreach (var use in byText)
                StringAssert.Contains(use, ".Filter(new() { Visible = true }).First", "every text check goes through the visible filter");
        }

        [TestMethod]
        public void Give_the_page_sheet_the_sections_the_skill_describes()
        {
            var sheet = Examples()["scheda"];
            foreach (var section in new[] { "## Azioni e chiamate", "## Da tenere d'occhio", "## Storia dei problemi" })
                Assert.IsTrue(Regex.IsMatch(sheet, "^" + Regex.Escape(section) + "$", RegexOptions.Multiline), $"missing section {section}");
            foreach (var field in new[] { "- Attesa:", "- Osservato:", "- Tempo massimo:", "- Rumore da ignorare:", "- Fine del lavoro:" })
                StringAssert.Contains(sheet, field);
        }
    }
}
