using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Extracts the TLDR; summary from markdown content.
    /// Moved verbatim from IndexingPipelineService so that the FileSystemWatcher
    /// and the git refresh path can keep MarkdownFile.Tldr fresh too — with
    /// incremental indexing the pipeline no longer re-extracts it on every open.
    /// </summary>
    public static class TldrExtractor
    {
        // Heading form: "### TLDR;", "## TL;DR", "#### tldr:", etc.
        private static readonly Regex _tldrHeadingRegex = new Regex(
            @"^(?<level>#{1,6})\s*TL\s*;?\s*DR\s*[;:]?\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Blockquote form: "> **TL;DR** — ...", "> TL;DR: ...", "> **TLDR;** ..."
        // The marker may sit inside or outside the bold wrappers; trailing punctuation
        // (colon, em-dash, en-dash, hyphen) is optional.
        private static readonly Regex _tldrBlockquoteRegex = new Regex(
            @"^>\s*(?:\*\*|__)?\s*TL\s*;?\s*DR\s*[;:]?\s*(?:\*\*|__)?\s*[:\-–—]?\s*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Bold inline (no blockquote) form: "**TL;DR** — ...", "**TL;DR:** ...", "__TLDR;__: ..."
        private static readonly Regex _tldrBoldInlineRegex = new Regex(
            @"^\s*(?:\*\*|__)\s*TL\s*;?\s*DR\s*[;:]?\s*(?:\*\*|__)\s*[:\-–—]?\s*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Used to find the next heading when extracting content after a TLDR heading
        private static readonly Regex _anyHeadingRegex = new Regex(
            @"^#{1,6}\s",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private const int TldrMaxChars = 800;

        /// <summary>
        /// Extracts the TLDR; section content from a markdown document.
        /// Returns null when no TLDR; marker is found.
        /// Recognized forms:
        ///   - heading:        "### TLDR;" + content until the next heading
        ///   - blockquote:     "> **TL;DR** — ..." + content of consecutive blockquote lines
        ///   - bold inline:    "**TL;DR** — ..." + rest of the same line
        /// </summary>
        public static string ExtractTldr(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return null;

            // --- 1) Heading form ---
            var headingMatch = _tldrHeadingRegex.Match(markdown);
            if (headingMatch.Success)
            {
                var startIdx = headingMatch.Index + headingMatch.Length;
                if (startIdx < markdown.Length)
                {
                    var rest = markdown.Substring(startIdx);
                    var nextHeading = _anyHeadingRegex.Match(rest);
                    var content = nextHeading.Success ? rest.Substring(0, nextHeading.Index) : rest;
                    var normalized = NormalizeTldrContent(content);
                    if (!string.IsNullOrEmpty(normalized)) return CapTldr(normalized);
                }
            }

            // --- 2) Blockquote form ---
            var bqMatch = _tldrBlockquoteRegex.Match(markdown);
            if (bqMatch.Success)
            {
                // Collect the rest of the first blockquote line + any consecutive "> ..." lines.
                var firstLineEnd = markdown.IndexOf('\n', bqMatch.Index + bqMatch.Length);
                var afterMarker = firstLineEnd < 0
                    ? markdown.Substring(bqMatch.Index + bqMatch.Length)
                    : markdown.Substring(bqMatch.Index + bqMatch.Length, firstLineEnd - (bqMatch.Index + bqMatch.Length));

                var sb = new StringBuilder();
                sb.AppendLine(afterMarker.TrimEnd('\r'));
                if (firstLineEnd >= 0)
                {
                    var remaining = markdown.Substring(firstLineEnd + 1).Split('\n');
                    foreach (var raw in remaining)
                    {
                        var line = raw.TrimEnd('\r');
                        if (string.IsNullOrWhiteSpace(line)) break;
                        if (!line.TrimStart().StartsWith(">")) break;
                        var stripped = Regex.Replace(line.TrimStart(), @"^>\s?", "");
                        sb.AppendLine(stripped);
                    }
                }
                var normalized = NormalizeTldrContent(sb.ToString());
                if (!string.IsNullOrEmpty(normalized)) return CapTldr(normalized);
            }

            // --- 3) Bold inline form ---
            var boldMatch = _tldrBoldInlineRegex.Match(markdown);
            if (boldMatch.Success)
            {
                var startIdx = boldMatch.Index + boldMatch.Length;
                if (startIdx < markdown.Length)
                {
                    var endOfLine = markdown.IndexOf('\n', startIdx);
                    var line = endOfLine < 0
                        ? markdown.Substring(startIdx)
                        : markdown.Substring(startIdx, endOfLine - startIdx);
                    var normalized = NormalizeTldrContent(line);
                    if (!string.IsNullOrEmpty(normalized)) return CapTldr(normalized);
                }
            }

            return null;
        }

        private static string NormalizeTldrContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            content = content.Trim();
            if (content.Length == 0) return null;
            content = Regex.Replace(content, @"\r\n?", "\n");
            content = Regex.Replace(content, @"\n{3,}", "\n\n");
            return content;
        }

        private static string CapTldr(string content)
        {
            if (content.Length <= TldrMaxChars) return content;
            return content.Substring(0, TldrMaxChars).TrimEnd() + "…";
        }
    }
}
