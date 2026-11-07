using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// N3 della valutazione del 07/11: «Scarta». Prima ripristinava dal remoto (<c>@{upstream}</c>): nel tuo lavoro cancellava dal file
    /// i tuoi commit non pubblicati, o ci metteva la versione di un collega scaricata e non unita. Ora, nel tuo lavoro, butta solo le
    /// modifiche non ancora in un commit (torna a HEAD); nella revisione di un agente riporta il file a dove il suo ramo è nato, con un
    /// commit nel suo ramo. Richiede <c>git</c> nel PATH.
    /// </summary>
    [TestClass]
    public class WorkingChangesDiscard_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        // ---- il tuo lavoro ----

        [TestMethod]
        public async Task Throw_away_only_what_is_not_in_a_commit_and_keep_the_unpushed_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "scarta-commit");
            var readme = Path.Combine(path, "README.md");
            File.WriteAllText(readme, "# doc\nnel mio commit\n");
            Git(path, "commit -qam \"mio, non pubblicato\"");
            File.WriteAllText(readme, "# doc\nnel mio commit\nin corso\n");

            await Service(ctx).DiscardAsync(path, null, "README.md");

            Assert.AreEqual("# doc\nnel mio commit\n", Read(readme), "torna al mio ultimo commit, non al remoto");
            StringAssert.Contains(Git(path, "log --format=%s -1").Out, "mio, non pubblicato");
            Assert.AreEqual(string.Empty, Git(path, "status --porcelain").Out.Trim(), "niente pronto per un commit che annulli il mio");
        }

        [TestMethod]
        public async Task Not_bring_in_a_colleague_version_downloaded_but_not_merged()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "scarta-collega");
            var origin = Git(path, "remote get-url origin").Out.Trim();
            var colleague = Path.Combine(ctx.Factory.DataDir, "collega-scarta");
            Git(ctx.Factory.DataDir, $"clone -q \"{origin}\" \"{colleague}\"");
            Git(colleague, "config user.email c@test.local"); Git(colleague, "config user.name C");
            File.WriteAllText(Path.Combine(colleague, "README.md"), "# doc\ndel collega\n");
            Git(colleague, "commit -qam collega"); Git(colleague, "push -q origin main");
            Git(path, "fetch -q origin");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\nin corso\n");

            await Service(ctx).DiscardAsync(path, null, "README.md");

            Assert.AreEqual("# doc\n", Read(Path.Combine(path, "README.md")), "la mia versione, non quella del collega");
        }

        [TestMethod]
        public async Task Refuse_a_file_that_differs_only_by_a_commit()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "scarta-solo-commit");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\ncommittato\n");
            Git(path, "commit -qam committato");

            var view = await Service(ctx).GetAsync(path, null);
            CollectionAssert.Contains(view.Repos[0].Files.Select(f => f.Path).ToList(), "README.md", "l'elenco lo mostra (da pubblicare)");
            CollectionAssert.DoesNotContain(view.Repos[0].Uncommitted.Select(f => f.Path).ToList(), "README.md", "ma non fra i non salvati: niente «Scarta»");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => Service(ctx).DiscardAsync(path, null, "README.md"));
            StringAssert.Contains(ex.Message, "commit");
            Assert.AreEqual("# doc\ncommittato\n", Read(Path.Combine(path, "README.md")));
        }

        [TestMethod]
        public async Task Undo_a_rename_and_a_new_file_added_to_the_index()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "scarta-rinomina");
            Git(path, "mv vecchio.md nuovo-nome.md");
            File.WriteAllText(Path.Combine(path, "aggiunto.md"), "# aggiunto\n");
            Git(path, "add aggiunto.md");

            await Service(ctx).DiscardAsync(path, null, "nuovo-nome.md", oldPath: "vecchio.md");
            await Service(ctx).DiscardAsync(path, null, "aggiunto.md");

            Assert.IsTrue(File.Exists(Path.Combine(path, "vecchio.md")));
            Assert.IsFalse(File.Exists(Path.Combine(path, "nuovo-nome.md")));
            Assert.IsFalse(File.Exists(Path.Combine(path, "aggiunto.md")));
            Assert.AreEqual(string.Empty, Git(path, "status --porcelain").Out.Trim());
        }

        // ---- la revisione di un agente ----

        [TestMethod]
        public async Task Take_a_committed_file_out_of_the_agent_work_with_a_commit_in_its_branch()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "scarta-agente");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();
            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            Assert.IsTrue(prep.Success, prep.Error);
            File.WriteAllText(Path.Combine(prep.WorktreePath, "README.md"), "# doc\ndell'agente\n");
            File.WriteAllText(Path.Combine(prep.WorktreePath, "consegna.md"), "# consegna\n");
            Assert.IsNotNull(await m.CommitAndPushBranchAsync(path, "alfa", "consegna"));
            // Nel frattempo il ramo principale va avanti: «com'era» è dove l'agente è partito, non qui.
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\ndell'utente, dopo\n");
            Git(path, "commit -qam dopo"); Git(path, "push -q origin main");

            var outcome = await Service(ctx).DiscardAsync(path, "alfa", "README.md");

            Assert.AreEqual("tolto dal lavoro dell'agente", outcome);
            Assert.AreEqual("# doc\n", Read(Path.Combine(prep.WorktreePath, "README.md")), "com'era quando l'agente è partito");
            StringAssert.Contains(Git(prep.WorktreePath, "log --format=%s -1").Out, "MDE: tolto README.md dal lavoro dell'agente");
            Assert.AreEqual(string.Empty, Git(prep.WorktreePath, "status --porcelain --untracked-files=no").Out.Trim());
            var view = await Service(ctx).GetAsync(path, "alfa");
            var files = view.Repos[0].Files.Select(f => f.Path).ToList();
            CollectionAssert.DoesNotContain(files, "README.md", "la richiesta di unione non lo porta più");
            CollectionAssert.Contains(files, "consegna.md", "il resto del lavoro resta");

            await Service(ctx).DiscardAsync(path, "alfa", "consegna.md");
            Assert.IsFalse(File.Exists(Path.Combine(prep.WorktreePath, "consegna.md")), "creato dall'agente: via");
            CollectionAssert.DoesNotContain((await Service(ctx).GetAsync(path, "alfa")).Repos[0].Files.Select(f => f.Path).ToList(), "consegna.md");
        }

        // ---- appoggi ----

        private static IWorkingChangesService Service(AgentCityContext ctx) => ctx.Factory.Services.GetRequiredService<IWorkingChangesService>();

        private static string Read(string file) => File.ReadAllText(file).Replace("\r\n", "\n");

        private static (int Code, string Out) Git(string cwd, string args)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git", Arguments = args, WorkingDirectory = cwd,
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
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
