using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentPathComparer_Should
    {
        [TestMethod]
        public void Match_the_same_path_ignoring_a_trailing_separator()
        {
            Assert.IsTrue(AgentPathComparer.Equals("/home/carlo/prj", "/home/carlo/prj/"));
            Assert.IsTrue(AgentPathComparer.Equals("/home/carlo/prj", "/home/carlo/prj"));
        }

        [TestMethod]
        public void Match_paths_that_normalize_to_the_same_target()
        {
            Assert.IsTrue(AgentPathComparer.Equals("/home/carlo/prj", "/home/carlo/x/../prj"));
        }

        [TestMethod]
        public void Distinguish_two_different_projects()
        {
            // Il cuore del fix #4: due progetti distinti non collidono, quindi un contextId
            // dell'uno non può agganciare la conversazione dell'altro.
            Assert.IsFalse(AgentPathComparer.Equals("/home/carlo/prj-a", "/home/carlo/prj-b"));
        }

        [TestMethod]
        public void Treat_null_or_empty_as_non_matching()
        {
            Assert.IsFalse(AgentPathComparer.Equals(null, "/p"));
            Assert.IsFalse(AgentPathComparer.Equals("/p", null));
            Assert.IsFalse(AgentPathComparer.Equals("", ""));
        }
    }
}
