using System.Collections.Generic;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentOwnershipCompose_Should
    {
        private const string Body = "# analyst\n\nSei un analista.";
        private const string Task = "Produci il workflow.";

        private static OwnershipEntry Own(string scope, string resp, string email, params string[] agents)
            => new OwnershipEntry { Scope = scope, Responsible = resp, GitEmail = email, Agents = agents };

        [TestMethod]
        public void Not_inject_ownership_when_null_or_empty()
        {
            Assert.IsFalse(AgentPromptComposer.ComposeRunPrompt(Body, Task, null, null).Contains("Ownership del progetto"));
            Assert.IsFalse(AgentPromptComposer.ComposeRunPrompt(Body, Task, null, new List<OwnershipEntry>()).Contains("Ownership del progetto"));
        }

        [TestMethod]
        public void Inject_scope_responsible_and_agents()
        {
            var ownership = new List<OwnershipEntry>
            {
                Own("WSAA", "Carlo", "carlo@x.it", "analyst", "dev"),
                Own("Batch", "Marco", "marco@y.it"),
            };

            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task, null, ownership);

            StringAssert.Contains(composed, "# Ownership del progetto");
            StringAssert.Contains(composed, "**WSAA** — resp. Carlo <carlo@x.it> (agenti: analyst, dev)");
            StringAssert.Contains(composed, "**Batch** — resp. Marco <marco@y.it>");
            Assert.IsFalse(composed.Contains("Marco <marco@y.it> (agenti"), "niente lista agenti se assente");
        }

        [TestMethod]
        public void Also_inject_ownership_in_the_wake_prompt()
        {
            var ownership = new List<OwnershipEntry> { Own("WSAA", "Carlo", "carlo@x.it", "dev") };
            var composed = AgentPromptComposer.ComposeMessageWakePrompt(
                Body, "external", "ciao", roster: null, topics: null, ownership: ownership);

            StringAssert.Contains(composed, "# Ownership del progetto");
            StringAssert.Contains(composed, "**WSAA**");
            // il messaggio resta DATO fra delimitatori (invariato dal 4a)
            StringAssert.Contains(composed, "# Messaggio ricevuto");
        }

        [TestMethod]
        public void Keep_backward_compatible_calls_without_ownership()
        {
            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task);
            Assert.IsFalse(composed.Contains("Ownership del progetto"));
        }
    }
}
