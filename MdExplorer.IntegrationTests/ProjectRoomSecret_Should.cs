using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il segreto del PROGETTO apre la sua stanza sul relay, per la città e per la chat (16/10/2026). Un
    /// progetto senza città lo riceve alla prima apertura della chat: la città resta spenta, il resto del
    /// file resta com'era, e una volta creato non cambia più (cambiarlo chiuderebbe fuori i colleghi).
    /// </summary>
    [TestClass]
    public class ProjectRoomSecret_Should
    {
        [TestMethod]
        public void Create_the_secret_without_turning_the_city_on_and_keep_it_afterwards()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("segreto-chat");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            var file = System.IO.Path.Combine(path, ".development.yml");
            System.IO.File.WriteAllText(file, "harness:\n  target: copilot\n");

            var (secret, created) = meta.EnsureRoomSecret(path);

            Assert.IsTrue(created);
            Assert.IsFalse(string.IsNullOrWhiteSpace(secret));
            var city = meta.GetAgentCity(path);
            Assert.IsFalse(city.Enabled, "il segreto serve alla chat: la città non si accende da sola");
            Assert.AreEqual(secret, city.RoomSecret);
            StringAssert.Contains(System.IO.File.ReadAllText(file), "target: copilot", "le altre sezioni restano");

            var (again, createdAgain) = meta.EnsureRoomSecret(path);
            Assert.IsFalse(createdAgain);
            Assert.AreEqual(secret, again, "un segreto che c'è già non si rigenera");
        }

        [TestMethod]
        public void Reuse_the_secret_of_an_active_city()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("segreto-citta");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            System.IO.File.WriteAllText(System.IO.Path.Combine(path, ".development.yml"),
                "agentCity:\n  enabled: true\n  roomSecret: segreto-della-citta\n");

            var (secret, created) = meta.EnsureRoomSecret(path);

            Assert.IsFalse(created);
            Assert.AreEqual("segreto-della-citta", secret, "città e chat usano la stessa stanza e lo stesso segreto");
        }
    }
}
