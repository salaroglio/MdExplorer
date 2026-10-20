using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static MdExplorer.IntegrationTests.ProjectTemplateCreator_Should;
using static MdExplorer.IntegrationTests.ProjectTemplateMethods_Should;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La città degli agenti scelta alla creazione (docs-internal/Sprints/2026-10-20-Citta-Degli-Agenti-Come-Funzione.md, C1):
    /// MDE porta da sé la città accesa, il documento delle responsabilità e il workflow «hello world», nella lingua del progetto.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectTemplateAgentCity_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        [TestMethod]
        public async Task Bring_the_city_with_its_hello_world_round_in_italian()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "con-la-citta");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Strato(ctx), AgentCity = true, Language = "it", MdeVersion = Mde, Harness = "claude",
            });

            foreach (var file in new[] { "citta/ciao.md", "citta/ciao.workflow.json", "citta/agenti/citta-ciao.agent.md" })
                Assert.IsTrue(File.Exists(Path.Combine(path, file)), file);
            Assert.IsFalse(Directory.Exists(Path.Combine(path, "citta", "guida")), "la documentazione non è stata chiesta");
            foreach (var skill in new[] { "mde-agent", "mde-workflow", "mde-ownership", "mde-prompt-for-agents" })
                Assert.IsTrue(File.Exists(Path.Combine(path, ".claude", "skills", skill, "SKILL.md")), $"con la città arriva la skill {skill}");
            var yml = Read(path, ".development.yml");
            StringAssert.Contains(yml, "agentCity:\n  enabled: true");
            StringAssert.Contains(yml, "  - citta/ciao.md");
            StringAssert.Contains(Read(path, "responsabilita.md"), "| Responsabile della città |  |  | citta-ciao |");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("Agent city turned on, with the example workflow citta/ciao.md")), string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out, "la città ha il suo commit");
            Assert.AreEqual("Agent city, with the example workflow", Git(path, "log -1 --format=%s").Out);
        }

        [TestMethod]
        public async Task Bring_the_city_in_english_with_english_names()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var english = ProjectTemplateComposition_Should.Foundation();
            english["main"][".mde-template/documenti/mde-template.yml"] = english["main"][".mde-template/documenti/mde-template.yml"]
                .Replace("  it: { structure: main }\n", "  en: { structure: main }\n")
                .Replace("theory:\n  it: { from: .mde-template/documenti/teoria, to: teoria/documenti }\n", "");
            var path = NewFolder(ctx, "with-the-city");

            await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Template(ctx, "documenti", english), AgentCity = true, Language = "en", MdeVersion = Mde, Harness = "claude",
            });

            Assert.IsTrue(File.Exists(Path.Combine(path, "agent-city", "hello.md")));
            Assert.IsTrue(File.Exists(Path.Combine(path, "agent-city", "agents", "city-hello.agent.md")));
            StringAssert.Contains(Read(path, "responsibilities.md"), "| City owner |  |  | city-hello |");
            StringAssert.Contains(Read(path, ".development.yml"), "  - agent-city/hello.md");
        }

        [TestMethod]
        public async Task Refuse_the_city_on_the_demo_that_has_its_own()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "demo-e-citta"), TemplateUrl = Template(ctx, "demo", Base()), AgentCity = true, Language = "it", MdeVersion = Mde,
            }));
            StringAssert.Contains(ex.Message, "already has its own agent city");
        }

        [TestMethod]
        public async Task Show_the_deck_of_the_city_in_its_language_or_in_english()
        {
            using var ctx = new AgentCityContext();
            StringAssert.Contains(await ctx.Client.GetStringAsync("/api/templateshowcase/agent-city?language=it"), "La città degli agenti");
            StringAssert.Contains(await ctx.Client.GetStringAsync("/api/templateshowcase/agent-city?language=de"), "The agent city");
        }

        [TestMethod]
        public async Task Bring_the_documentation_of_the_city_only_when_asked_and_only_with_the_city()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "con-la-guida");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Strato(ctx), AgentCity = true, AgentCityGuide = true, Language = "it", MdeVersion = Mde, Harness = "claude",
            });

            foreach (var file in new[] { "citta/guida/README.md", "citta/guida/04-scrivere-un-workflow.md", "citta/guida/iso-9001/README.md" })
                Assert.IsTrue(File.Exists(Path.Combine(path, file)), file);
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("The agent city's documentation in citta/guida/")), string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out);

            var alone = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "guida-senza-citta"), TemplateUrl = Strato(ctx), AgentCityGuide = true, Language = "it", MdeVersion = Mde, Harness = "claude",
            }));
            StringAssert.Contains(alone.Message, "is asked for together with the agent city");
        }

        [TestMethod]
        public void Carry_the_same_documentation_in_both_languages()
        {
            var it = ProjectTemplateCreator.AgentCityFiles(ProjectTemplateCreator.AgentCityStarterFor("it"), withGuide: true)
                .Count(f => f.Path.StartsWith("citta/guida/", StringComparison.Ordinal));
            var en = ProjectTemplateCreator.AgentCityFiles(ProjectTemplateCreator.AgentCityStarterFor("en"), withGuide: true)
                .Count(f => f.Path.StartsWith("agent-city/guide/", StringComparison.Ordinal));
            Assert.IsTrue(it >= 11, $"la guida italiana ha {it} file");
            Assert.AreEqual(it, en, "le due lingue hanno gli stessi documenti");
        }
    }
}
