using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Lo schedulatore nel servizio (S3), sul percorso che il responsabile vede: il capo viene lanciato a mano e si apre un
    /// giro; finito il suo turno, lo schedulatore mette la scheda del tecnico «da avviare» (start: ask-owner) nella posta,
    /// con il brief; il responsabile la avvia (indicazioni, motore, modello) o la rifiuta con un motivo. Ogni gesto va nel
    /// registro del giro. Un workflow rotto ferma i passaggi e lo dice.
    /// </summary>
    [TestClass]
    public class AgentOwnerStart_Should
    {
        private const string Workflow = @"{
  ""mde_workflow"": 2, ""title"": ""Prova"",
  ""steps"": [
    { ""id"": ""incarica"", ""agent"": ""capo"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""title"": ""Scheda tecnica"", ""trigger"": { ""after"": [""incarica""] }, ""start"": ""ask-owner"",
      ""brief"": ""Scrivi la scheda tecnica."" }
  ]
}";

        private static string Setup(AgentCityContext ctx, string name, string workflowJson = Workflow)
        {
            var (_, path) = ctx.SeedProject(name);
            ctx.SetGitEmail(path, "io@test.local");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            meta.SetAgentCity(path, new MdExplorer.Service.Models.AgentCityConfig { Enabled = true, UseAgentWorktrees = false });

            ctx.WriteLlmCitizen(path, "capo", "Chi incarica", new[] { "user" });
            ctx.WriteLlmCitizen(path, "tecnico", "Chi scrive la scheda", new[] { "capo", "user" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "capo");
            ctx.Trust(path, "tecnico");
            ctx.OwnAgents(path, "capo", "tecnico");

            File.WriteAllText(Path.Combine(path, "workflow.md"), "---\nmde_type: workflow\nworkflow: prova.workflow.json\n---\n# Prova\n");
            File.WriteAllText(Path.Combine(path, "prova.workflow.json"), workflowJson);
            var city = meta.GetAgentCity(path);
            city.WorkflowDoc = "workflow.md";
            meta.SetAgentCity(path, city);

            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            return path;
        }

        /// <summary>Il capo lanciato a mano: apre il giro; quando finisce, la scheda del tecnico deve essere «da avviare».</summary>
        private static async Task<AgentMessage> LaunchAndWaitForHold(AgentCityContext ctx, string path)
        {
            await ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path,
                AgentFilePath = Path.Combine(path, "capo.agent.md"),
                PreparedPrompt = "comincia il giro",
                TriggerSource = "manual",
                UseWorktree = false,
            });

            var messages = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico" && x.WorkflowRound != null));
            var held = messages.SingleOrDefault(x => x.ToAgent == "tecnico");
            Assert.IsNotNull(held, "lo schedulatore doveva far partire la scheda del tecnico");
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingOwner, held.DeferredReason, "ask-owner: aspetta il responsabile");
            Assert.AreEqual("scheda", held.WorkflowStep);
            StringAssert.Contains(held.Body, "Scrivi la scheda tecnica.", "l'agente riceve il brief del workflow");
            return held;
        }

        private static RoundState Round(string path, AgentMessage held) => RoundLedger.Load(path, held.WorkflowRound);

        private static string[] Events(RoundState round, string step) => round.EventsOf(step).Select(e => e.Type).ToArray();

        [TestMethod]
        public async Task Hold_the_next_step_until_its_owner_starts_it_and_record_every_gesture()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "sched-avvia");

            var held = await LaunchAndWaitForHold(ctx, path);
            Assert.AreEqual(1, ctx.Runner.Calls, "solo il capo ha lavorato: il tecnico aspetta");
            var round = Round(path, held);
            CollectionAssert.AreEqual(new[] { RoundEventType.Started, RoundEventType.Done }, Events(round, "incarica"));
            CollectionAssert.AreEqual(new[] { RoundEventType.Held }, Events(round, "scheda"));

            var (_, inbox) = await ctx.GetJson("/api/A2A/mailbox/inbox?projectPath=" + System.Uri.EscapeDataString(path));
            Assert.AreEqual("Scheda tecnica", inbox.RootElement.GetProperty("toStart")[0].GetProperty("step").GetString());

            var (status, body) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start",
                "{\"note\":\"guarda prima il capitolo 3\",\"provider\":\"claude\",\"model\":\"opus\"}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            await ctx.WaitForMessages(m => m.Any(x => x.Id == held.Id && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(2, ctx.Runner.Calls);
            StringAssert.Contains(ctx.Runner.LastRequest.ComposedPrompt, "guarda prima il capitolo 3");
            Assert.AreEqual("claude", ctx.Runner.LastRequest.RequestedProvider);
            Assert.AreEqual("opus", ctx.Runner.LastRequest.RequestedModel);

            // Il turno del tecnico finito senza artefatto (il passo non ne dichiara): concluso.
            await WaitFor(() => Events(Round(path, held), "scheda").LastOrDefault() == RoundEventType.Done);
            var started = Round(path, held).EventsOf("scheda").Single(e => e.Type == RoundEventType.Started);
            Assert.AreEqual("guarda prima il capitolo 3", started.Note, "il registro dice con quali indicazioni è partito");
            Assert.AreEqual("claude", started.Engine);
            Assert.AreEqual("io@test.local", started.By);
        }

        [TestMethod]
        public async Task Record_the_reason_when_the_owner_does_not_start_the_step()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "sched-rifiuta");
            var held = await LaunchAndWaitForHold(ctx, path);

            var (noReason, _) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/decline", "{\"reason\":\"  \"}");
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, noReason, "chi l'ha chiesto deve sapere perché");

            var (status, body) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/decline", "{\"reason\":\"lo fa lo studio esterno\"}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            var declined = Round(path, held).EventsOf("scheda").Last();
            Assert.AreEqual(RoundEventType.Declined, declined.Type);
            Assert.AreEqual("lo fa lo studio esterno", declined.Note);
            await Task.Delay(2500);
            Assert.AreEqual(1, ctx.Runner.Calls, "non avviato: il tecnico non parte");
        }

        [TestMethod]
        public async Task Refuse_a_message_between_agents_and_say_why_when_the_configured_workflow_is_broken()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "sched-rotto", Workflow.Replace("\"start\": \"ask-owner\"", "\"strat\": \"ask-owner\""));

            var token = ctx.MintRunToken("capo", path, null);
            var (status, body) = await ctx.SendAuthenticated(token, "tecnico", "[INCARICO] scrivi la scheda tecnica");

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
            StringAssert.Contains(body, "non si legge");
            StringAssert.Contains(body, "strat");
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "tecnico"));
        }

        private const string WithReply = @"{
  ""mde_workflow"": 2, ""title"": ""Gara"",
  ""variables"": { ""codice"": ""il bando"" },
  ""steps"": [
    { ""id"": ""ricerca"", ""agent"": ""capo"", ""trigger"": { ""launch"": true }, ""start"": ""manual"" },
    { ""id"": ""scheda"", ""agent"": ""tecnico"", ""title"": ""Scheda tecnica"", ""trigger"": { ""reply"": ""avvia-giro"", ""to"": ""ricerca"" },
      ""start"": ""ask-owner"", ""brief"": ""Scrivi la scheda tecnica sul bando {codice}."" }
  ]
}";

        [TestMethod]
        public async Task Open_one_round_per_pressed_button_with_its_own_value()
        {
            // W19: la ricerca trova due bandi, l'agente propone due pulsanti; ogni pulsante premuto apre il suo giro.
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "sched-pulsanti", WithReply);
            var runId = System.Guid.NewGuid();
            await ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                RunId = runId, ProjectPath = path, AgentFilePath = Path.Combine(path, "capo.agent.md"),
                PreparedPrompt = "cerca i bandi", TriggerSource = "manual", UseWorktree = false,
            });
            Assert.AreEqual(0, ctx.Messages().Count(m => m.ToAgent == "tecnico"), "senza la scelta della persona non parte niente");

            // Il messaggio della ricerca, con i due pulsanti che l'agente ha proposto.
            var conversation = SeedSearchMessage(ctx, path, runId.ToString("N"));
            foreach (var codice in new[] { "NC-1", "NC-2", "NC-1" })
            {
                var (status, body) = await ctx.PostJson("/api/A2A/mailbox/reply",
                    $"{{\"conversationId\":\"{conversation}\",\"body\":\"avvia {codice}\",\"choice\":true}}");
                Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            }

            var held = (await ctx.WaitForMessages(m => m.Count(x => x.ToAgent == "tecnico") >= 2)).Where(x => x.ToAgent == "tecnico").ToList();
            Assert.AreEqual(2, held.Count, "due bandi, due giri; lo stesso pulsante premuto due volte non ne apre un terzo");
            Assert.AreEqual(2, held.Select(h => h.WorkflowRound).Distinct().Count(), "ogni pulsante il suo giro");
            CollectionAssert.AreEquivalent(new[] { "NC-1", "NC-2" },
                held.Select(h => h.Body.Contains("NC-1") ? "NC-1" : h.Body.Contains("NC-2") ? "NC-2" : "?").ToList(), "ogni giro con il suo {codice}");
            Assert.AreEqual(0, ctx.Messages().Count(m => m.ToAgent == "capo" && m.FromAgent == "user"),
                "il pulsante lo esegue lo schedulatore: il capo non riceve il messaggio");
            Assert.AreEqual(2, RoundLedger.RoundIds(path).Count);
        }

        private static System.Guid SeedSearchMessage(AgentCityContext ctx, string path, string runId)
        {
            var replies = System.Text.Json.JsonSerializer.Serialize(new[] { "NC-1", "NC-2" }.Select(c => new MdExplorer.Features.Agents.ResolvedReply
            {
                Id = "avvia-giro", Label = "Avvia il giro su " + c, Description = "le schede su " + c, Message = "avvia " + c,
                Values = new System.Collections.Generic.Dictionary<string, string> { ["codice"] = c },
            }).ToList());
            var mailbox = ctx.Factory.Services.GetRequiredService<IAgentMailbox>();
            var r = mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = path, FromAgent = "capo", ToAgent = "user", Body = "[ESITO] due bandi compatibili", RunId = runId, Replies = replies,
            });
            Assert.IsTrue(r.Accepted, r.RejectionReason);
            return r.ConversationId;
        }

        private static async Task WaitFor(System.Func<bool> condition, int timeoutMs = 15000)
        {
            var deadline = System.DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (System.DateTime.UtcNow < deadline)
            {
                if (condition()) return;
                await Task.Delay(250);
            }
            Assert.IsTrue(condition(), "la condizione non si è avverata in tempo");
        }
    }
}
