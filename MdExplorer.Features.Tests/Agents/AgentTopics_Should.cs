using System.Collections.Generic;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentTopics_Should
    {
        [TestMethod]
        public void Join_trims_drops_blanks_and_dedups_preserving_order()
        {
            var joined = AgentTopics.Join(new[] { " deploy ", "", "urgent", "Deploy", "  " });
            Assert.AreEqual("deploy\nurgent", joined);
        }

        [TestMethod]
        public void Join_returns_null_when_there_is_nothing_usable()
        {
            Assert.IsNull(AgentTopics.Join(null));
            Assert.IsNull(AgentTopics.Join(new List<string>()));
            Assert.IsNull(AgentTopics.Join(new[] { "   ", "" }));
        }

        [TestMethod]
        public void Split_is_the_inverse_of_join()
        {
            var joined = AgentTopics.Join(new[] { "deploy", "urgent" });
            CollectionAssert.AreEqual(new[] { "deploy", "urgent" }, AgentTopics.Split(joined));
        }

        [TestMethod]
        public void Split_of_empty_is_an_empty_list()
        {
            Assert.AreEqual(0, AgentTopics.Split(null).Count);
            Assert.AreEqual(0, AgentTopics.Split("   ").Count);
        }
    }
}
