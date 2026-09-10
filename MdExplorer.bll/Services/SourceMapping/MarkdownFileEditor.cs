using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.SourceMapping
{
    /// <summary>Where a block goes relative to its anchor block.</summary>
    public enum BlockPosition
    {
        Before,
        After
    }

    public enum MarkdownEditStatus
    {
        Applied,

        /// <summary>The lines at the anchor are no longer what the caller saw: nothing was changed.</summary>
        Conflict,

        /// <summary>The line range does not exist in the file: nothing was changed.</summary>
        InvalidRange
    }

    /// <summary>
    /// The outcome of an edit. Only <see cref="MarkdownEditStatus.Applied"/> carries new content;
    /// the other two say why nothing was written.
    /// </summary>
    public sealed class MarkdownEdit
    {
        private MarkdownEdit() { }

        public MarkdownEditStatus Status { get; private set; }

        /// <summary>The whole new file content, with the file's own line ending.</summary>
        public string NewContent { get; private set; }

        /// <summary>
        /// 1-based lines the new text occupies in <see cref="NewContent"/>. When an edit only
        /// deleted lines, <see cref="LastLine"/> is <see cref="FirstLine"/> - 1.
        /// </summary>
        public int FirstLine { get; private set; }
        public int LastLine { get; private set; }

        /// <summary>For <see cref="MarkdownEditStatus.Conflict"/>: what the lines contain now.</summary>
        public string CurrentFragment { get; private set; }

        /// <summary>For <see cref="MarkdownEditStatus.InvalidRange"/>.</summary>
        public string Error { get; private set; }

        internal static MarkdownEdit Applied(string content, int firstLine, int lastLine)
            => new MarkdownEdit { Status = MarkdownEditStatus.Applied, NewContent = content, FirstLine = firstLine, LastLine = lastLine };

        internal static MarkdownEdit Conflict(string currentFragment)
            => new MarkdownEdit { Status = MarkdownEditStatus.Conflict, CurrentFragment = currentFragment };

        internal static MarkdownEdit Invalid(string error)
            => new MarkdownEdit { Status = MarkdownEditStatus.InvalidRange, Error = error };
    }

    /// <summary>
    /// Surgical edits of a markdown file by line number — the write side of
    /// <see cref="MarkdownSourceMapService"/>, whose <c>data-mde-line-start/end</c> attributes
    /// give the lines.
    ///
    /// <para>
    /// Extracted from <c>AiSelectionController.Replace</c> ("Usa AI"), which already did it right:
    /// the lines are re-checked against what the caller saw and the edit is refused if they
    /// changed; the file's line ending is kept; the BOM is kept. Pasting an image at a point of
    /// the document needs exactly the same guarantees, and a second copy would drift from the
    /// first at the first fix made to only one of them.
    /// </para>
    ///
    /// <para>
    /// The text-in, text-out functions are pure: no disk, testable on strings. The file system
    /// watcher and the SignalR notification stay with the callers, where they were.
    /// </para>
    ///
    /// <para>
    /// Line semantics are the source map's: 1-based, and the text is split on <c>\n</c>, so a
    /// file ending with a newline has one more, empty, "line" at the end. Kept byte for byte from
    /// the original — changing it would shift every line number "Usa AI" works with.
    /// </para>
    /// </summary>
    public static class MarkdownFileEditor
    {
        /// <summary>Splits on <c>\n</c> and drops the <c>\r</c> of CRLF endings.</summary>
        public static string[] SplitLines(string text)
        {
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length > 0 && line[line.Length - 1] == '\r')
                {
                    lines[i] = line.Substring(0, line.Length - 1);
                }
            }
            return lines;
        }

        /// <summary>
        /// The file's line ending. One CRLF is enough to make it CRLF: a rewritten mixed file
        /// comes out all CRLF, as it always did.
        /// </summary>
        public static string LineEnding(string text) => text.Contains("\r\n") ? "\r\n" : "\n";

        public static bool IsValidRange(string[] lines, int startLine, int endLine)
            => startLine >= 1 && endLine >= startLine && endLine <= lines.Length;

        public static string InvalidRangeMessage(int startLine, int endLine, int totalLines)
            => $"Invalid line range {startLine}-{endLine}: the file has {totalLines} lines";

        /// <summary>The lines, joined with <c>\n</c> whatever the file's line ending.</summary>
        public static string Fragment(string[] lines, int startLine, int endLine)
            => string.Join("\n", lines.Skip(startLine - 1).Take(endLine - startLine + 1));

        /// <summary>
        /// Replaces lines <paramref name="startLine"/>..<paramref name="endLine"/> with
        /// <paramref name="newText"/>, if they still read <paramref name="expectedOriginalText"/>.
        /// An empty <paramref name="newText"/> deletes the lines — it does not leave one empty line.
        /// </summary>
        public static MarkdownEdit ReplaceLines(string text, int startLine, int endLine, string expectedOriginalText, string newText)
        {
            var lines = SplitLines(text);
            if (!IsValidRange(lines, startLine, endLine))
            {
                return MarkdownEdit.Invalid(InvalidRangeMessage(startLine, endLine, lines.Length));
            }

            var current = Fragment(lines, startLine, endLine);
            if (!string.Equals(current, expectedOriginalText.Replace("\r\n", "\n"), StringComparison.Ordinal))
            {
                return MarkdownEdit.Conflict(current);
            }

            var newLines = newText.Length == 0
                ? Array.Empty<string>()
                : newText.Replace("\r\n", "\n").Split('\n');

            var result = lines.Take(startLine - 1)
                .Concat(newLines)
                .Concat(lines.Skip(endLine))
                .ToArray();

            return MarkdownEdit.Applied(Join(text, result), startLine, startLine + newLines.Length - 1);
        }

        /// <summary>
        /// Inserts <paramref name="block"/> right before or right after the anchor block
        /// (lines <paramref name="anchorStartLine"/>..<paramref name="anchorEndLine"/>), if the
        /// anchor still reads <paramref name="expectedAnchorText"/>.
        ///
        /// <para>
        /// The block is kept apart from its neighbours by one empty line on each side, adding one
        /// only where there is none: glued to a paragraph it would become part of it, and right
        /// under a table or a list it could be read as their continuation.
        /// </para>
        /// </summary>
        public static MarkdownEdit InsertBlock(
            string text,
            int anchorStartLine,
            int anchorEndLine,
            BlockPosition position,
            string expectedAnchorText,
            string block)
        {
            var lines = SplitLines(text);
            if (!IsValidRange(lines, anchorStartLine, anchorEndLine))
            {
                return MarkdownEdit.Invalid(InvalidRangeMessage(anchorStartLine, anchorEndLine, lines.Length));
            }

            var current = Fragment(lines, anchorStartLine, anchorEndLine);
            if (!string.Equals(current, (expectedAnchorText ?? string.Empty).Replace("\r\n", "\n"), StringComparison.Ordinal))
            {
                return MarkdownEdit.Conflict(current);
            }

            var insertAt = position == BlockPosition.Before ? anchorStartLine - 1 : anchorEndLine;
            return InsertAt(text, lines, insertAt, block);
        }

        /// <summary>
        /// Appends <paramref name="block"/> after the last line with content, one empty line apart.
        /// Trailing empty lines stay at the end of the file, after the block.
        /// </summary>
        public static MarkdownEdit AppendBlock(string text, string block)
        {
            var lines = SplitLines(text);
            var lastContent = Array.FindLastIndex(lines, line => !IsBlank(line));
            return InsertAt(text, lines, lastContent + 1, block);
        }

        public static bool HasUtf8Bom(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            Span<byte> bom = stackalloc byte[3];
            return stream.Read(bom) == 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
        }

        /// <summary>Writes with or without the BOM, as the file had it.</summary>
        public static Task WriteAsync(string filePath, string content, bool withUtf8Bom)
            => File.WriteAllTextAsync(filePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: withUtf8Bom));

        /// <param name="insertAt">0-based index in <paramref name="lines"/> the block goes before.</param>
        private static MarkdownEdit InsertAt(string text, string[] lines, int insertAt, string block)
        {
            var blockLines = TrimBlankEdges(SplitLines(block ?? string.Empty));
            if (blockLines.Length == 0)
            {
                throw new ArgumentException("The block to insert is empty.", nameof(block));
            }

            var result = new List<string>(lines.Length + blockLines.Length + 2);
            result.AddRange(lines.Take(insertAt));
            if (result.Count > 0 && !IsBlank(result[result.Count - 1]))
            {
                result.Add(string.Empty);
            }

            var firstLine = result.Count + 1;
            result.AddRange(blockLines);
            var lastLine = result.Count;

            var after = lines.Skip(insertAt).ToArray();
            if (after.Length > 0 && !IsBlank(after[0]))
            {
                result.Add(string.Empty);
            }
            result.AddRange(after);

            return MarkdownEdit.Applied(Join(text, result.ToArray()), firstLine, lastLine);
        }

        /// <summary>
        /// Joins with the original file's line ending and keeps its final newline, exactly as
        /// "Usa AI" has always done.
        /// </summary>
        private static string Join(string originalText, string[] resultLines)
        {
            var eol = LineEnding(originalText);
            var newContent = string.Join(eol, resultLines);
            if (originalText.EndsWith("\n") && !newContent.EndsWith(eol))
            {
                newContent += eol;
            }
            return newContent;
        }

        private static string[] TrimBlankEdges(string[] lines)
        {
            var first = Array.FindIndex(lines, line => !IsBlank(line));
            if (first < 0) return Array.Empty<string>();
            var last = Array.FindLastIndex(lines, line => !IsBlank(line));
            return lines.Skip(first).Take(last - first + 1).ToArray();
        }

        private static bool IsBlank(string line) => string.IsNullOrWhiteSpace(line);
    }
}
