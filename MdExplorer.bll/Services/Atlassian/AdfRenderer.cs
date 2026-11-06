using System;
using System.Collections.Generic;
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
        public static string ToText(string adfJson) => ToText(adfJson, mediaNames: null);

        /// <summary>
        /// Come <see cref="ToText(string)"/>, con i nomi degli allegati: la chiave è l'id
        /// del file nel media store di Atlassian (<c>attrs.id</c> del nodo <c>media</c>),
        /// il valore il nome del file. Confluence lo espone come <c>fileId</c> sull'allegato
        /// della pagina; Jira no, e lì resta il nome scritto nel nodo stesso (<c>alt</c>).
        /// </summary>
        public static string ToText(string adfJson, IReadOnlyDictionary<string, string> mediaNames)
        {
            if (string.IsNullOrWhiteSpace(adfJson)) return string.Empty;
            JsonDocument doc;
            try { doc = JsonDocument.Parse(adfJson); }
            catch (JsonException) { return adfJson; }
            using (doc) { return ToText(doc.RootElement, mediaNames); }
        }

        public static string ToText(JsonElement adf) => ToText(adf, mediaNames: null);

        public static string ToText(JsonElement adf, IReadOnlyDictionary<string, string> mediaNames)
        {
            var sb = new StringBuilder();
            Render(adf, sb, listPrefix: null, orderedIndex: 0, new Media(mediaNames));
            // Collapse 3+ newlines to a blank-line separator and trim edges.
            return CollapseBlankLines(sb.ToString()).Trim();
        }

        /// <summary>Cosa il renderer sa degli allegati del documento che sta leggendo.</summary>
        private sealed class Media
        {
            private readonly IReadOnlyDictionary<string, string> _names;
            public Media(IReadOnlyDictionary<string, string> names) { _names = names; }

            public string NameOf(string mediaId) =>
                !string.IsNullOrEmpty(mediaId) && _names != null && _names.TryGetValue(mediaId, out var n) ? n : null;
        }

        /// <summary>
        /// Un'immagine (o un file) incorporata nel testo. Senza questo il nodo spariva
        /// del tutto e l'LLM non sapeva nemmeno che ci fosse: ora lascia un segnaposto col
        /// nome del file, lo stesso che compare nell'elenco degli allegati, così l'LLM
        /// sa che c'è e quale allegato scaricare per vederla.
        /// </summary>
        private static void RenderMedia(JsonElement node, StringBuilder sb, Media media)
        {
            string id = null, alt = null, url = null, kind = null;
            if (node.TryGetProperty("attrs", out var a) && a.ValueKind == JsonValueKind.Object)
            {
                id = GetString(a, "id");
                alt = GetString(a, "alt");
                url = GetString(a, "url");
                kind = GetString(a, "type");
            }

            // Immagine esterna: è un indirizzo pubblico, non un allegato.
            if (kind == "external" && !string.IsNullOrEmpty(url))
            {
                sb.Append("![").Append(alt ?? "").Append("](").Append(url).Append(')');
                return;
            }

            var name = media.NameOf(id) ?? alt;
            sb.Append(string.IsNullOrEmpty(name)
                ? "[embedded attachment (name unknown, see the attachments list)]"
                : $"[embedded attachment: {name}]");
        }

        private static string GetString(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static void Render(JsonElement node, StringBuilder sb, string listPrefix, int orderedIndex, Media media)
        {
            if (node.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in node.EnumerateArray())
                    Render(child, sb, listPrefix, orderedIndex, media);
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
                    RenderContent(node, sb, null, 0, media);
                    sb.Append("\n\n");
                    break;

                case "heading":
                    var level = 1;
                    if (node.TryGetProperty("attrs", out var hAttrs) &&
                        hAttrs.TryGetProperty("level", out var lvl) &&
                        lvl.ValueKind == JsonValueKind.Number)
                        level = Math.Clamp(lvl.GetInt32(), 1, 6);
                    sb.Append(new string('#', level)).Append(' ');
                    RenderContent(node, sb, null, 0, media);
                    sb.Append("\n\n");
                    break;

                case "bulletList":
                    RenderList(node, sb, ordered: false, media);
                    break;

                case "orderedList":
                    RenderList(node, sb, ordered: true, media);
                    break;

                case "listItem":
                    sb.Append(listPrefix ?? "- ");
                    RenderContent(node, sb, null, 0, media);
                    // Trim trailing paragraph spacing inside a list item.
                    TrimTrailing(sb);
                    sb.Append('\n');
                    break;

                case "codeBlock":
                    sb.Append("```\n");
                    RenderContent(node, sb, null, 0, media);
                    sb.Append("\n```\n\n");
                    break;

                case "blockquote":
                    sb.Append("> ");
                    RenderContent(node, sb, null, 0, media);
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

                case "media":
                case "mediaInline":
                    RenderMedia(node, sb, media);
                    break;

                case "mediaSingle":
                    RenderContent(node, sb, null, 0, media);
                    sb.Append("\n\n");
                    break;

                case "mediaGroup":
                    if (node.TryGetProperty("content", out var group) && group.ValueKind == JsonValueKind.Array)
                        foreach (var item in group.EnumerateArray())
                        {
                            Render(item, sb, null, 0, media);
                            sb.Append('\n');
                        }
                    sb.Append('\n');
                    break;

                default:
                    // doc, table, and anything unknown: recurse content.
                    RenderContent(node, sb, listPrefix, orderedIndex, media);
                    break;
            }
        }

        private static void RenderContent(JsonElement node, StringBuilder sb, string listPrefix, int orderedIndex, Media media)
        {
            if (node.TryGetProperty("content", out var content))
                Render(content, sb, listPrefix, orderedIndex, media);
        }

        private static void RenderList(JsonElement node, StringBuilder sb, bool ordered, Media media)
        {
            if (!node.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return;
            var i = 1;
            foreach (var item in content.EnumerateArray())
            {
                var prefix = ordered ? $"{i}. " : "- ";
                Render(item, sb, prefix, i, media);
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
