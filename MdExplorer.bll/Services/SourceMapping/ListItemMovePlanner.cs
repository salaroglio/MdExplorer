using Markdig;
using System;

namespace MdExplorer.Features.Services.SourceMapping
{
    public enum ListItemMovePlanStatus
    {
        /// <summary>The file is not the one the page was built from: the page's lines would point to the wrong place.</summary>
        DocumentChanged,
        /// <summary>The item is already where it was asked to go.</summary>
        NoChange,
        /// <summary>It cannot be done safely: <see cref="ListItemMovePlan.Refusal"/> says why.</summary>
        Refused,
        /// <summary>Write <see cref="ListItemMovePlan.NewText"/>.</summary>
        Ready,
    }

    public sealed record ListItemMovePlan(ListItemMovePlanStatus Status, string NewText, ListItemMoveRefusal? Refusal, string Detail)
    {
        /// <summary>The fingerprint of the file after the move, for the page that asked.</summary>
        public string NewSourceHash => NewText == null ? null : MarkdownFileEditor.SourceHash(NewText);
    }

    /// <summary>
    /// What <c>POST api/mdfiles/MoveListItem</c> decides, without touching the disk: the controller
    /// reads the file, asks here, and writes only on <see cref="ListItemMovePlanStatus.Ready"/>.
    /// Same order as <c>EditRenderedText</c>: first the fingerprint (409), then the move itself (422).
    /// </summary>
    public static class ListItemMovePlanner
    {
        public static ListItemMovePlan Plan(string text, string expectedSourceHash, int itemLine, int toIndex, MarkdownPipeline pipeline)
        {
            if (!string.Equals(MarkdownFileEditor.SourceHash(text), expectedSourceHash, StringComparison.OrdinalIgnoreCase))
            {
                return new ListItemMovePlan(ListItemMovePlanStatus.DocumentChanged, null, null, null);
            }

            var move = ListItemReorderer.Move(text, itemLine, toIndex, pipeline);
            if (move.Applied) return new ListItemMovePlan(ListItemMovePlanStatus.Ready, move.NewText, null, null);
            if (move.NoChange) return new ListItemMovePlan(ListItemMovePlanStatus.NoChange, null, null, move.Detail);
            return new ListItemMovePlan(ListItemMovePlanStatus.Refused, null, move.Refusal, move.Detail);
        }
    }
}
