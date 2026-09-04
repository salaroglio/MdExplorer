using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 5b — memoria degli agenti su Fuseki, end-to-end attraverso il Service reale e i
    /// controller /api/A2A/memory. Il cuore è l'<b>isolamento</b>: il named graph è forzato
    /// dall'identità del RunToken, quindi l'agente B non vede mai i fatti dell'agente A.
    /// <para>Richiede un Fuseki reale su <c>http://localhost:3030</c>: se assente il test è
    /// <c>Inconclusive</c> (non fallisce la suite).</para>
    /// </summary>
    [TestClass]
    public class AgentMemory_Should
    {
        private const string Fuseki = "http://localhost:3030";

        private static async Task<bool> FusekiUp()
        {
            try
            {
                using var h = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var r = await h.GetAsync(Fuseki + "/$/ping");
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        private static async Task<(HttpStatusCode Status, JsonElement Body)> PostToken(
            AgentCityContext ctx, string url, string runToken, object payload)
        {
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            req.Headers.Add("X-MDE-Run-Token", runToken);
            var resp = await ctx.Client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            JsonElement json;
            try { json = string.IsNullOrWhiteSpace(body) ? default : JsonDocument.Parse(body).RootElement; }
            catch (JsonException) { throw new Exception($"Risposta non-JSON ({(int)resp.StatusCode}) da {url}: {body}"); }
            return (resp.StatusCode, json);
        }

        [TestMethod]
        public async Task Isolate_each_agent_memory_and_share_the_shared_graph()
        {
            if (!await FusekiUp())
            {
                Assert.Inconclusive("Fuseki non raggiungibile su " + Fuseki + " — test saltato.");
                return;
            }

            using var ctx = new AgentCityContext();
            var (projectId, path) = ctx.SeedProject("mem-iso");
            ctx.WriteLlmCitizen(path, "agent-a", "A", new[] { "*" });
            ctx.WriteLlmCitizen(path, "agent-b", "B", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "agent-a");
            ctx.Trust(path, "agent-b");

            // Dataset unico per il test → nessuna interferenza con altri dati Fuseki.
            var dataset = "mde_it_mem_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.EnableFuseki(projectId, Fuseki, dataset);

            var tokenA = ctx.MintRunToken("agent-a", path, Guid.NewGuid().ToString());
            var tokenB = ctx.MintRunToken("agent-b", path, Guid.NewGuid().ToString());

            try
            {
                // A e B asseriscono ciascuno un fatto privato sullo stesso topic 'pagamenti'.
                var a1 = await PostToken(ctx, "/api/A2A/memory/assert", tokenA,
                    new { statement = "il batch pagamenti di A gira alle 02:00", about = "pagamenti", confidence = 0.9 });
                Assert.AreEqual(HttpStatusCode.OK, a1.Status, a1.Body.ToString());
                Assert.IsTrue(a1.Body.GetProperty("asserted").GetBoolean());

                var b1 = await PostToken(ctx, "/api/A2A/memory/assert", tokenB,
                    new { statement = "il segreto di B sul modulo pagamenti", about = "pagamenti", confidence = 0.9 });
                Assert.AreEqual(HttpStatusCode.OK, b1.Status, b1.Body.ToString());

                // B interroga 'pagamenti': deve vedere SOLO il proprio fatto, mai quello di A.
                var qb = await PostToken(ctx, "/api/A2A/memory/query", tokenB,
                    new { topics = new[] { "pagamenti" } });
                Assert.AreEqual(HttpStatusCode.OK, qb.Status, qb.Body.ToString());
                var factsB = Statements(qb.Body);
                CollectionAssert.Contains(factsB, "il segreto di B sul modulo pagamenti");
                CollectionAssert.DoesNotContain(factsB, "il batch pagamenti di A gira alle 02:00");

                // A interroga: vede il proprio, non quello di B.
                var qa = await PostToken(ctx, "/api/A2A/memory/query", tokenA,
                    new { topics = new[] { "pagamenti" } });
                var factsA = Statements(qa.Body);
                CollectionAssert.Contains(factsA, "il batch pagamenti di A gira alle 02:00");
                CollectionAssert.DoesNotContain(factsA, "il segreto di B sul modulo pagamenti");

                // Un topic non pertinente non restituisce nulla.
                var qNone = await PostToken(ctx, "/api/A2A/memory/query", tokenA,
                    new { topics = new[] { "argomento-inesistente" } });
                Assert.AreEqual(0, Statements(qNone.Body).Count);
            }
            finally
            {
                try { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); } catch { }
            }
        }

        [TestMethod]
        public async Task Refuse_when_no_run_token()
        {
            using var ctx = new AgentCityContext();
            var content = new StringContent("{\"statement\":\"x\"}", Encoding.UTF8, "application/json");
            var resp = await ctx.Client.PostAsync("/api/A2A/memory/assert", content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [TestMethod]
        public async Task Refuse_when_fuseki_disabled_for_project()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("mem-nofuseki");
            ctx.WriteLlmCitizen(path, "agent-a", "A", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "agent-a");
            var token = ctx.MintRunToken("agent-a", path, Guid.NewGuid().ToString());

            var r = await PostToken(ctx, "/api/A2A/memory/assert", token,
                new { statement = "qualcosa", about = "x" });
            Assert.AreEqual(HttpStatusCode.Conflict, r.Status);   // 409: memoria non abilitata
        }

        private static List<string> Statements(JsonElement body)
        {
            var list = new List<string>();
            if (body.ValueKind == JsonValueKind.Undefined) return list;
            foreach (var f in body.GetProperty("facts").EnumerateArray())
                list.Add(f.GetProperty("statement").GetString());
            return list;
        }
    }
}
