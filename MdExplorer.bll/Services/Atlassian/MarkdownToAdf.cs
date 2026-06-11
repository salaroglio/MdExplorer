using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Converts markdown into Atlassian Document Format (ADF) — the JSON document
    /// tree Confluence/Jira Cloud accept for rich text. The inverse of
    /// <see cref="AdfRenderer"/>. Parsing is delegated to Markdig (BSD-2-Clause,
    /// CommonMark-compliant) so we get a robust AST; this class only maps that AST
    /// to ADF nodes. Pure and dependency-light (Markdig + System.Text.Json).
    ///
    /// Supported: headings, paragraphs, bold/italic/strikethrough/inline-code,
    /// links, bullet/ordered lists (nested), fenced/indented code blocks,
    /// blockquotes, thematic breaks (rule) and pipe tables. Images degrade to a
    /// link (ADF media requires a separate upload, out of scope).
    /// </summary>
    public static class MarkdownToAdf
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseEmphasisExtras()   // strikethrough (~~), etc.
            .Build();

        /// <summary>A text mark (strong/em/code/strike/link). Href is set for links.</summary>
        private readonly struct Mark
        {
            public readonly string Type;
            public readonly string Href;
            public Mark(string type, string href = null) { Type = type; Href = href; }
        }

        /// <summary>Returns the ADF document as a JSON string (for body.value).</summary>
        public static string ToAdfJson(string markdown) => ToAdfDoc(markdown).ToJsonString();

        /// <summary>Returns the ADF document root: { version:1, type:"doc", content:[...] }.</summary>
        public static JsonObject ToAdfDoc(string markdown)
        {
            var content = new JsonArray();
            if (!string.IsNullOrEmpty(markdown))
            {
                var doc = Markdown.Parse(markdown, Pipeline);
                foreach (var block in doc)
                {
                    var node = RenderBlock(block);
                    if (node != null) content.Add(node);
                }
            }
            // An ADF doc must have at least one block; fall back to an empty paragraph.
            if (content.Count == 0)
                content.Add(new JsonObject { ["type"] = "paragraph", ["content"] = new JsonArray() });

            return new JsonObject
            {
                ["version"] = 1,
                ["type"] = "doc",
                ["content"] = content
            };
        }

        // ── blocks ──────────────────────────────────────────────────

        private static JsonNode RenderBlock(Block block)
        {
            switch (block)
            {
                case HeadingBlock h:
                {
                    var level = h.Level < 1 ? 1 : (h.Level > 6 ? 6 : h.Level);
                    return new JsonObject
                    {
                        ["type"] = "heading",
                        ["attrs"] = new JsonObject { ["level"] = level },
                        ["content"] = RenderInlines(h.Inline)
                    };
                }

                case ParagraphBlock p:
                    return new JsonObject
                    {
                        ["type"] = "paragraph",
                        ["content"] = RenderInlines(p.Inline)
                    };

                case ListBlock list:
                    return RenderList(list);

                case QuoteBlock quote:
                {
                    var inner = new JsonArray();
                    foreach (var child in quote)
                    {
                        var n = RenderBlock(child);
                        if (n != null) inner.Add(n);
                    }
                    if (inner.Count == 0)
                        inner.Add(new JsonObject { ["type"] = "paragraph", ["content"] = new JsonArray() });
                    return new JsonObject { ["type"] = "blockquote", ["content"] = inner };
                }

                case FencedCodeBlock fenced:
                    return RenderCode(fenced, fenced.Info);

                case CodeBlock code:
                    return RenderCode(code, null);

                case ThematicBreakBlock:
                    return new JsonObject { ["type"] = "rule" };

                case Table table:
                    return RenderTable(table);

                default:
                    return null;   // unknown/unsupported block — skip rather than emit junk
            }
        }

        private static JsonObject RenderList(ListBlock list)
        {
            var items = new JsonArray();
            foreach (var child in list)
            {
                if (child is not ListItemBlock li) continue;
                var itemContent = new JsonArray();
                foreach (var b in li)
                {
                    var n = RenderBlock(b);
                    if (n != null) itemContent.Add(n);
                }
                // A listItem must contain at least one block (a paragraph).
                if (itemContent.Count == 0)
                    itemContent.Add(new JsonObject { ["type"] = "paragraph", ["content"] = new JsonArray() });
                items.Add(new JsonObject { ["type"] = "listItem", ["content"] = itemContent });
            }

            if (list.IsOrdered)
            {
                var node = new JsonObject { ["type"] = "orderedList", ["content"] = items };
                if (int.TryParse(list.OrderedStart, out var start) && start != 1)
                    node["attrs"] = new JsonObject { ["order"] = start };
                return node;
            }
            return new JsonObject { ["type"] = "bulletList", ["content"] = items };
        }

        private static JsonObject RenderCode(LeafBlock code, string info)
        {
            var sb = new StringBuilder();
            var lines = code.Lines;
            for (var i = 0; i < lines.Count; i++)
            {
                sb.Append(lines.Lines[i].Slice.ToString());
                if (i < lines.Count - 1) sb.Append('\n');
            }
            var text = sb.ToString();

            var node = new JsonObject { ["type"] = "codeBlock" };
            var lang = info?.Trim();
            if (!string.IsNullOrEmpty(lang))
                node["attrs"] = new JsonObject { ["language"] = lang };
            // codeBlock content is a single text node; empty text nodes are invalid.
            var inner = new JsonArray();
            if (text.Length > 0)
                inner.Add(new JsonObject { ["type"] = "text", ["text"] = text });
            node["content"] = inner;
            return node;
        }

        private static JsonObject RenderTable(Table table)
        {
            var rows = new JsonArray();
            foreach (var rowObj in table)
            {
                if (rowObj is not TableRow row) continue;
                var cells = new JsonArray();
                foreach (var cellObj in row)
                {
                    if (cellObj is not TableCell cell) continue;
                    var cellBlocks = new JsonArray();
                    foreach (var b in cell)
                    {
                        var n = RenderBlock(b);
                        if (n != null) cellBlocks.Add(n);
                    }
                    if (cellBlocks.Count == 0)
                        cellBlocks.Add(new JsonObject { ["type"] = "paragraph", ["content"] = new JsonArray() });
                    cells.Add(new JsonObject
                    {
                        ["type"] = row.IsHeader ? "tableHeader" : "tableCell",
                        ["attrs"] = new JsonObject(),
                        ["content"] = cellBlocks
                    });
                }
                rows.Add(new JsonObject { ["type"] = "tableRow", ["content"] = cells });
            }
            return new JsonObject
            {
                ["type"] = "table",
                ["attrs"] = new JsonObject { ["isNumberColumnEnabled"] = false, ["layout"] = "default" },
                ["content"] = rows
            };
        }

        // ── inlines ─────────────────────────────────────────────────

        private static JsonArray RenderInlines(ContainerInline container)
        {
            var arr = new JsonArray();
            if (container != null)
                RenderInlineContainer(container, new List<Mark>(), arr);
            return arr;
        }

        private static void RenderInlineContainer(ContainerInline container, List<Mark> marks, JsonArray output)
        {
            foreach (var inline in container)
                RenderInline(inline, marks, output);
        }

        private static void RenderInline(Inline inline, List<Mark> marks, JsonArray output)
        {
            switch (inline)
            {
                case LiteralInline lit:
                    AppendText(lit.Content.ToString(), marks, output);
                    break;

                case CodeInline code:
                    AppendText(code.Content, Plus(marks, new Mark("code")), output);
                    break;

                case EmphasisInline em:
                {
                    var mark = EmphasisMark(em);
                    var newMarks = mark.HasValue ? Plus(marks, mark.Value) : marks;
                    RenderInlineContainer(em, newMarks, output);
                    break;
                }

                case LinkInline link:
                {
                    // Images and links both carry a URL; ADF media needs a separate
                    // upload, so an image degrades to a link to its source.
                    var url = link.Url ?? string.Empty;
                    var linkMarks = Plus(marks, new Mark("link", url));
                    var before = output.Count;
                    RenderInlineContainer(link, linkMarks, output);
                    // Link with no text (e.g. bare image) → show the URL as the text.
                    if (output.Count == before && !string.IsNullOrEmpty(url))
                        AppendText(url, linkMarks, output);
                    break;
                }

                case AutolinkInline auto:
                    AppendText(auto.Url, Plus(marks, new Mark("link", auto.Url)), output);
                    break;

                case LineBreakInline lb:
                    if (lb.IsHard)
                        output.Add(new JsonObject { ["type"] = "hardBreak" });
                    else
                        AppendText(" ", marks, output);   // soft break → space
                    break;

                case ContainerInline c:
                    RenderInlineContainer(c, marks, output);   // fallback: descend
                    break;

                default:
                    // HtmlInline, HtmlEntityInline already resolved by Markdig, etc.
                    var s = inline?.ToString();
                    if (!string.IsNullOrEmpty(s)) AppendText(s, marks, output);
                    break;
            }
        }

        private static void AppendText(string text, List<Mark> marks, JsonArray output)
        {
            if (string.IsNullOrEmpty(text)) return;   // ADF text nodes must be non-empty
            var node = new JsonObject { ["type"] = "text", ["text"] = text };
            if (marks.Count > 0)
            {
                var marksArr = new JsonArray();
                foreach (var m in marks)
                {
                    var mark = new JsonObject { ["type"] = m.Type };
                    if (m.Type == "link")
                        mark["attrs"] = new JsonObject { ["href"] = m.Href ?? string.Empty };
                    marksArr.Add(mark);
                }
                node["marks"] = marksArr;
            }
            output.Add(node);
        }

        private static Mark? EmphasisMark(EmphasisInline em)
        {
            // '*'/'_' → bold (2) or italic (1); '~' → strikethrough (2).
            if (em.DelimiterChar == '~')
                return new Mark("strike");
            return em.DelimiterCount >= 2 ? new Mark("strong") : new Mark("em");
        }

        private static List<Mark> Plus(List<Mark> marks, Mark extra)
        {
            var copy = new List<Mark>(marks.Count + 1);
            copy.AddRange(marks);
            copy.Add(extra);
            return copy;
        }
    }
}
