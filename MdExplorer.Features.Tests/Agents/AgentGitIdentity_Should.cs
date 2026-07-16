using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentGitIdentity_Should
    {
        [TestMethod]
        public void Build_the_agent_email_under_the_reserved_domain()
        {
            Assert.AreEqual("stem-curator@agents.mde", AgentGitIdentity.EmailFor("stem-curator"));
            Assert.AreEqual("stem-curator@agents.mde", AgentGitIdentity.EmailFor("  stem-curator  "));
        }

        [TestMethod]
        public void Sign_author_and_committer_as_the_agent()
        {
            var env = AgentGitIdentity.EnvFor("deploy-sentinel");

            Assert.AreEqual("deploy-sentinel", env[AgentGitIdentity.EnvAuthorName]);
            Assert.AreEqual("deploy-sentinel@agents.mde", env[AgentGitIdentity.EnvAuthorEmail]);
            Assert.AreEqual("deploy-sentinel", env[AgentGitIdentity.EnvCommitterName]);
            Assert.AreEqual("deploy-sentinel@agents.mde", env[AgentGitIdentity.EnvCommitterEmail]);
        }

        [TestMethod]
        public void Produce_nothing_without_a_name()
        {
            Assert.AreEqual(0, AgentGitIdentity.EnvFor(null).Count);
            Assert.AreEqual(0, AgentGitIdentity.EnvFor("   ").Count);
        }
    }
}
