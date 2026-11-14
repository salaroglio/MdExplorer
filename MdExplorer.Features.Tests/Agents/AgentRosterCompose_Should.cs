using System.Collections.Generic;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    [TestClass]
    public class AgentRosterCompose_Should
    {
        private const string Body = "# stem-curator\n\nSei un curatore.";
        private const string Task = "Classifica questi stem.";

        private static AgentRosterEntry Entry(string name, string role, params string[] skills)
            => new AgentRosterEntry { Name = name, Role = role, Skills = new List<string>(skills) };

        [TestMethod]
        public void Not_inject_any_roster_section_when_roster_is_null()
        {
            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task, null);

            Assert.IsFalse(composed.Contains("Colleghi nel progetto"));
            StringAssert.Contains(composed, "# Task");
            StringAssert.Contains(composed, Task);
        }

        [TestMethod]
        public void Not_inject_a_roster_section_when_roster_is_empty()
        {
            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task, new List<AgentRosterEntry>());
            Assert.IsFalse(composed.Contains("Colleghi nel progetto"));
        }

        [TestMethod]
        public void Inject_colleagues_with_role_and_skills()
        {
            var roster = new List<AgentRosterEntry>
            {
                Entry("cobol-pipeline", "Pipeline COBOL", "parse-cobol", "emit-ttl"),
                Entry("deploy-sentinel", "Guardiano del deploy"),
            };

            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task, roster);

            StringAssert.Contains(composed, "# Colleghi nel progetto");
            StringAssert.Contains(composed, "**cobol-pipeline** — Pipeline COBOL (skill: parse-cobol, emit-ttl)");
            StringAssert.Contains(composed, "**deploy-sentinel** — Guardiano del deploy");
            // niente "(skill: ...)" per chi non ha skill
            Assert.IsFalse(composed.Contains("Guardiano del deploy (skill"));
        }

        [TestMethod]
        public void Keep_the_task_section_after_the_roster()
        {
            var roster = new List<AgentRosterEntry> { Entry("mate", "collega") };
            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task, roster);

            var rosterIdx = composed.IndexOf("Colleghi nel progetto");
            var taskIdx = composed.IndexOf("# Task");
            Assert.IsTrue(rosterIdx >= 0 && taskIdx > rosterIdx, "la rubrica precede il task");
            StringAssert.Contains(composed, Task);
        }

        [TestMethod]
        public void Skip_nameless_roster_entries()
        {
            var roster = new List<AgentRosterEntry>
            {
                Entry("", "senza nome"),
                Entry("valido", "collega valido"),
            };

            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task, roster);

            StringAssert.Contains(composed, "**valido**");
            Assert.IsFalse(composed.Contains("senza nome"));
        }

        [TestMethod]
        public void Preserve_backward_compatible_two_argument_call()
        {
            // La firma a due argomenti deve continuare a funzionare (parametro opzionale).
            var composed = AgentPromptComposer.ComposeRunPrompt(Body, Task);
            StringAssert.Contains(composed, "# Task");
            Assert.IsFalse(composed.Contains("Colleghi nel progetto"));
        }
    }
}
