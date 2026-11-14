using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6c — gate umano delle richieste federate (§12.6). Una richiesta arrivata da
    /// un'altra città NON fa partire nulla: resta pending finché l'umano non autorizza. Su
    /// approvazione l'agente locale è svegliato in una conversazione con FederationId; su
    /// rifiuto non gira niente. Testato in locale (il trasporto relay è verificato a parte).
    /// </summary>
    [TestClass]
    public class FederationGate_Should
    {
        private static Guid Receive(AgentCityContext ctx, string path, string target)
        {
            var receiver = ctx.Factory.Services.GetRequiredService<IFederatedRequestReceiver>();
            return receiver.Receive(path, new FederatedRequestPayload
            {
                FederationId = Guid.NewGuid().ToString(),
                FromOwner = "marco@acme.it",
                FromAgent = "analyst",
                TargetAgent = target,
                Scope = "WSAA-TOT",
                Message = "Genera il Java dal workflow allegato.",
                Topics = new List<string> { "java" },
            });
        }

        [TestMethod]
        public async Task Hold_a_federated_request_pending_without_running_anything()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("gate");
            ctx.WriteLlmCitizen(path, "javadev", "Sviluppatore Java", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "javadev");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            var reqId = Receive(ctx, path, "javadev");

            // La richiesta è pending nell'endpoint del gate...
            var (status, json) = await ctx.GetJson($"/api/A2A/federation/requests?projectPath={Uri.EscapeDataString(path)}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);
            var req = json.RootElement.GetProperty("requests").EnumerateArray()
                .Single(r => r.GetProperty("id").GetGuid() == reqId);
            Assert.AreEqual("pending", req.GetProperty("status").GetString());
            Assert.AreEqual("javadev", req.GetProperty("targetAgent").GetString());

            // ...e NESSUN run è partito: nessun messaggio verso l'agente (il gate è il guardrail).
            await Task.Delay(1500);
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "javadev"),
                "nessun run prima dell'autorizzazione umana");
        }

        [TestMethod]
        public async Task Wake_the_local_agent_only_after_approval_with_federation_id()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("gate-approve");
            ctx.WriteLlmCitizen(path, "javadev", "Sviluppatore Java", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "javadev");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            var reqId = Receive(ctx, path, "javadev");

            var (status, _) = await ctx.PostJson($"/api/A2A/federation/requests/{reqId}/approve", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            // Ora l'agente locale è svegliato.
            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "javadev"));
            var msg = msgs.First(x => x.ToAgent == "javadev");
            Assert.AreEqual("user", msg.FromAgent, "l'umano vouches: mittente user (hop esente)");

            // La conversazione porta la correlazione federata.
            var conv = ctx.Conversations().Single(c => c.Id == msg.ConversationId);
            Assert.IsNotNull(conv.FederationId, "conversazione correlata al lato d'origine");
            Assert.AreEqual("marco@acme.it", conv.RemoteOwner);
            Assert.AreEqual("analyst", conv.RemoteAgent);
        }

        [TestMethod]
        public async Task Run_nothing_when_the_request_is_rejected()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("gate-reject");
            ctx.WriteLlmCitizen(path, "javadev", "Sviluppatore Java", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "javadev");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            var reqId = Receive(ctx, path, "javadev");

            var (status, _) = await ctx.PostJson($"/api/A2A/federation/requests/{reqId}/reject", null);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            await Task.Delay(1500);
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "javadev"), "rifiutata → nessun run");

            // Ri-decidere una richiesta già decisa è fail-loud.
            var (again, _) = await ctx.PostJson($"/api/A2A/federation/requests/{reqId}/approve", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, again);
        }
    }
}
