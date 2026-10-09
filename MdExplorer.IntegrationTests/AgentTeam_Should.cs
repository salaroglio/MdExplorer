using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
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
    /// Un agente di un team (W20-W22). Il tecnico risponde a me e a Marco: chi preme «Avvia il giro» sceglie chi fa la scheda,
    /// e da lì il passo è di quella persona, sul suo computer. Chi ce l'ha lo può passare a un collega del team. Il registro dei
    /// giri sta sul ramo mde/giri, non nel ramo su cui lavora la persona.
    /// </summary>
    [TestClass]
    public class AgentTeam_Should
    {
        private const string Me = "io@test.local";
        private const string Marco = "marco@test.local";

        private const string Workflow = @"{
  ""mde_workflow"": 2, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""capo"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""title"": ""Scheda tecnica"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"", ""brief"": ""Scrivi la scheda tecnica sul bando {codice}."" PRODUCES }
  ]
}";

        private static string Setup(AgentCityContext ctx, string name, bool produces = false)
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
            ctx.WriteLlmCitizen(path, "capo", "Chi cerca i bandi", new[] { "user" });
            ctx.WriteLlmCitizen(path, "tecnico", "Chi scrive la scheda", new[] { "capo", "user" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "capo");
            ctx.Trust(path, "tecnico");

            File.WriteAllText(Path.Combine(path, "responsabilita.md"),
                "---\nmde_type: ownership\n---\n# Chi risponde\n\n| Ambito | Responsabile | Git Email | Agenti |\n|---|---|---|---|\n" +
                $"| Commerciale | Io | {Me} | capo |\n| Tecnica | Io | {Me} | tecnico |\n| Tecnica (2) | Marco | {Marco} | tecnico |\n");
            File.WriteAllText(Path.Combine(path, "workflow.md"), "---\nmde_type: workflow\nworkflow: prova.workflow.json\n---\n# Prova\n");
            File.WriteAllText(Path.Combine(path, "prova.workflow.json"),
                Workflow.Replace(" PRODUCES", produces ? @", ""produces"": [""schede/tecnica.md""]" : ""));
            Directory.CreateDirectory(Path.Combine(path, "schede"));
            File.WriteAllText(Path.Combine(path, "schede", ".gitkeep"), "");
            Git(path, "add", "-A");
            Git(path, "commit", "-m", "base");
            Git(path, "push", "-u", "origin", "main");

            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            return path;
        }

        private static string Ledger(AgentCityContext ctx, string path) => ctx.Factory.Services.GetRequiredService<IRoundStore>().Root(path);

        /// <summary>Il capo lanciato a mano, e il suo messaggio con il pulsante del bando NC-1.</summary>
        private static async Task<(Guid Conversation, Guid Message)> Search(AgentCityContext ctx, string path)
        {
            var runId = Guid.NewGuid();
            await ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                RunId = runId, ProjectPath = path, AgentFilePath = Path.Combine(path, "capo.agent.md"),
                PreparedPrompt = "cerca i bandi", TriggerSource = "manual", UseWorktree = false,
            });
            var replies = System.Text.Json.JsonSerializer.Serialize(new[]
            {
                new ResolvedReply
                {
                    Id = "avvia-giro", Label = "Avvia il giro su NC-1", Description = "le schede su NC-1", Message = "avvia NC-1",
                    Values = new System.Collections.Generic.Dictionary<string, string> { ["codice"] = "NC-1" },
                },
            });
            var r = ctx.Factory.Services.GetRequiredService<IAgentMailbox>().Enqueue(new EnqueueRequest
            {
                ProjectPath = path, FromAgent = "capo", ToAgent = "user", Body = "[ESITO] un bando compatibile",
                RunId = runId.ToString("N"), Replies = replies,
            });
            Assert.IsTrue(r.Accepted, r.RejectionReason);
            var message = ctx.Messages().Single(m => m.ToAgent == "user" && m.FromAgent == "capo" && m.Replies != null);
            return (r.ConversationId, message.Id);
        }

        private static Task<(System.Net.HttpStatusCode, string)> Press(AgentCityContext ctx, Guid conversation, string assignTo)
            => ctx.PostJson("/api/A2A/mailbox/reply",
                $"{{\"conversationId\":\"{conversation}\",\"body\":\"avvia NC-1\",\"choice\":true" +
                (assignTo == null ? "" : $",\"assign\":{{\"scheda\":\"{assignTo}\"}}") + "}");

        [TestMethod]
        public async Task Let_whoever_presses_the_button_choose_who_in_the_team_does_the_step()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "team-sceglie");
            var (conversation, message) = await Search(ctx, path);

            var (_, targets) = await ctx.GetJson($"/api/A2A/mailbox/reply-targets?messageId={message}&replyId=avvia-giro");
            var target = targets.RootElement.GetProperty("targets").EnumerateArray().Single();
            Assert.IsTrue(target.GetProperty("needsChoice").GetBoolean(), "il tecnico ha un team: si sceglie");
            CollectionAssert.AreEquivalent(new[] { Me, Marco },
                target.GetProperty("owners").EnumerateArray().Select(o => o.GetProperty("email").GetString()).ToList());

            var (unchosen, why) = await Press(ctx, conversation, null);
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, unchosen, "con un team non si indovina");
            StringAssert.Contains(why, "scegli a chi va");

            var (status, body) = await Press(ctx, conversation, Marco);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            await Task.Delay(2000);
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "tecnico"), "la scheda è di Marco: sul mio computer non parte");

            var round = RoundLedger.Load(Ledger(ctx, path), RoundLedger.RoundIds(Ledger(ctx, path)).Single());
            Assert.AreEqual(Marco, round.EventsOf("ricerca").Single(e => e.Type == RoundEventType.Replied).Assign["scheda"],
                "la scelta è nel registro: il computer di Marco la legge e fa partire la sua scheda");

            // Il registro sta sul suo ramo, pubblicato; il ramo della persona non lo vede.
            Assert.IsFalse(Directory.Exists(Path.Combine(path, ".mde", "giri")), "niente registro nella copia della persona");
            StringAssert.Contains(Git(path, "ls-tree", "-r", "--name-only", "origin/mde/giri"), "/ricerca.json");
            Assert.AreEqual("", Git(path, "status", "--porcelain", "--", ".mde").Trim());
        }

        [TestMethod]
        public async Task Pass_a_held_step_to_a_colleague_of_the_team_and_take_one_passed_to_me()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "team-passa");
            var (conversation, _) = await Search(ctx, path);
            var (status, body) = await Press(ctx, conversation, Me);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            var held = (await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico"))).Single(x => x.ToAgent == "tecnico");

            var (_, inbox) = await ctx.GetJson("/api/A2A/mailbox/inbox?projectPath=" + Uri.EscapeDataString(path));
            Assert.AreEqual(Marco, inbox.RootElement.GetProperty("toStart")[0].GetProperty("passTo")[0].GetProperty("email").GetString());

            var (stranger, _) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/pass", "{\"to\":\"paolo@test.local\"}");
            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, stranger, "si passa solo a chi risponde dello stesso agente");

            var (passed, passBody) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/pass", $"{{\"to\":\"{Marco}\",\"note\":\"vado in ferie\"}}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, passed, passBody);
            var gone = ctx.Messages().Single(m => m.Id == held.Id);
            Assert.AreEqual(AgentMessage.StateEnum.Failed, gone.State, "esce dalla mia posta");
            StringAssert.Contains(gone.Error, Marco);
            var (late, _) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, late);
            var assigned = RoundLedger.Load(Ledger(ctx, path), held.WorkflowRound).EventsOf("scheda").Last();
            Assert.AreEqual(RoundEventType.Assigned, assigned.Type);
            Assert.AreEqual(Marco, assigned.Owner);
            Assert.AreEqual("vado in ferie", assigned.Note);

            // Marco me lo ripassa dal suo computer: il suo evento arriva nel registro, e la ripresa me lo rimette in posta.
            RoundLedger.Append(Ledger(ctx, path), held.WorkflowRound, "scheda", "tecnico", new RoundEvent
            {
                Type = RoundEventType.Assigned, At = DateTime.UtcNow, By = Marco, Owner = Me, Note = "rientrato tu",
            });
            ctx.Factory.Services.GetRequiredService<IAgentWorkflowExecutor>().Reconcile(path);
            var back = ctx.Messages().Where(m => m.ToAgent == "tecnico" && m.State == AgentMessage.StateEnum.Pending).ToList();
            Assert.AreEqual(1, back.Count, "il «da avviare» torna da me, una volta sola");
            StringAssert.Contains(back[0].Body, "Scrivi la scheda tecnica sul bando NC-1.", "con lo stesso incarico");
        }

        [TestMethod]
        public async Task Let_the_colleague_who_receives_the_step_approve_what_was_delivered_on_the_other_computer()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "team-approva", produces: true);

            // Il tecnico di Marco ha consegnato dal suo computer: il suo ramo è su origin; poi Marco passa la scheda a me.
            const string branch = "agent/marco/tecnico/2027-03-08-scheda";
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
                Type = RoundEventType.Replied, At = now, By = Me, Reply = "avvia-giro",
                Values = new() { ["codice"] = "NC-1" }, Assign = new() { ["scheda"] = Marco },
            });
            RoundLedger.Append(ledger, id, "scheda", "tecnico", new RoundEvent { Type = RoundEventType.Started, At = now, By = Marco, Run = run });
            RoundLedger.Append(ledger, id, "scheda", "tecnico", new RoundEvent { Type = RoundEventType.Delivered, At = now, By = Marco, Run = run, Branch = branch, Files = new() { "schede/tecnica.md" } });
            RoundLedger.Append(ledger, id, "scheda", "tecnico", new RoundEvent { Type = RoundEventType.Assigned, At = now, By = Marco, Owner = Me, Note = "ferie" });

            ctx.Factory.Services.GetRequiredService<IAgentWorkflowExecutor>().Reconcile(path);

            var (_, list) = await ctx.GetJson("/api/AgentReview/requests?projectPath=" + Uri.EscapeDataString(path));
            var request = list.RootElement.GetProperty("requests").EnumerateArray().Single();
            Assert.AreEqual("tecnico", request.GetProperty("agentName").GetString());
            var (status, body) = await ctx.PostJson($"/api/AgentReview/requests/{request.GetProperty("id").GetString()}/approve", "{\"nobody\":true}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            Git(path, "fetch", "--quiet", "origin");
            StringAssert.Contains(Git(path, "show", "origin/main:schede/tecnica.md"), "# Scheda tecnica", "il lavoro fatto da Marco è entrato");
            var approved = RoundLedger.Load(ledger, id).EventsOf("scheda").Last();
            Assert.AreEqual(RoundEventType.Approved, approved.Type);
            Assert.AreEqual(Me, approved.By);
            StringAssert.Contains(Git(path, "log", "-1", "--format=%s", "origin/mde/giri"), "approvato",
                "il registro si pubblica anche dopo che l'approvazione ha fatto avanzare main su origin");
        }

        private static string Git(string cwd, params string[] args)
        {
            var r = GitCli.Run(cwd, args);
            if (r.Code != 0) throw new InvalidOperationException($"git {string.Join(" ", args)}: {r.Err}");
            return r.Out;
        }
    }
}
