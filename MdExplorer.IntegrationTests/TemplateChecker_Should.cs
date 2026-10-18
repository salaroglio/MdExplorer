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
    /// La verifica di un template (<c>CheckTemplate</c>, docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md): sul
    /// repository di chi lo scrive, il ramo in cui lavora dai file e gli altri rami dichiarati dal loro commit, tutti i
    /// problemi in un giro. <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class TemplateChecker_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(System.AppContext.BaseDirectory);

        private const string Manifest = @"mde_template: 1
id: ddd
kind: method
title: ""Sviluppo con DDD""
version: 1.0.0
languages:
  it: { structure: main, example: esempio }
roles:
  - { role: Analista DDD, agents: [ddd-analista] }
workflows:
  - .mde-template/ddd/workflows/ddd.md
instructions: .mde-template/ddd/istruzioni.md
seeds:
  - { from: .mde-template/ddd/semi/glossario.md, to: ddd/glossario.md }
";

        private static Dictionary<string, string> Structure() => new()
        {
            [".mde-template/ddd/mde-template.yml"] = Manifest,
            [".mde-template/ddd/CHANGELOG.md"] = "## 1.0.0 — 2026-10-18\n- Prima versione.\n",
            [".mde-template/ddd/README.md"] = "# Sviluppo con DDD\n",
            [".mde-template/ddd/agents/ddd-analista.agent.md"] = Card,
            [".mde-template/ddd/workflows/ddd.md"] = "---\nmde_type: workflow\nworkflow: ddd.workflow.json\n---\n# Modello del dominio\n",
            [".mde-template/ddd/workflows/ddd.workflow.json"] = DddWorkflow,
            [".mde-template/ddd/istruzioni.md"] = "Il glossario sta in ddd/glossario.md.\n",
            [".mde-template/ddd/semi/glossario.md"] = "# Glossario\n",
            [".claude/skills/ddd-modella/SKILL.md"] = "---\nname: ddd-modella\ndescription: modella il dominio\n---\n# Modella\n",
        };

        /// <summary>Il repository di chi scrive il metodo: la struttura su main, l'esempio che parte da lì, il tag.</summary>
        private static string Repository(AgentCityContext ctx, string name, Dictionary<string, string> main, bool example = true, bool tag = true)
        {
            var work = Path.Combine(ctx.Factory.DataDir, "autore", name);
            Directory.CreateDirectory(work);
            Git(work, "init -q -b main");
            Git(work, "config user.email autore@test.local"); Git(work, "config user.name Autore"); Git(work, "config commit.gpgsign false");
            Write(work, main);
            Git(work, "add -A"); Git(work, "commit -q -m struttura");
            if (tag) Git(work, "tag v1.0.0");
            if (example)
            {
                Git(work, "checkout -q -b esempio");
                Write(work, new() { [".mde-template/ddd/esempio/glossario.md"] = "# Glossario di Alpina\n" });
                Git(work, "add -A"); Git(work, "commit -q -m esempio");
                Git(work, "checkout -q main");
            }
            return work;
        }

        private static void Write(string root, Dictionary<string, string> files)
        {
            foreach (var (file, text) in files)
            {
                var full = Path.Combine(root, file);
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full, text);
            }
        }

        private static async Task<TemplateCheckReport> Check(AgentCityContext ctx, string folder)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<ITemplateChecker>().CheckAsync(folder);
        }

        private static string Describe(TemplateCheckReport r) => string.Join("\n", r.Issues.Select(i => $"{i.Severity} [{i.Branch}] {i.File}: {i.Message}"));

        [TestMethod]
        public async Task Find_nothing_wrong_in_a_method_ready_to_publish()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var work = Repository(ctx, "pronto", Structure());

            var report = await Check(ctx, Path.Combine(work, ".mde-template"));

            Assert.IsTrue(report.Ok, Describe(report));
            Assert.AreEqual(0, report.Warnings, Describe(report));
            Assert.AreEqual("ddd", report.Id);
            Assert.AreEqual("1.0.0", report.Version);
            CollectionAssert.AreEqual(new[] { "main:working-tree", "esempio:commit" }, report.Branches.Select(b => $"{b.Branch}:{b.From}").ToArray());
            Assert.AreEqual("", Git(work, "status --porcelain").Out, "il repository di chi scrive non si tocca");
        }

        [TestMethod]
        public async Task Say_everything_wrong_in_one_pass()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var main = Structure();
            main[".mde-template/ddd/CHANGELOG.md"] = "## 0.9.0 — 2026-10-18\n- Prima versione.\n";
            main.Remove(".mde-template/ddd/README.md");
            main[".mde-template/ddd/agents/analista.agent.md"] = Card.Replace("ddd-analista", "analista");
            main[".claude/skills/ddd-contesti/SKILL.md"] = "---\nname: contesti\n---\n";
            main["gare/fuori.md"] = "fuori\n";
            main[".mde-template/ddd/workflows/ddd.workflow.json"] = DddWorkflow.Replace("\"skill\": \"modella\"", "\"skill\": \"inesistente\"");
            var work = Repository(ctx, "guasto", main, example: false, tag: false);
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "manifesto-bis.md"), "non committato\n");
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "mde-template.yml"),
                Manifest.Replace("  - { role: Analista DDD, agents: [ddd-analista] }", "  - { role: Analista DDD, agents: [ddd-analista, ddd-fantasma] }"));

            var report = await Check(ctx, work);
            var all = Describe(report);

            Assert.IsFalse(report.Ok, all);
            StringAssert.Contains(all, "error [main] gare/fuori.md: The file is outside the method's zone");
            StringAssert.Contains(all, "error [main] .mde-template/ddd/CHANGELOG.md:");
            StringAssert.Contains(all, "warning [main] .mde-template/ddd/README.md: The template's README is missing");
            StringAssert.Contains(all, "error [main] .mde-template/ddd/agents/analista.agent.md: The agent 'analista' does not start with the method's id");
            StringAssert.Contains(all, "error [main] .claude/skills/ddd-contesti/SKILL.md: The skill's front matter must have «name: ddd-contesti»");
            StringAssert.Contains(all, "The role «Analista DDD» names the agent 'ddd-fantasma', which has no card in the branch.");
            StringAssert.Contains(all, "error [main] .mde-template/ddd/workflows/ddd.workflow.json:");
            StringAssert.Contains(all, "error [] .mde-template/ddd/mde-template.yml: The manifest declares the branch 'esempio', which is not in the repository.");
            StringAssert.Contains(all, "warning [] : The tag v1.0.0 of the version 1.0.0 is missing.");
            StringAssert.Contains(all, "warning [main] : The branch 'main' has uncommitted changes");
            Assert.AreEqual(1, report.Branches.Count, "l'esempio dichiarato non c'è: si controlla solo main");
        }

        [TestMethod]
        public async Task Check_the_other_branches_from_their_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var work = Repository(ctx, "rami", Structure());
            Git(work, "checkout -q esempio");
            File.WriteAllText(Path.Combine(work, "fuori.md"), "nell'esempio, fuori zona\n");
            Git(work, "add -A"); Git(work, "commit -q -m fuori");
            Git(work, "checkout -q main");
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "CHANGELOG.md"), "## 1.1.0 — 2026-10-19\n- Nuova.\n\n## 1.0.0 — 2026-10-18\n- Prima versione.\n");
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "mde-template.yml"), Manifest.Replace("version: 1.0.0", "version: 1.1.0"));
            Git(work, "commit -q -am 1.1.0");

            var report = await Check(ctx, work);
            var all = Describe(report);

            StringAssert.Contains(all, "error [esempio] fuori.md: The file is outside the method's zone");
            StringAssert.Contains(all, "warning [esempio] .mde-template/ddd/mde-template.yml: The structure 'main' is at 1.1.0, the example 'esempio' at 1.0.0");
            StringAssert.Contains(all, "warning [esempio] : The example 'esempio' does not contain the last commit of the structure 'main'");
            StringAssert.Contains(all, "The tag v1.1.0 of the version 1.1.0 is missing.");
            Assert.IsFalse(all.Contains("v1.0.0 of the version"), "il tag della 1.0.0 c'è");
        }

        [TestMethod]
        public async Task Refuse_a_folder_that_is_not_a_repository()
        {
            using var ctx = new AgentCityContext();
            var folder = Path.Combine(ctx.Factory.DataDir, "non-repo");
            Directory.CreateDirectory(folder);
            var ex = await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => Check(ctx, folder));
            StringAssert.Contains(ex.Message, "is not a git repository");
        }
    }
}
