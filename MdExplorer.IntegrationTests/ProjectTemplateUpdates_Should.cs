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
    /// Gli aggiornamenti dei template (P3 di docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md): un aggiornamento è una
    /// versione nuova del manifesto, annunciata dalle voci del CHANGELOG; chi rompe qualcosa si conferma prima; il manifesto si
    /// riapplica; ci si può staccare per sempre; una vecchia demo con «upstream» diventa un template.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class ProjectTemplateUpdates_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        /// <summary>Chi cura il template pubblica sul ramo: <paramref name="files"/> scritti (null = tolto), un commit, push.</summary>
        private static void Publish(AgentCityContext ctx, string url, string branch, Dictionary<string, string> files)
        {
            var work = Path.Combine(ctx.Factory.DataDir, "pubblica-" + Guid.NewGuid().ToString("N"));
            Git(ctx.Factory.DataDir, $"clone -q -b {branch} \"{url}\" \"{work}\"");
            Git(work, "config user.email autore@test.local"); Git(work, "config user.name Autore"); Git(work, "config commit.gpgsign false");
            foreach (var (file, text) in files)
            {
                var full = Path.Combine(work, file);
                if (text == null) { File.Delete(full); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                File.WriteAllText(full, text);
            }
            Git(work, "add -A"); Git(work, "commit -q -m nuova-versione"); Git(work, $"push -q origin {branch}");
        }

        private static async Task<IReadOnlyList<TemplateUpdateInfo>> Updates(AgentCityContext ctx, string path)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IProjectTemplateUpdates>().UpdatesAsync(path, fetch: true, Mde);
        }

        private static async Task<TemplateUpdateOutcome> Update(AgentCityContext ctx, string path, string id, bool accept = false)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IProjectTemplateUpdates>()
                .UpdateAsync(new TemplateUpdateRequest { ProjectPath = path, Id = id, AcceptIncompatible = accept, MdeVersion = Mde });
        }

        private static string DemoManifest(string version) => $"mde_template: 1\nid: demo\nkind: project\ntitle: La demo\nversion: {version}\n" +
                                                             "languages:\n  it: { structure: main }\n  en: { structure: en }\n";

        [TestMethod]
        public async Task Offer_a_new_version_with_its_notes_and_take_it_from_the_language_branch_on_a_colleagues_clone()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var url = Template(ctx, "demo", DemoBranches());
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = NewFolder(ctx, "aggiornata");
            await Create(ctx, new TemplateCreateRequest { Path = path, TemplateUrl = url, Language = "en", MdeVersion = Mde });

            // Un collega clona il progetto da origin: il remoto del template non viaggia col clone.
            var colleague = Path.Combine(ctx.Factory.DataDir, "progetti", "collega");
            Git(Path.Combine(ctx.Factory.DataDir, "progetti"), $"clone -q \"{path}.origin.git\" collega");
            Git(colleague, "config user.email collega@test.local"); Git(colleague, "config user.name Collega"); Git(colleague, "config commit.gpgsign false");

            // Commit nuovi senza versione nuova: non è un aggiornamento, e lo si dice.
            Publish(ctx, url, "en", new() { ["README.md"] = "# The demo\nversion 2\n" });
            var same = (await Updates(ctx, colleague)).Single();
            Assert.AreEqual(url, Git(colleague, "remote get-url template-demo").Out, "ricreato dalla riga di .development.yml");
            StringAssert.Contains(same.Problem, "still at version 1.0.0");

            Publish(ctx, url, "en", new()
            {
                [".mde-template/demo/mde-template.yml"] = DemoManifest("1.1.0"),
                [".mde-template/demo/CHANGELOG.md"] = "# CHANGELOG\n\n## 1.1.0 — 2026-10-19\n- Il README ha la versione.\n\n## 1.0.0 — 2026-10-18\n- Prima.\n",
            });
            var info = (await Updates(ctx, colleague)).Single();
            Assert.IsNull(info.Problem, info.Problem);
            Assert.AreEqual("1.0.0", info.Current);
            Assert.AreEqual("1.1.0", info.Available);
            Assert.AreEqual("1.1.0", info.Entries.Single().Version, "solo le voci dopo la versione del progetto");
            Assert.IsFalse(info.Incompatible);

            var outcome = await Update(ctx, colleague, "demo");

            Assert.AreEqual("1.1.0", outcome.To);
            Assert.AreEqual("# The demo\nversion 2\n", File.ReadAllText(Path.Combine(colleague, "README.md")), "dal ramo della lingua");
            StringAssert.Contains(File.ReadAllText(Path.Combine(colleague, ".development.yml")), "version: 1.1.0");
            Assert.AreEqual("", Git(colleague, "status --porcelain").Out);
            Assert.AreEqual(Git(colleague, "rev-parse HEAD").Out, Git(colleague, "rev-parse origin/main").Out, "origin allineato");
            Assert.IsNull((await Updates(ctx, colleague)).Single().Available, "aggiornato: niente di nuovo");
        }

        [TestMethod]
        public async Task Ask_to_confirm_what_a_major_version_breaks_then_reapply_the_manifest()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = await ProjectTemplateMethods_Should.Project(ctx, "metodo-2");
            var url = Template(ctx, "ddd", DddBranches());
            await Add(ctx, path, url);

            Publish(ctx, url, "main", new()
            {
                [".mde-template/ddd/mde-template.yml"] = Ddd().Replace("version: 1.0.0", "version: 2.0.0")
                    .Replace("  - { role: Product owner }\n", "  - { role: Esperto di dominio }\n"),
                [".mde-template/ddd/istruzioni.md"] = "Il glossario ora è .mde-template/ddd/linguaggio.md.\n",
                [".mde-template/ddd/CHANGELOG.md"] = "# CHANGELOG\n\n## 2.0.0 — 2026-11-01\n" +
                    "⚠️ incompatibile: il glossario si chiama linguaggio.md. Cosa fare: rinomina i rimandi.\n- Nuovo ruolo.\n\n## 1.0.0 — 2026-10-18\n- Prima.\n",
            });
            var info = (await Updates(ctx, path)).Single(u => u.Id == "ddd");
            Assert.IsTrue(info.Incompatible, info.Problem);
            var head = Git(path, "rev-parse HEAD").Out;

            var refused = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Update(ctx, path, "ddd"));
            StringAssert.Contains(refused.Message, "Read and confirm before updating");
            Assert.AreEqual(head, Git(path, "rev-parse HEAD").Out);

            File.AppendAllText(Path.Combine(path, "README.md"), "mia modifica\n");
            var dirty = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Update(ctx, path, "ddd", accept: true));
            StringAssert.Contains(dirty.Message, "uncommitted changes");
            Git(path, "checkout -- README.md");

            var outcome = await Update(ctx, path, "ddd", accept: true);

            Assert.AreEqual("2.0.0", outcome.To);
            var ownership = Read(path, "responsabilita.md");
            StringAssert.Contains(ownership, "| Esperto di dominio |", "il ruolo nuovo");
            StringAssert.Contains(ownership, "| Product owner |", "il ruolo tolto resta: è del gruppo");
            Assert.IsTrue(outcome.Notes.Any(n => n.Contains("no longer has the roles Product owner")), string.Join("\n", outcome.Notes));
            var claude = Read(path, "CLAUDE.md");
            StringAssert.Contains(claude, "<!-- mde-template:ddd -->\nIl glossario ora è .mde-template/ddd/linguaggio.md.\n<!-- /mde-template:ddd -->");
            Assert.IsFalse(claude.Contains("glossario.md: usalo") || claude.Contains("sta in .mde-template/ddd/glossario.md"), "il blocco vecchio è sostituito");
            StringAssert.Contains(Read(path, ".development.yml"), "version: 2.0.0");
            Assert.AreEqual("", Git(path, "status --porcelain").Out);
        }

        [TestMethod]
        public async Task Refuse_an_update_whose_changelog_is_not_right()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = await ProjectTemplateMethods_Should.Project(ctx, "changelog");
            var url = Template(ctx, "ddd", DddBranches());
            await Add(ctx, path, url);

            Publish(ctx, url, "main", new()
            {
                [".mde-template/ddd/mde-template.yml"] = Ddd().Replace("version: 1.0.0", "version: 2.0.0"),
                [".mde-template/ddd/CHANGELOG.md"] = "# CHANGELOG\n\n## 2.0.0 — 2026-11-01\n- Tutto nuovo.\n\n## 1.0.0 — 2026-10-18\n- Prima.\n",
            });
            var info = (await Updates(ctx, path)).Single(u => u.Id == "ddd");
            StringAssert.Contains(info.Problem, "is a major version: its entry must say what it breaks");

            Publish(ctx, url, "main", new() { [".mde-template/ddd/CHANGELOG.md"] = "# CHANGELOG\n\n## 2.0.0 — 2026-11-01\n⚠️ incompatibile: x. Cosa fare: y.\n" });
            StringAssert.Contains((await Updates(ctx, path)).Single(u => u.Id == "ddd").Problem, "no entry for 1.0.0, the project's version");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Update(ctx, path, "ddd", accept: true));
        }

        [TestMethod]
        public async Task Detach_for_good_and_stop_offering_updates()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = await ProjectTemplateMethods_Should.Project(ctx, "staccata");

            using (var scope = ctx.Factory.Services.CreateScope())
                await scope.ServiceProvider.GetRequiredService<IProjectTemplateUpdates>().DetachAsync(path, "demo");

            StringAssert.Contains(Read(path, ".development.yml"), "detached: " + DateTime.Now.ToString("yyyy-MM-dd"));
            Assert.AreNotEqual(0, Git(path, "remote get-url template-demo").Code, "il remoto è tolto");
            Assert.AreEqual(0, (await Updates(ctx, path)).Count, "staccato: niente più aggiornamenti");
            Assert.AreEqual("", Git(path, "status --porcelain").Out);
            using (var scope = ctx.Factory.Services.CreateScope())
            {
                var again = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                    scope.ServiceProvider.GetRequiredService<IProjectTemplateUpdates>().DetachAsync(path, "demo"));
                StringAssert.Contains(again.Message, "already detached");
            }
        }

        [TestMethod]
        public async Task Turn_an_old_demo_with_upstream_into_the_template()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            // La demo di prima: niente manifesto. Chi l'ha clonata ha il remoto «upstream».
            var url = Template(ctx, "demo", new() { ["main"] = new() { ["README.md"] = "# La demo\n", [".development.yml"] = "harness:\n  target: copilot\n" } });
            Directory.CreateDirectory(Path.Combine(ctx.Factory.DataDir, "progetti"));
            var path = Path.Combine(ctx.Factory.DataDir, "progetti", "vecchia");
            Git(Path.Combine(ctx.Factory.DataDir, "progetti"), $"clone -q \"{url}\" vecchia");
            Git(path, "remote rename origin upstream");
            Git(path, "config user.email io@test.local"); Git(path, "config user.name Io"); Git(path, "config commit.gpgsign false");

            // La demo diventa un template.
            Publish(ctx, url, "main", new()
            {
                [".mde-template/demo/mde-template.yml"] = DemoManifest("1.1.0"),
                [".mde-template/demo/CHANGELOG.md"] = "# CHANGELOG\n\n## 1.1.0 — 2026-10-19\n- La demo è un template.\n\n## 1.0.0 — 2026-10-18\n- Prima.\n",
            });
            var info = (await Updates(ctx, path)).Single();
            Assert.IsTrue(info.Migration);
            Assert.AreEqual("0.0.0", info.Current);
            Assert.AreEqual("1.1.0", info.Available);
            Assert.AreEqual(2, info.Entries.Count, "per una vecchia demo ogni voce è nuova");

            await Update(ctx, path, "demo");

            Assert.AreEqual(url, Git(path, "remote get-url template-demo").Out, "upstream è diventato template-demo");
            Assert.AreNotEqual(0, Git(path, "remote get-url upstream").Code);
            var yml = Read(path, ".development.yml");
            StringAssert.Contains(yml, "- id: demo\n");
            StringAssert.Contains(yml, "kind: project");
            StringAssert.Contains(yml, "language: it");
            StringAssert.Contains(yml, "version: 1.1.0");
            Assert.IsFalse((await Updates(ctx, path)).Single().Migration, "la riga ora c'è");
        }
    }
}
