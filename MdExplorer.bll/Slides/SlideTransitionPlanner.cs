using Markdig;
using MdExplorer.Features.Services.SourceMapping;
using System;

namespace MdExplorer.Features.Slides
{
    public enum SlideTransitionPlanStatus
    {
        /// <summary>The file is not the one the page was built from: the page's lines would point to the wrong place.</summary>
        DocumentChanged,
        /// <summary>It is already like that.</summary>
        NoChange,
        /// <summary>It cannot be done safely: <see cref="SlideTransitionPlan.Refusal"/> says why.</summary>
        Refused,
        /// <summary>Write <see cref="SlideTransitionPlan.NewText"/>.</summary>
        Ready,
    }

    public sealed record SlideTransitionPlan(SlideTransitionPlanStatus Status, string NewText, SlideTransitionRefusal? Refusal, string Detail)
    {
        /// <summary>The fingerprint of the file after the change.</summary>
        public string NewSourceHash => NewText == null ? null : MarkdownFileEditor.SourceHash(NewText);
    }

    /// <summary>
    /// What <c>POST api/mdfiles/SetSlideTransition</c> decides, without touching the disk: the controller reads the
    /// file, asks here, and writes only on <see cref="SlideTransitionPlanStatus.Ready"/>. Fingerprint first (409),
    /// then the change itself (422), as <c>EditRenderedText</c> and <c>MoveListItem</c>.
    /// </summary>
    public static class SlideTransitionPlanner
    {
        /// <param name="scope">"slide" (the slide starting on <paramref name="line"/>) or "deck" (the whole presentation).</param>
        /// <param name="transition">One of <see cref="SlideTransitionEditor.Transitions"/>; for a slide, null takes its own away (the deck's applies).</param>
        public static SlideTransitionPlan Plan(string text, string expectedSourceHash, string scope, int line, string transition, MarkdownPipeline pipeline)
        {
            if (!string.Equals(MarkdownFileEditor.SourceHash(text), expectedSourceHash, StringComparison.OrdinalIgnoreCase))
            {
                return new SlideTransitionPlan(SlideTransitionPlanStatus.DocumentChanged, null, null, null);
            }

            var edit = string.Equals(scope, "deck", StringComparison.OrdinalIgnoreCase)
                ? SlideTransitionEditor.SetDeck(text, transition)
                : SlideTransitionEditor.SetSlide(text, line, transition, pipeline);
            if (edit.Applied) return new SlideTransitionPlan(SlideTransitionPlanStatus.Ready, edit.NewText, null, null);
            if (edit.NoChange) return new SlideTransitionPlan(SlideTransitionPlanStatus.NoChange, null, null, edit.Detail);
            return new SlideTransitionPlan(SlideTransitionPlanStatus.Refused, null, edit.Refusal, edit.Detail);
        }
    }
}
