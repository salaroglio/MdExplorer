using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.JiraLinks;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdExplorer.Features.Services.SourceMapping
{
    /// <summary>
    /// One piece of what the page shows inside the block being corrected, read from the DOM:
    /// a text node, or an element with no text of its own (an image, a line break, a checkbox).
    /// </summary>
    public sealed class RenderedRun
    {
        /// <summary>The text of a DOM text node; null for an object.</summary>
        public string Text { get; set; }

        /// <summary>Tag of an element with no text of its own (<c>img</c>, <c>br</c>, <c>input</c>); null for text.</summary>
        public string Object { get; set; }

        /// <summary>
        /// Tags of the elements between the block and this node, outermost first. Only formatting
        /// tags count (<c>strong</c>, <c>em</c>, <c>a</c>, <c>code</c>, …); others are ignored.
        /// </summary>
        public string[] Path { get; set; }
    }

    /// <summary>Which block of the page is corrected.</summary>
    public sealed class RenderedTextTarget
    {
        /// <summary>1-based <c>data-mde-line-start</c> of the block — of the table, for a cell.</summary>
        public int Line { get; set; }

        /// <summary>For a cell: 0-based row, the header row being 0 (the separator line is not a row).</summary>
        public int? Row { get; set; }

        /// <summary>For a cell: 0-based column.</summary>
        public int? Column { get; set; }
    }

    public enum RenderedTextEditStatus
    {
        Applied,

        /// <summary>The text after is the text before: nothing to write.</summary>
        NoChange,

        /// <summary>The correction cannot be made safely: nothing was changed, <see cref="RenderedTextEdit.Refusal"/> says why.</summary>
        Refused
    }

    public enum RenderedTextRefusal
    {
        None,

        /// <summary>No paragraph, heading or cell with text starts where the page says.</summary>
        BlockNotFound,

        /// <summary>The block holds something this editor does not know how the page shows (math, footnotes, …).</summary>
        UnsupportedContent,

        /// <summary>What the page showed is not what the file renders: page out of date, or rendered differently by MDE.</summary>
        RenderedTextMismatch,

        /// <summary>A line break would be added or removed.</summary>
        LineBreakNotAllowed,

        /// <summary>An emoji, image, entity, checkbox, the address of a link or unmapped text would be changed.</summary>
        ProtectedContentTouched,

        /// <summary>Text was given a formatting it has nowhere around it.</summary>
        FormattingNotAllowed,

        EditTooLarge,

        /// <summary>The corrected file would not render as what was typed, or its structure would change.</summary>
        VerificationFailed,

        /// <summary>All the text was deleted, but the block cannot go away with its lines safely (a list item with sub-items, a setext heading, a block in a quote…).</summary>
        BlockDeletionNotAllowed
    }

    public sealed class RenderedTextEdit
    {
        private RenderedTextEdit() { }

        public RenderedTextEditStatus Status { get; private set; }

        /// <summary>The whole new file content; only for <see cref="RenderedTextEditStatus.Applied"/>.</summary>
        public string NewContent { get; private set; }

        public RenderedTextRefusal Refusal { get; private set; }

        /// <summary>For <see cref="RenderedTextEditStatus.Refused"/>: what exactly stopped the correction.</summary>
        public string Detail { get; private set; }

        /// <summary>All the text was deleted and the block went away with its lines (a list item with its bullet).</summary>
        public bool BlockDeleted { get; private set; }

        internal static RenderedTextEdit Applied(string content)
            => new RenderedTextEdit { Status = RenderedTextEditStatus.Applied, NewContent = content };

        internal static RenderedTextEdit Deleted(string content)
            => new RenderedTextEdit { Status = RenderedTextEditStatus.Applied, NewContent = content, BlockDeleted = true };

        internal static RenderedTextEdit NoChange()
            => new RenderedTextEdit { Status = RenderedTextEditStatus.NoChange };

        internal static RenderedTextEdit Refused(RenderedTextRefusal refusal, string detail)
            => new RenderedTextEdit { Status = RenderedTextEditStatus.Refused, Refusal = refusal, Detail = detail };
    }

    /// <summary>
    /// Corrections of the text of a block made on the rendered page, turned into edits of exactly
    /// the characters touched in the markdown file.
    ///
    /// <para>
    /// The page never sends HTML. It sends the block's runs (text nodes with their formatting
    /// path) before and after the correction. The file is parsed with Markdig
    /// (<c>UsePreciseSourceLocation</c>): every piece of text has its exact position in the file.
    /// The runs before must read exactly as the file renders; the difference between before and
    /// after is applied, by position, to the pieces it falls on. The formatting path of the typed
    /// characters decides whether they go inside or outside a bold or a link — what the page shows.
    /// </para>
    ///
    /// <para>
    /// Only text changes. The one structural change allowed (user decision, 14/09/2026): a bold,
    /// italic, link or inline code whose text is deleted entirely disappears with its markers.
    /// </para>
    ///
    /// <para>
    /// Before anything is returned the new file is parsed again: same blocks, and the corrected
    /// block renders exactly the runs after. Otherwise the correction is refused, never approximated.
    /// Pure, like <see cref="MarkdownFileEditor"/>: text in, text out.
    /// </para>
    /// </summary>
    public static class RenderedTextEditor
    {
        private static readonly HashSet<string> FormattingTags = new HashSet<string>(StringComparer.Ordinal)
        {
            "strong", "em", "a", "code", "del", "sub", "sup", "ins", "mark"
        };

        // The diff is quadratic on the changed middle; a correction is a few characters.
        private const long MaxDiffCells = 4_000_000;

        private static readonly Regex EntityAhead = new Regex(@"^&(#[0-9]+|#[xX][0-9a-fA-F]+|[A-Za-z][A-Za-z0-9]*);", RegexOptions.Compiled);

        /// <param name="text">The file content, read as the page was rendered from it.</param>
        /// <param name="pipeline">The page's pipeline (<see cref="DocumentViewPipeline"/>): the file must be read as the page was rendered.</param>
        public static RenderedTextEdit Apply(
            string text,
            MarkdownPipeline pipeline,
            RenderedTextTarget target,
            IReadOnlyList<RenderedRun> before,
            IReadOnlyList<RenderedRun> after)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (after == null) throw new ArgumentNullException(nameof(after));

            var pageBefore = Trim(PageSymbols(before));
            var pageAfter = Trim(PageSymbols(after));
            if (SameSequence(pageBefore, pageAfter))
            {
                return RenderedTextEdit.NoChange();
            }

            var document = Markdown.Parse(text, pipeline);
            var leaf = FindLeaf(document, text, target, out var notFound);
            if (leaf == null)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.BlockNotFound, notFound);
            }

            List<Symbol> source;
            try
            {
                source = Trim(SourceSymbols(text, leaf));
            }
            catch (UnsupportedInlineException ex)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.UnsupportedContent, ex.Message);
            }
            if (source.Count == 0)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.BlockNotFound, $"The block at line {target.Line} has no text to correct.");
            }

            var map = Align(source, pageBefore);
            if (map == null)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.RenderedTextMismatch,
                    $"The text the page shows for the block at line {target.Line} is not what the file renders.");
            }

            // Everything deleted: the block goes away with its lines — a list item with its bullet
            // (user's request, 25/09/2026). A cell stays, empty: a table cannot lose half a row.
            if (pageAfter.Count == 0 && !target.Row.HasValue)
            {
                return DeleteBlock(document, text, pipeline, leaf);
            }

            var ops = Diff(pageBefore, pageAfter);
            if (ops == null)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.EditTooLarge, "The correction changes too much of the block at once.");
            }

            var edits = new List<SourceEdit>();
            var refused = Translate(text, source, map, pageBefore.Count, ops, edits);
            if (refused != null)
            {
                return refused;
            }

            var newText = ApplyEdits(text, edits);
            return Verify(document, newText, pipeline, target, pageAfter);
        }

        // ── Symbols: one per character or object, on both sides ────────────────────────────

        private enum SymbolKind
        {
            Char,
            Object
        }

        private sealed class Container
        {
            public Container(Inline node, string tag, bool isSealed)
            {
                Node = node;
                Tag = tag;
                Sealed = isSealed;
            }

            public Inline Node { get; }
            public string Tag { get; }

            /// <summary>Its text is not free text: an autolink or a Jira key (it is the address), unmapped code.</summary>
            public bool Sealed { get; }
        }

        private sealed class Symbol
        {
            public SymbolKind Kind;
            public char Char;
            public string Tag;
            public string[] Path;
            public string PathKey;

            // Source side only.
            public Container[] Containers = Array.Empty<Container>();
            public Inline Owner;
            public bool Editable;
            public int Start;
            public int End;
            public bool FirstOfOwner;
            public bool LastOfOwner;
        }

        private sealed class UnsupportedInlineException : Exception
        {
            public UnsupportedInlineException(string message) : base(message) { }
        }

        private static List<Symbol> PageSymbols(IReadOnlyList<RenderedRun> runs)
        {
            var symbols = new List<Symbol>();
            foreach (var run in runs)
            {
                var path = (run.Path ?? Array.Empty<string>())
                    .Select(tag => tag.ToLowerInvariant())
                    .Where(FormattingTags.Contains)
                    .ToArray();
                var pathKey = string.Join("/", path);

                if (run.Object != null)
                {
                    symbols.Add(new Symbol { Kind = SymbolKind.Object, Tag = run.Object.ToLowerInvariant(), Path = path, PathKey = pathKey });
                    continue;
                }

                // EditH1 re-serialises the page with CRLF: the DOM line break is the \n.
                foreach (var c in (run.Text ?? string.Empty).Replace("\r", string.Empty))
                {
                    symbols.Add(new Symbol { Kind = SymbolKind.Char, Char = c, Path = path, PathKey = pathKey });
                }
            }
            return symbols;
        }

        private static List<Symbol> SourceSymbols(string text, LeafBlock leaf)
        {
            var symbols = new List<Symbol>();
            if (leaf.Inline != null)
            {
                Walk(text, leaf.Inline, new List<Container>(), symbols);
            }
            for (var i = 0; i < symbols.Count; i++)
            {
                symbols[i].FirstOfOwner = i == 0 || symbols[i - 1].Owner != symbols[i].Owner;
                symbols[i].LastOfOwner = i == symbols.Count - 1 || symbols[i + 1].Owner != symbols[i].Owner;
            }
            return symbols;
        }

        private static void Walk(string text, ContainerInline container, List<Container> stack, List<Symbol> symbols)
        {
            foreach (var inline in container)
            {
                switch (inline)
                {
                    case EmojiInline emoji: // before LiteralInline: an emoji is one
                        // The page shows the character: no page command turns :name: into a PNG
                        // (FromEmojiToPng is not an ICommandHtml). Protected text.
                        foreach (var c in emoji.Content.ToString())
                        {
                            AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Char, Char = c, Owner = emoji });
                        }
                        break;

                    case LiteralInline literal:
                        AddLiteral(text, literal, stack, symbols);
                        break;

                    case CodeInline code:
                        AddCode(text, code, stack, symbols);
                        break;

                    case LinkInline link when link.IsImage:
                        AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Object, Tag = "img", Owner = link });
                        break;

                    case LinkInline link:
                        stack.Add(new Container(link, "a", link.IsAutoLink || link is JiraLink));
                        Walk(text, link, stack, symbols);
                        stack.RemoveAt(stack.Count - 1);
                        break;

                    case EmphasisInline emphasis:
                        var tag = EmphasisTag(emphasis)
                            ?? throw new UnsupportedInlineException($"Unsupported emphasis \"{new string(emphasis.DelimiterChar, emphasis.DelimiterCount)}\".");
                        stack.Add(new Container(emphasis, tag, false));
                        Walk(text, emphasis, stack, symbols);
                        stack.RemoveAt(stack.Count - 1);
                        break;

                    case HtmlEntityInline entity:
                        foreach (var c in entity.Transcoded.ToString())
                        {
                            AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Char, Char = c, Owner = entity });
                        }
                        break;

                    case LineBreakInline lineBreak:
                        // Markdig writes "<br />\n" for a hard break, "\n" for a soft one.
                        if (lineBreak.IsHard)
                        {
                            AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Object, Tag = "br", Owner = lineBreak });
                        }
                        AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Char, Char = '\n', Owner = lineBreak });
                        break;

                    case HtmlInline _:
                        // A tag: nothing visible of its own.
                        break;

                    case AutolinkInline autolink:
                        stack.Add(new Container(autolink, "a", true));
                        foreach (var c in autolink.Url)
                        {
                            AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Char, Char = c, Owner = autolink });
                        }
                        stack.RemoveAt(stack.Count - 1);
                        break;

                    case TaskList _:
                        AddSymbol(symbols, stack, new Symbol { Kind = SymbolKind.Object, Tag = "input", Owner = inline });
                        break;

                    default:
                        throw new UnsupportedInlineException($"The block contains {inline.GetType().Name}, which cannot be corrected on the page.");
                }
            }
        }

        private static void AddSymbol(List<Symbol> symbols, List<Container> stack, Symbol symbol)
        {
            symbol.Containers = stack.ToArray();
            symbol.Path = stack.Select(c => c.Tag).ToArray();
            symbol.PathKey = string.Join("/", symbol.Path);
            symbols.Add(symbol);
        }

        private static void AddLiteral(string text, LiteralInline literal, List<Container> stack, List<Symbol> symbols)
        {
            var content = literal.Content.ToString();
            if (content.Length == 0)
            {
                return;
            }
            // Markdig 1.1.2 always gives inline positions (measured 14/09/2026, with and without
            // UsePreciseSourceLocation). Should a version stop doing it, fail loudly: without
            // positions nothing can be corrected in place.
            if (literal.Span.IsEmpty)
            {
                throw new InvalidOperationException($"Markdig gave no position in the file to the text \"{content}\": corrections on the page need inline source positions.");
            }

            var starts = new int[content.Length];
            var ends = new int[content.Length];
            var mapped = TryMapLiteral(text, literal.Span.Start, literal.Span.End + 1, content, starts, ends);
            for (var j = 0; j < content.Length; j++)
            {
                AddSymbol(symbols, stack, new Symbol
                {
                    Kind = SymbolKind.Char,
                    Char = content[j],
                    Owner = literal,
                    Editable = mapped,
                    Start = mapped ? starts[j] : 0,
                    End = mapped ? ends[j] : 0
                });
            }
        }

        /// <summary>
        /// Each character of a literal's content to its place in the file. The span may hold
        /// whitespace Markdig trimmed from the content (table cells) and backslash escapes
        /// (<c>\|</c> shows <c>|</c>); anything else leaves the literal unmapped.
        /// </summary>
        private static bool TryMapLiteral(string text, int spanStart, int spanEnd, string content, int[] starts, int[] ends)
        {
            if (spanStart < 0 || spanEnd > text.Length)
            {
                return false;
            }
            for (var lead = 0; spanStart + lead <= spanEnd; lead++)
            {
                if (lead > 0 && !char.IsWhiteSpace(text[spanStart + lead - 1]))
                {
                    return false;
                }
                if (TryWalkLiteral(text, spanStart + lead, spanEnd, content, starts, ends))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryWalkLiteral(string text, int i, int spanEnd, string content, int[] starts, int[] ends)
        {
            for (var j = 0; j < content.Length; j++)
            {
                if (i >= spanEnd)
                {
                    return false;
                }
                if (text[i] == content[j])
                {
                    starts[j] = i;
                    ends[j] = i + 1;
                    i++;
                }
                else if (text[i] == '\\' && i + 1 < spanEnd && text[i + 1] == content[j] && IsAsciiPunctuation(content[j]))
                {
                    starts[j] = i;
                    ends[j] = i + 2;
                    i += 2;
                }
                else
                {
                    return false;
                }
            }
            for (; i < spanEnd; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static void AddCode(string text, CodeInline code, List<Container> stack, List<Symbol> symbols)
        {
            var content = code.Content ?? string.Empty;
            var offset = -1;
            var innerStart = code.Span.Start + code.DelimiterCount;
            var innerEnd = code.Span.End + 1 - code.DelimiterCount;
            if (!code.Span.IsEmpty && innerStart >= 0 && innerEnd <= text.Length && innerEnd - innerStart >= content.Length)
            {
                var inner = text.Substring(innerStart, innerEnd - innerStart);
                if (inner == content)
                {
                    offset = innerStart;
                }
                // CommonMark strips one space on each side: `` a ` b `` shows "a ` b".
                else if (inner.Length == content.Length + 2 && inner[0] == ' ' && inner[inner.Length - 1] == ' '
                         && string.CompareOrdinal(inner, 1, content, 0, content.Length) == 0)
                {
                    offset = innerStart + 1;
                }
            }

            stack.Add(new Container(code, "code", offset < 0));
            for (var j = 0; j < content.Length; j++)
            {
                AddSymbol(symbols, stack, new Symbol
                {
                    Kind = SymbolKind.Char,
                    Char = content[j],
                    Owner = code,
                    Editable = offset >= 0,
                    Start = offset + j,
                    End = offset + j + 1
                });
            }
            stack.RemoveAt(stack.Count - 1);
        }

        private static string EmphasisTag(EmphasisInline emphasis)
        {
            switch (emphasis.DelimiterChar)
            {
                case '*':
                case '_':
                    return emphasis.DelimiterCount == 2 ? "strong" : emphasis.DelimiterCount == 1 ? "em" : null;
                case '~':
                    return emphasis.DelimiterCount == 2 ? "del" : emphasis.DelimiterCount == 1 ? "sub" : null;
                case '^':
                    return emphasis.DelimiterCount == 1 ? "sup" : null;
                case '+':
                    return emphasis.DelimiterCount == 2 ? "ins" : null;
                case '=':
                    return emphasis.DelimiterCount == 2 ? "mark" : null;
                default:
                    return null;
            }
        }

        /// <summary>Whitespace at the edges of a block is layout (cell padding, EditH1's indentation), not text.</summary>
        private static List<Symbol> Trim(List<Symbol> symbols)
        {
            var first = symbols.FindIndex(s => !IsWhitespace(s));
            if (first < 0)
            {
                return new List<Symbol>();
            }
            var last = symbols.FindLastIndex(s => !IsWhitespace(s));
            return symbols.GetRange(first, last - first + 1);
        }

        private static bool IsWhitespace(Symbol s) => s.Kind == SymbolKind.Char && char.IsWhiteSpace(s.Char);

        private static bool Same(Symbol a, Symbol b)
        {
            if (a.Kind != b.Kind || a.PathKey != b.PathKey)
            {
                return false;
            }
            return a.Kind == SymbolKind.Char ? a.Char == b.Char : a.Tag == b.Tag;
        }

        private static bool SameSequence(List<Symbol> a, List<Symbol> b)
            => a.Count == b.Count && a.Zip(b, Same).All(same => same);

        // ── The block ───────────────────────────────────────────────────────────────────────

        private static LeafBlock FindLeaf(MarkdownDocument document, string text, RenderedTextTarget target, out string notFound)
        {
            notFound = null;
            if (target.Row.HasValue != target.Column.HasValue)
            {
                notFound = "A cell needs both its row and its column.";
                return null;
            }

            if (target.Row.HasValue)
            {
                var table = document.Descendants().OfType<Table>().FirstOrDefault(t => t.Line + 1 == target.Line);
                if (table == null)
                {
                    notFound = $"No table starts at line {target.Line}.";
                    return null;
                }
                if (target.Row.Value < 0 || target.Row.Value >= table.Count || !(table[target.Row.Value] is TableRow row))
                {
                    notFound = $"The table at line {target.Line} has no row {target.Row.Value}.";
                    return null;
                }
                if (target.Column.Value < 0 || target.Column.Value >= row.Count || !(row[target.Column.Value] is TableCell cell))
                {
                    notFound = $"Row {target.Row.Value} of the table at line {target.Line} has no column {target.Column.Value}.";
                    return null;
                }
                var cellLeaves = cell.OfType<LeafBlock>().ToList();
                if (cellLeaves.Count != 1)
                {
                    notFound = cellLeaves.Count == 0 ? EmptyCell : "The cell holds more than one block.";
                    return null;
                }
                return cellLeaves[0];
            }

            // Where the block's text starts, as the page's data-mde-line-start says (a setext heading
            // starts on its text, not on its underline: MarkdownSourceMapService.StartLine).
            var lineStarts = MarkdownSourceMapService.BuildLineStartOffsets(text);
            var leaves = document.Descendants().OfType<LeafBlock>()
                .Where(b => (b is ParagraphBlock || b is HeadingBlock)
                            && MarkdownSourceMapService.StartLine(b, lineStarts) + 1 == target.Line
                            && !(b.Parent is TableCell))
                .ToList();
            if (leaves.Count != 1)
            {
                notFound = leaves.Count == 0
                    ? $"No paragraph or heading starts at line {target.Line}."
                    : $"More than one block starts at line {target.Line}.";
                return null;
            }
            return leaves[0];
        }

        private const string EmptyCell = "The cell is empty: there is no text to correct.";

        /// <summary>
        /// Blocks and where they start: a correction changes text, never this. What is inside a
        /// cell does not count — a cleared cell is still the same cell — nor the line of a cell:
        /// Markdig gives an empty cell line 0 (measured 14/09/2026). The row keeps its cell count.
        /// </summary>
        // ── Deleting a whole block ──────────────────────────────────────────────────────────

        /// <summary>
        /// Removes the lines of the block whose text was all deleted: a list item holding only that text
        /// (with its bullet), or a paragraph or ATX heading of the document. Anything else is refused. The
        /// lines must hold that block and nothing else, and the new file must have exactly the blocks of
        /// the old one minus this one — same lists, same looseness, same text: if removing the lines would
        /// join two lists or change how a list is laid out, nothing is written.
        /// </summary>
        private static RenderedTextEdit DeleteBlock(MarkdownDocument document, string text, MarkdownPipeline pipeline, LeafBlock leaf)
        {
            Block removed;
            if (leaf is ParagraphBlock && leaf.Parent is ListItemBlock item)
            {
                if (item.Count != 1)
                    return RenderedTextEdit.Refused(RenderedTextRefusal.BlockDeletionNotAllowed, "The list item holds more than its text (sub-items or more paragraphs).");
                removed = item;
            }
            else if ((leaf is ParagraphBlock || leaf is HeadingBlock) && leaf.Parent is MarkdownDocument)
            {
                if (leaf is HeadingBlock heading && heading.IsSetext)
                    return RenderedTextEdit.Refused(RenderedTextRefusal.BlockDeletionNotAllowed, "A setext heading: its underline may be a slide separator.");
                removed = leaf;
            }
            else
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.BlockDeletionNotAllowed, "Only a list item, a paragraph or a heading of the document goes away with its lines.");
            }

            var lineStarts = MarkdownSourceMapService.BuildLineStartOffsets(text);
            int LineOf(int offset)
            {
                var line = Array.BinarySearch(lineStarts, offset);
                return line >= 0 ? line : ~line - 1;
            }
            int LineEnd(int line) => line + 1 < lineStarts.Length ? lineStarts[line + 1] : text.Length;

            var firstLine = LineOf(removed.Span.Start);
            var lastLine = LineOf(removed.Span.End);
            // The lines must hold this block and nothing else.
            if (text.Substring(lineStarts[firstLine], removed.Span.Start - lineStarts[firstLine]).Trim().Length > 0
                || text.Substring(removed.Span.End + 1, LineEnd(lastLine) - removed.Span.End - 1).Trim().Length > 0)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.BlockDeletionNotAllowed, "Its lines hold something else too.");
            }

            var start = lineStarts[firstLine];
            var end = LineEnd(lastLine);
            // A paragraph or heading takes one blank line with it, so the blocks around keep one between them.
            if (!(removed is ListItemBlock) && lastLine + 1 < lineStarts.Length && text.Substring(end, LineEnd(lastLine + 1) - end).Trim().Length == 0)
            {
                end = LineEnd(lastLine + 1);
            }
            var newText = text.Remove(start, end - start);

            var expected = DeletionSignature(document, text, removed);
            var actual = DeletionSignature(Markdown.Parse(newText, pipeline), newText, null);
            if (!expected.SequenceEqual(actual))
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.BlockDeletionNotAllowed,
                    "Removing the lines would change the other blocks (two lists joined, a list laid out differently…).");
            }
            return RenderedTextEdit.Deleted(newText);
        }

        /// <summary>
        /// The blocks of a document without their lines (which move when lines go away): kind, children,
        /// list style and looseness, text. <paramref name="removed"/> and what is inside it are left out,
        /// and its parent counts one child less (and is left out when it has none left).
        /// </summary>
        private static List<string> DeletionSignature(MarkdownDocument document, string text, Block removed)
        {
            var signature = new List<string>();
            foreach (var block in document.Descendants().OfType<Block>())
            {
                if (removed != null && (block == removed || IsInside(block, removed))) continue;
                var count = block is ContainerBlock container ? container.Count - (removed != null && removed.Parent == block ? 1 : 0) : 0;
                if (block is ContainerBlock && count == 0 && removed != null && removed.Parent == block) continue;
                var entry = block.GetType().Name;
                if (block is ContainerBlock) entry += "#" + count;
                if (block is ListBlock list) entry += (list.IsOrdered ? " ordered" : " bullet") + (list.IsLoose ? " loose" : " tight");
                if (block is LeafBlock && block.Span.Length > 0) entry += ":" + text.Substring(block.Span.Start, block.Span.Length).Trim();
                signature.Add(entry);
            }
            return signature;
        }

        private static bool IsInside(Block block, Block container)
        {
            for (var parent = block.Parent; parent != null; parent = parent.Parent)
            {
                if (parent == container) return true;
            }
            return false;
        }

        private static List<string> Signature(MarkdownDocument document)
            => document.Descendants().OfType<Block>()
                .Where(b => !(b.Parent is TableCell))
                .Select(b => b is TableCell
                    ? nameof(TableCell)
                    : b.GetType().Name + "@" + b.Line + (b is ContainerBlock c ? "#" + c.Count : string.Empty))
                .ToList();

        // ── Page ↔ source ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// For each page symbol, the index of the source symbol it shows; null when the page does
        /// not show exactly what the source renders.
        /// </summary>
        private static int[] Align(List<Symbol> source, List<Symbol> page)
        {
            if (source.Count != page.Count)
            {
                return null;
            }
            var map = new int[page.Count];
            for (var i = 0; i < page.Count; i++)
            {
                if (!Same(source[i], page[i]))
                {
                    return null;
                }
                map[i] = i;
            }
            return map;
        }

        private enum OpKind { Keep, Delete, Insert }

        private readonly struct DiffOp
        {
            public DiffOp(OpKind kind, int beforeIndex, Symbol inserted)
            {
                Kind = kind;
                BeforeIndex = beforeIndex;
                Inserted = inserted;
            }

            public OpKind Kind { get; }

            /// <summary>Keep/Delete: the symbol before. Insert: the gap, i.e. the index of the symbol before it goes in front of.</summary>
            public int BeforeIndex { get; }

            public Symbol Inserted { get; }
        }

        /// <summary>Longest common subsequence on (character, formatting path); null when too large.</summary>
        private static List<DiffOp> Diff(List<Symbol> before, List<Symbol> after)
        {
            int n = before.Count, m = after.Count;
            var prefix = 0;
            while (prefix < n && prefix < m && Same(before[prefix], after[prefix]))
            {
                prefix++;
            }
            var suffix = 0;
            while (suffix < n - prefix && suffix < m - prefix && Same(before[n - 1 - suffix], after[m - 1 - suffix]))
            {
                suffix++;
            }

            int rows = n - prefix - suffix, cols = m - prefix - suffix;
            if ((long)(rows + 1) * (cols + 1) > MaxDiffCells)
            {
                return null;
            }

            // lcs[i, j] = LCS length of before[prefix+i..] and after[prefix+j..] (middle only).
            var lcs = new int[rows + 1, cols + 1];
            for (var i = rows - 1; i >= 0; i--)
            {
                for (var j = cols - 1; j >= 0; j--)
                {
                    lcs[i, j] = Same(before[prefix + i], after[prefix + j])
                        ? lcs[i + 1, j + 1] + 1
                        : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
                }
            }

            var ops = new List<DiffOp>(n + cols);
            for (var k = 0; k < prefix; k++)
            {
                ops.Add(new DiffOp(OpKind.Keep, k, null));
            }
            int bi = 0, aj = 0;
            while (bi < rows || aj < cols)
            {
                if (bi < rows && aj < cols && Same(before[prefix + bi], after[prefix + aj]))
                {
                    ops.Add(new DiffOp(OpKind.Keep, prefix + bi, null));
                    bi++;
                    aj++;
                }
                else if (aj < cols && (bi == rows || lcs[bi, aj + 1] >= lcs[bi + 1, aj]))
                {
                    ops.Add(new DiffOp(OpKind.Insert, prefix + bi, after[prefix + aj]));
                    aj++;
                }
                else
                {
                    ops.Add(new DiffOp(OpKind.Delete, prefix + bi, null));
                    bi++;
                }
            }
            for (var k = n - suffix; k < n; k++)
            {
                ops.Add(new DiffOp(OpKind.Keep, k, null));
            }
            return ops;
        }

        private sealed class SourceEdit
        {
            public SourceEdit(int start, int end, string text)
            {
                Start = start;
                End = end;
                Text = text;
            }

            public int Start { get; }

            /// <summary>Exclusive; equal to <see cref="Start"/> for an insertion.</summary>
            public int End { get; }

            public string Text { get; }
        }

        private static RenderedTextEdit Translate(string text, List<Symbol> source, int[] map, int beforeCount, List<DiffOp> ops, List<SourceEdit> edits)
        {
            var deleted = new HashSet<int>();
            foreach (var op in ops.Where(o => o.Kind == OpKind.Delete))
            {
                deleted.Add(map[op.BeforeIndex]);
            }

            // Insertions: consecutive characters typed in one gap with one formatting.
            var insertions = new List<(int Position, string Text, bool InCode)>();
            var index = 0;
            while (index < ops.Count)
            {
                if (ops[index].Kind != OpKind.Insert)
                {
                    index++;
                    continue;
                }
                var gap = ops[index].BeforeIndex;
                var path = ops[index].Inserted.Path;
                var pathKey = ops[index].Inserted.PathKey;
                var typed = new StringBuilder();
                while (index < ops.Count && ops[index].Kind == OpKind.Insert && ops[index].BeforeIndex == gap && ops[index].Inserted.PathKey == pathKey)
                {
                    var symbol = ops[index].Inserted;
                    if (symbol.Kind != SymbolKind.Char)
                    {
                        return RenderedTextEdit.Refused(RenderedTextRefusal.ProtectedContentTouched, $"A <{symbol.Tag}> cannot be added on the page.");
                    }
                    if (symbol.Char == '\n')
                    {
                        return RenderedTextEdit.Refused(RenderedTextRefusal.LineBreakNotAllowed, "A line break cannot be added.");
                    }
                    typed.Append(symbol.Char);
                    index++;
                }

                var left = gap > 0 ? source[map[gap - 1]] : null;
                var right = gap < beforeCount ? source[map[gap]] : null;
                var position = PositionAfter(left, path) ?? PositionBefore(right, path);
                if (position == null && new[] { left, right }.Any(s => s != null && IsPrefix(path, s.Path) && s.Containers.Take(path.Length).Any(c => c.Sealed)))
                {
                    return RenderedTextEdit.Refused(RenderedTextRefusal.ProtectedContentTouched,
                        (left ?? right).Containers.Any(c => c.Node is CodeInline) ? "Inline code the file writes differently cannot be changed on the page." : "The text of an address link cannot be changed on the page.");
                }
                if (position == null)
                {
                    return RenderedTextEdit.Refused(RenderedTextRefusal.FormattingNotAllowed,
                        $"\"{typed}\" cannot go there with the formatting it has on the page ({(pathKey.Length == 0 ? "none" : pathKey)}).");
                }
                insertions.Add((position.Value, typed.ToString(), path.Length > 0 && path[path.Length - 1] == "code"));
            }

            // A formatting whose text is all deleted, with nothing typed back inside, goes with its markers.
            var handled = new HashSet<int>();
            var containers = source.SelectMany(s => s.Containers.Select((c, depth) => (Container: c, Depth: depth)))
                .GroupBy(x => x.Container)
                .Select(g => g.First())
                .OrderBy(x => x.Depth)
                .Select(x => x.Container);
            foreach (var container in containers)
            {
                var members = Enumerable.Range(0, source.Count).Where(i => source[i].Containers.Contains(container)).ToList();
                if (members.All(handled.Contains))
                {
                    continue;
                }
                var start = container.Node.Span.Start;
                var end = container.Node.Span.End;
                if (members.All(deleted.Contains)
                    && members.All(i => source[i].Kind == SymbolKind.Char && !(source[i].Owner is LineBreakInline))
                    && !insertions.Any(ins => start < ins.Position && ins.Position <= end))
                {
                    edits.Add(new SourceEdit(start, end + 1, string.Empty));
                    handled.UnionWith(members);
                }
            }

            foreach (var i in deleted.Where(i => !handled.Contains(i)).OrderBy(i => i))
            {
                var symbol = source[i];
                if (symbol.Owner is LineBreakInline)
                {
                    return RenderedTextEdit.Refused(RenderedTextRefusal.LineBreakNotAllowed, "A line break cannot be removed.");
                }
                // An autolink's text has positions, but it is the address: changing it moves the link.
                if (!symbol.Editable || symbol.Containers.Any(c => c.Sealed))
                {
                    return RenderedTextEdit.Refused(RenderedTextRefusal.ProtectedContentTouched, Describe(symbol) + " cannot be changed on the page.");
                }
                edits.Add(new SourceEdit(symbol.Start, symbol.End, string.Empty));
            }

            foreach (var insertion in insertions)
            {
                edits.Add(new SourceEdit(insertion.Position, insertion.Position,
                    insertion.InCode ? insertion.Text : Escape(text, insertion.Position, insertion.Text)));
            }
            return null;
        }

        /// <summary>Where in the file text with formatting <paramref name="path"/> goes right after <paramref name="s"/>.</summary>
        private static int? PositionAfter(Symbol s, string[] path)
        {
            if (s == null || !IsPrefix(path, s.Path) || s.Containers.Take(path.Length).Any(c => c.Sealed))
            {
                return null;
            }
            if (path.Length < s.Path.Length)
            {
                return s.Containers[path.Length].Node.Span.End + 1;
            }
            if (s.Containers.Any(c => c.Sealed))
            {
                return null;
            }
            if (s.Editable)
            {
                return s.End;
            }
            return IsAtomicOwner(s) && s.LastOfOwner ? s.Owner.Span.End + 1 : (int?)null;
        }

        /// <summary>Where in the file text with formatting <paramref name="path"/> goes right before <paramref name="s"/>.</summary>
        private static int? PositionBefore(Symbol s, string[] path)
        {
            if (s == null || !IsPrefix(path, s.Path) || s.Containers.Take(path.Length).Any(c => c.Sealed))
            {
                return null;
            }
            if (path.Length < s.Path.Length)
            {
                return s.Containers[path.Length].Node.Span.Start;
            }
            if (s.Containers.Any(c => c.Sealed))
            {
                return null;
            }
            if (s.Editable)
            {
                return s.Start;
            }
            return IsAtomicOwner(s) && s.FirstOfOwner ? s.Owner.Span.Start : (int?)null;
        }

        /// <summary>
        /// Inlines whose span is exactly what they show. Not a line break: its span stops at the
        /// <c>\r</c> of a CRLF, and "after it" would be the middle of the line ending.
        /// </summary>
        private static bool IsAtomicOwner(Symbol s)
            => s.Owner is HtmlEntityInline || s.Owner is EmojiInline || s.Owner is TaskList
               || (s.Owner is LinkInline link && link.IsImage)
               || (s.Owner is LiteralInline && !s.Editable);

        private static bool IsPrefix(string[] prefix, string[] path)
            => prefix.Length <= path.Length && prefix.Zip(path, string.Equals).All(same => same);

        private static string Describe(Symbol s)
        {
            if (s.Containers.Any(c => c.Sealed && !(c.Node is CodeInline)))
            {
                return "The text of an address link";
            }
            switch (s.Owner)
            {
                case EmojiInline _: return "An emoji";
                case HtmlEntityInline _: return "An HTML entity";
                case TaskList _: return "A checkbox";
                case LinkInline link when link.IsImage: return "An image";
                case CodeInline _: return "Inline code the file writes differently";
                default: return "Text the file writes differently from how it shows";
            }
        }

        /// <summary>
        /// Typed text as markdown that shows it literally. Only what could start markdown is
        /// escaped, so a corrected sentence stays readable in the file; the verification catches
        /// whatever slips through.
        /// </summary>
        private static string Escape(string text, int position, string typed)
        {
            var atLineStart = AtLineStart(text, position);
            var sb = new StringBuilder(typed.Length + 4);
            for (var i = 0; i < typed.Length; i++)
            {
                var c = typed[i];
                var previous = i > 0 ? typed[i - 1] : position > 0 ? text[position - 1] : '\0';
                var next = i + 1 < typed.Length ? typed[i + 1] : position < text.Length ? text[position] : '\0';

                var escape = "\\`*_[]<|~^${}".IndexOf(c) >= 0
                    || ((c == '=' || c == '+') && (previous == c || next == c))
                    || (atLineStart && i == 0 && "#>-+=".IndexOf(c) >= 0)
                    || (atLineStart && (c == '.' || c == ')') && i > 0 && typed.Take(i).All(char.IsDigit))
                    || (c == '&' && EntityAhead.IsMatch(typed.Substring(i) + text.Substring(position, Math.Min(40, text.Length - position))));
                if (escape)
                {
                    sb.Append('\\');
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool AtLineStart(string text, int position)
        {
            var j = position - 1;
            while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
            {
                j--;
            }
            return j < 0 || text[j] == '\n';
        }

        private static bool IsAsciiPunctuation(char c)
            => c < 128 && char.IsPunctuation(c) || c < 128 && char.IsSymbol(c);

        private static string ApplyEdits(string text, List<SourceEdit> edits)
        {
            // At one position an insertion goes before the deletion that starts there.
            var ordered = edits
                .Select((edit, order) => (edit, order))
                .OrderBy(x => x.edit.Start)
                .ThenBy(x => x.edit.End > x.edit.Start ? 1 : 0)
                .ThenBy(x => x.order)
                .Select(x => x.edit);

            var sb = new StringBuilder(text.Length + 16);
            var cursor = 0;
            foreach (var edit in ordered)
            {
                if (edit.Start < cursor)
                {
                    throw new InvalidOperationException($"Overlapping source edits at {edit.Start}: the correction was translated wrongly.");
                }
                sb.Append(text, cursor, edit.Start - cursor);
                sb.Append(edit.Text);
                cursor = edit.End;
            }
            sb.Append(text, cursor, text.Length - cursor);
            return sb.ToString();
        }

        private static RenderedTextEdit Verify(MarkdownDocument oldDocument, string newText, MarkdownPipeline pipeline, RenderedTextTarget target, List<Symbol> pageAfter)
        {
            var newDocument = Markdown.Parse(newText, pipeline);
            if (!Signature(oldDocument).SequenceEqual(Signature(newDocument)))
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.VerificationFailed, "The correction would change the blocks of the document.");
            }

            var leaf = FindLeaf(newDocument, newText, target, out var notFound);
            if (leaf == null)
            {
                // Clearing a cell is a correction: the cell stays, with no paragraph in it.
                if (target.Row.HasValue && pageAfter.Count == 0 && notFound == EmptyCell)
                {
                    return RenderedTextEdit.Applied(newText);
                }
                return RenderedTextEdit.Refused(RenderedTextRefusal.VerificationFailed, "After the correction: " + notFound);
            }

            List<Symbol> symbols;
            try
            {
                symbols = Trim(SourceSymbols(newText, leaf));
            }
            catch (UnsupportedInlineException ex)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.VerificationFailed, "After the correction: " + ex.Message);
            }

            if (Align(symbols, pageAfter) == null)
            {
                return RenderedTextEdit.Refused(RenderedTextRefusal.VerificationFailed,
                    "The corrected file would not show what was typed (the text would be read as markdown).");
            }
            return RenderedTextEdit.Applied(newText);
        }
    }
}
