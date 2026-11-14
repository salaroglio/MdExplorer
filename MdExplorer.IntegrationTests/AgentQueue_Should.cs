using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6d — la coda di lavoro di un agente. Mostra i messaggi non conclusi (inclusi i
    /// parcheggiati, col motivo) e le richieste federate in attesa di gate; permette di
    /// forzare-ora o scartare. Testato in locale.
    /// </summary>
    [TestClass]
    public class AgentQueue_Should
    {
        private static async Task ParkAMessage(AgentCityContext ctx, System.Guid projectKey, string path)
        {
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");
            ctx.Gate.DeferFirst = 1000;   // parcheggia sempre (per la durata del test)

            await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");
            await ctx.WaitForMessages(m => m.Any(x =>
                x.ToAgent == "worker" && x.DeferredReason == AgentMessage.DeferredReasonEnum.Resources));
        }

        [TestMethod]
        public async Task List_a_parked_message_with_its_reason()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("queue");
            await ParkAMessage(ctx, projectKey, path);

            var (status, json) = await ctx.GetJson($"/api/A2A/agents/worker/queue?projectPath={Uri.EscapeDataString(path)}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            var messages = json.RootElement.GetProperty("messages").EnumerateArray().ToList();
            Assert.AreEqual(1, messages.Count, "il messaggio parcheggiato è in coda");
            Assert.AreEqual("resources", messages[0].GetProperty("deferredReason").GetString());
            Assert.AreEqual("pending", messages[0].GetProperty("state").GetString());
        }

        [TestMethod]
        public async Task Discard_a_queued_message()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("queue-discard");
            await ParkAMessage(ctx, projectKey, path);

            var id = ctx.Messages().First(m => m.ToAgent == "worker").Id;
            var (status, _) = await ctx.PostJson($"/api/A2A/agents/queue/{id}/discard", null);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            Assert.AreEqual(AgentMessage.StateEnum.Failed, ctx.Messages().Single(m => m.Id == id).State);

            var (_, json) = await ctx.GetJson($"/api/A2A/agents/worker/queue?projectPath={Uri.EscapeDataString(path)}");
            Assert.AreEqual(0, json.RootElement.GetProperty("messages").GetArrayLength(), "lo scartato esce dalla coda");
        }

        [TestMethod]
        public async Task Force_a_pending_message_and_reject_forcing_a_concluded_one()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("queue-force");
            await ParkAMessage(ctx, projectKey, path);

            var id = ctx.Messages().First(m => m.ToAgent == "worker").Id;
            var (forced, _) = await ctx.PostJson($"/api/A2A/agents/queue/{id}/force", null);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, forced);

            // Scartato (concluso) → forzarlo è fail-loud.
            await ctx.PostJson($"/api/A2A/agents/queue/{id}/discard", null);
            var (again, _) = await ctx.PostJson($"/api/A2A/agents/queue/{id}/force", null);
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, again);
        }

        [TestMethod]
        public async Task Show_pending_federated_requests_targeting_the_agent()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("queue-fed");
            ctx.WriteLlmCitizen(path, "javadev", "Java", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "javadev");

            ctx.Factory.Services.GetRequiredService<IFederatedRequestReceiver>()
                .Receive(path, new FederatedRequestPayload
                {
                    FederationId = Guid.NewGuid().ToString(),
                    FromOwner = "marco@acme.it",
                    FromAgent = "analyst",
                    TargetAgent = "javadev",
                    Scope = "WSAA",
                    Message = "genera java",
                    Topics = new List<string>(),
                });

            var (status, json) = await ctx.GetJson($"/api/A2A/agents/javadev/queue?projectPath={Uri.EscapeDataString(path)}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);
            var fed = json.RootElement.GetProperty("federatedPending").EnumerateArray().ToList();
            Assert.AreEqual(1, fed.Count);
            Assert.AreEqual("marco@acme.it", fed[0].GetProperty("fromOwner").GetString());
        }
    }
}
