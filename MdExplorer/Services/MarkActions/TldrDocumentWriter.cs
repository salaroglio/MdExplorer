using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Services.MarkActions
{
    /// <summary>
    /// Deterministically inserts or replaces the <c>## TL;DR</c> block at the top of a
    /// markdown document. The LLM only produces the TL;DR text — this writer normalizes
    /// it into clean markdown and is the only thing that touches the file, so the
    /// document body is never altered by the model.
    /// </summary>
    public static class TldrDocumentWriter
    {
        // Markdown is written without a BOM (standard for .md files).
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        // The TL;DR heading line: "## TL;DR", "##TLDR", "## TL; DR:", case-insensitive.
        private static readonly Regex TldrHeadingRegex = new Regex(
            @"^[ \t]*#{2,3}[ \t]*TL[ \t]*;?[ \t]*DR\b[^\r\n]*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Any level-1..6 heading — used to find where the existing TL;DR block ends.
        private static readonly Regex AnyHeadingRegex = new Regex(
            @"^[ \t]*#{1,6}[ \t]",
            RegexOptions.Compiled | RegexOptions.Multiline);

        // A YAML frontmatter block at the very top of the file.
        private static readonly Regex FrontmatterRegex = new Regex(
            @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n",
            RegexOptions.Compiled | RegexOptions.Singleline);

        // A numbered-list marker: "1. ", "2) ".
        private static readonly Regex NumberedBulletRegex = new Regex(
            @"^\d+[.)]\s+(.*)$", RegexOptions.Compiled);

        // Bullet markers an LLM may emit — markdown ones plus common Unicode glyphs.
        private static readonly string[] BulletMarkers =
            { "- ", "* ", "+ ", "• ", "● ", "‣ ", "▪ ", "· ", "– ", "— " };

        /// <summary>
        /// Turns the raw LLM answer into a clean, parser-friendly <c>## TL;DR</c> block:
        /// strips code fences and any restated "TL;DR" marker line, de-indents, joins
        /// hard-wrapped bullet continuation lines back into their bullet, and normalizes
        /// every bullet to a single <c>- </c> line. Returns <c>null</c> when there is no
        /// usable content.
        /// <para>
        /// This matters because the TOC's TL;DR parser stops the bullet list at the first
        /// non-bullet line — a hard-wrapped bullet would otherwise drop the rest of the list.
        /// </para>
        /// </summary>
        public static string NormalizeTldrBlock(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var text = StripCodeFence(raw.Trim());
            if (string.IsNullOrWhiteSpace(text)) return null;

            var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            var prose = new List<string>();
            var bullets = new List<string>();
            var inBullets = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0) continue;

                // Drop the heading and any restated "TL;DR" marker line the LLM echoed.
                if (IsTldrMarkerLine(line)) continue;

                var bulletText = TryStripBulletMarker(line);
                if (bulletText != null)
                {
                    bullets.Add(bulletText);
                    inBullets = true;
                }
                else if (inBullets)
                {
                    // Continuation of a hard-wrapped bullet → fold back into it.
                    if (bullets.Count > 0)
                        bullets[bullets.Count - 1] = (bullets[bullets.Count - 1] + " " + line).Trim();
                }
                else
                {
                    prose.Add(line);
                }
            }

            if (prose.Count == 0 && bullets.Count == 0) return null;

            var sb = new StringBuilder();
            sb.Append("## TL;DR");
            if (prose.Count > 0)
                sb.Append("\n\n").Append(string.Join(" ", prose));
            if (bullets.Count > 0)
            {
                sb.Append("\n\n");
                sb.Append(string.Join("\n", bullets.Select(b => "- " + b)));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Writes <paramref name="tldrBlock"/> (already a clean <c>## TL;DR</c> section,
        /// e.g. from <see cref="NormalizeTldrBlock"/>) into the document at
        /// <paramref name="filePath"/>. If the document already has a <c>## TL;DR</c>
        /// section it is replaced in place; otherwise the block is inserted right after
        /// the YAML frontmatter (or at the very top when there is none).
        /// </summary>
        public static void UpsertTldrBlock(string filePath, string tldrBlock)
        {
            if (string.IsNullOrWhiteSpace(tldrBlock))
                throw new ArgumentException("TL;DR block is empty.", nameof(tldrBlock));

            var content = File.ReadAllText(filePath);
            var nl = content.Contains("\r\n") ? "\r\n" : "\n";
            var block = NormalizeNewlines(tldrBlock.Trim(), nl);

            var heading = TldrHeadingRegex.Match(content);
            if (heading.Success)
            {
                // Replace the existing block: heading line → next heading (or EOF).
                var searchFrom = heading.Index + heading.Length;
                var next = AnyHeadingRegex.Match(content, Math.Min(searchFrom, content.Length));
                var end = next.Success ? next.Index : content.Length;

                var before = content.Substring(0, heading.Index).TrimEnd('\r', '\n', ' ', '\t');
                var after = content.Substring(end).TrimStart('\r', '\n');

                var rebuilt = (before.Length > 0 ? before + nl + nl : string.Empty)
                              + block
                              + (after.Length > 0 ? nl + nl + after : nl);
                File.WriteAllText(filePath, rebuilt, Utf8NoBom);
                return;
            }

            // Insert: after the YAML frontmatter if present, else at the top.
            var insertAt = 0;
            if (content.StartsWith("---", StringComparison.Ordinal))
            {
                var fm = FrontmatterRegex.Match(content);
                if (fm.Success && fm.Index == 0)
                    insertAt = fm.Length;
            }

            var head = content.Substring(0, insertAt);
            var tail = content.Substring(insertAt).TrimStart('\r', '\n');
            var result = head + block + (tail.Length > 0 ? nl + nl + tail : nl);
            File.WriteAllText(filePath, result, Utf8NoBom);
        }

        /// <summary>True for the "## TL;DR" heading or any line that just restates the marker.</summary>
        private static bool IsTldrMarkerLine(string line)
        {
            var s = line.TrimStart('#', '-', '*', '+', '•', '●', '‣', '▪', '·', '–', '—', '>', ' ', '\t');
            s = s.Replace("**", string.Empty).Replace("__", string.Empty).Trim();
            var compact = s.Replace(" ", string.Empty).Replace("\t", string.Empty)
                           .Replace(";", string.Empty).TrimEnd(':', '.');
            return compact.Equals("TLDR", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Returns the bullet text with its marker stripped, or null if not a bullet.</summary>
        private static string TryStripBulletMarker(string line)
        {
            foreach (var marker in BulletMarkers)
            {
                if (line.StartsWith(marker, StringComparison.Ordinal))
                    return line.Substring(marker.Length).Trim();
            }
            var m = NumberedBulletRegex.Match(line);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static string StripCodeFence(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var t = text.Trim();
            if (!t.StartsWith("```", StringComparison.Ordinal)) return t;

            var firstNl = t.IndexOf('\n');
            if (firstNl >= 0) t = t.Substring(firstNl + 1);
            var lastFence = t.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0) t = t.Substring(0, lastFence);
            return t.Trim();
        }

        private static string NormalizeNewlines(string text, string nl)
        {
            var unified = text.Replace("\r\n", "\n").Replace("\r", "\n");
            return nl == "\n" ? unified : unified.Replace("\n", nl);
        }
    }
}
