using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6a — attivazione della città in <c>.development.yml</c>. Il punto critico
    /// (trappola documentata nel piano): la sezione <c>agentCity</c> deve stare nel modello
    /// tipizzato, altrimenti una write di participants — che passa dallo stesso round-trip —
    /// la <b>cancellerebbe</b>. Qui lo verifichiamo end-to-end col servizio reale.
    /// </summary>
    [TestClass]
    public class AgentCityActivation_Should
    {
        private static IProjectMetadataService Meta(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();

        [TestMethod]
        public void Generate_a_room_secret_on_first_activation()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("activate");
            var meta = Meta(ctx);

            var saved = meta.SetAgentCity(path, new AgentCityConfig { Enabled = true });

            Assert.IsTrue(saved.Enabled);
            Assert.IsFalse(string.IsNullOrWhiteSpace(saved.RoomSecret), "il room secret è generato alla prima attivazione");

            // Ri-attivare NON ruota il secret (è una credenziale condivisa via git).
            var again = meta.SetAgentCity(path, new AgentCityConfig { Enabled = true });
            Assert.AreEqual(saved.RoomSecret, again.RoomSecret);
        }

        [TestMethod]
        public void Survive_a_participants_write()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("survive");
            var meta = Meta(ctx);

            var saved = meta.SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "docs/ownership.md" });
            var secret = saved.RoomSecret;

            // Una write di participants passa dallo stesso round-trip tipizzato: se agentCity
            // non fosse nel modello, verrebbe cancellata qui.
            meta.SetParticipants(path, new List<ProjectParticipant>
            {
                new ProjectParticipant { GitEmail = "carlo@x.it", DisplayName = "Carlo" },
            });

            var after = meta.GetAgentCity(path);
            Assert.IsNotNull(after, "agentCity NON deve essere cancellata da una write di participants");
            Assert.IsTrue(after.Enabled);
            Assert.AreEqual("docs/ownership.md", after.OwnershipDoc);
            Assert.AreEqual(secret, after.RoomSecret);
        }

        [TestMethod]
        public void Report_null_when_no_config_present()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("empty");
            Assert.IsNull(Meta(ctx).GetAgentCity(path), "assente → null (federazione spenta, retrocompat)");
        }

        [TestMethod]
        public async Task Inject_the_ownership_table_into_the_wake_prompt_when_active()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("ownership-live");

            // Federazione attiva + doc di ownership valido su disco.
            Meta(ctx).SetAgentCity(path, new AgentCityConfig { Enabled = true, OwnershipDoc = "ownership.md" });
            File.WriteAllText(Path.Combine(path, "ownership.md"), @"---
mde_type: ownership
---
| Ambito | Responsabile | Git Email | Agenti |
|--------|--------------|-----------|--------|
| WSAA-TOT | Carlo | carlo@x.it | worker |
");

            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");
            Assert.IsFalse(rpc.IsError, $"errore inatteso: {rpc.ErrorCode} {rpc.ErrorMessage}");

            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            var prompt = ctx.Runner.LastRequest?.ComposedPrompt;
            Assert.IsNotNull(prompt, "il runner deve aver ricevuto un prompt");
            StringAssert.Contains(prompt, "# Ownership del progetto");
            StringAssert.Contains(prompt, "WSAA-TOT");
        }

        [TestMethod]
        public async Task Not_inject_ownership_when_the_city_is_off()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("ownership-off");

            // Doc presente ma federazione NON attivata → nessuna iniezione.
            File.WriteAllText(Path.Combine(path, "ownership.md"), "---\nmde_type: ownership\n---\n| Ambito | Git Email |\n|--|--|\n| S | c@x.it |\n");

            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");
            await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "worker" && x.State == AgentMessage.StateEnum.Processed));

            var prompt = ctx.Runner.LastRequest?.ComposedPrompt;
            Assert.IsNotNull(prompt);
            Assert.IsFalse(prompt.Contains("# Ownership del progetto"), "città spenta → niente ownership nel prompt");
        }
    }
}
