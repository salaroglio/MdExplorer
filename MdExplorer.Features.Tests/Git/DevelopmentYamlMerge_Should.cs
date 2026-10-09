using MdExplorer.Features.Git;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Git
{
    /// <summary>
    /// L'unione di <c>.development.yml</c> voce per voce: il caso visto su Windows il 07/10 (la città accesa sul computer, la
    /// sorgente che aggiunge il documento del workflow nello stesso blocco) e i suoi vicini.
    /// </summary>
    [TestClass]
    public class DevelopmentYamlMerge_Should
    {
        private const string Base = "harness:\n  target: copilot\nagentCity:\n  ownershipDoc: citta-degli-agenti/gara/responsabilita.md\n";

        [TestMethod]
        public void Keep_the_city_turned_on_here_and_take_the_new_line_of_the_source()
        {
            const string ours = "harness:\n  target: copilot\nagentCity:\n  enabled: true\n  ownershipDoc: citta-degli-agenti/gara/responsabilita.md\n  roomSecret: XYZ\n";
            const string theirs = "harness:\n  target: copilot\nagentCity:\n  ownershipDoc: citta-degli-agenti/gara/responsabilita.md\n  workflowDoc: citta-degli-agenti/gara/workflow.md\n";

            var merged = DevelopmentYamlMerge.Merge(Base, ours, theirs, out var conflict);

            Assert.IsNull(conflict);
            Assert.AreEqual("harness:\n  target: copilot\nagentCity:\n  enabled: true\n  ownershipDoc: citta-degli-agenti/gara/responsabilita.md\n" +
                            "  roomSecret: XYZ\n  workflowDoc: citta-degli-agenti/gara/workflow.md\n", merged);
        }

        [TestMethod]
        public void Stop_and_name_the_entry_both_sides_changed_differently()
        {
            var merged = DevelopmentYamlMerge.Merge(Base,
                Base.Replace("target: copilot", "target: claude"),
                Base.Replace("target: copilot", "target: opencode"), out var conflict);

            Assert.IsNull(merged);
            Assert.AreEqual("harness.target", conflict);
        }

        [TestMethod]
        public void Follow_a_removal_of_the_source_when_the_entry_is_unchanged_here()
        {
            var merged = DevelopmentYamlMerge.Merge(Base, Base, "agentCity:\n  ownershipDoc: citta-degli-agenti/gara/responsabilita.md\n", out var conflict);

            Assert.IsNull(conflict);
            Assert.IsFalse(merged.Contains("harness"), "la sorgente l'ha tolta e qui non era cambiata");
        }

        [TestMethod]
        public void Merge_also_without_a_common_ancestor_when_the_two_sides_add_different_entries()
        {
            var merged = DevelopmentYamlMerge.Merge(null, "a: 1\n", "b: 2\n", out var conflict);
            Assert.IsNull(conflict);
            Assert.AreEqual("a: 1\nb: 2\n", merged);
        }

        [TestMethod]
        public void Give_back_the_demo_file_unchanged_when_nothing_differs()
        {
            const string demo = "folders: []\ncompatibility:\n  mode: mdexplorer\n  gitHubOptions:\n    embedImages: false\n    stripInteractive: true\n" +
                                "    preserveEmoji: true\nyamlAutoGeneration:\n  enabled: true\n  excludePaths:\n  - .github\nexternalBrowser:\n  enabled: false\n" +
                                "  openAllExternal: false\n  urlPatterns: []\nproject:\n  participants: []\nharness:\n  target: copilot\nagentCity:\n" +
                                "  ownershipDoc: citta-degli-agenti/gara/responsabilita.md\n  workflowDoc: citta-degli-agenti/gara/workflow.md\n";
            Assert.AreEqual(demo, DevelopmentYamlMerge.Merge(demo, demo, demo, out _));
        }
    }
}
