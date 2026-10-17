using Markdig;
using Markdig.Syntax;
using MdExplorer.Features.Tests.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Tests.E2e
{
    /// <summary>
    /// The skill mde-e2e-signals puts in a site the half of a contract whose other half is E2e.Signal and
    /// E2e.EnableSignals of the mde-e2e support file: attribute, value format and switch must agree, or a site
    /// instrumented by one skill is never seen by the scripts of the other. That the helper and E2e.Signal work
    /// together in a real browser was verified by hand (sprint 2026-09-28-Script-E2E-Affidabili, F0 and F1).
    /// </summary>
    [TestClass]
    public class MdeE2eSignalsSkill_Should
    {
        private static string Skill(string name) =>
            File.ReadAllText(Path.Combine(SlidePage.RepositoryRoot(), "MdExplorer", "skills", name, "SKILL.md"));

        private static string Example(string skill, string id)
        {
            var document = Markdown.Parse(Skill(skill), new MarkdownPipelineBuilder().UseYamlFrontMatter().Build());
            return document.Descendants<FencedCodeBlock>().Single(b => (b.Arguments ?? "").Trim() == "esempio=" + id).Lines.ToString();
        }

        [TestMethod]
        public void Carry_its_own_name_in_the_front_matter()
        {
            StringAssert.Contains(Skill("mde-e2e-signals"), "\nname: mde-e2e-signals\n");
        }

        [TestMethod]
        public void Write_the_attribute_the_mde_e2e_scripts_wait_for()
        {
            var helper = Example("mde-e2e-signals", "aiuto");
            var support = Example("mde-e2e", "supporto");
            StringAssert.Contains(helper, "setAttribute('data-mde-' + name, state + ':' + key)");
            StringAssert.Contains(support, "var attribute = \"data-mde-\" + name;");
            StringAssert.Contains(support, "ready:{key}");
            StringAssert.Contains(support, "error:{key}");
        }

        [TestMethod]
        public void Switch_on_with_the_key_the_mde_e2e_scripts_set()
        {
            StringAssert.Contains(Example("mde-e2e-signals", "aiuto"), "localStorage.getItem('mde-e2e') === '1'");
            StringAssert.Contains(Example("mde-e2e", "supporto"), "localStorage.setItem('mde-e2e', '1')");
        }

        [TestMethod]
        public void Print_in_the_console_the_line_the_mde_e2e_skill_tells_the_agent_to_read()
        {
            StringAssert.Contains(Example("mde-e2e-signals", "aiuto"), "console.info('[mde] ' + state + ' ' + name + ' ' + key");
            StringAssert.Contains(Skill("mde-e2e"), "`[mde] start grafo cob:bs522`");
        }

        [TestMethod]
        public void Signal_start_before_the_first_wait_of_the_handler()
        {
            var use = Example("mde-e2e-signals", "uso");
            var start = use.IndexOf("signal('start'");
            Assert.IsTrue(start >= 0);
            Assert.IsTrue(start < use.IndexOf("await"), "start must come before the first await");
            Assert.IsTrue(use.IndexOf("signal('ready'") > use.IndexOf("disegnaFiltri"), "ready after the page is updated");
            StringAssert.Contains(use, "signal('error'");
        }
    }
}
