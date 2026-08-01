using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 7d — handoff via branch. Deliverable pushato per refspec; sync al ref di handoff come
    /// primo predicato (ref assente → git-sync-failed, conflitto → merge-conflict-with-main);
    /// cleanup dei branch <c>agent/*</c> fusi; trasporto <c>HandoffRef</c>/<c>BaseCommit</c> nel
    /// payload di <c>RequestIntervention</c>. <para>Richiede <c>git</c> nel PATH.</para>
    /// </summary>
    [TestClass]
    public class AgentHandoff_Should
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

        private static string SetupGitProject(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init --bare");
            Git(path, "init -b main");
            Git(path, "config user.email agent@test.local");
            Git(path, "config user.name Test");
            Git(path, "config commit.gpgsign false");
            File.WriteAllText(Path.Combine(path, "README.md"), "# base\n");
            Git(path, "add -A");
            Git(path, "commit -m base");
            Git(path, $"remote add origin \"{origin}\"");
            Git(path, "push -u origin main");
            return path;
        }

        private static bool GitAvail() => Git(Path.GetTempPath(), "--version").Code == 0;

        [TestMethod]
        public async Task Publish_the_activity_branch_by_refspec()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "hd-push");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var prep = await m.PrepareForRunAsync(path, "worker", "act1");
            Assert.IsTrue(prep.Success, prep.Error);
            File.WriteAllText(Path.Combine(prep.WorktreePath, "deliverable.md"), "# out\n");

            var pushed = await m.CommitAndPushBranchAsync(path, "worker", "deliverable");
            Assert.IsNotNull(pushed, "il branch deve essere pubblicato");
            // Locale sobrio, pubblicato parlante: sono due nomi diversi per costruzione.
            Assert.AreEqual("agent/worker/act1", pushed.LocalBranch);
            StringAssert.StartsWith(pushed.Branch, "agent/");
            Assert.AreNotEqual(pushed.LocalBranch, pushed.Branch, "il pubblicato non è il locale");
            Assert.IsFalse(string.IsNullOrWhiteSpace(pushed.HeadSha));

            // Su origin esiste il nome PUBBLICATO — quello che il peer usera' per l'handoff —
            // e NON il nome locale, che resta in casa.
            var lsRemote = Git(path, $"ls-remote origin {pushed.Branch}").Out;
            StringAssert.Contains(lsRemote, pushed.Branch, "il branch pubblicato è su origin");

            var localOnRemote = Git(path, $"ls-remote origin {pushed.LocalBranch}").Out;
            Assert.IsTrue(string.IsNullOrWhiteSpace(localOnRemote),
                "il nome locale non deve esistere su origin");
        }

        [TestMethod]
        public async Task Fail_sync_when_the_handoff_ref_is_missing()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "hd-missing");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            var prep = await m.PrepareForRunAsync(path, "reviewer", "actB", handoffRef: "agent/ghost/nope");
            Assert.IsFalse(prep.Success);
            Assert.IsTrue(prep.SyncFailed, "ref di handoff assente → git-sync-failed");
            Assert.AreEqual("git-sync-failed", prep.Error);
        }

        [TestMethod]
        public async Task Merge_the_handoff_ref_into_the_run_branch()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "hd-merge");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            // Agente A produce e pubblica agent/worker/actA con un file unico.
            var prepA = await m.PrepareForRunAsync(path, "worker", "actA");
            Assert.IsTrue(prepA.Success, prepA.Error);
            File.WriteAllText(Path.Combine(prepA.WorktreePath, "fromA.md"), "# A\n");
            var pushedA = await m.CommitAndPushBranchAsync(path, "worker", "A output");
            Assert.IsNotNull(pushedA);

            // Agente B prepara col ref di handoff di A → il merge porta il file di A.
            var prepB = await m.PrepareForRunAsync(path, "reviewer", "actB", handoffRef: pushedA.Branch);
            Assert.IsTrue(prepB.Success, prepB.Error);
            Assert.IsTrue(File.Exists(Path.Combine(prepB.WorktreePath, "fromA.md")), "il lavoro di A è nel worktree di B");
        }

        [TestMethod]
        public async Task List_and_delete_only_merged_agent_branches()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "hd-cleanup");
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();

            // Branch fuso (punta a main): trivially --merged main. Branch avanti: NON fuso.
            Git(path, "branch agent/test/merged");
            Git(path, "checkout -b agent/test/ahead");
            File.WriteAllText(Path.Combine(path, "ahead.md"), "x");
            Git(path, "add -A"); Git(path, "commit -m ahead");
            Git(path, "checkout main");

            var merged = await m.ListMergedAgentBranchesAsync(path, "main");
            CollectionAssert.Contains(merged.ToList(), "agent/test/merged");
            CollectionAssert.DoesNotContain(merged.ToList(), "agent/test/ahead", "un branch non fuso NON va toccato");

            await m.DeleteBranchAsync(path, "agent/test/merged", remoteToo: false);
            var after = Git(path, "branch --list agent/test/merged").Out.Trim();
            Assert.AreEqual(string.Empty, after, "il branch fuso è stato cancellato");
        }

        [TestMethod]
        public async Task Attach_handoff_ref_to_the_intervention_when_worktrees_are_on()
        {
            if (!GitAvail()) { Assert.Inconclusive("git non disponibile."); return; }
            using var ctx = new AgentCityContext();
            var path = SetupGitProject(ctx, "hd-transport");
            // Ownership: WSAA-TOT → marco@acme.it/javadev.
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "ownership.md" });
            ctx.Factory.Services.GetRequiredService<MdExplorer.Services.AgentRun.IAgentWorktreePreference>().Set(path, true);
            File.WriteAllText(Path.Combine(path, "ownership.md"), @"---
mde_type: ownership
---
| Ambito | Git Email | Agenti |
|--------|-----------|--------|
| WSAA-TOT | marco@acme.it | javadev |
");
            // L'agente d'origine ha un worktree con un branch d'attività (verrà pubblicato).
            var m = ctx.Factory.Services.GetRequiredService<IAgentWorktreeManager>();
            var prep = await m.PrepareForRunAsync(path, "analyst", "actOrigin");
            Assert.IsTrue(prep.Success, prep.Error);

            var convId = ctx.SeedConversation(path);
            var token = ctx.MintRunToken("analyst", path, convId.ToString());
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(new { scope = "WSAA-TOT", message = "genera java", topics = new[] { "java" } });
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/request-intervention")
            { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") };
            req.Headers.Add("X-MDE-Run-Token", token);
            var resp = await ctx.Client.SendAsync(req);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());

            var sent = ctx.FederationSender.LastPayload;
            Assert.IsNotNull(sent);
            // Il ref di handoff deve essere il nome PUBBLICATO: il locale su origin non esiste,
            // e il peer farebbe 'merge origin/<locale>' senza trovarlo.
            StringAssert.StartsWith(sent.HandoffRef, "agent/", "il ref di handoff è un branch d'agente");
            StringAssert.Contains(sent.HandoffRef, "analyst", "porta il nome dell'agente d'origine");
            Assert.AreNotEqual("agent/analyst/actOrigin", sent.HandoffRef,
                "non è il nome locale: quello non è pubblicato su origin");
            Assert.IsFalse(string.IsNullOrWhiteSpace(sent.BaseCommit), "il baseCommit accompagna l'handoff");
        }
    }
}
