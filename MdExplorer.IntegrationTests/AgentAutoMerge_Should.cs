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
    /// Fase 7g — cancello del merge. Il deliverable-doc pushato viene auto-fuso nel default e
    /// pushato (gate meccanico che auto-approva; opt-in <c>autoMergeAgentDeliverables</c>); un
    /// conflitto → not-ready (l'agente rilavora). Il merge gira nel worktree in detached HEAD.
    /// <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentAutoMerge_Should
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

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        [TestMethod]
        public async Task Auto_merge_a_clean_deliverable_into_default()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "am-clean");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var prep = await m.PrepareForRunAsync(path, "worker", "act1");
            Assert.IsTrue(prep.Success, prep.Error);
            File.WriteAllText(Path.Combine(prep.WorktreePath, "deliverable.md"), "# out\n");
            var pushed = await m.CommitAndPushBranchAsync(path, "worker", "deliverable");
            Assert.IsNotNull(pushed);

            var outcome = await m.MergeDeliverableIntoDefaultAsync(path, "worker", pushed.Branch);
            Assert.AreEqual(DeliverableMergeOutcome.Merged, outcome);

            // Il deliverable è ora nel default di origin.
            Git(path, "fetch origin");
            Assert.AreEqual(0, Git(path, "cat-file -e origin/main:deliverable.md").Code, "il deliverable è in origin/main");
        }

        [TestMethod]
        public async Task Report_conflict_when_the_deliverable_clashes_with_main()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (_, path) = SetupGitProject(ctx, "am-conflict");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            // Branch d'attività del reviewer che cambia README (partendo da base).
            var wt = await m.EnsureWorktreeAsync(path, "reviewer");
            Git(wt, "checkout -B agent/reviewer/actB");
            File.WriteAllText(Path.Combine(wt, "README.md"), "# reviewer-side\n");
            Git(wt, "add -A"); Git(wt, "commit -m reviewer-change");

            // Nel frattempo il default avanza in modo conflittuale (stessa riga di README).
            File.WriteAllText(Path.Combine(path, "README.md"), "# main-side\n");
            Git(path, "add -A"); Git(path, "commit -m main-change"); Git(path, "push origin main");

            var outcome = await m.MergeDeliverableIntoDefaultAsync(path, "reviewer", "agent/reviewer/actB");
            Assert.AreEqual(DeliverableMergeOutcome.Conflict, outcome, "il merge conflittuale → not-ready, niente merge");
        }

        [TestMethod]
        public async Task Auto_merge_end_to_end_when_the_flag_is_on()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var (key, path) = SetupGitProject(ctx, "am-e2e");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, AutoMergeAgentDeliverables = true });
            ctx.Factory.Services.GetRequiredService<MdExplorer.Services.AgentRun.IAgentWorktreePreference>().Set(path, true);

            // Il fake agente scrive un deliverable nel suo worktree.
            ctx.Runner.Behavior = (req, _) =>
            {
                File.WriteAllText(Path.Combine(req.WorkingDirectory, "from-agent.md"), "# agent output\n");
                return Task.FromResult("ok");
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, key, "worker", "produci");
            Assert.IsFalse(rpc.IsError, $"{rpc.ErrorCode} {rpc.ErrorMessage}");
            await ctx.WaitForMessages(msgs => msgs.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            // Poll: l'auto-merge (post-run) è best-effort dopo il MarkProcessed → attendi che origin/main abbia il file.
            var deadline = DateTime.UtcNow.AddSeconds(20);
            bool merged = false;
            while (DateTime.UtcNow < deadline)
            {
                Git(path, "fetch origin");
                if (Git(path, "cat-file -e origin/main:from-agent.md").Code == 0) { merged = true; break; }
                await Task.Delay(500);
            }
            Assert.IsTrue(merged, "col flag ON il deliverable è auto-fuso in origin/main");
        }
    }
}
