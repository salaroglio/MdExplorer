using System;
using System.Collections.Generic;
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
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// L'avviso di lavoro non processato (17/10/2026). Lato origine: una richiesta a un altro computer senza esito da 24
    /// ore → un avviso in posta, una volta; gli esiti automatici <c>completed</c> e <c>declined</c> chiudono l'attesa
    /// senza svegliare l'agente d'origine. Lato destinazione: il turno finito e il rifiuto della persona tornano da soli.
    /// </summary>
    [TestClass]
    public class OverdueWork_Should
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

        /// <summary>L'analista chiede un intervento a javadev, che sta sul computer di Marco.</summary>
        private static async Task<(string Path, Guid RequestId)> Requested(AgentCityContext ctx, string name)
        {
            var (_, path) = ctx.SeedProject(name);
            SetupOwnership(ctx, path);
            var convId = ctx.SeedConversation(path);
            var token = ctx.MintRunToken("analyst", path, convId.ToString());
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/request-intervention")
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(
                    new { scope = "WSAA-TOT", message = "Genera il Java dal workflow.", topics = new[] { "java" } }), Encoding.UTF8, "application/json"),
            };
            req.Headers.Add("X-MDE-Run-Token", token);
            var resp = await ctx.Client.SendAsync(req);
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            return (path, Guid.Parse(ctx.FederationSender.LastPayload.RequestId));
        }

        private static IOverdueWorkWatcher Watcher(AgentCityContext ctx) => ctx.Factory.Services.GetRequiredService<IOverdueWorkWatcher>();

        private static Task Result(AgentCityContext ctx, string path, Guid requestId, string verdict)
            => ctx.Factory.Services.GetRequiredService<IFederatedResultReceiver>().Receive(path, new FederatedResultPayload
            {
                Kind = FederationKind.InterventionResult, RequestId = requestId.ToString(), Verdict = verdict,
            });

        private static List<AgentMessage> ToUser(AgentCityContext ctx) => ctx.Messages().Where(m => m.ToAgent == "user").ToList();

        [TestMethod]
        public async Task Warn_the_person_once_when_a_request_has_no_outcome_after_24_hours()
        {
            using var ctx = new AgentCityContext();
            var (_, requestId) = await Requested(ctx, "overdue-warn");

            Assert.AreEqual(0, Watcher(ctx).CheckDispatches(DateTime.UtcNow.AddHours(23)), "prima delle 24 ore niente");
            Assert.AreEqual(0, ToUser(ctx).Count);

            Assert.AreEqual(1, Watcher(ctx).CheckDispatches(DateTime.UtcNow.AddHours(25)));
            var notice = ToUser(ctx).Single();
            Assert.AreEqual("analyst", notice.FromAgent, "viene dall'agente che ha mandato il lavoro");
            StringAssert.Contains(notice.Body, "marco@acme.it", "dice con chi verificare");
            StringAssert.Contains(notice.Body, "more than 24 hours");
            Assert.AreEqual(FederationDispatch.StatusEnum.Pending, ctx.Dispatches().Single(d => d.RequestId == requestId).Status,
                "l'avviso non chiude l'attesa: l'esito può ancora arrivare");

            Assert.AreEqual(0, Watcher(ctx).CheckDispatches(DateTime.UtcNow.AddHours(50)), "una volta sola");
            Assert.AreEqual(1, ToUser(ctx).Count);
        }

        [TestMethod]
        public async Task Close_the_wait_without_waking_anyone_when_the_remote_turn_completed()
        {
            using var ctx = new AgentCityContext();
            var (path, requestId) = await Requested(ctx, "overdue-completed");

            await Result(ctx, path, requestId, FederationVerdict.Completed);

            Assert.AreEqual(FederationDispatch.StatusEnum.Completed, ctx.Dispatches().Single(d => d.RequestId == requestId).Status);
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "analyst"), "niente risveglio dell'agente d'origine (niente ping-pong)");
            Assert.AreEqual(0, ToUser(ctx).Count);
            Assert.AreEqual(0, Watcher(ctx).CheckDispatches(DateTime.UtcNow.AddHours(25)), "con l'esito arrivato nessun avviso");
        }

        [TestMethod]
        public async Task Tell_the_person_when_the_colleague_declined_the_request()
        {
            using var ctx = new AgentCityContext();
            var (path, requestId) = await Requested(ctx, "overdue-declined");

            await Result(ctx, path, requestId, FederationVerdict.Declined);

            Assert.AreEqual(FederationDispatch.StatusEnum.Completed, ctx.Dispatches().Single(d => d.RequestId == requestId).Status);
            var notice = ToUser(ctx).Single();
            StringAssert.Contains(notice.Body, "marco@acme.it declined");
            Assert.IsFalse(ctx.Messages().Any(m => m.ToAgent == "analyst"));
            Assert.AreEqual(0, Watcher(ctx).CheckDispatches(DateTime.UtcNow.AddHours(25)));
        }

        // ---- Lato DESTINAZIONE: l'esito torna da solo ----

        private static Guid Receive(AgentCityContext ctx, string path, Guid requestId)
            => ctx.Factory.Services.GetRequiredService<IFederatedRequestReceiver>().Receive(path, new FederatedRequestPayload
            {
                FederationId = Guid.NewGuid().ToString(),
                RequestId = requestId.ToString(),
                FromOwner = "anna@acme.it",
                FromAgent = "analyst",
                TargetAgent = "javadev",
                Scope = "WSAA-TOT",
                Message = "Genera il Java dal workflow allegato.",
                Topics = new List<string> { "java" },
            });

        [TestMethod]
        public async Task Report_declined_to_the_origin_when_the_person_rejects_the_request()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("overdue-gate-reject");
            ctx.WriteLlmCitizen(path, "javadev", "Sviluppatore Java", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "javadev");
            var requestId = Guid.NewGuid();
            var gateId = Receive(ctx, path, requestId);

            var (status, _) = await ctx.PostJson($"/api/A2A/federation/requests/{gateId}/reject", null);
            Assert.AreEqual(HttpStatusCode.OK, status);

            var sent = ctx.FederationSender.LastResultPayload;
            Assert.IsNotNull(sent, "il rifiuto torna all'origine");
            Assert.AreEqual(FederationVerdict.Declined, sent.Verdict);
            Assert.AreEqual(requestId.ToString(), sent.RequestId);
            Assert.AreEqual(FederationRoom.ComputeUserId("anna@acme.it"), ctx.FederationSender.LastTargetOwnerId);
        }

        [TestMethod]
        public async Task Report_completed_to_the_origin_when_the_approved_turn_finishes()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("overdue-gate-completed");
            ctx.WriteLlmCitizen(path, "javadev", "Sviluppatore Java", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "javadev");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("fatto");
            var requestId = Guid.NewGuid();
            var gateId = Receive(ctx, path, requestId);

            var (status, _) = await ctx.PostJson($"/api/A2A/federation/requests/{gateId}/approve", "{}");
            Assert.AreEqual(HttpStatusCode.OK, status);

            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (ctx.FederationSender.LastResultPayload == null && DateTime.UtcNow < deadline)
                await Task.Delay(200);
            var sent = ctx.FederationSender.LastResultPayload;
            Assert.IsNotNull(sent, "il turno finito torna all'origine da solo");
            Assert.AreEqual(FederationVerdict.Completed, sent.Verdict);
            Assert.AreEqual(requestId.ToString(), sent.RequestId);
        }
    }
}
