using System.Collections.Generic;
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
    }
}
