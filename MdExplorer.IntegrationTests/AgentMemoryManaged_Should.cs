using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Features.Utilities;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Memoria degli agenti sul Fuseki <b>gestito</b>: con <c>Managed=true</c> il Service avvia il Fuseki
    /// del pacchetto (D19) su porta loopback e la memoria funziona senza alcun Fuseki esterno.
    /// Richiede Java 21 sulla macchina: assente → Inconclusive dichiarato.
    /// </summary>
    [TestClass]
    public class AgentMemoryManaged_Should
    {
        private static bool Java21Available(out string reason)
        {
            var probe = new JavaLocator(null).Probe("java");
            if (!probe.IsRunnable) { reason = "java non eseguibile dal PATH: " + probe.Error; return false; }
            if (probe.Major == null || probe.Major < 21) { reason = $"serve Java 21, trovato «{probe.Raw}»"; return false; }
            reason = null;
            return true;
        }

        private static async Task<(HttpStatusCode Status, JsonElement Body)> PostToken(AgentCityContext ctx, string url, string token, object payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            { Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json") };
            req.Headers.Add("X-MDE-Run-Token", token);
            var resp = await ctx.Client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            try { return (resp.StatusCode, string.IsNullOrWhiteSpace(body) ? default : JsonDocument.Parse(body).RootElement); }
            catch (JsonException) { throw new Exception($"Risposta non-JSON ({(int)resp.StatusCode}) da {url}: {body}"); }
        }

        [TestMethod]
        public async Task Start_the_bundled_fuseki_and_serve_memory()
        {
            if (!Java21Available(out var why)) { Assert.Inconclusive(why); return; }

            using var ctx = new AgentCityContext();

            var (projectId, path) = ctx.SeedProject("mem-managed");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            // Managed=true, dataset dedicato: nessun Fuseki esterno coinvolto.
            ctx.EnableManagedFuseki(projectId, "mde_managed_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            var token = ctx.MintRunToken("worker", path, Guid.NewGuid().ToString());
            var a = await PostToken(ctx, "/api/A2A/memory/assert", token,
                new { statement = "gestito: il fatto viene servito dal Fuseki del pacchetto", about = "infra", confidence = 0.9 });
            Assert.AreEqual(HttpStatusCode.OK, a.Status, a.Body.ToString());

            var q = await PostToken(ctx, "/api/A2A/memory/query", token, new { topics = new[] { "infra" } });
            Assert.AreEqual(HttpStatusCode.OK, q.Status, q.Body.ToString());
            var statements = q.Body.GetProperty("facts").EnumerateArray()
                .Select(f => f.GetProperty("statement").GetString()).ToList();
            CollectionAssert.Contains(statements, "gestito: il fatto viene servito dal Fuseki del pacchetto");
        }
    }
}
