using System.Collections.Generic;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentTrustHasher_Should
    {
        private static AgentCardDescriptor Card(string name = "stem-curator", string role = "Curatore")
            => new AgentCardDescriptor
            {
                Name = name,
                Role = role,
                Skills = new List<AgentCardSkill> { new AgentCardSkill { Id = "s1", Description = "d1" } },
                AcceptsMessagesFrom = new List<string> { "cobol-pipeline", "user" },
                MaxHops = 12,
            };

        [TestMethod]
        public void Be_stable_for_identical_content()
        {
            var h1 = AgentTrustHasher.ComputeHash(Card(), new[] { "read", "write" });
            var h2 = AgentTrustHasher.ComputeHash(Card(), new[] { "read", "write" });
            Assert.AreEqual(h1, h2);
            Assert.AreEqual(64, h1.Length); // SHA-256 hex
        }

        [TestMethod]
        public void Change_when_the_a2a_block_changes()
        {
            var baseline = AgentTrustHasher.ComputeHash(Card(role: "Curatore"), new[] { "read" });
            var changed = AgentTrustHasher.ComputeHash(Card(role: "Altro ruolo"), new[] { "read" });
            Assert.AreNotEqual(baseline, changed);
        }

        [TestMethod]
        public void Change_when_the_tools_field_changes()
        {
            var readOnly = AgentTrustHasher.ComputeHash(Card(), new[] { "read" });
            var withWrite = AgentTrustHasher.ComputeHash(Card(), new[] { "read", "write" });
            Assert.AreNotEqual(readOnly, withWrite,
                "aggiungere 'write' deve far decadere il trust (R3, anti-escalation)");
        }

        [TestMethod]
        public void Treat_null_and_empty_tools_consistently()
        {
            var nullTools = AgentTrustHasher.ComputeHash(Card(), null);
            var emptyTools = AgentTrustHasher.ComputeHash(Card(), new string[0]);
            Assert.AreEqual(nullTools, emptyTools);
        }

        [TestMethod]
        public void Distinguish_agents_by_accepts_messages_from()
        {
            var a = Card();
            a.AcceptsMessagesFrom = new List<string> { "user" };
            var b = Card();
            b.AcceptsMessagesFrom = new List<string> { "user", "cobol-pipeline" };
            Assert.AreNotEqual(
                AgentTrustHasher.ComputeHash(a, null),
                AgentTrustHasher.ComputeHash(b, null),
                "allargare accepts_messages_from deve far decadere il trust (R3)");
        }
    }
}
