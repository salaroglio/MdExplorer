using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MdExplorer.Features.Tests.Slides
{
    /// <summary>What <c>POST api/mdfiles/SetSlideTransition</c> decides: 409, no change, 422 or write.</summary>
    [TestClass]
    public class SlideTransitionPlanner_Should
    {
        private const string Deck = "---\ntitle: T\ndocument_type: slides\n---\n# Uno\n\n---\n\n## Due\n";

        private static SlideTransitionPlan Plan(string text, string hash, string scope, int line, string transition)
            => SlideTransitionPlanner.Plan(text, hash, scope, line, transition, DocumentViewPipeline.Build(null));

        [TestMethod]
        public void ReadyForASlideWithTheNewTextAndItsFingerprint()
        {
            var plan = Plan(Deck, MarkdownFileEditor.SourceHash(Deck), "slide", 5, "zoom");

            Assert.AreEqual(SlideTransitionPlanStatus.Ready, plan.Status);
            StringAssert.Contains(plan.NewText, "<!-- .slide: data-transition=\"zoom\" -->\n# Uno");
            Assert.AreEqual(MarkdownFileEditor.SourceHash(plan.NewText), plan.NewSourceHash);
        }

        [TestMethod]
        public void ReadyForTheWholeDeck()
        {
            var plan = Plan(Deck, MarkdownFileEditor.SourceHash(Deck), "deck", 0, "fade");

            Assert.AreEqual(SlideTransitionPlanStatus.Ready, plan.Status);
            StringAssert.Contains(plan.NewText, "reveal:\n  config:\n    transition: fade\n---");
        }

        [TestMethod]
        public void RefuseWhenTheFileIsNotTheOneThePageWasBuiltFrom()
        {
            var plan = Plan(Deck, MarkdownFileEditor.SourceHash("altro\n"), "slide", 5, "zoom");

            Assert.AreEqual(SlideTransitionPlanStatus.DocumentChanged, plan.Status);
            Assert.IsNull(plan.NewText);
        }

        [TestMethod]
        public void NotWriteWhenItIsAlreadyLikeThat()
        {
            var text = Deck.Replace("# Uno\n", "<!-- .slide: data-transition=\"zoom\" -->\n# Uno\n");

            Assert.AreEqual(SlideTransitionPlanStatus.NoChange, Plan(text, MarkdownFileEditor.SourceHash(text), "slide", 5, "zoom").Status);
        }

        [TestMethod]
        public void SayWhyItIsRefused()
        {
            var plan = Plan(Deck, MarkdownFileEditor.SourceHash(Deck), "slide", 5, "spin");

            Assert.AreEqual(SlideTransitionPlanStatus.Refused, plan.Status);
            Assert.AreEqual(SlideTransitionRefusal.UnknownTransition, plan.Refusal);
        }
    }
}
