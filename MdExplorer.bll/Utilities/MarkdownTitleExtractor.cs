using System.Text.RegularExpressions;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Extracts the human-readable document title from markdown content.
    /// Resolution chain (deterministic, no silent fallback beyond it):
    ///   1. YAML front matter "title:" value
    ///   2. first ATX H1 ("# ...")
    ///   3. null — caller decides (bookmarks fall back to the file name)
    /// </summary>
    public static class MarkdownTitleExtractor
    {
        private const int TitleMaxChars = 120;

        // "title: My doc", "title: 'My doc'", "Title: "My doc"" inside the front matter block
        private static readonly Regex _frontMatterTitleRegex = new Regex(
            @"^title\s*:\s*(?<value>.+?)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex _h1Regex = new Regex(
            @"^#\s+(?<value>.+?)\s*#*\s*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // [text](url) → text
        private static readonly Regex _mdLinkRegex = new Regex(
            @"\[(?<text>[^\]]*)\]\([^)]*\)",
            RegexOptions.Compiled);

        public static string ExtractTitle(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return null;

            // --- 1) YAML front matter ---
            var trimmed = markdown.TrimStart('\uFEFF', ' ', '\t');
            if (trimmed.StartsWith("---"))
            {
                var closingIdx = trimmed.IndexOf("\n---", 3, System.StringComparison.Ordinal);
                if (closingIdx > 0)
                {
                    var frontMatter = trimmed.Substring(3, closingIdx - 3);
                    var fmMatch = _frontMatterTitleRegex.Match(frontMatter);
                    if (fmMatch.Success)
                    {
                        var normalized = Normalize(fmMatch.Groups["value"].Value.Trim().Trim('"', '\''));
                        if (normalized != null) return normalized;
                    }
                }
            }

            // --- 2) first H1 (outside fenced code blocks: "# comment" in a
            // bash/python fence must not be mistaken for a heading) ---
            var h1Match = _h1Regex.Match(StripFencedCode(markdown));
            if (h1Match.Success)
            {
                return Normalize(h1Match.Groups["value"].Value);
            }

            return null;
        }

        private static string StripFencedCode(string markdown)
        {
            if (markdown.IndexOf("```", System.StringComparison.Ordinal) < 0 &&
                markdown.IndexOf("~~~", System.StringComparison.Ordinal) < 0)
            {
                return markdown;
            }

            var sb = new System.Text.StringBuilder(markdown.Length);
            var insideFence = false;
            string fenceMarker = null;
            foreach (var rawLine in markdown.Split('\n'))
            {
                var line = rawLine.TrimStart();
                if (!insideFence && (line.StartsWith("```") || line.StartsWith("~~~")))
                {
                    insideFence = true;
                    fenceMarker = line.Substring(0, 3);
                    continue;
                }
                if (insideFence)
                {
                    if (line.StartsWith(fenceMarker)) insideFence = false;
                    continue;
                }
                sb.Append(rawLine).Append('\n');
            }
            return sb.ToString();
        }

        private static string Normalize(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            title = _mdLinkRegex.Replace(title, "${text}");
            title = title.Replace("**", "").Replace("__", "").Replace("`", "").Trim();
            if (title.Length == 0) return null;
            if (title.Length > TitleMaxChars) title = title.Substring(0, TitleMaxChars).TrimEnd() + "…";
            return title;
        }
    }
}
