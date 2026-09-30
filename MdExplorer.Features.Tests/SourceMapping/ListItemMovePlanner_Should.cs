using MdExplorer.Features.Services.SourceMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.SourceMapping
{
    /// <summary>What <c>POST api/mdfiles/MoveListItem</c> decides: 409, no change, 422 or write.</summary>
    [TestClass]
    public class ListItemMovePlanner_Should
    {
        private const string Text = "- a\n- b\n- c\n";

        private static ListItemMovePlan Plan(string text, string hash, int line, int to)
            => ListItemMovePlanner.Plan(text, hash, line, to, DocumentViewPipeline.Build(null));

        [TestMethod]
        public void ReadyWithTheNewTextAndItsFingerprint()
        {
            var plan = Plan(Text, MarkdownFileEditor.SourceHash(Text), 1, 2);

            Assert.AreEqual(ListItemMovePlanStatus.Ready, plan.Status);
            Assert.AreEqual("- b\n- c\n- a\n", plan.NewText);
            Assert.AreEqual(MarkdownFileEditor.SourceHash("- b\n- c\n- a\n"), plan.NewSourceHash, "the page must know the file's new fingerprint");
        }

        [TestMethod]
        public void RefuseWhenTheFileIsNotTheOneThePageWasBuiltFrom()
        {
            var plan = Plan(Text, MarkdownFileEditor.SourceHash("- x\n- y\n"), 1, 2);

            Assert.AreEqual(ListItemMovePlanStatus.DocumentChanged, plan.Status);
            Assert.IsNull(plan.NewText, "nothing to write");
        }

        [TestMethod]
        public void NotWriteWhenTheItemIsAlreadyThere()
        {
            var plan = Plan(Text, MarkdownFileEditor.SourceHash(Text), 2, 1);

            Assert.AreEqual(ListItemMovePlanStatus.NoChange, plan.Status);
            Assert.IsNull(plan.NewText);
        }

        [TestMethod]
        public void SayWhyItIsRefused()
        {
            var plan = Plan(Text, MarkdownFileEditor.SourceHash(Text), 1, 7);

            Assert.AreEqual(ListItemMovePlanStatus.Refused, plan.Status);
            Assert.AreEqual(ListItemMoveRefusal.PositionOutOfRange, plan.Refusal);
            Assert.IsNull(plan.NewText);
        }
    }
}
