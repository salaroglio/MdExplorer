using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class ConversationHopGuard_Should
    {
        [DataTestMethod]
        [DataRow(null, 8)]   // default
        [DataRow(12, 12)]    // override valido
        [DataRow(20, 16)]    // cap harness
        [DataRow(0, 1)]      // floor
        [DataRow(-5, 1)]     // floor su negativi
        public void Clamp_the_hop_limit(int? maxHops, int expected)
        {
            Assert.AreEqual(expected, ConversationHopGuard.ClampHopLimit(maxHops));
        }

        [TestMethod]
        public void Count_a_message_between_two_agents()
        {
            var d = ConversationHopGuard.Evaluate("cobol-pipeline", "stem-curator", currentHopCount: 3, hopLimit: 8);
            Assert.IsTrue(d.Allowed);
            Assert.IsFalse(d.Exempt);
            Assert.AreEqual(4, d.NewHopCount);
            Assert.IsFalse(d.Exhausted);
        }

        [DataTestMethod]
        [DataRow("user", "stem-curator")]   // da user
        [DataRow("stem-curator", "user")]   // verso user
        [DataRow("USER", "x")]              // case-insensitive
        public void Exempt_messages_to_or_from_user(string from, string to)
        {
            var d = ConversationHopGuard.Evaluate(from, to, currentHopCount: 7, hopLimit: 8);
            Assert.IsTrue(d.Allowed, "l'escalation non deve mai morire per budget");
            Assert.IsTrue(d.Exempt);
            Assert.AreEqual(7, d.NewHopCount, "un messaggio esente non consuma hop");
            Assert.IsFalse(d.Exhausted);
        }

        [TestMethod]
        public void Reject_and_exhaust_when_the_limit_is_reached()
        {
            var d = ConversationHopGuard.Evaluate("a", "b", currentHopCount: 8, hopLimit: 8);
            Assert.IsFalse(d.Allowed);
            Assert.IsTrue(d.Exhausted);
            Assert.AreEqual(8, d.NewHopCount);
        }

        [TestMethod]
        public void Count_fan_out_each_recipient_as_one_hop()
        {
            // A che scrive a 10 agenti = 10 hop: qui simuliamo 3 destinatari da hop 0.
            var hop = 0;
            for (var i = 0; i < 3; i++)
            {
                var d = ConversationHopGuard.Evaluate("a", "recipient-" + i, hop, hopLimit: 8);
                Assert.IsTrue(d.Allowed);
                hop = d.NewHopCount;
            }
            Assert.AreEqual(3, hop, "ogni destinatario del fan-out conta un hop");
        }

        [TestMethod]
        public void Let_the_human_reopen_after_exhaustion()
        {
            // Conversazione esaurita (hop 8/8): un messaggio agente→agente è rifiutato…
            Assert.IsFalse(ConversationHopGuard.Evaluate("a", "b", 8, 8).Allowed);
            // …ma user→agente resta sempre ammesso (riapertura umana).
            Assert.IsTrue(ConversationHopGuard.Evaluate("user", "b", 8, 8).Allowed);
        }
    }
}
