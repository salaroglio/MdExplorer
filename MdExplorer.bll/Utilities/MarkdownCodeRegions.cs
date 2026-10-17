using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Where the code and the tables of a markdown text are, as Markdig — the parser that renders
    /// the page — sees them.
    ///
    /// <para>
    /// The commands rewrite the markdown with regular expressions before Markdig runs, and a
    /// regular expression cannot tell a line of code from a line of text. Measured on 215 real
    /// documents (11/09/2026): an image written as an example inside a code block became the
    /// image's HTML, a link got <c>?connectionId=…</c>, a <c>```plantuml</c> example inside a
    /// <c>````</c> block became a diagram, <c>:x:</c> became a clickable span. A code block must
    /// show what the file says. Asking Markdig is the only way to agree with the page on fences of
    /// any length, <c>~~~</c>, indented code, code in lists and quotes, inline <c>`code`</c>.
    /// </para>
    ///
    /// <para>
    /// Offsets are positions in the text given to <see cref="Of"/>. A command that rewrites the
    /// text must ask again for the new text.
    /// </para>
    ///
    /// <para>
    /// The text is parsed at the first question, not before: most commands find nothing to
    /// rewrite in most documents, and then pay nothing (a parse is ~3 ms on a 2,200-line
    /// document, the page asks up to ten times).
    /// </para>
    /// </summary>
    public sealed class MarkdownCodeRegions
    {
        // What decides code and tables in the page's pipeline (MdExplorerController).
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();

        private readonly List<(int Start, int ContentStart, int End)> _codeBlocks = new();
        private readonly List<(int Start, int End)> _inlineCode = new();
        private readonly List<(int Start, int End)> _tables = new();
        private readonly string _markdown;
        private bool _parsed;

        private MarkdownCodeRegions(string markdown)
        {
            _markdown = markdown;
        }

        public static MarkdownCodeRegions Of(string markdown) => new MarkdownCodeRegions(markdown);

        private void EnsureParsed()
        {
            if (_parsed)
            {
                return;
            }
            _parsed = true;
            Parse(_markdown, this);
        }

        private static void Parse(string markdown, MarkdownCodeRegions regions)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return;
            }

            var lineStarts = LineStarts(markdown);
            var document = Markdown.Parse(markdown, Pipeline);

            // Whole lines, from the line the block starts on: how far the span starts inside an
            // indented or quoted line is Markdig's business, the line is not.
            foreach (var block in document.Descendants<CodeBlock>())
            {
                var firstLine = block.Line;
                // The span of a fence left open covers only its opening line (measured, Markdig
                // 1.1.2), while the page shows everything after it as code: the content lines say
                // where the block really ends.
                var lastOffset = Math.Max(block.Span.Start, block.Span.End);
                for (var i = 0; i < block.Lines.Count; i++)
                {
                    lastOffset = Math.Max(lastOffset, block.Lines.Lines[i].Position);
                }
                var lastLine = LineOf(lineStarts, lastOffset);
                var start = lineStarts[firstLine];
                var end = LineEnd(markdown, lineStarts, lastLine);
                // A fenced block's own opening line is not its content: the fence commands start
                // there, and must keep working on their own block.
                var contentStart = block is FencedCodeBlock
                    ? LineEnd(markdown, lineStarts, firstLine) + 1
                    : start;
                regions._codeBlocks.Add((start, contentStart, end));
            }

            foreach (var code in document.Descendants<CodeInline>())
            {
                regions._inlineCode.Add((code.Span.Start, code.Span.End));
            }

            foreach (var table in document.Descendants<Table>())
            {
                var start = lineStarts[table.Line];
                var end = LineEnd(markdown, lineStarts, LineOf(lineStarts, table.Span.End));
                regions._tables.Add((start, end));
            }
        }

        /// <summary>
        /// Whether the character at <paramref name="offset"/> is shown as code: inside a code block
        /// (the opening line of a fenced block excluded) or inside inline code.
        /// </summary>
        public bool IsCode(int offset)
        {
            EnsureParsed();
            return _codeBlocks.Any(b => offset >= b.ContentStart && offset <= b.End)
                || _inlineCode.Any(c => offset >= c.Start && offset <= c.End);
        }

        /// <summary>Whether the character at <paramref name="offset"/> belongs to a table.</summary>
        public bool IsInTable(int offset)
        {
            EnsureParsed();
            return _tables.Any(t => offset >= t.Start && offset <= t.End);
        }

        private static int[] LineStarts(string text)
        {
            var starts = new List<int> { 0 };
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') starts.Add(i + 1);
            }
            return starts.ToArray();
        }

        private static int LineOf(int[] lineStarts, int offset)
        {
            var index = Array.BinarySearch(lineStarts, offset);
            return index >= 0 ? index : ~index - 1;
        }

        /// <summary>Offset of the last character of the line, its line ending included.</summary>
        private static int LineEnd(string text, int[] lineStarts, int line)
            => line + 1 < lineStarts.Length ? lineStarts[line + 1] - 1 : text.Length - 1;
    }
}
