using MdExplorer.Services.MarkDiagram;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Ragguagli» for the MarkAgent tab (sprint 2026-09-29-Motore-LLM-Unico, D9): the block put at the head of the
    /// user's next message in the tab. Measured live on 29/09/2026 with the three engines: the tab's LLM reported the
    /// changes confirmed from «spiega il diagramma».
    /// </summary>
    [TestClass]
    public class MarkAgentBriefings_Should
    {
        [TestMethod]
        public void Tell_the_tab_what_was_done_and_to_answer_the_user_not_the_briefings()
        {
            var block = MarkAgentBriefings.Block(new[] { "prima modifica", "seconda modifica" });

            StringAssert.StartsWith(block, "[Ragguagli sulle attività fatte in parallelo in MdExplorer");
            StringAssert.Contains(block, "- prima modifica\n");
            StringAssert.Contains(block, "- seconda modifica\n");
            StringAssert.Contains(block, "rispondi al messaggio dell'utente che segue");
            StringAssert.Contains(block, "[Fine dei ragguagli]");
        }

        [TestMethod]
        public void Give_nothing_for_a_connection_without_briefings()
        {
            var briefings = new MarkAgentBriefings(null, null);
            Assert.AreEqual(0, briefings.TakeAll("nessuna").Count);
            Assert.AreEqual(0, briefings.TakeAll(null).Count);
        }
    }
}
