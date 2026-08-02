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
    /// I posti di lavoro sono un pool, non una cartella per agente.
    /// <para>
    /// Un worktree costa una copia intera della documentazione: con un posto a testa, una città di
    /// dieci agenti moltiplicava per dieci il progetto sul disco. Con due posti — il default — due
    /// agenti lavorano davvero insieme e il terzo subentra a chi è fermo da più tempo, che è quello
    /// che farebbe una persona con due scrivanie.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentWorktreePool_Should
    {
        [TestMethod]
        public async Task Put_the_workplaces_inside_the_project()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-dentro");
            var m = Manager(ctx);

            var prep = await m.PrepareForRunAsync(path, "alfa", "att1");
            Assert.IsTrue(prep.Success, prep.Error);

            // Dentro il progetto: se la documentazione si sposta o si cancella, i posti la
            // seguono invece di restare orfani in AppData.
            StringAssert.StartsWith(prep.WorktreePath, Path.Combine(path, ".worktrees"));
            Assert.AreEqual("slot-1", Path.GetFileName(prep.WorktreePath),
                "i posti si chiamano per numero: il nome della cartella non dice più chi ci lavora");
        }

        [TestMethod]
        public async Task Give_two_agents_two_different_workplaces()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-due");
            var m = Manager(ctx);

            var a = await m.PrepareForRunAsync(path, "alfa", "att1");
            var b = await m.PrepareForRunAsync(path, "beta", "att1");
            Assert.IsTrue(a.Success, a.Error);
            Assert.IsTrue(b.Success, b.Error);

            Assert.AreNotEqual(a.WorktreePath, b.WorktreePath,
                "due agenti insieme sono il motivo per cui i posti di default sono due");

            var slots = await m.ListSlotsAsync(path);
            CollectionAssert.AreEquivalent(
                new[] { "alfa", "beta" },
                slots.Select(x => x.Agent).ToList(),
                "chi occupa un posto lo dice git, dal branch in checkout");
        }

        [TestMethod]
        public async Task Take_the_agent_back_to_its_own_workplace()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-ritorno");
            var m = Manager(ctx);

            var first = await m.PrepareForRunAsync(path, "alfa", "att1");
            var again = await m.PrepareForRunAsync(path, "alfa", "att2");

            Assert.AreEqual(first.WorktreePath, again.WorktreePath,
                "lo stesso agente ritrova il suo posto: cambia l'attività, non la scrivania");
        }

        [TestMethod]
        public async Task Hand_the_workplace_over_when_the_pool_is_full()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-subentro");
            var m = Manager(ctx);

            var a = await m.PrepareForRunAsync(path, "alfa", "att1");
            var b = await m.PrepareForRunAsync(path, "beta", "att1");
            Assert.IsTrue(a.Success && b.Success);

            // I due turni finiscono: le scrivanie non sono più prenotate, ma restano occupate
            // dal lavoro dei due agenti (il branch in checkout).
            m.ReleaseSlot(a.WorktreePath);
            m.ReleaseSlot(b.WorktreePath);

            // Entrambi i posti presi. Il terzo agente non deve fallire: subentra.
            var c = await m.PrepareForRunAsync(path, "gamma", "att1");
            Assert.IsTrue(c.Success, c.Error);

            var slots = await m.ListSlotsAsync(path);
            Assert.AreEqual(2, slots.Count, "i posti restano due: nessuno se ne aggiunge di nascosto");
            CollectionAssert.Contains(slots.Select(x => x.Agent).ToList(), "gamma");
        }

        [TestMethod]
        public async Task Never_take_a_workplace_where_a_person_is_working()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-sessione");
            var m = Manager(ctx);
            var hold = ctx.Factory.Services.GetRequiredService<IAgentWorktreeHoldService>();

            var a = await m.PrepareForRunAsync(path, "alfa", "att1");
            var b = await m.PrepareForRunAsync(path, "beta", "att1");
            Assert.IsTrue(a.Success && b.Success);
            m.ReleaseSlot(a.WorktreePath);
            m.ReleaseSlot(b.WorktreePath);

            // Una persona sta rivedendo il lavoro di alfa: dentro quel posto c'è del lavoro
            // umano non salvato, che un subentro cancellerebbe con reset --hard.
            hold.Open(path, "alfa", "revisione");

            var c = await m.PrepareForRunAsync(path, "gamma", "att1");
            Assert.IsTrue(c.Success, c.Error);
            Assert.AreEqual(b.WorktreePath, c.WorktreePath,
                "gamma subentra a beta, non a chi ha una sessione aperta");
        }

        [TestMethod]
        public async Task Say_why_when_there_is_nowhere_left_to_work()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-pieno");
            var m = Manager(ctx);
            var hold = ctx.Factory.Services.GetRequiredService<IAgentWorktreeHoldService>();

            m.ReleaseSlot((await m.PrepareForRunAsync(path, "alfa", "att1")).WorktreePath);
            m.ReleaseSlot((await m.PrepareForRunAsync(path, "beta", "att1")).WorktreePath);
            hold.Open(path, "alfa", "revisione");
            hold.Open(path, "beta", "revisione");

            // Nessun posto disponibile: la risposta deve dire cosa fare, non "errore interno".
            var c = await m.PrepareForRunAsync(path, "gamma", "att1");
            Assert.IsFalse(c.Success, "con tutti i posti bloccati il run non può partire");
            StringAssert.Contains(c.Error, "posti di lavoro");
            StringAssert.Contains(c.Error, "impostazioni del progetto",
                "l'errore deve dire anche come sbloccarsi");
        }

        [TestMethod]
        public async Task Rebuild_the_work_when_its_workplace_was_recycled()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "pool-rimateria");
            var m = Manager(ctx);

            var a = await m.PrepareForRunAsync(path, "alfa", "att1");
            File.WriteAllText(Path.Combine(a.WorktreePath, "lavoro-di-alfa.md"), "# fatto da alfa\n");
            var push = await m.CommitAndPushBranchAsync(path, "alfa", "lavoro di alfa");
            Assert.IsNotNull(push, "il lavoro va committato e pubblicato");

            // Altri due agenti si prendono i posti: quello di alfa viene riciclato.
            m.ReleaseSlot(a.WorktreePath);
            m.ReleaseSlot((await m.PrepareForRunAsync(path, "beta", "att1")).WorktreePath);
            m.ReleaseSlot((await m.PrepareForRunAsync(path, "gamma", "att1")).WorktreePath);
            Assert.IsNull(await m.FindAgentWorktreeAsync(path, "alfa"), "alfa non occupa più nessun posto");

            // Ma il lavoro non è perso: è un branch, e "ci metto mano" lo rimette su un posto.
            var back = await m.MaterializeForReviewAsync(path, "alfa", push.LocalBranch);
            Assert.IsTrue(File.Exists(Path.Combine(back, "lavoro-di-alfa.md")),
                "il lavoro di alfa deve tornare disponibile per la revisione");
        }

        // ---- infrastruttura ----

        private static IAgentWorktreeManager Manager(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

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
            // Il bare nasce con HEAD su 'master' mentre pubblichiamo 'main': senza questo,
            // origin/HEAD non si risolve e il prepare non trova il branch di base.
            Git(origin, "symbolic-ref HEAD refs/heads/main");

            Git(path, "init -b main");
            Git(path, "config user.email carlo@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# doc\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\"");
            Git(path, "push -u origin main");

            return (key, path);
        }
    }
}
