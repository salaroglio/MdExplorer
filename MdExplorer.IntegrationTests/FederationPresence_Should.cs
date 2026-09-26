using System.Collections.Generic;
using System.Text.Json;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6b (locale) — assemblaggio dell'annuncio di presenza cifrato per un progetto, senza
    /// rete. Verifica che la città attiva produca una busta apribile col room secret (§12.5) e
    /// che la città spenta non produca nulla.
    /// </summary>
    [TestClass]
    public class FederationPresence_Should
    {
        private static readonly List<AgentRosterEntry> Trusted = new()
        {
            new AgentRosterEntry { Name = "analyst", Role = "Analista", Skills = new List<string> { "workflow" } },
        };

        [TestMethod]
        public void Build_an_encrypted_announce_for_an_active_city()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fed-active");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            var svc = ctx.Factory.Services.GetRequiredService<IFederationPresenceService>();

            var saved = meta.SetAgentCity(path, new AgentCityConfig { Enabled = true });

            var announce = svc.BuildAnnounce(path, "git@github.com:acme/repo.git", "carlo@x.it", Trusted);

            Assert.IsNotNull(announce, "città attiva → annuncio prodotto");
            Assert.AreEqual(FederationRoom.ComputeRoomId("git@github.com:acme/repo.git"), announce.RoomId);
            StringAssert.StartsWith(announce.EncryptedPresence, "mdfed.v1.");

            // La busta si apre col room secret condiviso e ricostruisce la presenza.
            var json = FederationCrypto.Decrypt(saved.RoomSecret, announce.RoomId, announce.EncryptedPresence);
            var presence = JsonSerializer.Deserialize<CityPresence>(json);
            Assert.AreEqual("carlo@x.it", presence.GitEmail);
            Assert.AreEqual("analyst", presence.Agents[0].Name);
        }

        [TestMethod]
        public void Produce_nothing_when_the_city_is_off()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fed-off");
            var svc = ctx.Factory.Services.GetRequiredService<IFederationPresenceService>();

            // Nessuna attivazione → null.
            Assert.IsNull(svc.BuildAnnounce(path, "git@github.com:acme/repo.git", "carlo@x.it", Trusted));
        }

        [TestMethod]
        public void Produce_nothing_without_a_git_origin()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("fed-noremote");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            var svc = ctx.Factory.Services.GetRequiredService<IFederationPresenceService>();

            meta.SetAgentCity(path, new AgentCityConfig { Enabled = true });

            // Città attiva ma senza remoto → nessuna stanza, nessun annuncio.
            Assert.IsNull(svc.BuildAnnounce(path, null, "carlo@x.it", Trusted));
        }
    }
}
