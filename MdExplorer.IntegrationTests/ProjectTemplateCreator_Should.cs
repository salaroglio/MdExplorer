using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Nuovo progetto da un template (P1 di docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md): il template è un
    /// repository git con il suo manifesto; il progetto nasce dal ramo della lingua scelta, ricorda il template in
    /// <c>.development.yml</c> e si pubblica su <c>origin</c>.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectTemplateCreator_Should
    {
        internal const string Mde = "2026.10.18";

        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        internal static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true,
                }
            };
            p.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            p.Start();
            var o = p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            p.WaitForExit(20000);
            return (p.ExitCode, o.Trim());
        }

        internal static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        internal static async Task<TemplateCreateOutcome> Create(AgentCityContext ctx, TemplateCreateRequest request)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IProjectTemplateCreator>().CreateAsync(request);
        }

        /// <summary>Un repository di template: un ramo per ogni voce di <paramref name="branches"/>, con i suoi file.</summary>
        internal static string Template(AgentCityContext ctx, string name, Dictionary<string, Dictionary<string, string>> branches)
        {
            var root = Path.Combine(ctx.Factory.DataDir, "template", name);
            var bare = Path.Combine(root, name + ".git");
            var work = Path.Combine(root, "lavoro");
            Directory.CreateDirectory(bare);
            Git(bare, "init -q --bare -b main");
            Directory.CreateDirectory(work);
            Git(work, "init -q -b main");
            Git(work, "config user.email autore@test.local"); Git(work, "config user.name Autore"); Git(work, "config commit.gpgsign false");
            foreach (var (branch, files) in branches)
            {
                Git(work, $"checkout -q --orphan {branch}");
                Git(work, "rm -rq --cached --ignore-unmatch .");
                foreach (var e in Directory.GetFileSystemEntries(work))
                    if (Path.GetFileName(e) != ".git") { if (Directory.Exists(e)) Directory.Delete(e, true); else File.Delete(e); }
                foreach (var (file, text) in files)
                {
                    var full = Path.Combine(work, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(full));
                    File.WriteAllText(full, text);
                }
                Git(work, "add -A"); Git(work, $"commit -q -m {branch}");
                Git(work, $"push -q \"{bare}\" {branch}");
            }
            return bare;
        }

        private static string Demo(string language, string requires = null) => $@"mde_template: 1
id: demo
kind: project
title: La demo
version: 1.0.0
{(requires == null ? "" : $"requires:\n  mde: {requires}\n")}languages:
  it: {{ structure: main }}
  en: {{ structure: en }}
";

        internal static Dictionary<string, Dictionary<string, string>> DemoBranches(string requires = null) => new()
        {
            ["main"] = new() { [".mde-template/demo/mde-template.yml"] = Demo("it", requires), ["README.md"] = "# La demo\n", [".development.yml"] = "harness:\n  target: copilot\n" },
            ["en"] = new() { [".mde-template/demo/mde-template.yml"] = Demo("en", requires), ["README.md"] = "# The demo\n", [".development.yml"] = "harness:\n  target: copilot\n" },
        };

        /// <summary>Lo strato comune dei test (un template <c>foundation</c>), da cui nasce un progetto con dei metodi.</summary>
        internal static string Strato(AgentCityContext ctx) => Template(ctx, "documenti-" + Guid.NewGuid().ToString("N")[..8], ProjectTemplateComposition_Should.Foundation());

        internal static string NewFolder(AgentCityContext ctx, string name) => Path.Combine(ctx.Factory.DataDir, "progetti", name);

        [TestMethod]
        public async Task Create_the_project_from_the_branch_of_the_language_and_remember_the_template()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "demo", DemoBranches());
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "la-mia-demo");

            var outcome = await Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "en", MdeVersion = Mde });

            Assert.AreEqual("demo", outcome.Id);
            Assert.AreEqual("1.0.0", outcome.Version);
            Assert.AreEqual(path + ".origin.git", outcome.Origin);
            Assert.AreEqual("# The demo\n", File.ReadAllText(Path.Combine(path, "README.md")));
            Assert.AreEqual("main", Git(path, "rev-parse --abbrev-ref HEAD").Out);
            Assert.AreEqual(url, Git(path, "remote get-url template-demo").Out);

            var yml = File.ReadAllText(Path.Combine(path, ".development.yml"));
            StringAssert.Contains(yml, "target: copilot", "la configurazione del template resta");
            StringAssert.Contains(yml, "templates:\n- id: demo\n");
            StringAssert.Contains(yml, "kind: project");
            StringAssert.Contains(yml, "language: en");
            StringAssert.Contains(yml, "version: 1.0.0");
            StringAssert.Contains(yml, "roomSecret: ", "il segreto della stanza nasce nel commit della creazione, non all'apertura");

            Assert.AreEqual("", Git(path, "status --porcelain").Out, "tutto committato");
            Assert.AreEqual("2", Git(path, "rev-list --count HEAD").Out, "la storia del template più il commit della creazione");
            Assert.AreEqual(Git(path, "rev-parse HEAD").Out, Git(path, "rev-parse origin/main").Out, "pubblicato su origin");
            Assert.AreEqual("origin/main", Git(path, "rev-parse --abbrev-ref origin/HEAD").Out);
            Assert.AreEqual("origin/main", Git(path, "rev-parse --abbrev-ref main@{upstream}").Out);
        }

        [TestMethod]
        public async Task Create_from_a_base_whose_workflow_is_old_and_say_it_stays_stopped()
        {
            // 18/10/2026: il ramo inglese della demo aveva ancora un workflow v2. La base nasce lo stesso, e lo si dice.
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var branches = DemoBranches();
            branches["en"][".development.yml"] = "harness:\n  target: copilot\nagentCity:\n  workflowDocs:\n  - wf.md\n";
            branches["en"]["wf.md"] = "---\nmde_type: workflow\nworkflow: wf.workflow.json\n---\n# wf\n";
            branches["en"]["wf.workflow.json"] = "{ \"mde_workflow\": 2, \"title\": \"Old\", \"steps\": [] }";
            var url = Template(ctx, "demo", branches);
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "workflow-vecchio");

            var outcome = await Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "en", MdeVersion = Mde });

            Assert.IsTrue(Directory.Exists(path));
            Assert.IsTrue(System.Linq.Enumerable.Any(outcome.Notes, n => n.Contains("workflows cannot be read") && n.Contains("stay stopped")),
                string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain").Out);
        }

        [TestMethod]
        public async Task Give_the_list_of_templates_that_ships_with_MdExplorer()
        {
            using var ctx = new AgentCityContext();
            var (status, json) = await ctx.GetJson("/api/MdProjects/TemplateCatalog");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);
            var demo = System.Linq.Enumerable.Single(json.RootElement.GetProperty("templates").EnumerateArray(), t => t.GetProperty("id").GetString() == "demo");
            Assert.AreEqual("project", demo.GetProperty("kind").GetString());
            Assert.AreEqual("https://github.com/salaroglio/mdexplorer-demo.git", demo.GetProperty("url").GetString());
        }

        [TestMethod]
        public async Task Publish_on_the_empty_repository_the_group_gave()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "demo", DemoBranches());
            var group = Path.Combine(ctx.Factory.DataDir, "gruppo.git");
            Directory.CreateDirectory(group);
            Git(group, "init -q --bare -b main");
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "del-gruppo");

            var outcome = await Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "it", OriginUrl = group, MdeVersion = Mde });

            Assert.AreEqual(group, outcome.Origin);
            Assert.IsFalse(Directory.Exists(path + ".origin.git"), "con un repository del gruppo niente origin locale");
            Assert.AreEqual(Git(path, "rev-parse HEAD").Out, Git(group, "rev-parse main").Out);
            Assert.AreEqual("# La demo\n", File.ReadAllText(Path.Combine(path, "README.md")));
        }

        [TestMethod]
        public async Task Take_the_example_of_a_method_and_keep_its_files_in_its_own_folder()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            const string manifest = "mde_template: 1\nid: gara-iso\nkind: method\ntitle: Gara ISO\nversion: 0.1.0\n" +
                                    "languages:\n  it: { structure: main, example: esempio }\ninstructions: .mde-template/gara-iso/istruzioni.md\n";
            var url = Template(ctx, "gara-iso", new()
            {
                ["main"] = new() { [".mde-template/gara-iso/mde-template.yml"] = manifest, [".mde-template/gara-iso/istruzioni.md"] = "x\n" },
                ["esempio"] = new()
                {
                    [".mde-template/gara-iso/mde-template.yml"] = manifest, [".mde-template/gara-iso/istruzioni.md"] = "x\n",
                    [".mde-template/gara-iso/esempio/gara.md"] = "# gara\n", [".claude/skills/gara-iso-riesame/SKILL.md"] = "s\n",
                },
            });
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "con-esempio");

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Strato(ctx), Methods = new[] { url }, Language = "it", Example = true, MdeVersion = Mde, Harness = "claude",
            });
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("«Scrittura sistematica dei documenti» has no example in 'it': only its structure comes in")),
                string.Join("\n", outcome.Notes));
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, ".development.yml")), "  kind: foundation\n  language: it\n  example: false\n",
                "lo strato comune senza esempio: la riga dice che è arrivata la struttura");

            Assert.IsTrue(File.Exists(Path.Combine(path, ".mde-template/gara-iso/esempio/gara.md")));
            var yml = File.ReadAllText(Path.Combine(path, ".development.yml"));
            StringAssert.Contains(yml, "- id: gara-iso\n  url: " + url + "\n  language: it\n  example: true\n", "un metodo non scrive kind");
        }

        [TestMethod]
        public async Task Refuse_a_method_with_files_outside_its_folder_and_leave_nothing_behind()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "fuori", new()
            {
                ["main"] = new()
                {
                    [".mde-template/ddd/mde-template.yml"] = "mde_template: 1\nid: ddd\nkind: method\ntitle: DDD\nversion: 1.0.0\nlanguages:\n  it: { structure: main }\n",
                    ["README.md"] = "# fuori zona\n",
                },
            });
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "fuori");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = Strato(ctx), Methods = new[] { url }, Language = "it", MdeVersion = Mde, Harness = "claude" }));

            StringAssert.Contains(ex.Message, "README.md");
            Assert.IsFalse(Directory.Exists(path), "la cartella creata è stata tolta");
            Assert.IsFalse(Directory.Exists(path + ".origin.git"));
        }

        [TestMethod]
        public async Task Refuse_a_repository_that_is_not_a_template()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "normale", new() { ["main"] = new() { ["README.md"] = "# un repository qualsiasi\n" } });
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "normale");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "it", MdeVersion = Mde }));

            StringAssert.Contains(ex.Message, "mde-template.yml");
            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public async Task Refuse_a_template_that_needs_a_newer_MdExplorer()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "nuovo", DemoBranches(requires: "2027.1.1"));
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "troppo-nuovo");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "it", MdeVersion = Mde }));

            StringAssert.Contains(ex.Message, "2027.1.1");
            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public async Task Never_create_into_a_folder_that_already_has_files()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "demo", DemoBranches());
            var path = NewFolder(ctx, "piena");
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "mio.md"), "# mio\n");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "it", MdeVersion = Mde }));

            StringAssert.Contains(ex.Message, path);
            Assert.AreEqual("# mio\n", File.ReadAllText(Path.Combine(path, "mio.md")), "non si tocca niente");
        }
    }
}
