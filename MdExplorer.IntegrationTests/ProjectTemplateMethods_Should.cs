using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static MdExplorer.IntegrationTests.ProjectTemplateCreator_Should;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Più template in un progetto (P2 di docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md): un metodo si unisce
    /// con la sua storia, porta solo la sua zona, e MDE scrive per lui nei file comuni — la riga e i workflow in
    /// <c>.development.yml</c>, i ruoli aperti nelle responsabilità, le istruzioni nel blocco del metodo.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectTemplateMethods_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(System.AppContext.BaseDirectory);

        internal const string Card = "---\na2a:\n  name: ddd-analista\n  role: Analista DDD\n  accepts_messages_from: [\"user\"]\n  skills:\n    - id: modella\n---\n# ddd-analista\n\nSei l'analista DDD.\n";

        internal const string DddWorkflow = @"{
  ""mde_workflow"": 3, ""title"": ""Modello del dominio"",
  ""steps"": [ { ""id"": ""modella"", ""agent"": ""ddd-analista"", ""skill"": ""modella"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" } ]
}";

        internal static string Ddd(string requires = null) => $@"mde_template: 1
id: ddd
kind: method
title: ""Sviluppo con DDD""
version: 1.0.0
{(requires == null ? "" : $"requires:\n  templates: [{requires}]\n")}languages:
  it: {{ structure: main }}
roles:
  - {{ role: Analista DDD, agents: [ddd-analista] }}
  - {{ role: Product owner }}
workflows:
  - .mde-template/ddd/workflows/ddd.md
instructions: .mde-template/ddd/istruzioni.md
";

        internal static Dictionary<string, Dictionary<string, string>> DddBranches(string requires = null, string workflow = DddWorkflow, string extraFile = null) => new()
        {
            ["main"] = new Dictionary<string, string>
            {
                [".mde-template/ddd/mde-template.yml"] = Ddd(requires),
                [".mde-template/ddd/agents/ddd-analista.agent.md"] = Card,
                [".mde-template/ddd/workflows/ddd.md"] = "---\nmde_type: workflow\nworkflow: ddd.workflow.json\n---\n# Modello del dominio\n",
                [".mde-template/ddd/workflows/ddd.workflow.json"] = workflow,
                [".mde-template/ddd/istruzioni.md"] = "Il linguaggio del dominio sta in .mde-template/ddd/glossario.md.\n",
                [".claude/skills/ddd-modella/SKILL.md"] = "---\nname: ddd-modella\ndescription: modella il dominio\n---\n# Modella\n",
            }.Concat(extraFile == null ? new Dictionary<string, string>() : new Dictionary<string, string> { [extraFile] = "fuori\n" })
             .ToDictionary(kv => kv.Key, kv => kv.Value),
        };

        /// <summary>La base: la demo dei test di P1, con un CLAUDE.md del gruppo.</summary>
        internal static Dictionary<string, Dictionary<string, string>> Base()
        {
            var b = DemoBranches();
            b["main"]["CLAUDE.md"] = "# Istruzioni del gruppo\n\nRispondi in italiano.\n";
            return b;
        }

        internal static async Task<TemplateAddOutcome> Add(AgentCityContext ctx, string path, string url)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IProjectTemplateCreator>()
                .AddMethodAsync(new TemplateAddRequest { ProjectPath = path, TemplateUrl = url, Language = "it", MdeVersion = Mde });
        }

        internal static async Task<string> Project(AgentCityContext ctx, string name)
        {
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, name);
            await Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = Template(ctx, "demo", Base()), Language = "it", MdeVersion = Mde });
            Git(path, "config user.email io@test.local"); Git(path, "config user.name Io"); Git(path, "config commit.gpgsign false");
            return path;
        }

        internal static string Read(string path, string file) => File.ReadAllText(Path.Combine(path, file));

        [TestMethod]
        public async Task Add_a_method_to_a_project_and_write_for_it_in_the_common_files()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = await Project(ctx, "con-ddd");
            var before = Git(path, "rev-list --count HEAD").Out;

            var outcome = await Add(ctx, path, Template(ctx, "ddd", DddBranches()));

            Assert.AreEqual("ddd", outcome.Id);
            Assert.IsTrue(File.Exists(Path.Combine(path, ".mde-template", "ddd", "agents", "ddd-analista.agent.md")), "la zona del metodo è nel progetto");
            Assert.IsTrue(File.Exists(Path.Combine(path, ".claude", "skills", "ddd-modella", "SKILL.md")), "le skill del metodo, lette da tutti i motori");

            var yml = Read(path, ".development.yml");
            StringAssert.Contains(yml, "- id: ddd\n");
            StringAssert.Contains(yml, "workflowDocs:\n  - .mde-template/ddd/workflows/ddd.md");
            StringAssert.Contains(yml, "ownershipDoc: responsabilita.md");

            var ownership = Read(path, "responsabilita.md");
            StringAssert.Contains(ownership, "mde_type: ownership", "il progetto non aveva il documento: nasce");
            StringAssert.Contains(ownership, "| Analista DDD |  |  | ddd-analista |");
            StringAssert.Contains(ownership, "| Product owner |  |  |  |");

            var claude = Read(path, "CLAUDE.md");
            StringAssert.StartsWith(claude, "# Istruzioni del gruppo\n\nRispondi in italiano.\n", "il testo del gruppo resta");
            StringAssert.Contains(claude, "<!-- mde-template:ddd -->\nIl linguaggio del dominio sta in .mde-template/ddd/glossario.md.\n<!-- /mde-template:ddd -->");
            StringAssert.Contains(Read(path, ".github/copilot-instructions.md"), "<!-- mde-template:ddd -->", "anche il file del motore del progetto");

            Assert.AreEqual("", Git(path, "status --porcelain").Out, "tutto in un commit");
            Assert.AreEqual((int.Parse(before) + 2).ToString(), Git(path, "rev-list --count HEAD").Out, "il commit del metodo più l'unione della sua storia");
            Assert.AreEqual("2", Git(path, "rev-list --parents -n 1 HEAD").Out.Split(' ').Length - 1 + "", "un commit di unione: due genitori");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("Open roles added in responsabilita.md: Analista DDD, Product owner")), string.Join("\n", outcome.Notes));

            // Il metodo c'è già: non si aggiunge una seconda volta.
            var again = await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => Add(ctx, path, Template(ctx, "ddd-bis", DddBranches())));
            StringAssert.Contains(again.Message, "already has the template 'ddd'");
        }

        [TestMethod]
        public async Task Create_a_project_from_a_base_and_a_method()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "base-e-ddd");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Template(ctx, "demo", Base()), Language = "it", MdeVersion = Mde,
                Methods = new[] { Template(ctx, "ddd", DddBranches(requires: "demo")) },
            });

            Assert.AreEqual("demo", outcome.Id);
            Assert.AreEqual("ddd", outcome.Methods.Single().Id, "requires.templates soddisfatto dalla base");
            StringAssert.Contains(Read(path, ".development.yml"), "- id: ddd\n");
            Assert.AreEqual(Git(path, "rev-parse HEAD").Out, Git(path, "rev-parse origin/main").Out, "pubblicato con il metodo");
            StringAssert.Contains(Read(path, "CLAUDE.md"), "<!-- mde-template:ddd -->");
            Assert.AreEqual("", Git(path, "status --porcelain").Out);
        }

        [TestMethod]
        public async Task Create_a_project_from_a_method_alone_with_the_engine_chosen_and_nothing_left_to_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "solo-ddd");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Template(ctx, "ddd", DddBranches()), Language = "it", MdeVersion = Mde, Harness = "claude",
            });

            Assert.AreEqual("ddd", outcome.Id);
            StringAssert.Contains(Read(path, ".development.yml"), "harness:\n  target: claude");
            StringAssert.Contains(Read(path, ".development.yml"), "agentCity:\n  enabled: true", "il metodo porta un workflow: la città si accende");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("Agent city turned on: the workflows of «Sviluppo con DDD» run there")), string.Join("\n", outcome.Notes));
            StringAssert.Contains(Read(path, "CLAUDE.md"), "<!-- mde-template:ddd -->", "le istruzioni nel file del motore scelto");
            var gitignore = Read(path, ".gitignore");
            foreach (var line in new[] { ".md/", ".mdapplicationtoopen", ".mdchangeignore", ".mdFoldersIgnore" })
                StringAssert.Contains(gitignore, line + "\n");
            Assert.IsTrue(File.Exists(Path.Combine(path, ".mdFoldersIgnore")), "i file di MDE ci sono, ignorati");
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out, "appena creato, niente da committare");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("Project engine: claude")), string.Join("\n", outcome.Notes));

            var bad = await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "motore-strano"), TemplateUrl = Template(ctx, "ddd-2", DddBranches()), Language = "it", MdeVersion = Mde, Harness = "vim",
            }));
            StringAssert.Contains(bad.Message, "The engine 'vim' does not exist");
        }

        [TestMethod]
        public async Task Say_that_fuseki_is_off_when_the_method_needs_it_and_leave_the_setting_alone()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = await Project(ctx, "con-fuseki");
            var branches = DddBranches();
            branches["main"][".mde-template/ddd/mde-template.yml"] = Ddd().Replace("languages:", "requires:\n  features: [fuseki]\nlanguages:");

            var outcome = await Add(ctx, path, Template(ctx, "ddd", branches));

            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("uses Fuseki, which is off for this project") && n.Contains("Fuseki tab")),
                          string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain").Out, "il metodo entra lo stesso, con la nota");
        }

        [TestMethod]
        public async Task Leave_the_project_as_it_was_when_the_method_cannot_be_added()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = await Project(ctx, "rifiuti");
            var head = Git(path, "rev-parse HEAD").Out;
            async Task<string> Refused(string url)
            {
                var ex = await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => Add(ctx, path, url));
                Assert.AreEqual(head, Git(path, "rev-parse HEAD").Out, ex.Message);
                Assert.AreEqual("", Git(path, "status --porcelain").Out, "niente lasciato indietro: " + ex.Message);
                Assert.AreEqual("origin\ntemplate-demo", Git(path, "remote").Out, "nessun remoto in più: " + ex.Message);
                return ex.Message;
            }

            StringAssert.Contains(await Refused(Template(ctx, "fuori", DddBranches(extraFile: "README.md"))), "has files outside its folder (README.md)");
            StringAssert.Contains(await Refused(Template(ctx, "dipende", DddBranches(requires: "iso-base"))), "needs iso-base, which the project does not have");
            StringAssert.Contains(await Refused(Template(ctx, "rotto", DddBranches(workflow: DddWorkflow.Replace("\"modella\" }", "\"inesistente\" }").Replace("\"skill\": \"modella\"", "\"skill\": \"inesistente\"")))),
                "the project's workflows cannot be read");
            StringAssert.Contains(await Refused(Template(ctx, "demo-bis", DemoBranches())), "already has the template 'demo'");
            Assert.IsFalse(File.Exists(Path.Combine(path, "responsabilita.md")), "il documento nato per il metodo rifiutato non resta");

            File.AppendAllText(Path.Combine(path, "README.md"), "una modifica\n");
            var dirty = await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(() => Add(ctx, path, Template(ctx, "ddd", DddBranches())));
            StringAssert.Contains(dirty.Message, "uncommitted changes");
        }
    }
}
