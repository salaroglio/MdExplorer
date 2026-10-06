using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Workflow degli agenti, F4: un incarico che il workflow dice <c>start: ask-owner</c> non sveglia l'agente. Aspetta,
    /// nella posta del responsabile, che lui lo avvii (con le sue indicazioni, il motore e il modello che sceglie) o lo
    /// rifiuti con un motivo. Un workflow configurato che non si legge non fa partire niente, e lo dice.
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

        private static async Task<AgentMessage> AssignAndWaitForHold(AgentCityContext ctx, string path)
        {
            var token = ctx.MintRunToken("capo", path, null);
            var (status, body) = await ctx.SendAuthenticated(token, "tecnico", "[INCARICO] scrivi la scheda tecnica");
            Assert.IsTrue((int)status < 300, $"invio rifiutato: {status} {body}");

            var messages = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico"
                && x.DeferredReason == AgentMessage.DeferredReasonEnum.AwaitingOwner));
            var held = messages.SingleOrDefault(x => x.ToAgent == "tecnico");
            Assert.IsNotNull(held, "l'incarico doveva arrivare al tecnico");
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingOwner, held.DeferredReason,
                $"il workflow dice ask-owner: l'incarico aspetta il responsabile (stato {held.State}, {held.DeferredReason}, {held.Error})");
            return held;
        }

        [TestMethod]
        public async Task Hold_the_assignment_until_the_owner_starts_it_with_notes_engine_and_model()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "owner-start");

            var held = await AssignAndWaitForHold(ctx, path);
            await Task.Delay(3000);
            Assert.AreEqual(0, ctx.Runner.Calls, "finché il responsabile non lo avvia, il tecnico non si sveglia");
            Assert.AreEqual(AgentMessage.DeferredReasonEnum.AwaitingOwner, ctx.Messages().Single(m => m.Id == held.Id).DeferredReason,
                "il parcheggio non si riprende da solo");

            var (inboxStatus, inbox) = await ctx.GetJson("/api/A2A/mailbox/inbox?projectPath=" + System.Uri.EscapeDataString(path));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, inboxStatus);
            var toStart = inbox.RootElement.GetProperty("toStart");
            Assert.AreEqual(1, toStart.GetArrayLength());
            Assert.AreEqual("Scheda tecnica", toStart[0].GetProperty("step").GetString());
            Assert.AreEqual("capo", toStart[0].GetProperty("fromAgent").GetString());
            Assert.IsTrue(inbox.RootElement.GetProperty("unread").GetInt32() >= 1, "un incarico da avviare accende il badge");

            var (startStatus, startBody) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start",
                "{\"note\":\"guarda prima il capitolo 3\",\"provider\":\"claude\",\"model\":\"opus\"}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, startStatus, startBody);

            await ctx.WaitForMessages(m => m.Any(x => x.Id == held.Id && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(1, ctx.Runner.Calls);
            var request = ctx.Runner.LastRequest;
            StringAssert.Contains(request.ComposedPrompt, "scrivi la scheda tecnica");
            StringAssert.Contains(request.ComposedPrompt, "guarda prima il capitolo 3");
            Assert.AreEqual("claude", request.RequestedProvider);
            Assert.AreEqual("opus", request.RequestedModel);

            var (againStatus, _) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, againStatus, "un incarico già avviato non si avvia due volte");
        }

        [TestMethod]
        public async Task Close_the_assignment_when_the_owner_declines_it_with_a_reason()
        {
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "owner-decline");
            var held = await AssignAndWaitForHold(ctx, path);

            var (noReason, _) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/decline", "{\"reason\":\"  \"}");
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, noReason, "chi l'ha chiesto deve sapere perché");

            var (status, body) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/decline", "{\"reason\":\"il bando non ci riguarda\"}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            var closed = ctx.Messages().Single(m => m.Id == held.Id);
            Assert.AreEqual(AgentMessage.StateEnum.Failed, closed.State);
            Assert.IsNotNull(closed.OwnerDeclinedAt);
            StringAssert.Contains(closed.Error, "il bando non ci riguarda");
            await Task.Delay(3000);
            Assert.AreEqual(0, ctx.Runner.Calls, "rifiutato, il tecnico non parte");
        }

        [TestMethod]
        public async Task Refuse_the_assignment_and_say_why_when_the_configured_workflow_is_broken()
        {
            // Dal F5 il workflow si controlla già all'invio: chi scrive riceve il motivo e lo può dire alla persona.
            // Il parcheggio «workflow-invalid» del dispatcher resta per ciò che era già in coda.
            using var ctx = new AgentCityContext();
            var path = Setup(ctx, "owner-broken", Workflow.Replace("\"start\": \"ask-owner\"", "\"strat\": \"ask-owner\""));

            var token = ctx.MintRunToken("capo", path, null);
            var (status, body) = await ctx.SendAuthenticated(token, "tecnico", "[INCARICO] scrivi la scheda tecnica");

            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, status, body);
            StringAssert.Contains(body, "non si legge");
            StringAssert.Contains(body, "strat");
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "tecnico"), "senza la regola non si sa chi avvia: non entra in coda");
            Assert.AreEqual(0, ctx.Runner.Calls);
        }
    }
}
