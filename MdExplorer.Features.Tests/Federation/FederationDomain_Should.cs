using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Federation
{
    [TestClass]
    public class FederationDomain_Should
    {
        private const string Secret = "room-secret-condiviso";

        [TestMethod]
        public void Compute_the_same_room_for_ssh_and_https_of_the_same_repo()
        {
            var https = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", Secret);
            var ssh = FederationRoom.ComputeRoomId("git@github.com:acme/repo.git", Secret);
            Assert.AreEqual(https, ssh, "SSH e HTTPS dello stesso repo → stessa stanza");
        }

        [TestMethod]
        public void Compute_a_room_id_in_the_form_the_relay_self_registers()
        {
            // Il relay registra senza chiave master solo /^r2-[0-9a-f]{32}$/ (rooms.js).
            var roomId = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", Secret);
            StringAssert.Matches(roomId, new System.Text.RegularExpressions.Regex("^r2-[0-9a-f]{32}$"));
        }

        [TestMethod]
        public void Compute_different_rooms_for_different_secrets_of_the_same_repo()
        {
            // Senza il segreto la stanza non si indovina: è ciò che rende sicura la registrazione libera.
            var a = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", Secret);
            var b = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", "un-altro-segreto");
            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Compute_different_rooms_for_a_fork_with_the_same_secret()
        {
            var original = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", Secret);
            var fork = FederationRoom.ComputeRoomId("https://github.com/someone/repo.git", Secret);
            Assert.AreNotEqual(original, fork);
        }

        [TestMethod]
        public void Refuse_to_compute_a_room_without_the_secret()
        {
            Assert.ThrowsException<System.ArgumentException>(
                () => FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", " "));
        }

        [TestMethod]
        public void Normalize_strips_protocol_credentials_suffix_and_slashes()
        {
            Assert.AreEqual("github.com/acme/repo",
                FederationRoom.NormalizeGitOrigin("https://user:pass@github.com/acme/repo.git/"));
            Assert.AreEqual("github.com/acme/repo",
                FederationRoom.NormalizeGitOrigin("git@github.com:acme/repo"));
        }

        [TestMethod]
        public void Normalize_collapses_dotgit_with_and_without_trailing_slash()
        {
            // Il bug che la review ha segnalato: 'repo.git' e 'repo.git/' devono dare la STESSA
            // stanza (prima chat e federazione divergevano su questo edge).
            var a = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git", Secret);
            var b = FederationRoom.ComputeRoomId("https://github.com/acme/repo.git/", Secret);
            var c = FederationRoom.ComputeRoomId("https://github.com/acme/repo", Secret);
            Assert.AreEqual(a, b, "repo.git/ e repo.git → stessa stanza");
            Assert.AreEqual(a, c, "…e uguale a repo senza suffisso");
        }

        [TestMethod]
        public void Compute_a_stable_case_insensitive_owner_id()
        {
            var a = FederationRoom.ComputeUserId("Carlo@X.IT");
            var b = FederationRoom.ComputeUserId("  carlo@x.it ");
            Assert.AreEqual(a, b);
            Assert.AreEqual(12, a.Length);
        }

        [TestMethod]
        public void Build_presence_maps_agents_and_lowercases_email()
        {
            var agents = new List<AgentRosterEntry>
            {
                new AgentRosterEntry { Name = "analyst", Role = "Analista", Skills = new List<string> { "workflow" } },
                new AgentRosterEntry { Name = "", Role = "senza nome" },   // scartato
            };

            var p = FederationPresenceBuilder.Build("room123", "Carlo@X.IT", agents);

            Assert.AreEqual("carlo@x.it", p.GitEmail);
            Assert.AreEqual(FederationRoom.ComputeUserId("carlo@x.it"), p.OwnerId);
            Assert.AreEqual(1, p.Agents.Count, "l'agente senza nome è scartato");
            Assert.AreEqual("analyst", p.Agents[0].Name);
            CollectionAssert.AreEqual(new[] { "workflow" }, p.Agents[0].Skills.ToArray());
        }

        [TestMethod]
        public void Fail_loud_building_presence_without_room_or_email()
        {
            Assert.ThrowsException<System.ArgumentException>(
                () => FederationPresenceBuilder.Build("", "carlo@x.it", null));
            Assert.ThrowsException<System.ArgumentException>(
                () => FederationPresenceBuilder.Build("room", "", null));
        }

        [TestMethod]
        public void Another_city_with_the_same_secret_opens_the_presence_envelope()
        {
            // Città A annuncia; il relay custodisce la busta chiusa; città B (stesso secret via
            // git, stessa stanza) la apre e ricostruisce la presenza. Prova presence + crypto insieme.
            const string secret = Secret;
            var roomId = FederationRoom.ComputeRoomId("git@github.com:acme/repo.git", secret);

            var presence = FederationPresenceBuilder.Build(roomId, "carlo@x.it",
                new List<AgentRosterEntry> { new AgentRosterEntry { Name = "analyst", Role = "Analista" } });
            var json = JsonSerializer.Serialize(presence);

            var envelope = FederationCrypto.Encrypt(secret, roomId, json);            // città A
            var opened = FederationCrypto.Decrypt(secret, roomId, envelope);           // città B
            var restored = JsonSerializer.Deserialize<CityPresence>(opened);

            Assert.AreEqual("carlo@x.it", restored.GitEmail);
            Assert.AreEqual("analyst", restored.Agents.Single().Name);
        }
    }
}
