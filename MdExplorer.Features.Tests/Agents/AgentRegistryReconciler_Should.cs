using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentRegistryReconciler_Should
    {
        private readonly AgentRegistryReconciler _reconciler = new AgentRegistryReconciler();

        private static DiscoveredAgentCard Llm(string name, string file, string parseError = null, string hash = null)
            => new DiscoveredAgentCard
            {
                Name = name,
                Kind = AgentIdentity.KindEnum.Llm,
                AgentFilePath = file,
                Role = "role of " + name,
                ParseError = parseError,
                CurrentA2ABlockHash = hash,
            };

        private static DiscoveredAgentCard Algo(string name)
            => new DiscoveredAgentCard { Name = name, Kind = AgentIdentity.KindEnum.Algorithmic };

        [TestMethod]
        public void Include_a_valid_citizen_with_no_error()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md") },
                Enumerable.Empty<ExistingIdentity>());

            var e = catalog.Single();
            Assert.IsTrue(e.IsCitizen);
            Assert.IsNull(e.RegistrationError);
            Assert.AreEqual("stem-curator", e.Name);
            Assert.AreEqual(AgentIdentity.KindEnum.Llm, e.Kind);
            Assert.IsFalse(e.Trusted, "senza identità persistita, il trust parte da false");
            Assert.IsNull(e.IdentityId);
        }

        [TestMethod]
        public void Exclude_both_agents_that_share_a_name_deterministically()
        {
            var catalog = _reconciler.Reconcile(
                new[]
                {
                    Llm("stem-curator", "/p/a.agent.md"),
                    Llm("stem-curator", "/p/sub/b.agent.md"),
                    Llm("reindexer", "/p/c.agent.md"),
                },
                Enumerable.Empty<ExistingIdentity>());

            var dupes = catalog.Where(e => e.Name == "stem-curator").ToList();
            Assert.AreEqual(2, dupes.Count);
            Assert.IsTrue(dupes.All(e => e.IsExcluded), "entrambi esclusi");
            Assert.IsTrue(dupes.All(e => e.RegistrationError.Contains("duplicat")));

            var ok = catalog.Single(e => e.Name == "reindexer");
            Assert.IsTrue(ok.IsCitizen);
        }

        [TestMethod]
        public void Treat_name_duplication_case_insensitively_across_sources()
        {
            // Un file .agent.md e un agente algoritmico con lo stesso nome (diverso case).
            var catalog = _reconciler.Reconcile(
                new[] { Llm("Reindexer", "/p/a.agent.md"), Algo("reindexer") },
                Enumerable.Empty<ExistingIdentity>());

            Assert.AreEqual(2, catalog.Count);
            Assert.IsTrue(catalog.All(e => e.IsExcluded));
            Assert.IsTrue(catalog.All(e => e.RegistrationError.Contains("duplicat")));
        }

        [TestMethod]
        public void Keep_a_parse_error_on_a_non_duplicated_agent()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm("bad-agent", "/p/a.agent.md", parseError: "Blocco 'a2a:' malformato: x") },
                Enumerable.Empty<ExistingIdentity>());

            var e = catalog.Single();
            Assert.IsTrue(e.IsExcluded);
            StringAssert.Contains(e.RegistrationError, "malformato");
        }

        [TestMethod]
        public void Let_duplicate_rule_win_over_an_individual_parse_error()
        {
            var catalog = _reconciler.Reconcile(
                new[]
                {
                    Llm("dup", "/p/a.agent.md", parseError: "qualche errore locale"),
                    Llm("dup", "/p/b.agent.md"),
                },
                Enumerable.Empty<ExistingIdentity>());

            Assert.IsTrue(catalog.All(e => e.RegistrationError.Contains("duplicat")));
        }

        [TestMethod]
        public void Exclude_a_nameless_discovery_with_its_parse_error()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm(null, "/p/broken.agent.md", parseError: "Blocco 'a2a:' presente ma vuoto") },
                Enumerable.Empty<ExistingIdentity>());

            var e = catalog.Single();
            Assert.IsTrue(e.IsExcluded);
            StringAssert.Contains(e.RegistrationError, "vuoto");
        }

        [TestMethod]
        public void Merge_persisted_trust_onto_a_valid_citizen_by_name()
        {
            var id = Guid.NewGuid();
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md") },
                new[]
                {
                    new ExistingIdentity
                    {
                        Id = id, Name = "stem-curator", Trusted = true, Enabled = true,
                        A2ABlockHash = "abc123",
                    }
                });

            var e = catalog.Single();
            Assert.IsTrue(e.IsCitizen);
            Assert.IsTrue(e.Trusted);
            Assert.IsTrue(e.Enabled);
            Assert.AreEqual("abc123", e.A2ABlockHash);
            Assert.AreEqual(id, e.IdentityId);
        }

        [TestMethod]
        public void Match_persisted_identity_case_insensitively()
        {
            var id = Guid.NewGuid();
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md") },
                new[] { new ExistingIdentity { Id = id, Name = "Stem-Curator", Trusted = true } });

            Assert.AreEqual(id, catalog.Single().IdentityId);
        }

        [TestMethod]
        public void Return_a_stable_alphabetical_order()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm("zeta", "/p/z.agent.md"), Llm("alpha", "/p/a.agent.md"), Algo("mid") },
                Enumerable.Empty<ExistingIdentity>());

            CollectionAssert.AreEqual(
                new[] { "alpha", "mid", "zeta" },
                catalog.Select(e => e.Name).ToArray());
        }

        [TestMethod]
        public void Handle_empty_sources()
        {
            var catalog = _reconciler.Reconcile(
                Enumerable.Empty<DiscoveredAgentCard>(),
                Enumerable.Empty<ExistingIdentity>());

            Assert.AreEqual(0, catalog.Count);
        }

        // ---- decadenza automatica del trust al cambio del blocco a2a:/tools: (R3) ----

        [TestMethod]
        public void Decay_trust_when_the_a2a_block_hash_changed()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md", hash: "NEW-hash") },
                new[]
                {
                    new ExistingIdentity
                    {
                        Id = Guid.NewGuid(), Name = "stem-curator",
                        Trusted = true, Enabled = true, A2ABlockHash = "OLD-hash",
                    }
                });

            var e = catalog.Single();
            Assert.IsTrue(e.TrustDecayed, "il blocco è cambiato → decadenza");
            Assert.IsFalse(e.Trusted);
            Assert.IsFalse(e.Enabled);
            Assert.AreEqual("NEW-hash", e.CurrentA2ABlockHash);
        }

        [TestMethod]
        public void Keep_trust_when_the_hash_is_unchanged()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md", hash: "same-hash") },
                new[]
                {
                    new ExistingIdentity
                    {
                        Id = Guid.NewGuid(), Name = "stem-curator",
                        Trusted = true, Enabled = true, A2ABlockHash = "same-hash",
                    }
                });

            var e = catalog.Single();
            Assert.IsFalse(e.TrustDecayed);
            Assert.IsTrue(e.Trusted);
            Assert.IsTrue(e.Enabled);
        }

        [TestMethod]
        public void Not_decay_an_agent_that_was_never_trusted()
        {
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md", hash: "new") },
                new[]
                {
                    new ExistingIdentity
                    {
                        Id = Guid.NewGuid(), Name = "stem-curator",
                        Trusted = false, Enabled = false, A2ABlockHash = "old",
                    }
                });

            Assert.IsFalse(catalog.Single().TrustDecayed);
        }

        [TestMethod]
        public void Not_decay_when_there_is_no_stored_hash_yet()
        {
            // Trust confermato prima che l'hash fosse in uso, oppure agente appena arruolato.
            var catalog = _reconciler.Reconcile(
                new[] { Llm("stem-curator", "/p/a.agent.md", hash: "current") },
                new[]
                {
                    new ExistingIdentity
                    {
                        Id = Guid.NewGuid(), Name = "stem-curator",
                        Trusted = true, Enabled = true, A2ABlockHash = null,
                    }
                });

            var e = catalog.Single();
            Assert.IsFalse(e.TrustDecayed);
            Assert.IsTrue(e.Trusted);
        }
    }
}
