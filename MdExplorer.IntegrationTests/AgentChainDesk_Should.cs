using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Una catena locale è un lavoro solo: il secondo agente eredita la scrivania e il ramo del
    /// primo.
    /// <para>
    /// «Analizza e poi implementa» nasce da un solo gesto umano. Su due rami separati diventano
    /// due richieste di merge per una cosa sola, con la seconda che dipende dalla prima. Su un
    /// ramo solo la revisione vede l'analisi <b>e</b> il codice insieme, e può giudicare se il
    /// secondo fa quello che il primo diceva.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentChainDesk_Should
    {
        [TestMethod]
        public async Task Hand_the_desk_over_without_wiping_it()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "catena-consegna");
            var m = Manager(ctx);

            // L'analista lavora e lascia il suo documento NON committato: è la consegna.
            var first = await m.PrepareForRunAsync(path, "analista", "att1");
            Assert.IsTrue(first.Success, first.Error);
            var analysis = Path.Combine(first.WorktreePath, "analisi.md");
            File.WriteAllText(analysis, "# analisi funzionale\n\nun comando che saluta\n");
            var branch = await m.CurrentBranchAsync(first.WorktreePath);
            m.ReleaseSlot(first.WorktreePath);   // il turno dell'analista finisce

            var second = await m.ContinueChainAsync(path, branch);

            Assert.IsTrue(second.Success, second.Error);
            Assert.AreEqual(first.WorktreePath, second.WorktreePath, "stessa scrivania");
            Assert.AreEqual(branch, await m.CurrentBranchAsync(second.WorktreePath), "stesso ramo");

            // Il punto di tutto: reset --hard e clean -fd servono a ripulire un posto che arriva
            // da un lavoro ESTRANEO. Qui quello che c'è sopra è il motivo per cui il secondo
            // agente è stato chiamato, e ripulirlo cancellerebbe la consegna.
            Assert.IsTrue(File.Exists(analysis), "il documento dell'analista deve sopravvivere al passaggio");
            StringAssert.Contains(File.ReadAllText(analysis), "un comando che saluta");
        }

        [TestMethod]
        public async Task Wait_instead_of_stealing_the_desk_from_someone_still_writing()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "catena-attesa");
            var m = Manager(ctx);

            // L'analista NON ha ancora finito: la prenotazione del suo turno è ancora aperta.
            var first = await m.PrepareForRunAsync(path, "analista", "att1");
            var branch = await m.CurrentBranchAsync(first.WorktreePath);

            var second = await m.ContinueChainAsync(path, branch);

            // Non è un errore: è un «non adesso». «Lo stato in cui il mittente si trova» esiste
            // solo quando il mittente si è fermato.
            Assert.IsFalse(second.Success);
            Assert.IsTrue(second.Busy, "dev'essere un'attesa, non un fallimento che consuma tentativi");

            m.ReleaseSlot(first.WorktreePath);
            Assert.IsTrue((await m.ContinueChainAsync(path, branch)).Success,
                "finito il turno del mittente, la catena riparte");
        }

        [TestMethod]
        public async Task Wait_when_a_person_is_reviewing_that_desk()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "catena-revisione");
            var m = Manager(ctx);
            var hold = ctx.Factory.Services.GetRequiredService<IAgentWorktreeHoldService>();

            var first = await m.PrepareForRunAsync(path, "analista", "att1");
            var branch = await m.CurrentBranchAsync(first.WorktreePath);
            m.ReleaseSlot(first.WorktreePath);
            hold.Open(path, "analista", "revisione");

            var second = await m.ContinueChainAsync(path, branch);

            Assert.IsTrue(second.Busy, "lì dentro c'è lavoro umano: la catena aspetta");
        }

        [TestMethod]
        public async Task Say_it_plainly_when_the_desk_was_recycled()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "catena-persa");
            var m = Manager(ctx);

            var first = await m.PrepareForRunAsync(path, "analista", "att1");
            var branch = await m.CurrentBranchAsync(first.WorktreePath);
            m.ReleaseSlot(first.WorktreePath);

            // Due agenti estranei si prendono i posti: quello della catena viene riciclato.
            m.ReleaseSlot((await m.PrepareForRunAsync(path, "tizio", "att1")).WorktreePath);
            m.ReleaseSlot((await m.PrepareForRunAsync(path, "caio", "att1")).WorktreePath);

            var second = await m.ContinueChainAsync(path, branch);

            // Inventarsi un posto pulito sarebbe peggio: il destinatario lavorerebbe senza quello
            // che gli è stato passato, e nessuno se ne accorgerebbe.
            Assert.IsFalse(second.Success);
            Assert.IsFalse(second.Busy, "non è un'attesa: la scrivania non tornerà da sola");
            StringAssert.Contains(second.Error, "catena si è interrotta");
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

        private static string SetupGitProject(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init --bare");
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
            return path;
        }
    }
}
