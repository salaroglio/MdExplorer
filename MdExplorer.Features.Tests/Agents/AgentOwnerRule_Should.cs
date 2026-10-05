using System.Collections.Generic;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentOwnerRule_Should
    {
        private static OwnershipEntry Row(string scope, string responsible, string email, params string[] agents)
            => new OwnershipEntry { Scope = scope, Responsible = responsible, GitEmail = email, Agents = agents };

        private static readonly List<OwnershipEntry> Table = new()
        {
            Row("Commerciale", "Anna", "anna@pentagroup.it", "account-manager"),
            Row("Tecnica", "Marco", "marco@pentagroup.it", "responsabile-tecnico"),
        };

        [TestMethod]
        public void Let_my_agent_work_here()
        {
            var verdict = AgentOwnerRule.Decide(Table, "account-manager", "Anna@Pentagroup.it");

            Assert.AreEqual(AgentOwnerKind.Mine, verdict.Kind, "the email is compared ignoring case");
            Assert.IsTrue(verdict.CanWorkHere);
            Assert.IsNull(verdict.Explain());
        }

        [TestMethod]
        public void Keep_someone_elses_agent_off_this_computer_and_say_whose_it_is()
        {
            var verdict = AgentOwnerRule.Decide(Table, "responsabile-tecnico", "anna@pentagroup.it");

            Assert.AreEqual(AgentOwnerKind.SomeoneElse, verdict.Kind);
            Assert.IsFalse(verdict.CanWorkHere);
            StringAssert.Contains(verdict.Explain(), "Marco (marco@pentagroup.it)");
            StringAssert.Contains(verdict.Explain(), "sei anna@pentagroup.it");
        }

        [TestMethod]
        public void Not_let_an_agent_of_nobody_work()
        {
            foreach (var table in new[] { Table, new List<OwnershipEntry>(), null })
            {
                var verdict = AgentOwnerRule.Decide(table, "responsabile-legale", "anna@pentagroup.it");

                Assert.AreEqual(AgentOwnerKind.Unassigned, verdict.Kind);
                Assert.IsFalse(verdict.CanWorkHere);
                StringAssert.Contains(verdict.Explain(), "non ha un responsabile");
            }
        }

        [TestMethod]
        public void Not_call_mine_an_agent_when_this_computer_has_no_identity()
        {
            var verdict = AgentOwnerRule.Decide(Table, "account-manager", "");

            Assert.AreEqual(AgentOwnerKind.SomeoneElse, verdict.Kind);
            StringAssert.Contains(verdict.Explain(), "non ha un'email git");
        }

        [TestMethod]
        public void Treat_an_agent_given_to_two_people_as_nobodys()
        {
            var table = new List<OwnershipEntry>(Table) { Row("Gare", "Luca", "luca@pentagroup.it", "account-manager") };

            var verdict = AgentOwnerRule.Decide(table, "account-manager", "anna@pentagroup.it");

            Assert.AreEqual(AgentOwnerKind.Contested, verdict.Kind);
            Assert.IsFalse(verdict.CanWorkHere, "not even for one of the two");
            StringAssert.Contains(verdict.Explain(), "anna@pentagroup.it, luca@pentagroup.it");
        }

        [TestMethod]
        public void Accept_the_same_person_on_two_rows()
        {
            var table = new List<OwnershipEntry>(Table) { Row("Gare", "Anna", "ANNA@pentagroup.it", "account-manager") };

            Assert.AreEqual(AgentOwnerKind.Mine, AgentOwnerRule.Decide(table, "account-manager", "anna@pentagroup.it").Kind);
        }
    }
}
