using System.Linq;
using MdExplorer.Features.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Configuration
{
    /// <summary>
    /// Le righe ritirate di <c>.development.yml</c> si cancellano per nome, non si ignorano: la lettura resta
    /// rigida, e una riga ritirata non si confonde con un errore di battitura.
    /// </summary>
    [TestClass]
    public class RetiredConfigKeys_Should
    {
        [TestMethod]
        public void Delete_the_retired_key_and_leave_the_rest_as_it_was()
        {
            const string yaml = "folders: []\n# la città\nagentCity:\n  enabled: true\n  autoMergeAgentDeliverables: true\n  ownershipDoc: doc.md\nharness:\n  target: copilot\n";

            var cleaned = RetiredConfigKeys.Remove(yaml, out var removed);

            Assert.AreEqual("folders: []\n# la città\nagentCity:\n  enabled: true\n  ownershipDoc: doc.md\nharness:\n  target: copilot\n", cleaned);
            CollectionAssert.AreEqual(new[] { "agentCity.autoMergeAgentDeliverables" }, removed.ToList());
        }

        [TestMethod]
        public void Not_touch_a_file_that_has_no_retired_key()
        {
            const string yaml = "agentCity:\r\n  enabled: true\r\n";

            var cleaned = RetiredConfigKeys.Remove(yaml, out var removed);

            Assert.AreSame(yaml, cleaned, "niente da togliere: il testo è lo stesso, non una copia riscritta");
            Assert.AreEqual(0, removed.Count);
        }

        [TestMethod]
        public void Keep_windows_line_endings()
        {
            var cleaned = RetiredConfigKeys.Remove("agentCity:\r\n  enabled: true\r\n  autoMergeAgentDeliverables: false\r\n", out _);

            Assert.AreEqual("agentCity:\r\n  enabled: true\r\n", cleaned);
        }

        [TestMethod]
        public void Delete_a_section_that_only_held_the_retired_key()
        {
            var cleaned = RetiredConfigKeys.Remove("folders: []\nagentCity:\n  autoMergeAgentDeliverables: true\nharness:\n  target: copilot\n", out _);

            Assert.AreEqual("folders: []\nharness:\n  target: copilot\n", cleaned);
        }

        [TestMethod]
        public void Not_delete_the_same_name_somewhere_else()
        {
            // Lo stesso nome in un'altra sezione, o più in profondità, non è la riga ritirata.
            const string yaml = "other:\n  autoMergeAgentDeliverables: true\nagentCity:\n  enabled: true\n  maintenance:\n    autoMergeAgentDeliverables: true\n";

            var cleaned = RetiredConfigKeys.Remove(yaml, out var removed);

            Assert.AreSame(yaml, cleaned);
            Assert.AreEqual(0, removed.Count);
        }

        [TestMethod]
        public void Leave_a_misspelled_key_where_it_is()
        {
            // Un errore di battitura NON è una riga ritirata: resta nel file, e la lettura rigida lo dirà.
            const string yaml = "agentCity:\n  enabeld: true\n  autoMergeAgentDeliverable: true\n";

            Assert.AreSame(yaml, RetiredConfigKeys.Remove(yaml, out _));
        }
    }
}
