using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Federation;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 7a — il cerchio di ritorno. Lato ORIGINE: <c>RequestIntervention</c> registra un
    /// ledger <see cref="FederationDispatch"/> pending; all'arrivo dell'<c>intervention-result</c>
    /// (qui invocato direttamente sul receiver, l'harness è a UNA città con relay stubbato) il
    /// dispatch si chiude e l'agente d'origine viene risvegliato. Lato DESTINAZIONE: l'endpoint
    /// <c>/api/A2A/federation/result</c> spedisce l'esito correlato. E2E two-city vero = prova
    /// manuale live, non qui.
    /// </summary>
    [TestClass]
    public class FederatedResult_Should
    {
        private static void SetupOwnership(AgentCityContext ctx, string path)
        {
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "ownership.md" });
            File.WriteAllText(Path.Combine(path, "ownership.md"), @"---
mde_type: ownership
---
| Ambito | Git Email | Agenti |
|--------|-----------|--------|
| WSAA-TOT | marco@acme.it | javadev |
");
        }

        private async Task<HttpResponseMessage> PostIntervention(
            AgentCityContext ctx, string token, string scope, string message)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new { scope, message, topics = new[] { "java" } });
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/request-intervention")
            { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            if (token != null) req.Headers.Add("X-MDE-Run-Token", token);
            return await ctx.Client.SendAsync(req);
        }

        private async Task<(HttpStatusCode Status, string Body)> PostResult(
            AgentCityContext ctx, string token, object body)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(body);
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/federation/result")
            { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            if (token != null) req.Headers.Add("X-MDE-Run-Token", token);
            var resp = await ctx.Client.SendAsync(req);
            return (resp.StatusCode, await resp.Content.ReadAsStringAsync());
        }

        // ---- Lato ORIGINE: ledger ----

        [TestMethod]
        public async Task Persist_a_pending_dispatch_when_it_routes_an_intervention()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fedresult-ledger");
            SetupOwnership(ctx, path);
            var convId = ctx.SeedConversation(path);
            var token = ctx.MintRunToken("analyst", path, convId.ToString());

            var resp = await PostIntervention(ctx, token, "WSAA-TOT", "Genera il Java dal workflow.");
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());

            var sent = ctx.FederationSender.LastPayload;
            var dispatch = ctx.Dispatches().SingleOrDefault();
            Assert.IsNotNull(dispatch, "deve esistere una riga di ledger per l'intervento smistato");
            Assert.AreEqual(FederationDispatch.StatusEnum.Pending, dispatch.Status);
            Assert.AreEqual(Guid.Parse(sent.RequestId), dispatch.RequestId, "la RequestId di ledger = quella spedita (chiave di correlazione)");
            Assert.AreEqual("analyst", dispatch.OriginAgent, "l'agente d'origine è dai claims, NON da StartedBy");
            Assert.AreEqual(convId, dispatch.ConversationId);
            Assert.AreEqual("javadev", dispatch.TargetAgent);
        }

        // ---- Lato ORIGINE: ritorno ----

        [TestMethod]
        public async Task Complete_the_dispatch_and_wake_the_origin_agent_on_result()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fedresult-return");
            SetupOwnership(ctx, path);
            var convId = ctx.SeedConversation(path);
            var token = ctx.MintRunToken("analyst", path, convId.ToString());
            await PostIntervention(ctx, token, "WSAA-TOT", "Genera il Java dal workflow.");
            var requestId = Guid.Parse(ctx.FederationSender.LastPayload.RequestId);

            // L'esito torna (nell'harness a una città, invocazione diretta del receiver).
            var receiver = ctx.Factory.Services.GetRequiredService<IFederatedResultReceiver>();
            receiver.Receive(path, new FederatedResultPayload
            {
                Kind = FederationKind.InterventionResult,
                RequestId = requestId.ToString(),
                Verdict = FederationVerdict.Success,
            });

            var dispatch = ctx.Dispatches().Single(d => d.RequestId == requestId);
            Assert.AreEqual(FederationDispatch.StatusEnum.Completed, dispatch.Status, "il dispatch si chiude all'arrivo dell'esito");
            Assert.IsNotNull(dispatch.CompletedAt);

            // L'agente d'origine è stato risvegliato: un AgentMessage con l'audit federated-result.
            var wake = ctx.Messages().SingleOrDefault(m =>
                m.ToAgent == "analyst" && m.TriggerSource == FederatedResultReceiver.TriggerSource);
            Assert.IsNotNull(wake, "l'agente d'origine deve essere risvegliato con TriggerSource=federated-result");
            Assert.AreEqual(FederatedResultReceiver.FederationSender, wake.FromAgent);
            Assert.AreEqual(convId, wake.ConversationId, "il risveglio rientra nella conversazione d'origine");
        }

        [TestMethod]
        public async Task Be_idempotent_on_a_redelivered_result()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fedresult-idem");
            SetupOwnership(ctx, path);
            var convId = ctx.SeedConversation(path);
            var token = ctx.MintRunToken("analyst", path, convId.ToString());
            await PostIntervention(ctx, token, "WSAA-TOT", "Genera il Java.");
            var requestId = Guid.Parse(ctx.FederationSender.LastPayload.RequestId);

            var receiver = ctx.Factory.Services.GetRequiredService<IFederatedResultReceiver>();
            var payload = new FederatedResultPayload
            {
                Kind = FederationKind.InterventionResult,
                RequestId = requestId.ToString(),
                Verdict = FederationVerdict.Success,
            };
            receiver.Receive(path, payload);
            receiver.Receive(path, payload);   // riconsegna del relay

            var wakes = ctx.Messages().Count(m =>
                m.ToAgent == "analyst" && m.TriggerSource == FederatedResultReceiver.TriggerSource);
            Assert.AreEqual(1, wakes, "una riconsegna non deve risvegliare due volte l'origine");
        }

        // ---- Filtro anti-avvelenamento ----

        [TestMethod]
        public void Drop_a_result_whose_request_id_is_unknown()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fedresult-unknown");

            var receiver = ctx.Factory.Services.GetRequiredService<IFederatedResultReceiver>();
            receiver.Receive(path, new FederatedResultPayload
            {
                Kind = FederationKind.InterventionResult,
                RequestId = Guid.NewGuid().ToString(),   // nessun dispatch corrispondente
                Verdict = FederationVerdict.Success,
            });

            Assert.AreEqual(0, ctx.Dispatches().Count, "nessun ledger creato");
            Assert.AreEqual(0, ctx.Messages().Count, "nessun agente risvegliato per una RequestId sconosciuta");
        }

        // ---- Lato DESTINAZIONE: endpoint ----

        [TestMethod]
        public async Task Send_a_correlated_result_from_the_destination_endpoint()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fedresult-endpoint");
            var convId = ctx.SeedConversation(path);
            var requestId = Guid.NewGuid();
            ctx.MarkConversationFederated(convId, Guid.NewGuid(), "origin@acme.it", "analyst");
            ctx.SetConversationRequestId(convId, requestId);
            var token = ctx.MintRunToken("javadev", path, convId.ToString());

            var (status, bodyText) = await PostResult(ctx, token,
                new { verdict = FederationVerdict.Rejected, reason = FederationReason.NotForMe });
            Assert.AreEqual(HttpStatusCode.OK, status, bodyText);

            var result = ctx.FederationSender.LastResultPayload;
            Assert.IsNotNull(result, "il sender deve aver spedito l'esito");
            Assert.AreEqual(FederationKind.InterventionResult, result.Kind);
            Assert.AreEqual(requestId.ToString(), result.RequestId, "correlata alla RequestId della conversazione");
            Assert.AreEqual(FederationVerdict.Rejected, result.Verdict);
            Assert.AreEqual(FederationReason.NotForMe, result.Reason);
            Assert.AreEqual(FederationRoom.ComputeUserId("origin@acme.it"), ctx.FederationSender.LastTargetOwnerId);
        }

        [TestMethod]
        public async Task Reject_the_result_endpoint_without_a_run_token()
        {
            using var ctx = new AgentCityContext();
            var (status, _) = await PostResult(ctx, null, new { verdict = "success" });
            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }

        [TestMethod]
        public async Task Fail_loud_when_the_conversation_carries_no_request_id()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fedresult-norid");
            var convId = ctx.SeedConversation(path);
            ctx.MarkConversationFederated(convId, Guid.NewGuid(), "origin@acme.it", "analyst");
            // NB: nessuna SetConversationRequestId → manca il ponte 7a.
            var token = ctx.MintRunToken("javadev", path, convId.ToString());

            var (status, _) = await PostResult(ctx, token, new { verdict = "success" });
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, status);
            Assert.IsNull(ctx.FederationSender.LastResultPayload, "niente da spedire senza correlazione");
        }
    }
}
