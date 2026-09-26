using System.Collections.Generic;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class OwnershipResolver_Should
    {
        private static readonly List<OwnershipEntry> Entries = new()
        {
            new OwnershipEntry { Scope = "WSAA-TOT", GitEmail = "carlo@x.it", Agents = new[] { "analyst", "dev" } },
            new OwnershipEntry { Scope = "Batch", GitEmail = "marco@y.it", Agents = new[] { "java-dev" } },
        };

        [TestMethod]
        public void Resolve_a_scope_case_insensitively()
        {
            var e = OwnershipResolver.Resolve(Entries, "wsaa-tot");
            Assert.IsNotNull(e);
            Assert.AreEqual("carlo@x.it", e.GitEmail);
        }

        [TestMethod]
        public void Return_null_for_an_unknown_scope()
        {
            Assert.IsNull(OwnershipResolver.Resolve(Entries, "sconosciuto"));
            Assert.IsNull(OwnershipResolver.Resolve(Entries, ""));
            Assert.IsNull(OwnershipResolver.Resolve(null, "WSAA-TOT"));
        }

        [TestMethod]
        public void Pick_the_preferred_agent_when_listed_else_the_first()
        {
            var e = OwnershipResolver.Resolve(Entries, "WSAA-TOT");
            Assert.AreEqual("dev", OwnershipResolver.PickAgent(e, "dev"));
            Assert.AreEqual("analyst", OwnershipResolver.PickAgent(e, "non-esiste"), "preferito assente → primo");
            Assert.AreEqual("analyst", OwnershipResolver.PickAgent(e), "nessun preferito → primo");
        }

        [TestMethod]
        public void Return_null_agent_when_the_entry_has_none()
        {
            var e = new OwnershipEntry { Scope = "X", GitEmail = "a@b.it", Agents = new string[0] };
            Assert.IsNull(OwnershipResolver.PickAgent(e));
        }
    }
}
