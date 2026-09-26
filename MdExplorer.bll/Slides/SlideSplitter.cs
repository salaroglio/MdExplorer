using MdExplorer.Features.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// One slide as written: its markdown, its speaker notes (null when it has none), and the
    /// 0-based line of the split text its markdown starts on.
    /// </summary>
    public sealed record SlideSource(string Markdown, string Notes, int FirstLine = 0);

    /// <summary>
    /// Cuts the body of a slide deck into slides, with the separators of reveal.js's markdown
    /// plugin:
    /// <list type="bullet">
    /// <item>a line <c>---</c> starts the next slide (reveal.js's <c>\r?\n---\r?\n</c>, so also right
    /// under a line of text: in a slide deck <c>Title</c> + <c>---</c> is a separator, not a setext
    /// heading);</item>
    /// <item>a line <c>--</c> starts the next slide of the same vertical stack;</item>
    /// <item>a line starting with <c>Note:</c> or <c>Notes:</c> turns the rest of the slide into
    /// speaker notes.</item>
    /// </list>
    /// None of them counts inside code: a <c>---</c> in a code block is the code's.
    /// </summary>
    public static class SlideSplitter
    {
        private static readonly Regex NotesLine = new(@"^notes?:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>The horizontal slides, each one the stack of its vertical slides (one slide for a plain one).</summary>
        public static IReadOnlyList<IReadOnlyList<SlideSource>> Split(string body)
        {
            body ??= string.Empty;
            var regions = MarkdownCodeRegions.Of(body);
            var stacks = new List<IReadOnlyList<SlideSource>>();
            var stack = new List<SlideSource>();
            var slideStart = 0;
            int? notesStart = null;
            var notesTextStart = 0;

            int LineOf(int offset)
            {
                var line = 0;
                for (var i = 0; i < offset && i < body.Length; i++)
                {
                    if (body[i] == '\n') line++;
                }
                return line;
            }

            void CloseSlide(int end)
            {
                var firstLine = LineOf(slideStart);
                stack.Add(notesStart is int n
                    ? new SlideSource(body.Substring(slideStart, n - slideStart), body.Substring(notesTextStart, end - notesTextStart).Trim(), firstLine)
                    : new SlideSource(body.Substring(slideStart, end - slideStart), null, firstLine));
                notesStart = null;
            }

            var lineStart = 0;
            while (lineStart <= body.Length)
            {
                var newline = body.IndexOf('\n', lineStart);
                var lineEnd = newline < 0 ? body.Length : newline;
                var line = body.Substring(lineStart, lineEnd - lineStart).TrimEnd('\r', ' ', '\t');
                var next = newline < 0 ? body.Length + 1 : newline + 1;

                if (line is "---" or "--" && !regions.IsCode(lineStart))
                {
                    CloseSlide(lineStart);
                    if (line == "---")
                    {
                        stacks.Add(stack);
                        stack = new List<SlideSource>();
                    }
                    slideStart = Math.Min(next, body.Length);
                }
                else if (notesStart == null && NotesLine.IsMatch(line) && !regions.IsCode(lineStart))
                {
                    notesStart = lineStart;
                    notesTextStart = lineStart + line.IndexOf(':') + 1;
                }
                lineStart = next;
            }
            CloseSlide(body.Length);
            stacks.Add(stack);
            return stacks;
        }
    }
}
