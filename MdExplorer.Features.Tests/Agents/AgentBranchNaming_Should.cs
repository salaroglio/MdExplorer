using System;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Il nome con cui il lavoro di un agente viene pubblicato. Si decide al push — l'unico
    /// istante in cui l'esito è noto — e l'etichetta si ricava dai file toccati, che è un fatto
    /// e non un'interpretazione.
    /// </summary>
    [TestClass]
    public class AgentBranchNaming_Should
    {
        private static readonly DateTime When = new(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc);

        private static string Compose(string[] files, string fallback = null)
            => AgentBranchNaming.ComposePublishedBranch(
                "salaroglio@hotmail.com", "wiki-curator", files, "a1b2c3d4e5f6", When, fallback);

        [TestMethod]
        public void Name_the_branch_after_the_folder_that_was_touched()
        {
            var branch = Compose(new[] { "llm-wiki/log.md", "llm-wiki/index.md" });

            Assert.AreEqual("agent/salaroglio/wiki-curator/2026-08-02-llm-wiki-a1b2c3", branch);
        }

        [TestMethod]
        public void Prefer_the_folder_over_the_file_name_for_a_single_file()
        {
            // "llm-wiki" dice più di "log": la cartella è il territorio, il file è un dettaglio.
            var branch = Compose(new[] { "llm-wiki/log.md" });

            StringAssert.Contains(branch, "-llm-wiki-");
        }

        [TestMethod]
        public void Use_the_file_name_when_it_sits_in_the_root()
        {
            var branch = Compose(new[] { "architecture.md" });

            StringAssert.Contains(branch, "-architecture-");
        }

        [TestMethod]
        public void Fall_back_to_the_scope_when_the_work_is_scattered()
        {
            // Cartelle diverse: non c'è una parola onesta da ricavare dai file, si usa l'ambito.
            var branch = Compose(new[] { "docs/architecture.md", "llm-wiki/log.md" }, fallback: "Architettura");

            StringAssert.Contains(branch, "-architettura-");
        }

        [TestMethod]
        public void Fall_back_to_misc_when_there_is_nothing_to_say()
        {
            var branch = Compose(new[] { "docs/a.md", "llm-wiki/b.md" }, fallback: null);

            StringAssert.Contains(branch, "-" + AgentBranchNaming.FallbackLabel + "-");
        }

        [TestMethod]
        public void Keep_the_same_name_when_the_same_activity_is_republished()
        {
            // L'id breve deriva dall'activityId: ripubblicare la stessa attività deve aggiornare
            // lo STESSO ramo, non crearne uno nuovo a ogni push.
            var first = Compose(new[] { "llm-wiki/log.md" });
            var second = Compose(new[] { "llm-wiki/log.md", "llm-wiki/index.md" });

            StringAssert.EndsWith(first, "-a1b2c3");
            StringAssert.EndsWith(second, "-a1b2c3");
        }

        [TestMethod]
        public void Produce_a_ref_git_accepts()
        {
            // Le regole dei ref vietano spazi, '..', '~^:?*[', il trattino iniziale e '.lock'.
            var branch = AgentBranchNaming.ComposePublishedBranch(
                "Mario Rossi <m.rossi@x.it>", "Wiki Curator!!", new[] { "car tella/../file strano.md" },
                "  ", When, "Ambito con: caratteri? strani*");

            foreach (var vietato in new[] { " ", "..", "~", "^", ":", "?", "*", "[", "\\" })
                Assert.IsFalse(branch.Contains(vietato), $"il ref non deve contenere '{vietato}': {branch}");

            Assert.IsFalse(branch.Contains("--"), $"niente trattini doppi: {branch}");
            Assert.IsFalse(branch.EndsWith(".lock"), branch);
            StringAssert.StartsWith(branch, "agent/");
        }

        [TestMethod]
        public void Say_unknown_instead_of_inventing_an_owner()
        {
            var branch = AgentBranchNaming.ComposePublishedBranch(
                null, "wiki-curator", new[] { "docs/a.md" }, "abcdef", When);

            StringAssert.StartsWith(branch, "agent/sconosciuto/wiki-curator/");
        }
    }
}
