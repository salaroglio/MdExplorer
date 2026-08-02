using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La vista delle differenze: cosa è cambiato qui, e rispetto a cosa.
    /// <para>
    /// La domanda è la stessa per il lavoro dell'utente e per quello di un agente, quindi la
    /// risposta arriva da un solo confronto contro il ramo di partenza — che comprende sia i
    /// commit già fatti sia le modifiche ancora da salvare. Un tab che mostrasse solo i commit
    /// direbbe "nessuna modifica" a una persona che sta scrivendo.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class WorkingChanges_Should
    {
        [TestMethod]
        public async Task See_what_the_user_changed_before_committing_anything()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-utente");

            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n\nriga aggiunta\n");   // modificato
            File.WriteAllText(Path.Combine(path, "nuovo.md"), "# nuovo\n");                    // mai visto da git
            File.Delete(Path.Combine(path, "vecchio.md"));                                     // eliminato

            var view = await Service(ctx).GetAsync(path, null);

            Assert.IsNull(view.Problem, view.Problem);
            Assert.AreEqual("user", view.ContextKind);
            var byPath = view.Files.ToDictionary(f => f.Path, f => f.Change);

            Assert.AreEqual("modified", byPath["README.md"]);
            Assert.AreEqual("deleted", byPath["vecchio.md"]);
            // Un file appena creato è il caso più comune, e non compare in nessun diff: senza
            // leggere anche i non tracciati risulterebbe inesistente.
            Assert.AreEqual("untracked", byPath["nuovo.md"]);
        }

        [TestMethod]
        public async Task Follow_the_context_to_the_agent_workplace()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-agente");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            Assert.IsTrue(prep.Success, prep.Error);
            File.WriteAllText(Path.Combine(prep.WorktreePath, "prodotto-da-alfa.md"), "# fatto\n");

            var mine = await Service(ctx).GetAsync(path, null);
            var his = await Service(ctx).GetAsync(path, "alfa");

            Assert.AreEqual("agent", his.ContextKind);
            Assert.AreEqual("alfa", his.ContextLabel);
            Assert.AreEqual(prep.WorktreePath, his.RootPath, "il contesto dell'agente è il suo posto di lavoro");
            CollectionAssert.Contains(his.Files.Select(f => f.Path).ToList(), "prodotto-da-alfa.md");

            // I due contesti non si mescolano: il lavoro dell'agente non è nel progetto dell'utente.
            CollectionAssert.DoesNotContain(mine.Files.Select(f => f.Path).ToList(), "prodotto-da-alfa.md");
        }

        [TestMethod]
        public async Task Include_what_the_agent_already_committed()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-committato");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            File.WriteAllText(Path.Combine(prep.WorktreePath, "consegna.md"), "# consegnato\n");
            Assert.IsNotNull(await m.CommitAndPushBranchAsync(path, "alfa", "consegna"));

            // Committato e pubblicato: è esattamente ciò che l'umano deve poter guardare prima
            // di autorizzare. Un confronto solo con la working tree non lo mostrerebbe.
            var view = await Service(ctx).GetAsync(path, "alfa");
            CollectionAssert.Contains(view.Files.Select(f => f.Path).ToList(), "consegna.md");
        }

        [TestMethod]
        public async Task Show_the_content_of_a_file_that_git_never_saw()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-diff-nuovo");
            File.WriteAllText(Path.Combine(path, "nuovo.md"), "# nuovo\nriga uno\n");

            var diff = await Service(ctx).DiffAsync(path, null, "nuovo.md");

            // Contro il ramo di partenza un file mai aggiunto non ha un "prima": il diff sarebbe
            // vuoto e sembrerebbe che non sia cambiato niente.
            StringAssert.Contains(diff, "+riga uno");
        }

        [TestMethod]
        public async Task Put_a_file_back_the_way_it_was()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-scarto");
            var readme = Path.Combine(path, "README.md");
            File.WriteAllText(readme, "# rovinato\n");
            var nuovo = Path.Combine(path, "mai-voluto.md");
            File.WriteAllText(nuovo, "# svista\n");

            await Service(ctx).DiscardAsync(path, null, "README.md");
            StringAssert.Contains(File.ReadAllText(readme), "# doc", "torna com'era sul ramo di partenza");

            // Un file mai aggiunto a git non ha un "com'era": l'unico ripristino possibile è che
            // non esista.
            await Service(ctx).DiscardAsync(path, null, "mai-voluto.md");
            Assert.IsFalse(File.Exists(nuovo));
        }

        [TestMethod]
        public async Task Refuse_to_look_outside_the_context()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-traversal");

            // Il percorso arriva dal client: senza controllo, un '../..' leggerebbe — o peggio
            // ripristinerebbe — file fuori dal progetto.
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => Service(ctx).DiffAsync(path, null, "../../../etc/passwd"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => Service(ctx).DiscardAsync(path, null, "../fuori.md"));
        }

        [TestMethod]
        public async Task Say_when_the_agent_has_nowhere_to_be_looked_at()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-senza-posto");

            var res = await ctx.Client.GetAsync(
                "/api/WorkingChanges/list?projectPath=" + Uri.EscapeDataString(path) + "&agent=fantasma");

            // Non "nessuna modifica": un agente senza posto di lavoro è una condizione che
            // l'utente può risolvere, e va detta.
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, res.StatusCode);
            StringAssert.Contains(await res.Content.ReadAsStringAsync(), "posto di lavoro");
        }

        [TestMethod]
        public async Task Answer_over_http_for_the_tab()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "chg-http");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n\nmodifica\n");

            var res = await ctx.Client.GetAsync(
                "/api/WorkingChanges/list?projectPath=" + Uri.EscapeDataString(path));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var body = await res.Content.ReadAsStringAsync();
            StringAssert.Contains(body, "README.md");
            StringAssert.Contains(body, "\"contextKind\":\"user\"");

            var discard = await ctx.Client.PostAsync("/api/WorkingChanges/discard",
                new StringContent(
                    $"{{\"projectPath\":{System.Text.Json.JsonSerializer.Serialize(path)},\"path\":\"README.md\"}}",
                    Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.OK, discard.StatusCode, await discard.Content.ReadAsStringAsync());
            StringAssert.Contains(File.ReadAllText(Path.Combine(path, "README.md")), "# doc");
        }

        // ---- infrastruttura ----

        private static IWorkingChangesService Service(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IWorkingChangesService>();

        private static (int Code, string Out) Git(string cwd, string args)
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
            p.WaitForExit(60000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static (Guid Key, string Path) SetupGitProject(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);

            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init --bare");
            Git(origin, "symbolic-ref HEAD refs/heads/main");

            Git(path, "init -b main");
            Git(path, "config user.email carlo@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");
            File.WriteAllText(Path.Combine(path, "vecchio.md"), "# vecchio\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\"");
            Git(path, "push -u origin main");

            return (key, path);
        }
    }
}
