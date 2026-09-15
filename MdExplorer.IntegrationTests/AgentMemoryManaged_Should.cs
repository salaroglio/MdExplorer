using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Roadmap Fuseki — istanza <b>gestita</b> (addon on-demand): con <c>Managed=true</c> il
    /// Service avvia una propria istanza Fuseki su porta loopback e la memoria funziona senza
    /// alcun Fuseki esterno. <para>Richiede il jar reale + <c>java</c> nel PATH: assenti →
    /// Inconclusive. Verifica anche il fail-loud quando l'addon non è installato.</para>
    /// </summary>
    [TestClass]
    public class AgentMemoryManaged_Should
    {
        // Dist reale usata per la verifica (Fuseki 4.10.0 già presente sulla macchina di sviluppo).
        private const string RealDist = "/home/carlo/Documents/sviluppo/dedagroup/raiffeisen/.tools/apache-jena-fuseki-4.10.0";
        private const string Version = "4.10.0";
        private static string RealJar => Path.Combine(RealDist, "fuseki-server.jar");

        private static bool JavaAndJarAvailable()
            => File.Exists(RealJar) && Directory.Exists(Path.Combine(RealDist, "webapp")) && !string.IsNullOrEmpty(WhichJava());

        private static string WhichJava()
        {
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                var candidate = Path.Combine(dir, "java");
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }

        // Allestisce il layout gestito: tools/fuseki/{ver}/ = dist COMPLETA (jar + webapp) via
        // symlink alla dist reale, + il marker current. È ciò che l'addon-downloader dovrà produrre.
        private static void SetupManagedLayout()
        {
            var toolsRoot = Path.Combine(CrossPlatformPath.GetMdExplorerDataDirectory(), "tools", "fuseki");
            var toolsVer = Path.Combine(toolsRoot, Version);
            Directory.CreateDirectory(toolsRoot);
            if (!Directory.Exists(toolsVer))
                Directory.CreateSymbolicLink(toolsVer, RealDist);
            File.WriteAllText(Path.Combine(toolsRoot, "current"), Version);
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
        public async Task Start_a_managed_instance_and_serve_memory()
        {
            if (!JavaAndJarAvailable()) { Assert.Inconclusive("jar Fuseki reale o java assenti."); return; }

            using var ctx = new AgentCityContext();
            SetupManagedLayout();

            var (projectId, path) = ctx.SeedProject("mem-managed");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            // Managed=true, dataset dedicato: nessun Fuseki esterno coinvolto.
            ctx.EnableManagedFuseki(projectId, "mde_managed_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            var token = ctx.MintRunToken("worker", path, Guid.NewGuid().ToString());
            var a = await PostToken(ctx, "/api/A2A/memory/assert", token,
                new { statement = "gestito: il fatto viene servito da un Fuseki avviato dal Service", about = "infra", confidence = 0.9 });
            Assert.AreEqual(HttpStatusCode.OK, a.Status, a.Body.ToString());

            var q = await PostToken(ctx, "/api/A2A/memory/query", token, new { topics = new[] { "infra" } });
            Assert.AreEqual(HttpStatusCode.OK, q.Status, q.Body.ToString());
            var statements = q.Body.GetProperty("facts").EnumerateArray()
                .Select(f => f.GetProperty("statement").GetString()).ToList();
            CollectionAssert.Contains(statements, "gestito: il fatto viene servito da un Fuseki avviato dal Service");
        }

        [TestMethod]
        public async Task Fail_loud_when_the_addon_is_not_installed()
        {
            using var ctx = new AgentCityContext();
            // NIENTE SetupManagedLayout: l'addon non è installato.
            var (projectId, path) = ctx.SeedProject("mem-managed-noaddon");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.EnableManagedFuseki(projectId, "mde_noaddon");

            var token = ctx.MintRunToken("worker", path, Guid.NewGuid().ToString());
            var a = await PostToken(ctx, "/api/A2A/memory/assert", token, new { statement = "x", about = "y" });
            Assert.AreEqual(HttpStatusCode.Conflict, a.Status);
            StringAssert.Contains(a.Body.GetProperty("error").GetString(), "Addon Fuseki non installato");
        }
    }
}
