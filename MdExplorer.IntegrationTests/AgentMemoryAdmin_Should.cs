using System;
using System.Linq;
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
    /// Fase 5d — vista umana + curatela della memoria (/api/mem). L'umano ispeziona i fatti di
    /// tutti i cittadini, cambia la confidence e rimuove fatti; la curatela è vincolata ai grafi
    /// del progetto. <para>Richiede Fuseki su localhost:3030: assente → Inconclusive.</para>
    /// </summary>
    [TestClass]
    public class AgentMemoryAdmin_Should
    {
        private const string Fuseki = "http://localhost:3030";

        private static async Task<bool> FusekiUp()
        {
            try { using var h = new HttpClient { Timeout = TimeSpan.FromSeconds(3) }; return (await h.GetAsync(Fuseki + "/$/ping")).IsSuccessStatusCode; }
            catch { return false; }
        }

        [TestMethod]
        public async Task List_curate_and_delete_facts_across_agents()
        {
            if (!await FusekiUp()) { Assert.Inconclusive("Fuseki non raggiungibile."); return; }

            using var ctx = new AgentCityContext();
            var (projectId, path) = ctx.SeedProject("mem-admin");
            ctx.WriteLlmCitizen(path, "agent-a", "A", new[] { "*" });
            ctx.WriteLlmCitizen(path, "agent-b", "B", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "agent-a");
            ctx.Trust(path, "agent-b");

            var dataset = "mde_it_admin_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.EnableFuseki(projectId, Fuseki, dataset);

            try
            {
                var tokenA = ctx.MintRunToken("agent-a", path, Guid.NewGuid().ToString());
                var tokenB = ctx.MintRunToken("agent-b", path, Guid.NewGuid().ToString());
                await AssertFact(ctx, tokenA, "fatto di A", "x", 0.5);
                await AssertFact(ctx, tokenB, "fatto di B", "y", 0.6);

                // La vista umana vede i fatti di ENTRAMBI, etichettati per agente.
                var listed = await GetJson(ctx, $"/api/mem/facts?projectPath={Uri.EscapeDataString(path)}");
                var facts = listed.GetProperty("facts").EnumerateArray().ToList();
                Assert.AreEqual(2, facts.Count, listed.ToString());
                var byStatement = facts.ToDictionary(f => f.GetProperty("statement").GetString());
                Assert.AreEqual("agent-a", byStatement["fatto di A"].GetProperty("agent").GetString());
                Assert.AreEqual("agent-b", byStatement["fatto di B"].GetProperty("agent").GetString());

                var factA = byStatement["fatto di A"];
                var uriA = factA.GetProperty("factUri").GetString();
                var graphA = factA.GetProperty("graph").GetString();

                // Curatela: alza la confidence di A a 0.95.
                var setResp = await ctx.Client.PostAsync("/api/mem/facts/confidence",
                    new StringContent(JsonSerializer.Serialize(new { projectPath = path, graph = graphA, factUri = uriA, confidence = 0.95 }),
                        Encoding.UTF8, "application/json"));
                Assert.AreEqual(HttpStatusCode.OK, setResp.StatusCode, await setResp.Content.ReadAsStringAsync());

                var afterSet = await GetJson(ctx, $"/api/mem/facts?projectPath={Uri.EscapeDataString(path)}&agent=agent-a");
                var confA = afterSet.GetProperty("facts").EnumerateArray().First().GetProperty("confidence").GetDouble();
                Assert.AreEqual(0.95, confA, 0.001);

                // Guard: un grafo che non è del progetto è rifiutato.
                var badResp = await ctx.Client.PostAsync("/api/mem/facts/confidence",
                    new StringContent(JsonSerializer.Serialize(new { projectPath = path, graph = "urn:mde:mem:agent:00000000-0000-0000-0000-000000000000", factUri = uriA, confidence = 0.1 }),
                        Encoding.UTF8, "application/json"));
                Assert.AreEqual(HttpStatusCode.BadRequest, badResp.StatusCode);

                // Delete del fatto di A: resta solo B.
                var delResp = await ctx.Client.DeleteAsync($"/api/mem/facts?projectPath={Uri.EscapeDataString(path)}&graph={Uri.EscapeDataString(graphA)}&factUri={Uri.EscapeDataString(uriA)}");
                Assert.AreEqual(HttpStatusCode.OK, delResp.StatusCode, await delResp.Content.ReadAsStringAsync());

                var afterDel = await GetJson(ctx, $"/api/mem/facts?projectPath={Uri.EscapeDataString(path)}");
                var remaining = afterDel.GetProperty("facts").EnumerateArray().Select(f => f.GetProperty("statement").GetString()).ToList();
                CollectionAssert.AreEqual(new[] { "fatto di B" }, remaining);

                // Il diario markdown riflette lo stato.
                var diary = await ctx.Client.GetStringAsync($"/api/mem/diary?projectPath={Uri.EscapeDataString(path)}");
                StringAssert.Contains(diary, "# Diario della memoria degli agenti");
                StringAssert.Contains(diary, "fatto di B");
                Assert.IsFalse(diary.Contains("fatto di A"), "il fatto rimosso non deve comparire nel diario");
            }
            finally
            {
                try { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); } catch { }
            }
        }

        [TestMethod]
        public async Task Refuse_when_fuseki_disabled()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("mem-admin-off");
            var resp = await ctx.Client.GetAsync($"/api/mem/facts?projectPath={Uri.EscapeDataString(path)}");
            Assert.AreEqual(HttpStatusCode.Conflict, resp.StatusCode);
        }

        private static async Task AssertFact(AgentCityContext ctx, string runToken, string statement, string about, double confidence)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/memory/assert")
            { Content = new StringContent(JsonSerializer.Serialize(new { statement, about, confidence }), Encoding.UTF8, "application/json") };
            req.Headers.Add("X-MDE-Run-Token", runToken);
            var resp = await ctx.Client.SendAsync(req);
            Assert.IsTrue(resp.IsSuccessStatusCode, await resp.Content.ReadAsStringAsync());
        }

        private static async Task<JsonElement> GetJson(AgentCityContext ctx, string url)
        {
            var resp = await ctx.Client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, body);
            return JsonDocument.Parse(body).RootElement;
        }
    }
}
