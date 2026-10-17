using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Features.AgentMemory;
using MdExplorer.Features.Federation;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.AgentMemory;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 7b — memoria dagli esiti. Alla riconciliazione (dentro <see cref="FederatedResultReceiver"/>)
    /// il verdict aggiorna la memoria dell'agente d'ORIGINE: <c>success</c> rinforza la confidence del
    /// fatto di routing, <c>rejected</c> la erode + asserisce un fatto sul CHI, <c>not-ready</c> la
    /// erode leggero + asserisce la <b>precondizione</b> sul COSA (aboutTag = reason macchina).
    /// <para>Richiede Fuseki su <c>http://localhost:3030</c>: assente → <c>Inconclusive</c>.</para>
    /// </summary>
    [TestClass]
    public class FederatedResultMemory_Should
    {
        private const string Fuseki = "http://localhost:3030";

        private static async Task<bool> FusekiUp()
        {
            try { using var h = new HttpClient { Timeout = TimeSpan.FromSeconds(3) }; return (await h.GetAsync(Fuseki + "/$/ping")).IsSuccessStatusCode; }
            catch { return false; }
        }

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

        /// <summary>Setup completo: progetto + agente d'origine cittadino (con identità) + ownership + Fuseki.</summary>
        private static (Guid projectKey, string path) SetupOriginCitizen(AgentCityContext ctx, string name, string dataset)
        {
            var (projectKey, path) = ctx.SeedProject(name);
            ctx.WriteLlmCitizen(path, "analyst", "Analyst", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "analyst");
            SetupOwnership(ctx, path);
            ctx.EnableFuseki(projectKey, Fuseki, dataset);
            return (projectKey, path);
        }

        private static async Task<Guid> RouteInterventionAndGetRequestId(AgentCityContext ctx, string path, Guid convId)
        {
            var token = ctx.MintRunToken("analyst", path, convId.ToString());
            var payload = System.Text.Json.JsonSerializer.Serialize(new { scope = "WSAA-TOT", message = "Genera il Java.", topics = new[] { "java" } });
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/request-intervention")
            { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            req.Headers.Add("X-MDE-Run-Token", token);
            var resp = await ctx.Client.SendAsync(req);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            return Guid.Parse(ctx.FederationSender.LastPayload.RequestId);
        }

        private static async Task<IReadOnlyList<MemoryFactDetail>> ReadFacts(AgentCityContext ctx, string path)
        {
            var registry = ctx.Factory.Services.GetRequiredService<IAgentRegistryService>();
            var id = registry.RefreshCatalog(path).First(e => e.Name == "analyst").IdentityId.Value;
            var resolver = ctx.Factory.Services.GetRequiredService<IFusekiConnectionResolver>();
            var memory = ctx.Factory.Services.GetRequiredService<IAgentMemoryService>();
            var conn = await resolver.ResolveAsync(path);
            return await memory.ListAsync(conn, new[] { AgentMemoryGraphs.ForAgent(id) }, 100);
        }

        private static async Task Receive(AgentCityContext ctx, string path, Guid requestId, string verdict, string reason = null)
        {
            var receiver = ctx.Factory.Services.GetRequiredService<IFederatedResultReceiver>();
            await receiver.Receive(path, new FederatedResultPayload
            {
                Kind = FederationKind.InterventionResult,
                RequestId = requestId.ToString(),
                Verdict = verdict,
                Reason = reason,
            });
        }

        [TestMethod]
        public async Task Reinforce_the_routing_fact_on_success()
        {
            if (!await FusekiUp()) { Assert.Inconclusive("Fuseki non raggiungibile."); return; }
            using var ctx = new AgentCityContext();
            var dataset = "mde_it_7b_ok_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var (_, path) = SetupOriginCitizen(ctx, "fed7b-ok", dataset);
            try
            {
                var convId = ctx.SeedConversation(path);
                var requestId = await RouteInterventionAndGetRequestId(ctx, path, convId);
                await Receive(ctx, path, requestId, FederationVerdict.Success);

                var facts = await ReadFacts(ctx, path);
                var routing = facts.SingleOrDefault(f => f.Statement.Contains("routing verso 'javadev'"));
                Assert.IsNotNull(routing, "il fatto di routing deve essere stato asserito");
                Assert.AreEqual(0.70, routing.Confidence, 0.001, "success: confidence moderata + rinforzo (0.6+0.1)");
                CollectionAssert.Contains(routing.Tags.ToList(), "java", "aboutTag = topic del messaggio");
            }
            finally { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); }
        }

        [TestMethod]
        public async Task Erode_and_assert_a_who_fact_on_rejected()
        {
            if (!await FusekiUp()) { Assert.Inconclusive("Fuseki non raggiungibile."); return; }
            using var ctx = new AgentCityContext();
            var dataset = "mde_it_7b_rej_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var (_, path) = SetupOriginCitizen(ctx, "fed7b-rej", dataset);
            try
            {
                var convId = ctx.SeedConversation(path);
                var requestId = await RouteInterventionAndGetRequestId(ctx, path, convId);
                await Receive(ctx, path, requestId, FederationVerdict.Rejected, FederationReason.NotForMe);

                var facts = await ReadFacts(ctx, path);
                var routing = facts.Single(f => f.Statement.Contains("routing verso 'javadev'"));
                Assert.AreEqual(0.45, routing.Confidence, 0.001, "rejected: confidence moderata - erosione (0.6-0.15)");

                var who = facts.SingleOrDefault(f => f.Statement.Contains("non è competente"));
                Assert.IsNotNull(who, "rejected deve asserire un fatto sul CHI");
                CollectionAssert.Contains(who.Tags.ToList(), FederationReason.NotForMe, "aboutTag = reason macchina not-for-me");
            }
            finally { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); }
        }

        [TestMethod]
        public async Task Assert_the_precondition_on_not_ready()
        {
            if (!await FusekiUp()) { Assert.Inconclusive("Fuseki non raggiungibile."); return; }
            using var ctx = new AgentCityContext();
            var dataset = "mde_it_7b_nr_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var (_, path) = SetupOriginCitizen(ctx, "fed7b-nr", dataset);
            try
            {
                var convId = ctx.SeedConversation(path);
                var requestId = await RouteInterventionAndGetRequestId(ctx, path, convId);
                var reason = FederationReason.PreconditionPrefix + "analisi-funzionale";
                await Receive(ctx, path, requestId, FederationVerdict.NotReady, reason);

                var facts = await ReadFacts(ctx, path);
                var routing = facts.Single(f => f.Statement.Contains("routing verso 'javadev'"));
                Assert.AreEqual(0.55, routing.Confidence, 0.001, "not-ready: erosione leggera (0.6-0.05)");

                var precondition = facts.SingleOrDefault(f => f.Statement.Contains("serve:"));
                Assert.IsNotNull(precondition, "not-ready deve asserire la precondizione sul COSA");
                CollectionAssert.Contains(precondition.Tags.ToList(), reason, "aboutTag = reason precondition:<x>");
            }
            finally { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); }
        }
    }
}
