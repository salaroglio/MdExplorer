using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.AgentMemory;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentMemory;
using MdExplorer.Services.AgentRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 7f — consolidamento. Un gesto umano promuove i fatti scelti nel <c>.agent.md</c> (e li
    /// rimuove da Fuseki) e decade il resto: confidence × fattore, cancellazione sotto il pavimento,
    /// esclusione dei fatti a confidence alta (proxy confirmedBy). <para>Richiede Fuseki :3030.</para>
    /// </summary>
    [TestClass]
    public class MemoryConsolidation_Should
    {
        private const string Fuseki = "http://localhost:3030";

        private static async Task<bool> FusekiUp()
        {
            try { using var h = new HttpClient { Timeout = TimeSpan.FromSeconds(3) }; return (await h.GetAsync(Fuseki + "/$/ping")).IsSuccessStatusCode; }
            catch { return false; }
        }

        private static async Task<string> Assert_(AgentCityContext ctx, string path, Guid agentId, string statement, double confidence)
        {
            var resolver = ctx.Factory.Services.GetRequiredService<IFusekiConnectionResolver>();
            var memory = ctx.Factory.Services.GetRequiredService<IAgentMemoryService>();
            var conn = await resolver.ResolveAsync(path);
            return await memory.AssertFactAsync(conn, agentId, new LearnedFactInput
            {
                Statement = statement, Confidence = confidence, RunId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow,
            });
        }

        private static async Task<IReadOnlyList<MemoryFactDetail>> Facts(AgentCityContext ctx, string path, Guid agentId)
        {
            var resolver = ctx.Factory.Services.GetRequiredService<IFusekiConnectionResolver>();
            var memory = ctx.Factory.Services.GetRequiredService<IAgentMemoryService>();
            var conn = await resolver.ResolveAsync(path);
            return await memory.ListAsync(conn, new[] { AgentMemoryGraphs.ForAgent(agentId) }, 100);
        }

        [TestMethod]
        public async Task Promote_chosen_facts_and_decay_the_rest()
        {
            if (!await FusekiUp()) { Assert.Inconclusive("Fuseki non raggiungibile."); return; }
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("consolidate");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            var dataset = "mde_it_7f_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            ctx.EnableFuseki(projectKey, Fuseki, dataset);

            try
            {
                var registry = ctx.Factory.Services.GetRequiredService<IAgentRegistryService>();
                var workerId = registry.RefreshCatalog(path).First(e => e.Name == "worker").IdentityId.Value;
                var graph = AgentMemoryGraphs.ForAgent(workerId);

                var uriPromote = await Assert_(ctx, path, workerId, "il batch pagamenti gira alle 02:00 UTC", 0.7);
                await Assert_(ctx, path, workerId, "fatto da far decadere", 0.6);   // 0.6*0.5 = 0.3 ≥ pavimento
                await Assert_(ctx, path, workerId, "fatto da cancellare", 0.2);      // 0.2*0.5 = 0.1 < pavimento
                await Assert_(ctx, path, workerId, "preferenza umana confermata", 0.95); // ≥ soglia alta → intatto

                // Conversazione con un messaggio che coinvolge 'worker' (user escluso dai partecipanti).
                var convId = ctx.SeedConversation(path);
                using (var scope = ctx.Factory.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    db.BeginTransaction();
                    db.GetDal<AgentMessage>().Save(new AgentMessage
                    {
                        ConversationId = convId, FromAgent = "user", ToAgent = "worker", ProjectPath = path,
                        State = AgentMessage.StateEnum.Processed, Attempts = 0, CreatedAt = DateTime.UtcNow,
                    });
                    db.Commit();
                }

                var body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    projectPath = path,
                    promote = new[] { new { factUri = uriPromote, graph, statement = "il batch pagamenti gira alle 02:00 UTC" } }
                });
                var resp = await ctx.Client.PostAsync($"/api/mem/conversations/{convId}/consolidate",
                    new StringContent(body, Encoding.UTF8, "application/json"));
                var respBody = await resp.Content.ReadAsStringAsync();
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, respBody);
                StringAssert.Contains(respBody, "\"promoted\":1");
                StringAssert.Contains(respBody, "\"decayed\":1");
                StringAssert.Contains(respBody, "\"deleted\":1");

                // Stato Fuseki: promosso e cancellato spariti; decaduto abbassato; confermato intatto.
                var after = await Facts(ctx, path, workerId);
                Assert.IsFalse(after.Any(f => f.Statement.Contains("02:00 UTC")), "il fatto promosso è rimosso da Fuseki");
                Assert.IsFalse(after.Any(f => f.Statement.Contains("da cancellare")), "il fatto sotto il pavimento è cancellato");
                var decayed = after.FirstOrDefault(f => f.Statement.Contains("far decadere"));
                Assert.IsNotNull(decayed);
                Assert.AreEqual(0.30, decayed.Confidence, 0.001, "decaduto = old × 0.5");
                var confirmed = after.FirstOrDefault(f => f.Statement.Contains("confermata"));
                Assert.IsNotNull(confirmed);
                Assert.AreEqual(0.95, confirmed.Confidence, 0.001, "confidence alta esclusa dal decadimento");

                // Diploma nel .agent.md: sezione consolidata col fatto promosso.
                var md = File.ReadAllText(Path.Combine(path, "worker.agent.md"));
                StringAssert.Contains(md, "## Memoria consolidata");
                StringAssert.Contains(md, "il batch pagamenti gira alle 02:00 UTC");
            }
            finally
            {
                try { using var h = new HttpClient(); await h.DeleteAsync($"{Fuseki}/$/datasets/{dataset}"); } catch { }
            }
        }
    }
}
