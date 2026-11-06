using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static MdExplorer.IntegrationTests.ProjectTemplateCreator_Should;
using static MdExplorer.IntegrationTests.ProjectTemplateMethods_Should;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il formato 2 dei template (docs-internal/Sprints/2026-10-20-Citta-Degli-Agenti-Come-Funzione.md, C3): le skill nella
    /// zona, copiate da MDE; agenti, ruoli e workflow in <c>agent_city</c>, che arrivano solo con la città accesa.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectTemplateFormat2_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private const string Manifest = @"mde_template: 2
id: ddd
kind: method
title: ""Sviluppo con DDD""
version: 2.0.0
languages:
  it: { structure: main }
skills:
  - .mde-template/ddd/skills/ddd-base
agent_city:
  roles:
    - { role: Analista DDD, agents: [ddd-analista] }
  workflows:
    - .mde-template/ddd/workflows/ddd.md
  skills:
    - .mde-template/ddd/skills/ddd-modella
instructions: .mde-template/ddd/istruzioni.md
";

        internal static Dictionary<string, Dictionary<string, string>> Method(Dictionary<string, string> extra = null) => new()
        {
            ["main"] = new Dictionary<string, string>
            {
                [".mde-template/ddd/mde-template.yml"] = Manifest,
                [".mde-template/ddd/CHANGELOG.md"] = "## 2.0.0 — 2026-10-20\n- Formato 2.\n",
                [".mde-template/ddd/README.md"] = "# DDD\n",
                [".mde-template/ddd/agents/ddd-analista.agent.md"] = Card,
                [".mde-template/ddd/workflows/ddd.md"] = "---\nmde_type: workflow\nworkflow: ddd.workflow.json\n---\n# Modello del dominio\n",
                [".mde-template/ddd/workflows/ddd.workflow.json"] = DddWorkflow,
                [".mde-template/ddd/istruzioni.md"] = "Il dominio sta in ddd/.\n",
                [".mde-template/ddd/skills/ddd-base/SKILL.md"] = "---\nname: ddd-base\ndescription: scrive il dominio\n---\n# Base\n",
                [".mde-template/ddd/skills/ddd-modella/SKILL.md"] = "---\nname: ddd-modella\ndescription: modella il dominio\n---\n# Modella\n",
                [".mde-template/ddd/skills/ddd-modella/esempio.md"] = "un esempio\n",
            }.Concat(extra ?? new Dictionary<string, string>()).ToDictionary(kv => kv.Key, kv => kv.Value),
        };

        [TestMethod]
        public async Task Leave_out_everything_of_the_agents_when_the_city_is_off()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "senza-citta");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Strato(ctx), Methods = new[] { Template(ctx, "ddd", Method()) }, Language = "it", MdeVersion = Mde, Harness = "claude",
            });

            Assert.AreEqual("# Base\n", Read(path, ".claude/skills/ddd-base/SKILL.md").Split("---\n").Last(), "la skill di base, copiata dalla zona");
            Assert.IsFalse(Directory.Exists(Path.Combine(path, ".claude", "skills", "ddd-modella")), "la skill degli agenti no");
            Assert.IsFalse(Directory.Exists(Path.Combine(path, ".claude", "skills", "mde-workflow")), "nemmeno quelle di MDE per gli agenti");
            var yml = Read(path, ".development.yml");
            Assert.IsFalse(yml.Contains("workflowDocs"), yml);
            Assert.IsFalse(yml.Contains("agentCity:\n  enabled: true"), "la città resta spenta:\n" + yml);
            Assert.IsFalse(File.Exists(Path.Combine(path, "responsabilita.md")) && Read(path, "responsabilita.md").Contains("Analista DDD"), "niente ruoli");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("«Sviluppo con DDD» has agents, roles and workflows for the agent city: they arrive when you turn it on")),
                string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out);
        }

        [TestMethod]
        public async Task Bring_the_agents_their_roles_workflows_and_skills_with_the_city()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "con-citta");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Strato(ctx), Methods = new[] { Template(ctx, "ddd", Method()) }, AgentCity = true,
                Language = "it", MdeVersion = Mde, Harness = "claude",
            });

            Assert.IsTrue(File.Exists(Path.Combine(path, ".claude", "skills", "ddd-modella", "SKILL.md")));
            Assert.AreEqual("un esempio\n", Read(path, ".claude/skills/ddd-modella/esempio.md"), "tutta la cartella della skill");
            var yml = Read(path, ".development.yml");
            StringAssert.Contains(yml, "  - .mde-template/ddd/workflows/ddd.md");
            StringAssert.Contains(yml, "  - citta/ciao.md");
            StringAssert.Contains(Read(path, "responsabilita.md"), "| Analista DDD |  |  | ddd-analista |");
            StringAssert.Contains(Read(path, "responsabilita.md"), "| Responsabile della città |  |  | citta-ciao |");
            Assert.IsFalse(outcome.Notes.Any(n => n.Contains("they arrive when you turn it on")), string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out);
        }

        [TestMethod]
        public async Task Refuse_a_format_2_method_that_keeps_its_skills_outside_its_folder()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var url = Template(ctx, "ddd", Method(new() { [".claude/skills/ddd-vecchia/SKILL.md"] = "---\nname: ddd-vecchia\n---\n" }));

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "skill-fuori"), TemplateUrl = Strato(ctx), Methods = new[] { url }, Language = "it", MdeVersion = Mde, Harness = "claude",
            }));
            StringAssert.Contains(ex.Message, ".claude/skills/ddd-vecchia/SKILL.md");
        }

        [TestMethod]
        public async Task Check_a_format_2_template_as_if_the_city_were_on()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var work = Path.Combine(Path.GetDirectoryName(Template(ctx, "ddd", Method(new() { [".claude/skills/ddd-vecchia/SKILL.md"] = "---\nname: ddd-vecchia\n---\n" }))), "lavoro");
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "skills", "ddd-base", "SKILL.md"), "---\nname: base\n---\n");

            using var scope = ctx.Factory.Services.CreateScope();
            var report = await scope.ServiceProvider.GetRequiredService<ITemplateChecker>().CheckAsync(work);
            var all = string.Join("\n", report.Issues.Select(i => $"{i.Severity} {i.File}: {i.Message} {i.Fix}"));

            StringAssert.Contains(all, "error .claude/skills/ddd-vecchia/SKILL.md: The file is outside the method's zone (.mde-template/ddd/)");
            StringAssert.Contains(all, "In format 2 the skills live in .mde-template/ddd/");
            StringAssert.Contains(all, "error .mde-template/ddd/skills/ddd-base/SKILL.md: The skill's front matter must have «name: ddd-base»");
            Assert.IsFalse(all.Contains("workflow"), "il workflow di agent_city si legge, con il suo agente: " + all);
        }

        [TestMethod]
        public void Know_the_agent_cards_that_belong_to_a_template()
        {
            var project = Path.Combine(Path.GetTempPath(), "progetto");
            Assert.IsTrue(MdExplorer.Service.Services.FoldersIgnoreService.IsInsideTemplateZones(
                Path.Combine(project, ".mde-template", "ddd", "agents", "ddd-analista.agent.md"), project));
            Assert.IsFalse(MdExplorer.Service.Services.FoldersIgnoreService.IsInsideTemplateZones(
                Path.Combine(project, "citta", "agenti", "citta-ciao.agent.md"), project), "l'agente della città è del progetto");
            Assert.IsFalse(MdExplorer.Service.Services.FoldersIgnoreService.IsInsideTemplateZones(
                Path.Combine(project + "-altro", ".mde-template", "x.agent.md"), project));
        }
    }
}
