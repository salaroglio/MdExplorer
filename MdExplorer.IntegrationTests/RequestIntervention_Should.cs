using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Features.Federation;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6c — lato ORIGINE della federazione: il tool/endpoint RequestIntervention. Un
    /// agente chiede l'intervento su un ambito; l'harness risolve dall'ownership CHI e QUALE
    /// agente, consuma 1 hop nella conversazione d'origine, correla col FederationId e spedisce
    /// (qui: fake sender). Il gate umano remoto è testato altrove.
    /// </summary>
    [TestClass]
    public class RequestIntervention_Should
    {
        private static void SetupOwnership(AgentCityContext ctx, string path)
        {
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "ownership.md" });
            File.WriteAllText(Path.Combine(path, "ownership.md"), @"---
mde_type: ownership
---
| Ruolo | Git Email | Agenti |
|--------|-----------|--------|
| WSAA-TOT | marco@acme.it | javadev |
");
        }

        private async Task<System.Net.Http.HttpResponseMessage> PostIntervention(
            AgentCityContext ctx, string token, string scope, string message)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new { scope, message, topics = new[] { "java" } });
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/request-intervention")
            { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            if (token != null) req.Headers.Add("X-MDE-Run-Token", token);
            return await ctx.Client.SendAsync(req);
        }

        [TestMethod]
        public async Task Resolve_the_scope_consume_a_hop_and_send_the_federated_request()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("intervention");
            SetupOwnership(ctx, path);

            var convId = ctx.SeedConversation(path);
            var token = ctx.MintRunToken("analyst", path, convId.ToString());

            var resp = await PostIntervention(ctx, token, "WSAA-TOT", "Genera il Java dal workflow.");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());

            // Il sender federato ha ricevuto la richiesta risolta correttamente.
            var sent = ctx.FederationSender.LastPayload;
            Assert.IsNotNull(sent, "il sender federato deve aver ricevuto la richiesta");
            Assert.AreEqual(path, ctx.FederationSender.LastProjectPath);
            Assert.AreEqual(FederationRoom.ComputeUserId("marco@acme.it"), ctx.FederationSender.LastTargetOwnerId);
            Assert.AreEqual("javadev", sent.TargetAgent);
            Assert.AreEqual("analyst", sent.FromAgent, "mittente certificato dal token");
            Assert.AreEqual("WSAA-TOT", sent.Scope);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sent.FederationId));

            // 1 hop consumato + correlazione sulla conversazione d'origine.
            var conv = ctx.Conversations().Single(c => c.Id == convId);
            Assert.AreEqual(1, conv.HopCount, "la richiesta federata è un fan-out: 1 hop");
            Assert.AreEqual(sent.FederationId, conv.FederationId?.ToString());
            Assert.AreEqual("marco@acme.it", conv.RemoteOwner);
            Assert.AreEqual("javadev", conv.RemoteAgent);
        }

        [TestMethod]
        public async Task Tell_the_agent_when_the_request_waits_for_a_computer_that_is_off()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("intervention-spento");
            SetupOwnership(ctx, path);
            var token = ctx.MintRunToken("analyst", path, ctx.SeedConversation(path).ToString());

            ctx.FederationSender.Result = MdExplorer.Services.Federation.RelayDelivery.Queued;
            var resp = await PostIntervention(ctx, token, "WSAA-TOT", "Genera il Java dal workflow.");
            var body = await resp.Content.ReadAsStringAsync();
            Assert.AreEqual(System.Net.HttpStatusCode.OK, resp.StatusCode, body);
            using var json = System.Text.Json.JsonDocument.Parse(body);
            Assert.AreEqual("queued", json.RootElement.GetProperty("delivery").GetString());
            StringAssert.Contains(json.RootElement.GetProperty("note").GetString(), "does not expire");

            ctx.FederationSender.Result = MdExplorer.Services.Federation.RelayDelivery.Delivered;
            var again = await (await PostIntervention(ctx, token, "WSAA-TOT", "E i test?")).Content.ReadAsStringAsync();
            using var json2 = System.Text.Json.JsonDocument.Parse(again);
            Assert.AreEqual("delivered", json2.RootElement.GetProperty("delivery").GetString());
        }

        [TestMethod]
        public async Task Ask_who_when_the_role_has_a_team_and_send_to_the_one_chosen()
        {
            // Workflow v3 (17/10/2026): lo stesso ruolo su più righe è un team; chi chiede sceglie, il servizio non indovina.
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("intervention-team");
            SetupOwnership(ctx, path);
            File.AppendAllText(Path.Combine(path, "ownership.md"), "| WSAA-TOT | lia@acme.it | javadev |\n");
            var token = ctx.MintRunToken("analyst", path, ctx.SeedConversation(path).ToString());

            var resp = await PostIntervention(ctx, token, "WSAA-TOT", "Genera il Java dal workflow.");
            var body = await resp.Content.ReadAsStringAsync();
            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, resp.StatusCode, body);
            StringAssert.Contains(body, "more than one person");
            StringAssert.Contains(body, "lia@acme.it");
            Assert.IsNull(ctx.FederationSender.LastPayload, "senza una scelta non parte niente");

            var payload = System.Text.Json.JsonSerializer.Serialize(new { scope = "WSAA-TOT", message = "Genera il Java.", owner = "lia@acme.it" });
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/request-intervention") { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            req.Headers.Add("X-MDE-Run-Token", token);
            var chosen = await ctx.Client.SendAsync(req);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, chosen.StatusCode, await chosen.Content.ReadAsStringAsync());
            Assert.AreEqual(FederationRoom.ComputeUserId("lia@acme.it"), ctx.FederationSender.LastTargetOwnerId);
        }

        [TestMethod]
        public async Task Fail_loud_on_an_unknown_scope()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("intervention-noscope");
            SetupOwnership(ctx, path);
            var token = ctx.MintRunToken("analyst", path, ctx.SeedConversation(path).ToString());

            var resp = await PostIntervention(ctx, token, "AMBITO-INESISTENTE", "ciao");
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, resp.StatusCode);
            Assert.IsNull(ctx.FederationSender.LastPayload, "niente da spedire per un ambito inesistente");
        }

        [TestMethod]
        public async Task Reject_without_a_run_token()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("intervention-noauth");
            SetupOwnership(ctx, path);

            var resp = await PostIntervention(ctx, null, "WSAA-TOT", "ciao");
            Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
        }
    }
}
