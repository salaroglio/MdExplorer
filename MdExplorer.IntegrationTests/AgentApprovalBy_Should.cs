using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Approvazione per ruolo (workflow v3, F5): con <c>approval.by</c> la scheda del tecnico la approva chi ha quel ruolo,
    /// non chi risponde del tecnico. Chi approva la decide dal suo computer e la scrive in un suo file del registro; sul
    /// computer del responsabile dell'agente la richiesta si chiude con quella decisione, e da lì un rifiuto si fa ripartire.
    /// </summary>
    [TestClass]
    public class AgentApprovalBy_Should
    {
        private const string Me = "io@test.local";
        private const string Marco = "marco@test.local";

        private const string Workflow = @"{
  ""mde_workflow"": 3, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""capo"", ""skill"": ""ricerca"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""skill"": ""scheda"", ""title"": ""Scheda tecnica"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"", ""brief"": ""Scrivi la scheda tecnica sul bando {codice}."", ""approval"": { ""by"": ""Direzione"" } }
  ]
}";

        /// <param name="tecnico">Chi risponde del tecnico.</param>
        /// <param name="direzione">Chi ha il ruolo che approva la scheda.</param>
        private static string Setup(AgentCityContext ctx, string name, string tecnico, string direzione)
        {
            var (_, path) = ctx.SeedProject(name);
            var origin = Path.Combine(ctx.Factory.DataDir, "origins", name + ".git");
            Directory.CreateDirectory(origin);
            Git(origin, "init", "--bare", "-b", "main");
            Git(path, "init", "-b", "main");
            Git(path, "config", "user.email", Me);
            Git(path, "config", "user.name", "Io");
            Git(path, "config", "commit.gpgsign", "false");
            Git(path, "remote", "add", "origin", origin);

            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            meta.SetAgentCity(path, new MdExplorer.Service.Models.AgentCityConfig
            {
                Enabled = true, UseAgentWorktrees = false, OwnershipDoc = "responsabilita.md", WorkflowDoc = "workflow.md",
            });
            ctx.WriteLlmCitizen(path, "capo", "Chi cerca i bandi", new[] { "user" }, jobs: AgentCityContext.CapoJobs);
            ctx.WriteLlmCitizen(path, "tecnico", "Chi scrive la scheda", new[] { "capo", "user" }, jobs: AgentCityContext.TecnicoJobs(true));
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "capo");
            ctx.Trust(path, "tecnico");

            File.WriteAllText(Path.Combine(path, "responsabilita.md"),
                "---\nmde_type: ownership\n---\n# Chi risponde\n\n| Ruolo | Responsabile | Git Email | Agenti |\n|---|---|---|---|\n" +
                $"| Commerciale | Io | {Me} | capo |\n| Tecnica | T | {tecnico} | tecnico |\n| Direzione | D | {direzione} | |\n");
            File.WriteAllText(Path.Combine(path, "workflow.md"), "---\nmde_type: workflow\nworkflow: prova.workflow.json\n---\n# Prova\n");
            File.WriteAllText(Path.Combine(path, "prova.workflow.json"), Workflow);
            Directory.CreateDirectory(Path.Combine(path, "schede"));
            File.WriteAllText(Path.Combine(path, "schede", ".gitkeep"), "");
            Git(path, "add", "-A");
            Git(path, "commit", "-m", "base");
            Git(path, "push", "-u", "origin", "main");

            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            return path;
        }

        private static string Ledger(AgentCityContext ctx, string path) => ctx.Factory.Services.GetRequiredService<IRoundStore>().Root(path);

        /// <summary>Un giro in cui il tecnico ha consegnato la scheda (dal computer di <paramref name="by"/>), con il ramo su origin.</summary>
        private static (string Round, string Run, string Branch) Delivered(AgentCityContext ctx, string path, string by)
        {
            const string branch = "agent/tecnico/2027-03-08-scheda";
            Git(path, "checkout", "-q", "-b", "consegna");
            File.WriteAllText(Path.Combine(path, "schede", "tecnica.md"), "# Scheda tecnica\n");
            Git(path, "add", "-A");
            Git(path, "commit", "-m", "la scheda del tecnico");
            Git(path, "push", "--quiet", "origin", "consegna:" + branch);
            Git(path, "checkout", "-q", "main");
            Git(path, "branch", "-D", "consegna");

            var ledger = Ledger(ctx, path);
            var json = WorkflowDocument.JsonPathOf(path, "workflow.md");
            var now = DateTime.UtcNow.AddMinutes(-30);
            var id = RoundLedger.NewRoundId(json, now);
            var run = Guid.NewGuid().ToString("N");
            RoundLedger.Open(ledger, new RoundHeader { Id = id, Workflow = json, StartedAt = now, StartedBy = Me });
            RoundLedger.Append(ledger, id, "ricerca", "capo", new RoundEvent { Type = RoundEventType.Started, At = now, By = Me });
            RoundLedger.Append(ledger, id, "ricerca", "capo", new RoundEvent { Type = RoundEventType.Done, At = now, By = Me });
            RoundLedger.Append(ledger, id, "ricerca", "capo", new RoundEvent
            {
                Type = RoundEventType.Replied, At = now, By = Me, Reply = "avvia-giro", Values = new() { ["codice"] = "NC-1" },
            });
            RoundLedger.Append(ledger, id, "scheda", "tecnico", new RoundEvent { Type = RoundEventType.Started, At = now, By = by, Run = run });
            RoundLedger.Append(ledger, id, "scheda", "tecnico", new RoundEvent { Type = RoundEventType.Delivered, At = now, By = by, Run = run, Branch = branch, Files = new() { "schede/tecnica.md" } });
            return (id, run, branch);
        }

        [TestMethod]
        public async Task Let_who_has_the_role_approve_from_his_computer_in_his_own_file()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "approva-ruolo", tecnico: Marco, direzione: Me);
            var (id, _, _) = Delivered(ctx, path, Marco);

            // Il tecnico è di Marco, ma la scheda la approvo io (Direzione): la consegna si apre sul mio computer.
            ctx.Factory.Services.GetRequiredService<IAgentWorkflowExecutor>().Reconcile(path);
            var (_, list) = await ctx.GetJson("/api/AgentReview/requests?projectPath=" + Uri.EscapeDataString(path));
            var request = list.RootElement.GetProperty("requests").EnumerateArray().Single();
            var headSha = request.GetProperty("headSha").GetString();

            var (_, rounds) = await ctx.GetJson("/api/A2A/workflow/rounds?projectPath=" + Uri.EscapeDataString(path));
            var scheda = rounds.RootElement.GetProperty("rounds")[0].GetProperty("steps").EnumerateArray().Single(s => s.GetProperty("step").GetString() == "scheda");
            Assert.AreEqual("approval", scheda.GetProperty("state").GetString());
            Assert.AreEqual(Me, scheda.GetProperty("owner").GetString(), "in approvazione aspetta chi approva, non Marco");

            var (status, body) = await ctx.PostJson($"/api/AgentReview/requests/{request.GetProperty("id").GetString()}/approve",
                "{\"nobody\":true,\"commit\":\"" + headSha + "\"}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            var ledger = Ledger(ctx, path);
            var approved = RoundLedger.Load(ledger, id).EventsOf("scheda").Last();
            Assert.AreEqual(RoundEventType.Approved, approved.Type);
            Assert.AreEqual(Me, approved.By);
            Assert.AreEqual(headSha, approved.Commit);
            var folder = RoundLedger.RoundFolder(ledger, id);
            Assert.IsFalse(File.ReadAllText(Path.Combine(folder, "scheda.json")).Contains("\"approved\""), "il file del passo resta di Marco");
            StringAssert.Contains(File.ReadAllText(Path.Combine(folder, "scheda" + RoundLedger.DecisionsExtension)), "\"approved\"", "la decisione sta nel mio file");
            Assert.AreEqual(StepStatus.Finished, WorkflowScheduler.View(Wf(path), RoundLedger.Load(ledger, id), Wf(path).Step("scheda")).Status);
        }

        [TestMethod]
        public async Task Close_the_owners_request_with_the_approvers_decision_and_let_the_owner_restart_it()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "approva-altrove", tecnico: Me, direzione: Marco);
            var (id, run, branch) = Delivered(ctx, path, Me);
            var svc = ctx.Factory.Services.GetRequiredService<IAgentMergeRequestService>();
            var head = Git(path, "rev-parse", "origin/" + branch).Trim();
            var request = svc.Open(path, "tecnico", branch, "origin/" + branch, head,
                new[] { new ChangedFile { Change = "added", Path = "schede/tecnica.md" } }, run);

            // Sul mio computer la consegna c'è, ma la decide Marco: non posso approvarla, e non è «da fare».
            var ex = Assert.ThrowsException<InvalidOperationException>(() => svc.Reject(request.Id, "no"));
            StringAssert.Contains(ex.Message, "is approved by marco@test.local (role Direzione)");
            var (_, inbox) = await ctx.GetJson("/api/A2A/mailbox/inbox?projectPath=" + Uri.EscapeDataString(path));
            Assert.AreEqual(0, inbox.RootElement.GetProperty("todo").GetInt32(), "la decide un altro");

            // Marco rifiuta dal suo computer: la sua decisione arriva nel registro, nel suo file.
            RoundLedger.AppendDecision(Ledger(ctx, path), id, "scheda", new RoundEvent
            {
                Type = RoundEventType.Rejected, At = DateTime.UtcNow, By = Marco, Run = run, Note = "manca il RPO", Commit = head,
            });
            ctx.Factory.Services.GetRequiredService<IAgentWorkflowExecutor>().Reconcile(path, refresh: false);

            var mirrored = svc.Get(request.Id);
            Assert.AreEqual(AgentMergeRequest.StatusEnum.Rejected, mirrored.Status, "la richiesta di qui si chiude con la decisione di Marco");
            Assert.AreEqual("manca il RPO", mirrored.Note);

            // Il tecnico lavora sul mio computer: il rifiuto lo faccio ripartire io.
            svc.Rework(request.Id);
            var again = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico" && x.WorkflowAttempt == 2));
            Assert.AreEqual("manca il RPO", again.Single(x => x.ToAgent == "tecnico" && x.WorkflowAttempt == 2).ReworkNote);
        }

        private static WorkflowDescriptor Wf(string path)
        {
            var r = WorkflowFile.ReadBound(path, File.ReadAllText(Path.Combine(path, "prova.workflow.json")));
            Assert.IsTrue(r.IsValid, string.Join("\n", r.Issues));
            return r.Descriptor;
        }

        private static string Git(string cwd, params string[] args)
        {
            var r = GitCli.Run(cwd, args);
            if (r.Code != 0) throw new InvalidOperationException($"git {string.Join(" ", args)}: {r.Err}");
            return r.Out;
        }
    }
}
