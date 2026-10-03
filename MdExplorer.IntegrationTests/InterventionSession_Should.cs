using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La <b>sessione d'intervento manuale</b> sul worktree di un agente.
    /// <para>
    /// Il punto non è vietare: è che finché un umano ci lavora l'agente sta in coda, e che
    /// <b>annullare non perde il lavoro</b> — la richiesta torna in coda perché l'agente la
    /// rifaccia. Senza quest'ultima parte, un merge rifiutato lascerebbe il lavoro in un limbo
    /// che nessuno riprende.
    /// </para>
    /// </summary>
    [TestClass]
    public class InterventionSession_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static IAgentWorktreeHoldService Sessions(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IAgentWorktreeHoldService>();

        [TestMethod]
        public void Queue_the_agent_while_a_session_is_open()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("sessione-coda");
            var sessions = Sessions(ctx);

            Assert.IsFalse(sessions.IsHeld(path, "worker"), "nessuna sessione all'inizio");

            sessions.Open(path, "worker", "sto sistemando a mano");

            Assert.IsTrue(sessions.IsHeld(path, "worker"));
            StringAssert.Contains(sessions.ReasonFor(path, "worker"), "a mano");

            // L'attivazione va in coda: e' la pausa esistente a farlo (deferred:user), quindi
            // i messaggi restano parcheggiati invece di fallire.
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var paused = db.GetDal<AgentPause>().GetList().ToList()
                .Any(p => p.AgentName == "worker");
            db.Commit();

            Assert.IsTrue(paused, "aprire la sessione mette l'agente in coda");
        }

        [TestMethod]
        public void Put_the_work_back_in_the_queue_when_the_human_cancels()
        {
            using var ctx = new AgentCityContext();
            var (key, path) = ctx.SeedProject("sessione-annulla");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("fatto");

            // Un lavoro concluso: e' quello che l'umano andra' a revisionare.
            var rpc = await_(GatewayRpc.SendMessage(ctx.Client, key, "worker", "fai il lavoro"));
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");
            await_(ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker"
                                                       && x.State == AgentMessage.StateEnum.Processed)));

            var sessions = Sessions(ctx);
            sessions.Open(path, "worker", "non mi convince");

            var result = sessions.Close(path, "worker", discardWork: true);

            Assert.IsTrue(result.Closed);
            Assert.IsTrue(result.Requeued >= 1, "annullare rimette la richiesta in coda, non la butta");
            Assert.IsFalse(sessions.IsHeld(path, "worker"), "la sessione e' chiusa");

            // Il messaggio e' tornato pending, coi tentativi azzerati: non e' un ritentativo
            // dopo un errore, e' lo stesso lavoro richiesto di nuovo.
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var msg = db.GetDal<AgentMessage>().GetList().ToList().First(m => m.ToAgent == "worker");
            db.Commit();

            Assert.AreEqual(AgentMessage.StateEnum.Pending, msg.State);
            Assert.AreEqual(0, msg.Attempts, "i tentativi si azzerano: non e' colpa dell'agente");
            Assert.IsNull(msg.ProcessedAt);
        }

        [TestMethod]
        public void Leave_the_work_alone_when_the_human_concludes()
        {
            using var ctx = new AgentCityContext();
            var (key, path) = ctx.SeedProject("sessione-concludi");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("fatto");

            var rpc = await_(GatewayRpc.SendMessage(ctx.Client, key, "worker", "fai il lavoro"));
            Assert.IsFalse(rpc.IsError);
            await_(ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker"
                                                       && x.State == AgentMessage.StateEnum.Processed)));

            var sessions = Sessions(ctx);
            sessions.Open(path, "worker", "ritocco io");
            var result = sessions.Close(path, "worker", discardWork: false);

            Assert.IsTrue(result.Closed);
            Assert.AreEqual(0, result.Requeued, "concludere non rimette nulla in coda");

            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var msg = db.GetDal<AgentMessage>().GetList().ToList().First(m => m.ToAgent == "worker");
            var stillPaused = db.GetDal<AgentPause>().GetList().ToList().Any(p => p.AgentName == "worker");
            db.Commit();

            Assert.AreEqual(AgentMessage.StateEnum.Processed, msg.State, "il lavoro concluso resta concluso");
            Assert.IsFalse(stillPaused, "chiudere la sessione toglie l'agente dalla coda");
        }

        [TestMethod]
        public async Task Refuse_to_wipe_a_worktree_with_an_open_session()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "sessione-prepare");
            var manager = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            // Il worktree esiste e l'umano ci sta lavorando.
            var first = await manager.PrepareForRunAsync(path, "worker", "att1");
            Assert.IsTrue(first.Success, first.Error);
            File.WriteAllText(Path.Combine(first.WorktreePath, "lavoro-umano.md"), "# non buttarmi\n");

            Sessions(ctx).Open(path, "worker", "intervento");

            // Rete sotto la rete: anche se la coda venisse aggirata, il prepare non ripulisce.
            var second = await manager.PrepareForRunAsync(path, "worker", "att2");

            Assert.IsFalse(second.Success, "con una sessione aperta il prepare deve rifiutarsi");
            StringAssert.Contains(second.Error, "intervento");
            Assert.IsTrue(File.Exists(Path.Combine(first.WorktreePath, "lavoro-umano.md")),
                "il lavoro umano non committato è ancora lì");
        }

        // ---- infrastruttura git minima ----

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
            p.WaitForExit(20000);
            return (p.ExitCode, o);
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        private static (Guid Key, string Path) SetupGitProject(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin); Git(origin, "init --bare");
            Git(path, "init -b main");
            Git(path, "config user.email agent@test.local"); Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A"); Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\""); Git(path, "push -u origin main");
            return (key, path);
        }

        /// <summary>Attesa sincrona nei test non-async, per tenerli leggibili.</summary>
        private static T await_<T>(Task<T> t) => t.GetAwaiter().GetResult();
    }
}
