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
    /// I metodi componibili (docs-internal/Sprints/2026-10-19-Metodi-Componibili.md, M2): un progetto nasce dallo strato
    /// comune più i metodi scelti, ognuno con la sua teoria se la si chiede; la demo sta da sola.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectTemplateComposition_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private const string Changelog = "# Changelog\n\n## {0} — 2026-10-19\n\n- Versione {0}.\n";

        /// <summary>Lo strato comune: una skill, un agente, e la teoria degli LLM wiki in italiano.</summary>
        internal static Dictionary<string, Dictionary<string, string>> Foundation(string id = "documenti") => new()
        {
            ["main"] = new Dictionary<string, string>
            {
                [$".mde-template/{id}/mde-template.yml"] = $@"mde_template: 1
id: {id}
kind: foundation
title: Document Driven Development
version: 1.0.0
languages:
  it: {{ structure: main }}
roles:
  - {{ role: Redattore, agents: [{id}-redattore] }}
theory:
  it: {{ from: .mde-template/{id}/teoria, to: teoria/{id} }}
",
                [$".mde-template/{id}/agents/{id}-redattore.agent.md"] = Card.Replace("ddd-analista", id + "-redattore"),
                [$".mde-template/{id}/teoria/llm-wiki.md"] = "# Gli LLM wiki\n",
                [$".claude/skills/{id}-scrivi/SKILL.md"] = $"---\nname: {id}-scrivi\ndescription: scrive\n---\n# Scrivi\n",
            },
        };

        /// <summary>Il metodo DDD dei test di P2, con la sua teoria (due documenti, uno con un nome accentato).</summary>
        private static Dictionary<string, Dictionary<string, string>> WithTheory(string version = "1.0.0", Dictionary<string, string> theory = null)
        {
            var b = DddBranches();
            b["main"][".mde-template/ddd/mde-template.yml"] = Ddd().Replace("version: 1.0.0", $"version: {version}")
                + "theory:\n  it: { from: .mde-template/ddd/teoria, to: teoria/ddd }\n";
            b["main"][".mde-template/ddd/CHANGELOG.md"] = string.Format(Changelog, version);
            foreach (var (file, text) in theory ?? new() { ["perché.md"] = "# Perché\n", ["capitoli/uno.md"] = "# Uno\n" })
                b["main"][".mde-template/ddd/teoria/" + file] = text;
            return b;
        }

        [TestMethod]
        public async Task Create_from_the_common_layer_and_a_method_with_the_theory_only_where_asked()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "composto");
            var foundation = Template(ctx, "documenti", Foundation());
            var ddd = Template(ctx, "ddd", WithTheory());

            var outcome = await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = foundation, Methods = new[] { ddd }, Theory = new[] { ddd },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            });

            Assert.AreEqual("documenti", outcome.Id);
            Assert.AreEqual("ddd", outcome.Methods.Single().Id);
            Assert.AreEqual("# Perché\n", Read(path, "teoria/ddd/perché.md"), "la teoria chiesta, dalla zona del metodo");
            Assert.AreEqual("# Uno\n", Read(path, "teoria/ddd/capitoli/uno.md"));
            Assert.IsFalse(Directory.Exists(Path.Combine(path, "teoria", "documenti")), "la teoria dello strato comune non è stata chiesta");
            Assert.IsTrue(File.Exists(Path.Combine(path, ".mde-template", "documenti", "teoria", "llm-wiki.md")), "resta nella zona: si potrà chiedere dopo");

            var yml = Read(path, ".development.yml");
            StringAssert.Contains(yml, "- id: documenti\n  url: " + foundation + "\n  kind: foundation\n");
            StringAssert.Contains(yml, "- id: ddd\n  url: " + ddd + "\n  language: it\n  example: false\n  theory: true\n");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("The theory of «Sviluppo con DDD» in teoria/ddd/: 2 documents")), string.Join("\n", outcome.Notes));
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out, "la teoria è nel commit del metodo");
        }

        [TestMethod]
        public async Task Keep_the_demo_alone_and_the_common_layer_first()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var ddd = Template(ctx, "ddd", DddBranches());

            var demo = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "demo-e-ddd"), TemplateUrl = Template(ctx, "demo", Base()), Methods = new[] { ddd }, Language = "it", MdeVersion = Mde,
            }));
            StringAssert.Contains(demo.Message, "already has everything included: methods are not added to it");
            Assert.IsFalse(Directory.Exists(NewFolder(ctx, "demo-e-ddd")), "niente lasciato indietro");

            var second = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "strato-dopo"), TemplateUrl = ddd, Methods = new[] { Template(ctx, "documenti", Foundation()) },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            }));
            StringAssert.Contains(second.Message, "is a method: a project starts from the common layer");

            var foundationLater = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "strato-come-metodo"), TemplateUrl = Template(ctx, "documenti", Foundation()),
                Methods = new[] { Template(ctx, "documenti-bis", Foundation("documenti-bis")) }, Language = "it", MdeVersion = Mde, Harness = "claude",
            }));
            StringAssert.Contains(foundationLater.Message, "is the common layer: a project takes it at creation, as the first template");
        }

        [TestMethod]
        public async Task Refuse_a_theory_that_is_not_there_or_not_chosen()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var foundation = Template(ctx, "documenti", Foundation());
            var bare = Template(ctx, "ddd", DddBranches());

            var none = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "senza-teoria"), TemplateUrl = foundation, Methods = new[] { bare }, Theory = new[] { bare },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            }));
            StringAssert.Contains(none.Message, "The template «Sviluppo con DDD» has no theory in 'it'");
            Assert.IsFalse(Directory.Exists(NewFolder(ctx, "senza-teoria")));

            var stray = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Create(ctx, new TemplateCreateRequest
            {
                Path = NewFolder(ctx, "teoria-estranea"), TemplateUrl = foundation, Theory = new[] { bare },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            }));
            StringAssert.Contains(stray.Message, "Theory can be asked only for the chosen templates");
        }

        [TestMethod]
        public async Task Bring_the_new_theory_with_an_update_without_bringing_back_what_the_group_removed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "teoria-aggiornata");
            var ddd = Template(ctx, "ddd", WithTheory());
            await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Template(ctx, "documenti", Foundation()), Methods = new[] { ddd }, Theory = new[] { ddd },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            });
            Git(path, "config user.email io@test.local"); Git(path, "config user.name Io"); Git(path, "config commit.gpgsign false");
            Git(path, "rm -q teoria/ddd/capitoli/uno.md"); Git(path, "commit -q -m via-uno");

            // La versione 1.1.0: cambia «perché», cambia «uno» (che il gruppo ha tolto), aggiunge «due».
            var work = Path.Combine(ctx.Factory.DataDir, "autore");
            Git(ctx.Factory.DataDir, $"clone -q \"{ddd}\" \"{work}\"");
            Git(work, "config user.email autore@test.local"); Git(work, "config user.name Autore"); Git(work, "config commit.gpgsign false");
            foreach (var (file, text) in WithTheory("1.1.0", new() { ["perché.md"] = "# Perché, meglio\n", ["capitoli/uno.md"] = "# Uno, meglio\n", ["capitoli/due.md"] = "# Due\n" })["main"])
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(work, file)));
                File.WriteAllText(Path.Combine(work, file), text);
            }
            File.WriteAllText(Path.Combine(work, ".mde-template", "ddd", "CHANGELOG.md"), "# Changelog\n\n## 1.1.0 — 2026-10-20\n\n- La teoria cresce.\n\n## 1.0.0 — 2026-10-19\n\n- Versione 1.0.0.\n");
            Git(work, "add -A"); Git(work, "commit -q -m nuova"); Git(work, "push -q origin main");

            using var scope = ctx.Factory.Services.CreateScope();
            var outcome = await scope.ServiceProvider.GetRequiredService<IProjectTemplateUpdates>()
                .UpdateAsync(new TemplateUpdateRequest { ProjectPath = path, Id = "ddd", MdeVersion = Mde });

            Assert.AreEqual("# Due\n", Read(path, "teoria/ddd/capitoli/due.md"), "il documento nuovo arriva");
            Assert.IsFalse(File.Exists(Path.Combine(path, "teoria", "ddd", "capitoli", "uno.md")), "il gruppo l'aveva tolto: non rinasce");
            Assert.AreEqual("# Perché\n", Read(path, "teoria/ddd/perché.md"), "il file del gruppo non si tocca");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("changed the models of teoria/ddd/perché.md")), string.Join("\n", outcome.Notes));
            StringAssert.Contains(Read(path, ".development.yml"), "  theory: true\n", "la riga ricorda ancora la teoria");
        }

        private static async Task<TemplateAddOutcome> AddTheory(AgentCityContext ctx, string path, string id)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IProjectTemplateTheory>().AddTheoryAsync(new TemplateTheoryRequest { ProjectPath = path, Id = id });
        }

        [TestMethod]
        public async Task Download_the_theory_later_from_the_zone_with_a_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "teoria-dopo");
            await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Template(ctx, "documenti", Foundation()), Methods = new[] { Template(ctx, "ddd", WithTheory()) },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            });
            Git(path, "config user.email io@test.local"); Git(path, "config user.name Io"); Git(path, "config commit.gpgsign false");
            Assert.IsFalse(Directory.Exists(Path.Combine(path, "teoria")), "alla creazione non era stata chiesta");

            var listed = await ctx.Client.GetStringAsync("/api/MdProjects/Templates?path=" + Uri.EscapeDataString(path));
            StringAssert.Contains(listed, "\"id\":\"ddd\"");
            StringAssert.Contains(listed, "\"theory\":null,\"version\":\"1.0.0\",\"detached\":null,\"theoryAvailable\":true", "si può scaricare");

            var head = Git(path, "rev-parse HEAD").Out;
            var outcome = await AddTheory(ctx, path, "ddd");

            Assert.AreEqual("# Perché\n", Read(path, "teoria/ddd/perché.md"));
            Assert.AreEqual("# Uno\n", Read(path, "teoria/ddd/capitoli/uno.md"));
            StringAssert.Contains(Read(path, ".development.yml"), "- id: ddd\n  url: ");
            StringAssert.Contains(Read(path, ".development.yml"), "  theory: true\n");
            Assert.AreEqual(head, Git(path, "rev-parse HEAD~1").Out, "un commit solo");
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out);
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("The theory of «Sviluppo con DDD» in teoria/ddd/: 2 documents")), string.Join("\n", outcome.Notes));

            // Lo strato comune, con le modifiche del gruppo non committate: prima si committano.
            File.AppendAllText(Path.Combine(path, ".development.yml"), "\n");
            var dirty = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => AddTheory(ctx, path, "documenti"));
            StringAssert.Contains(dirty.Message, "The project has uncommitted changes");
            Git(path, "checkout -q -- .development.yml");
            await AddTheory(ctx, path, "documenti");
            Assert.AreEqual("# Gli LLM wiki\n", Read(path, "teoria/documenti/llm-wiki.md"));

            StringAssert.Contains((await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => AddTheory(ctx, path, "ddd"))).Message,
                "The theory of «Sviluppo con DDD» is already in the project, in teoria/ddd/");
            StringAssert.Contains((await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => AddTheory(ctx, path, "gara"))).Message,
                "The project has no template 'gara': it has documenti, ddd");
        }

        [TestMethod]
        public async Task Refuse_the_theory_of_a_method_that_has_none_and_leave_the_project_as_it_was()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "senza-teoria-dopo");
            await Create(ctx, new TemplateCreateRequest
            {
                Path = path, TemplateUrl = Template(ctx, "documenti", Foundation()), Methods = new[] { Template(ctx, "ddd", DddBranches()) },
                Language = "it", MdeVersion = Mde, Harness = "claude",
            });
            var head = Git(path, "rev-parse HEAD").Out;

            var none = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => AddTheory(ctx, path, "ddd"));

            StringAssert.Contains(none.Message, "The template «Sviluppo con DDD» has no theory in 'it'");
            Assert.AreEqual(head, Git(path, "rev-parse HEAD").Out);
            Assert.AreEqual("", Git(path, "status --porcelain --untracked-files=all").Out);
        }

        [TestMethod]
        public async Task Name_the_responsibilities_document_in_the_language_of_the_project()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var english = Foundation();
            var manifest = english["main"][".mde-template/documenti/mde-template.yml"].Replace("  it: { structure: main }\n", "  en: { structure: main }\n")
                .Replace("theory:\n  it: { from: .mde-template/documenti/teoria, to: teoria/documenti }\n", "");
            english["main"][".mde-template/documenti/mde-template.yml"] = manifest;
            var path = NewFolder(ctx, "in-inglese");

            var outcome = await Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = Template(ctx, "documenti", english), Language = "en", MdeVersion = Mde, Harness = "claude" });

            Assert.IsTrue(File.Exists(Path.Combine(path, "responsibilities.md")), string.Join("\n", outcome.Notes));
            Assert.IsFalse(File.Exists(Path.Combine(path, "responsabilita.md")), "un progetto inglese non ha il file italiano");
            StringAssert.Contains(Read(path, ".development.yml"), "ownershipDoc: responsibilities.md");
            var doc = Read(path, "responsibilities.md");
            StringAssert.Contains(doc, "# Who answers for which agent");
            StringAssert.Contains(doc, "| Role | Owner | Git Email | Agents |", "scritto in inglese, e il lettore lo legge");
            StringAssert.Contains(doc, "| Redattore |");
        }
    }
}
