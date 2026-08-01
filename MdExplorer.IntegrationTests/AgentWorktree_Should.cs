using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    /// Fase 7c — isolamento d'esecuzione per-agente. Con <c>agentCity.useAgentWorktrees</c> attivo,
    /// il turno LLM gira in un <b>worktree git persistente</b> fuori dal progetto (branch fresco
    /// per-attività), non nella working tree dell'umano. Con il flag OFF (default) tutto invariato.
    /// <para>Richiede il binario <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentWorktree_Should
    {
        // Guardia contro l'hazard globale SetCurrentDirectory (§7h): un test precedente può
        // lasciare la cwd in una temp dir cancellata → Process.Start(git) fallirebbe a GetCwd().
        [TestInitialize]
        public void ResetCwd() => Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        private static (int Code, string Out, string Err) Git(string cwd, string args)
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
            var e = p.StandardError.ReadToEnd();
            p.WaitForExit(60000);
            return (p.ExitCode, o, e);
        }

        /// <summary>Progetto git reale con un remote 'origin' bare e il branch main pubblicato.</summary>
        private static (Guid key, string path) SetupGitProject(AgentCityContext ctx, string name)
        {
            var (key, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Assert.AreEqual(0, Git(origin, "init --bare").Code, "git init --bare");

            Assert.AreEqual(0, Git(path, "init -b main").Code, "git init");
            Git(path, "config user.email agent@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A");
            Assert.AreEqual(0, Git(path, "commit -m base").Code, "git commit");
            Git(path, $"remote add origin \"{origin}\"");
            Assert.AreEqual(0, Git(path, "push -u origin main").Code, "git push");
            return (key, path);
        }

        [TestMethod]
        public async Task Run_the_agent_in_an_isolated_worktree_when_enabled()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (key, path) = SetupGitProject(ctx, "wt-on");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig
                {
                    Enabled = true,
                    UseAgentWorktrees = true,
                    // Auto-merge spento ESPLICITAMENTE: qui si verifica l'isolamento, non la
                    // fusione. Col default acceso la fusione avviene a fine run nel worktree in
                    // detached HEAD, e il branch d'attività non sarebbe più osservabile qui.
                    AutoMergeAgentDeliverables = false,
                });

            // Il fake agente scrive uno scratch nel SUO cwd: deve finire nel worktree, non nel progetto.
            ctx.Runner.Behavior = (req, _) =>
            {
                File.WriteAllText(Path.Combine(req.WorkingDirectory, "agent-scratch.txt"), "x");
                return Task.FromResult("ok");
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "fai qualcosa");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            var manager = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();
            var worktree = manager.WorktreePathFor(path, "worker");

            // Il cwd del turno è il worktree, NON il progetto dell'umano.
            Assert.AreEqual(worktree, ctx.Runner.LastRequest.WorkingDirectory, "il turno gira nel worktree");
            Assert.AreNotEqual(path, ctx.Runner.LastRequest.WorkingDirectory);
            Assert.IsTrue(Directory.Exists(worktree), "il worktree esiste su disco");

            // Lo scratch dell'agente è nel worktree, non nella working tree dell'umano.
            Assert.IsTrue(File.Exists(Path.Combine(worktree, "agent-scratch.txt")), "scratch nel worktree");
            Assert.IsFalse(File.Exists(Path.Combine(path, "agent-scratch.txt")), "il progetto dell'umano resta intatto");

            // Branch fresco per-attività.
            var branch = Git(worktree, "rev-parse --abbrev-ref HEAD").Out.Trim();
            StringAssert.StartsWith(branch, "agent/worker/", "branch per-attività");
        }

        [TestMethod]
        public async Task Run_in_the_project_when_worktrees_are_off()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (key, path) = SetupGitProject(ctx, "wt-off");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            // Isolamento spento ESPLICITAMENTE: dal 2026-08-01 il default su un repo git con
            // origin è acceso, quindi qui si verifica il comportamento con il flag a false —
            // non più "cosa fa se non lo tocchi".
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, UseAgentWorktrees = false });
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "fai qualcosa");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            Assert.AreEqual(path, ctx.Runner.LastRequest.WorkingDirectory, "flag OFF: cwd = progetto");
            var manager = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();
            Assert.IsFalse(Directory.Exists(manager.WorktreePathFor(path, "worker")), "nessun worktree creato");
        }

        [TestMethod]
        public async Task Ensure_prepare_and_remove_a_worktree()
        {
            if (Git(Path.GetTempPath(), "--version").Code != 0) { Assert.Inconclusive("git non disponibile."); return; }

            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "wt-lifecycle");
            var manager = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var created = await manager.EnsureWorktreeAsync(path, "worker");
            Assert.IsTrue(Directory.Exists(created), "EnsureWorktree crea la cartella");

            var prep = await manager.PrepareForRunAsync(path, "worker", "abc123");
            Assert.IsTrue(prep.Success, prep.Error);
            Assert.AreEqual("agent/worker/abc123", Git(created, "rev-parse --abbrev-ref HEAD").Out.Trim());

            await manager.RemoveWorktreeAsync(path, "worker");
            Assert.IsFalse(Directory.Exists(created), "RemoveWorktree elimina la cartella (meccanismo del reaper)");
        }
    }
}
