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
        public void Sign_the_agent_as_author_and_the_responsible_person_as_committer()
        {
            var env = AgentGitIdentity.EnvFor("deploy-sentinel", "Mario Rossi", "mario.rossi@example.com");

            Assert.AreEqual("deploy-sentinel", env[AgentGitIdentity.EnvAuthorName]);
            Assert.AreEqual("deploy-sentinel@agents.mde", env[AgentGitIdentity.EnvAuthorEmail]);
            Assert.AreEqual("Mario Rossi", env[AgentGitIdentity.EnvCommitterName]);
            Assert.AreEqual("mario.rossi@example.com", env[AgentGitIdentity.EnvCommitterEmail]);
            Assert.AreEqual("deploy-sentinel", AgentGitIdentity.EnvFor("deploy-sentinel", "Mario Rossi", null)[AgentGitIdentity.EnvCommitterName],
                "senza email della persona il committer resta l'agente");
        }

        [TestMethod]
        public void Sign_an_approval_as_the_person()
        {
            var env = AgentGitIdentity.EnvForPerson("Mario Rossi", "mario.rossi@example.com");
            Assert.AreEqual("Mario Rossi", env[AgentGitIdentity.EnvAuthorName]);
            Assert.AreEqual("mario.rossi@example.com", env[AgentGitIdentity.EnvCommitterEmail]);
            Assert.AreEqual(0, AgentGitIdentity.EnvForPerson(null, "x@example.com").Count);
        }

        [TestMethod]
        public void Produce_nothing_without_a_name()
        {
            Assert.AreEqual(0, AgentGitIdentity.EnvFor(null).Count);
            Assert.AreEqual(0, AgentGitIdentity.EnvFor("   ").Count);
        }

        [TestMethod]
        public void Derive_the_signature_name_from_a_non_citizen_agent_file()
        {
            Assert.AreEqual("nightly-report", AgentGitIdentity.NameFromAgentFile("/p/agents/nightly-report.agent.md"));
            Assert.AreEqual("nightly-report", AgentGitIdentity.NameFromAgentFile("nightly-report.AGENT.MD"));
            // Nessun suffisso .agent.md → tiene il nome file com'è.
            Assert.AreEqual("foo", AgentGitIdentity.NameFromAgentFile("/p/foo"));
            // Vuoto/null → sentinella stabile.
            Assert.AreEqual("agent", AgentGitIdentity.NameFromAgentFile(null));
            Assert.AreEqual("agent", AgentGitIdentity.NameFromAgentFile(""));
        }
    }
}
