using System;
using System.Text;
using System.Text.Json;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Flattens Atlassian Document Format (ADF) — the JSON document tree that
    /// Jira Cloud REST API v3 returns for rich-text fields like description and
    /// comment bodies — into readable plain text with light markdown. Pure and
    /// dependency-free so it is unit-testable.
    /// </summary>
    public static class AdfRenderer
    {
        /// <summary>
        /// Renders an ADF JSON string. If the input is null/empty returns "".
        /// If the input is not valid JSON (e.g. an already-plain string from a
        /// non-ADF field) it is returned verbatim — this is a rendering choice,
        /// not a functional fallback that hides a precondition failure.
        /// </summary>
        public static string ToText(string adfJson)
        {
            if (string.IsNullOrWhiteSpace(adfJson)) return string.Empty;
            JsonDocument doc;
            try { doc = JsonDocument.Parse(adfJson); }
            catch (JsonException) { return adfJson; }
            using (doc) { return ToText(doc.RootElement); }
        }

        public static string ToText(JsonElement adf)
        {
            var sb = new StringBuilder();
            Render(adf, sb, listPrefix: null, orderedIndex: 0);
            // Collapse 3+ newlines to a blank-line separator and trim edges.
            return CollapseBlankLines(sb.ToString()).Trim();
        }

        private static void Render(JsonElement node, StringBuilder sb, string listPrefix, int orderedIndex)
        {
            if (node.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in node.EnumerateArray())
                    Render(child, sb, listPrefix, orderedIndex);
                return;
            }
            if (node.ValueKind != JsonValueKind.Object) return;

            var type = node.TryGetProperty("type", out var t) ? t.GetString() : null;

            switch (type)
            {
                case "text":
                    if (node.TryGetProperty("text", out var txt))
                        sb.Append(txt.GetString());
                    break;

                case "hardBreak":
                    sb.Append('\n');
                    break;

                case "paragraph":
                    RenderContent(node, sb, null, 0);
                    sb.Append("\n\n");
                    break;

                case "heading":
                    var level = 1;
                    if (node.TryGetProperty("attrs", out var hAttrs) &&
                        hAttrs.TryGetProperty("level", out var lvl) &&
                        lvl.ValueKind == JsonValueKind.Number)
                        level = Math.Clamp(lvl.GetInt32(), 1, 6);
                    sb.Append(new string('#', level)).Append(' ');
                    RenderContent(node, sb, null, 0);
                    sb.Append("\n\n");
                    break;

                case "bulletList":
                    RenderList(node, sb, ordered: false);
                    break;

                case "orderedList":
                    RenderList(node, sb, ordered: true);
                    break;

                case "listItem":
                    sb.Append(listPrefix ?? "- ");
                    RenderContent(node, sb, null, 0);
                    // Trim trailing paragraph spacing inside a list item.
                    TrimTrailing(sb);
                    sb.Append('\n');
                    break;

                case "codeBlock":
                    sb.Append("```\n");
                    RenderContent(node, sb, null, 0);
                    sb.Append("\n```\n\n");
                    break;

                case "blockquote":
                    sb.Append("> ");
                    RenderContent(node, sb, null, 0);
                    sb.Append("\n\n");
                    break;

                case "rule":
                    sb.Append("---\n\n");
                    break;

                case "mention":
                    if (node.TryGetProperty("attrs", out var mAttrs) &&
                        mAttrs.TryGetProperty("text", out var mText))
                        sb.Append(mText.GetString());
                    break;

                case "emoji":
                    if (node.TryGetProperty("attrs", out var eAttrs))
                    {
                        if (eAttrs.TryGetProperty("text", out var eText) && eText.ValueKind == JsonValueKind.String)
                            sb.Append(eText.GetString());
                        else if (eAttrs.TryGetProperty("shortName", out var eShort))
                            sb.Append(eShort.GetString());
                    }
                    break;

                case "inlineCard":
                    if (node.TryGetProperty("attrs", out var icAttrs) &&
                        icAttrs.TryGetProperty("url", out var icUrl))
                        sb.Append(icUrl.GetString());
                    break;

                default:
                    // doc, mediaGroup, table, and anything unknown: recurse content.
                    RenderContent(node, sb, listPrefix, orderedIndex);
                    break;
            }
        }

        private static void RenderContent(JsonElement node, StringBuilder sb, string listPrefix, int orderedIndex)
        {
            if (node.TryGetProperty("content", out var content))
                Render(content, sb, listPrefix, orderedIndex);
        }

        private static void RenderList(JsonElement node, StringBuilder sb, bool ordered)
        {
            if (!node.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return;
            var i = 1;
            foreach (var item in content.EnumerateArray())
            {
                var prefix = ordered ? $"{i}. " : "- ";
                Render(item, sb, prefix, i);
                i++;
            }
            sb.Append('\n');
        }

        private static void TrimTrailing(StringBuilder sb)
        {
            while (sb.Length > 0 && (sb[sb.Length - 1] == '\n' || sb[sb.Length - 1] == ' '))
                sb.Length--;
        }

        private static string CollapseBlankLines(string s)
        {
            var sb = new StringBuilder(s.Length);
            int newlineRun = 0;
            foreach (var c in s)
            {
                if (c == '\r') continue;
                if (c == '\n')
                {
                    newlineRun++;
                    if (newlineRun <= 2) sb.Append('\n');
                }
                else
                {
                    newlineRun = 0;
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
