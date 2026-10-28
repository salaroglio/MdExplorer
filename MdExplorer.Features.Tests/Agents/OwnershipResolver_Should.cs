using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class OwnershipResolver_Should
    {
        private static readonly List<OwnershipEntry> Entries = new()
        {
            new OwnershipEntry { Role = "WSAA-TOT", GitEmail = "carlo@x.it", Agents = new[] { "analyst", "dev" } },
            new OwnershipEntry { Role = "Batch", GitEmail = "marco@y.it", Agents = new[] { "java-dev" } },
            new OwnershipEntry { Role = "Batch", GitEmail = "anna@y.it", Agents = new[] { "java-dev" } },
        };

        [TestMethod]
        public void Resolve_a_role_case_insensitively()
        {
            var rows = OwnershipResolver.Resolve(Entries, "wsaa-tot");
            Assert.AreEqual("carlo@x.it", rows.Single().GitEmail);
        }

        [TestMethod]
        public void Return_every_person_of_a_team_role()
        {
            CollectionAssert.AreEqual(new[] { "marco@y.it", "anna@y.it" },
                OwnershipResolver.Resolve(Entries, "Batch").Select(e => e.GitEmail).ToList(), "who asks chooses: the resolver does not guess");
        }

        [TestMethod]
        public void Return_nothing_for_an_unknown_role()
        {
            Assert.AreEqual(0, OwnershipResolver.Resolve(Entries, "sconosciuto").Count);
            Assert.AreEqual(0, OwnershipResolver.Resolve(Entries, "").Count);
            Assert.AreEqual(0, OwnershipResolver.Resolve(null, "WSAA-TOT").Count);
        }

        [TestMethod]
        public void Pick_the_preferred_agent_when_listed_else_the_first()
        {
            var e = OwnershipResolver.Resolve(Entries, "WSAA-TOT").Single();
            Assert.AreEqual("dev", OwnershipResolver.PickAgent(e, "dev"));
            Assert.AreEqual("analyst", OwnershipResolver.PickAgent(e, "non-esiste"), "preferito assente → primo");
            Assert.AreEqual("analyst", OwnershipResolver.PickAgent(e), "nessun preferito → primo");
        }

        [TestMethod]
        public void Return_null_agent_when_the_entry_has_none()
        {
            var e = new OwnershipEntry { Role = "X", GitEmail = "a@b.it", Agents = new string[0] };
            Assert.IsNull(OwnershipResolver.PickAgent(e));
        }
    }
}
