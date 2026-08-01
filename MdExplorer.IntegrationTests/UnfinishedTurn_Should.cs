using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase A del piano runner-agnostico: <b>un turno che non arriva in fondo non è un
    /// successo</b>.
    /// <para>
    /// Col vecchio contratto (<c>Task&lt;string&gt;</c>) l'unico modo di fallire era sollevare:
    /// un runner che restituisce testo — è il caso del tetto di iterazioni di un ciclo di tool
    /// calling — veniva preso per riuscito. Da lì partiva tutta la macchina a valle:
    /// <c>MarkProcessed</c>, pubblicazione del branch del deliverable, auto-merge, e su un run
    /// federato un verdetto di <i>successo</i> che rinforza la confidence del fatto di routing.
    /// Un fallimento muto che insegna la cosa sbagliata.
    /// </para>
    /// <para>Richiede <c>git</c> nel PATH per la parte sul deliverable.</para>
    /// </summary>
    [TestClass]
    public class UnfinishedTurn_Should
    {
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

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
            Git(path, "config user.email agent@test.local"); Git(path, "config user.name Test"); Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A"); Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\""); Git(path, "push -u origin main");
            return (key, path);
        }

        [TestMethod]
        public async Task Not_mark_the_message_processed()
        {
            using var ctx = new AgentCityContext();
            var (key, path) = ctx.SeedProject("unfinished");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");

            // Il runner NON solleva: restituisce il messaggio di resa del ciclo di tool calling.
            ctx.Runner.FailWith = AgentTurnOutcome.Exhausted;
            ctx.Runner.Behavior = (_, __) =>
                Task.FromResult("Tool execution loop exceeded maximum iterations. Please try a simpler request.");

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "fai qualcosa di lungo");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");

            // Il turno gira (il fake viene chiamato) ma il messaggio non deve mai diventare Processed.
            var msgs = await ctx.WaitForMessages(m2 => m2.Any(x => x.ToAgent == "worker" && x.Attempts > 0));
            var m = msgs.First(x => x.ToAgent == "worker");
            Assert.AreNotEqual(AgentMessage.StateEnum.Processed, m.State,
                "un turno lasciato a metà non può risultare processato");
            Assert.IsTrue(ctx.Runner.Calls > 0, "il turno è stato comunque eseguito");
        }

        [TestMethod]
        public async Task Not_publish_a_deliverable_from_a_half_done_turn()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (key, path) = SetupGitProject(ctx, "unfinished-deliverable");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig
                {
                    Enabled = true,
                    UseAgentWorktrees = true,
                    AutoMergeAgentDeliverables = true,
                });

            // L'agente scrive davvero qualcosa nel worktree, poi il turno si esaurisce: il
            // lavoro è a metà, e proprio per questo NON va pubblicato.
            ctx.Runner.FailWith = AgentTurnOutcome.Exhausted;
            ctx.Runner.Behavior = (req, _) =>
            {
                File.WriteAllText(Path.Combine(req.WorkingDirectory, "meta-lavoro.md"), "# incompleto\n");
                return Task.FromResult("mi fermo qui");
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "produci");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");
            await ctx.WaitForMessages(msgs => msgs.Any(x => x.ToAgent == "worker" && x.Attempts > 0));

            // Dà tempo a un eventuale (indesiderato) push/auto-merge di manifestarsi.
            await Task.Delay(3000);

            Git(path, "fetch origin");
            Assert.AreNotEqual(0, Git(path, "cat-file -e origin/main:meta-lavoro.md").Code,
                "il lavoro a metà NON deve arrivare in origin/main");
        }
    }
}
