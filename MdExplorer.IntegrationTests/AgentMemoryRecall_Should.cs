using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 5c — recupero al risveglio: quando l'agente viene svegliato da un messaggio, il
    /// dispatcher interroga la sua memoria filtrando per i topic del messaggio e inietta i fatti
    /// nel prompt (sezione "# Memoria rilevante"). Chiude il ciclo "vive perché ricorda".
    /// <para>Richiede Fuseki su <c>http://localhost:3030</c>: assente → <c>Inconclusive</c>.</para>
    /// </summary>
    [TestClass]
    public class AgentMemoryRecall_Should
    {
        private const string Fuseki = "http://localhost:3030";

        private static async Task<bool> FusekiUp()
        {
            try
            {
                using var h = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                return (await h.GetAsync(Fuseki + "/$/ping")).IsSuccessStatusCode;
            }
            catch { return false; }
        }

        [TestMethod]
        public async Task Inject_the_agents_own_memory_filtered_by_message_topics()
        {
            if (!await FusekiUp())
            {
                Assert.Inconclusive("Fuseki non raggiungibile su " + Fuseki + " — test saltato.");
                return;
            }

            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("mem-recall");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");

            var dataset = "mde_it_recall_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.EnableFuseki(projectKey, Fuseki, dataset);
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            try
            {
                // worker impara due fatti: uno su 'pagamenti', uno su 'logging'.
                var token = ctx.MintRunToken("worker", path, Guid.NewGuid().ToString());
                await Assert_(ctx, token, "il batch pagamenti gira alle 02:00 UTC", "pagamenti");
                await Assert_(ctx, token, "i log finiscono in /var/log/app", "logging");

                // Arriva un messaggio con topic 'pagamenti': il wake deve iniettare SOLO il fatto pertinente.
                var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker",
                    "controlla lo scheduling del batch", topics: new[] { "pagamenti" });
                Assert.IsFalse(rpc.IsError, $"errore inatteso: {rpc.ErrorCode} {rpc.ErrorMessage}");

                await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

                var prompt = ctx.Runner.LastRequest?.ComposedPrompt;
                Assert.IsNotNull(prompt, "il runner deve aver ricevuto un prompt");
                StringAssert.Contains(prompt, "# Memoria rilevante");
                StringAssert.Contains(prompt, "il batch pagamenti gira alle 02:00 UTC");
                Assert.IsFalse(prompt.Contains("i log finiscono in /var/log/app"),
                    "un fatto su un topic estraneo NON deve essere iniettato");
            }
            finally
            {
                try { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); } catch { }
            }
        }

        [TestMethod]
        public async Task Wake_without_memory_when_fuseki_is_disabled()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("mem-recall-off");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            // Fuseki NON abilitato: il risveglio procede senza sezione memoria (caso normale).
            await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti", topics: new[] { "pagamenti" });
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            var prompt = ctx.Runner.LastRequest?.ComposedPrompt;
            Assert.IsNotNull(prompt);
            Assert.IsFalse(prompt.Contains("# Memoria rilevante"), "memoria spenta → niente sezione");
        }

        private static async Task Assert_(AgentCityContext ctx, string runToken, string statement, string about)
        {
            var payload = JsonSerializer.Serialize(new { statement, about, confidence = 0.9 });
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/memory/assert")
            { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
            req.Headers.Add("X-MDE-Run-Token", runToken);
            var resp = await ctx.Client.SendAsync(req);
            Assert.IsTrue(resp.IsSuccessStatusCode, $"assert '{about}' fallita: {(int)resp.StatusCode} {await resp.Content.ReadAsStringAsync()}");
        }
    }
}
