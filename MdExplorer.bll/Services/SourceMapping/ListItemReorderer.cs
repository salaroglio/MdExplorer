using Markdig;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Services.SourceMapping
{
    /// <summary>The outcome of moving a list item: the new text, or why nothing was written.</summary>
    public sealed record ListItemMove(string NewText, string Refusal)
    {
        public bool Applied => NewText != null;

        public static ListItemMove Done(string text) => new(text, null);

        public static ListItemMove Refused(string reason) => new(null, reason);
    }

    /// <summary>
    /// Moves a list item among its siblings in the markdown file, by lines: the item goes with all
    /// it holds (sub-items, more paragraphs, code) and the marker of each POSITION stays where it is,
    /// so a numbered list stays numbered (<c>1. 2. 3.</c>, <c>5. 6. 7.</c>, or <c>1. 1. 1.</c> as it was).
    ///
    /// <para>
    /// Nothing is written unless the new file, read again, is exactly the old one with the sibling
    /// items in the new order: the blocks outside the list are the same, and each item is the same
    /// item (kind, children, list style, looseness, text), only elsewhere. Same rule as
    /// <c>RenderedTextEditor.DeleteBlock</c>; nothing is repaired silently.
    /// </para>
    ///
    /// <para>
    /// Measured (Markdig 1.1.2, sprint F0): a <c>ListItemBlock</c>'s <c>Line</c> is reliable, its
    /// <c>Span.End</c> is inclusive and in a loose list reaches the <c>\n</c> after the item — so the
    /// item's lines are cut with <c>Line</c>, not with offsets; a nested item is a
    /// <c>ListItemBlock</c> of its own list; <c>10.</c> is one column wider than <c>9.</c>, and the
    /// item's continuation lines are indented by the marker's width: refused when it would change.
    /// </para>
    /// </summary>
    public static class ListItemReorderer
    {
        /// <param name="text">The markdown file as it is on disk.</param>
        /// <param name="itemLine">1-based line the item starts on (<c>data-mde-line-start</c>).</param>
        /// <param name="toIndex">0-based position among the siblings the item goes to.</param>
        public static ListItemMove Move(string text, int itemLine, int toIndex, MarkdownPipeline pipeline)
        {
            var document = Markdown.Parse(text, pipeline);
            var item = document.Descendants().OfType<ListItemBlock>().FirstOrDefault(i => i.Line == itemLine - 1);
            if (item == null)
                return ListItemMove.Refused($"Line {itemLine} is not the start of a list item.");

            var list = (ListBlock)item.Parent;
            var items = list.OfType<ListItemBlock>().ToList();
            var from = items.IndexOf(item);
            if (items.Count < 2)
                return ListItemMove.Refused("The list has one item only.");
            if (toIndex < 0 || toIndex >= items.Count)
                return ListItemMove.Refused($"The list has {items.Count} items: position {toIndex} does not exist.");
            if (toIndex == from)
                return ListItemMove.Refused("The item is already there.");

            var starts = MarkdownSourceMapService.BuildLineStartOffsets(text);
            string Line(int line) => text.Substring(starts[line], (line + 1 < starts.Length ? starts[line + 1] : text.Length) - starts[line]);
            int LineOf(int offset)
            {
                var index = Array.BinarySearch(starts, offset);
                return index >= 0 ? index : ~index - 1;
            }

            // Each item: its lines, cut where the next sibling starts. The blank lines that follow an
            // item are the separator of its POSITION and stay there.
            var count = items.Count;
            var firsts = items.Select(i => i.Line).ToArray();
            var lastLine = LineOf(items[count - 1].Span.End);
            var bodies = new List<List<string>>();
            var separators = new List<string>();
            var prefixes = new List<string>();
            for (var i = 0; i < count; i++)
            {
                var end = i + 1 < count ? firsts[i + 1] : lastLine + 1;
                var lines = Enumerable.Range(firsts[i], end - firsts[i]).Select(Line).ToList();
                var blanks = 0;
                while (i + 1 < count && lines.Count - blanks > 1 && lines[lines.Count - 1 - blanks].Trim().Length == 0) blanks++;
                separators.Add(string.Concat(lines.Skip(lines.Count - blanks)));
                bodies.Add(lines.Take(lines.Count - blanks).ToList());

                var child = items[i].Count > 0 ? items[i][0] : null;
                if (child == null || child.Line != firsts[i])
                    return ListItemMove.Refused("An item with nothing on the line of its marker cannot be moved.");
                prefixes.Add(text.Substring(starts[firsts[i]], child.Span.Start - starts[firsts[i]]));
            }

            var order = Enumerable.Range(0, count).ToList();
            order.RemoveAt(from);
            order.Insert(toIndex, from);
            for (var position = 0; position < count; position++)
            {
                if (prefixes[order[position]].Length != prefixes[position].Length)
                    return ListItemMove.Refused($"The marker \"{prefixes[order[position]].Trim()}\" and \"{prefixes[position].Trim()}\" have a different width: the item's lines would no longer line up.");
            }

            var eol = text.Contains("\r\n") ? "\r\n" : "\n";
            var lastEndsWithEol = bodies[count - 1][bodies[count - 1].Count - 1].EndsWith("\n", StringComparison.Ordinal);
            var block = new StringBuilder();
            for (var position = 0; position < count; position++)
            {
                var source = order[position];
                var body = new List<string>(bodies[source]);
                // The marker belongs to the position: numbers stay in order, the content moves.
                body[0] = prefixes[position] + body[0].Substring(prefixes[source].Length);
                var joined = string.Concat(body);
                if (position < count - 1)
                {
                    if (!joined.EndsWith("\n", StringComparison.Ordinal)) joined += eol;
                    block.Append(joined).Append(separators[position]);
                }
                else
                {
                    joined = joined.TrimEnd('\r', '\n');
                    block.Append(joined);
                    if (lastEndsWithEol) block.Append(eol);
                }
            }

            var blockStart = starts[firsts[0]];
            var blockEnd = lastLine + 1 < starts.Length ? starts[lastLine + 1] : text.Length;
            var newText = text.Substring(0, blockStart) + block + text.Substring(blockEnd);

            // Read again: the same blocks, the items only in another order.
            var listIndex = document.Descendants().OfType<ListBlock>().ToList().IndexOf(list);
            var newLists = Markdown.Parse(newText, pipeline);
            var newList = newLists.Descendants().OfType<ListBlock>().ElementAtOrDefault(listIndex);
            var newItems = newList?.OfType<ListItemBlock>().ToList();
            var expectedItems = order.Select(i => ItemSignature(items[i], text)).ToList();
            if (newItems == null
                || !OutsideSignature(document, text, list).SequenceEqual(OutsideSignature(newLists, newText, newList))
                || !expectedItems.SequenceEqual(newItems.Select(i => ItemSignature(i, newText))))
            {
                return ListItemMove.Refused("Moving the item would change something else in the file (a list laid out differently, another block): nothing was written.");
            }
            return ListItemMove.Done(newText);
        }

        /// <summary>Every block of the document but what is inside the items of <paramref name="list"/>.</summary>
        private static List<string> OutsideSignature(MarkdownDocument document, string text, ListBlock list)
            => document.Descendants().OfType<Block>()
                .Where(b => !InsideItemsOf(b, list))
                .Select(b => Entry(b, text))
                .ToList();

        private static bool InsideItemsOf(Block block, ListBlock list)
        {
            for (var parent = block.Parent; parent != null; parent = parent.Parent)
            {
                if (parent == list) return true;
            }
            return false;
        }

        private static string ItemSignature(ListItemBlock item, string text)
            => string.Join("\n", new[] { item }.Cast<Block>().Concat(item.Descendants().OfType<Block>()).Select(b => Entry(b, text)));

        private static string Entry(Block block, string text)
        {
            var entry = block.GetType().Name;
            if (block is ContainerBlock container) entry += "#" + container.Count;
            if (block is ListBlock list) entry += (list.IsOrdered ? " ordered" : " bullet") + (list.IsLoose ? " loose" : " tight");
            if (block is LeafBlock && block.Span.Length > 0) entry += ":" + text.Substring(block.Span.Start, block.Span.Length).Trim();
            return entry;
        }
    }
}
