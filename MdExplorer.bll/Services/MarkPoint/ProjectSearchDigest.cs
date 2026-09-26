using MdExplorer.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.MarkPoint
{
    /// <summary>
    /// The keywords MarkAgent chose for a search, in the grammar of Mark Search
    /// (<c>mark-search.component.ts</c>, phase 1): a <c>```json</c> block or bare JSON holding
    /// <c>{"keywords": ["…"]}</c>. Both forms are declared valid in the instructions, so accepting
    /// either is grammar, not a fallback; anything else is an error to show, never guessed.
    /// </summary>
    public static class MarkSearchKeywords
    {
        public const int MaxKeywords = 5;

        private static readonly Regex Fence = new(@"```(?:json)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase);

        /// <summary>The keywords, trimmed, at most <see cref="MaxKeywords"/>; empty when MarkAgent chose none.</summary>
        /// <exception cref="FormatException">The answer does not hold the keywords as asked.</exception>
        public static IReadOnlyList<string> Parse(string answer)
        {
            var payload = Payload(answer ?? string.Empty)
                ?? throw new FormatException("MarkAgent non ha indicato le parole da cercare nel formato atteso.");
            try
            {
                using var json = JsonDocument.Parse(payload);
                if (json.RootElement.ValueKind != JsonValueKind.Object
                    || !json.RootElement.TryGetProperty("keywords", out var keywords)
                    || keywords.ValueKind != JsonValueKind.Array
                    || keywords.EnumerateArray().Any(k => k.ValueKind != JsonValueKind.String))
                {
                    throw new FormatException("MarkAgent non ha indicato le parole da cercare nel formato atteso.");
                }
                return keywords.EnumerateArray()
                    .Select(k => k.GetString().Trim())
                    .Where(k => k.Length > 0)
                    .Take(MaxKeywords)
                    .ToList();
            }
            catch (JsonException ex)
            {
                throw new FormatException("MarkAgent non ha indicato le parole da cercare nel formato atteso.", ex);
            }
        }

        private static string Payload(string text)
        {
            var fence = Fence.Match(text);
            if (fence.Success) return fence.Groups[1].Value.Trim();
            var trimmed = text.Trim();
            return trimmed.StartsWith("{") || trimmed.StartsWith("[") ? trimmed : null;
        }
    }

    /// <summary>A paragraph of a project file, with the 1-based line it starts on.</summary>
    public sealed record ProjectPassage(int Line, string Text);

    /// <summary>A project file the search found: which keywords found it, how much they weigh, what it says.</summary>
    public sealed record ProjectSource(string Path, IReadOnlyList<string> Keywords, double Weight, IReadOnlyList<ProjectPassage> Passages);

    /// <summary>
    /// What the project says about a set of keywords, from the searches Mark Search runs (one
    /// <see cref="ISearchService.SearchAsync"/> per keyword, the function behind <c>api/search/quick</c>).
    ///
    /// <para>
    /// The search finds whole words as written, one phrase per call: a file found by more keywords
    /// ranks higher, and a keyword found in few files weighs more than one found everywhere
    /// (measured in F0: without the weight, common words push the right file out of the list).
    /// The snippets of the index are a dozen words: the passages given to MarkAgent are read from
    /// the file itself, the paragraphs holding the keywords.
    /// </para>
    ///
    /// Sprint: docs-internal/Sprints/2026-09-25-Slide-Chiedi-A-MarkAgent.md
    /// </summary>
    public static class ProjectSearchDigest
    {
        public const int MaxSources = 5;
        public const int MaxCharsPerSource = 2500;
        public const int MaxCharsPerPassage = 1000;

        /// <summary>A line that starts a passage by itself: a table row, a list item.</summary>
        private static readonly Regex StartsUnit = new(@"^\s{0,3}(\||[-*+]\s|\d+[.)]\s)");

        /// <summary>
        /// The files found, best first, at most <see cref="MaxSources"/>, each with its passages.
        /// <paramref name="readFile"/> reads a project-relative path and returns null when the file
        /// cannot be read; <paramref name="exclude"/> (full or project-relative paths) is left out — the
        /// deck the question comes from and the documents the point links to are given whole. Files under <paramref name="excludedFolders"/>
        /// (project-relative, '/') are left out too: the skills an agentic environment copies into the
        /// project are MdExplorer's documentation, not the project's.
        /// </summary>
        public static IReadOnlyList<ProjectSource> Build(
            IReadOnlyList<(string Keyword, SearchResult Result)> searches,
            string projectPath,
            IReadOnlyCollection<string> exclude,
            Func<string, string> readFile,
            IReadOnlyCollection<string> excludedFolders = null)
        {
            var folders = (excludedFolders ?? Array.Empty<string>()).Select(f => f.Trim('/') + "/").ToList();
            var excluded = new HashSet<string>((exclude ?? Array.Empty<string>()).Where(e => e != null).Select(e => Relative(e, projectPath)),
                StringComparer.OrdinalIgnoreCase);
            var foundBy = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (keyword, result) in searches)
            {
                // Markdown documents only, as Mark Search by default: a file found by name can be an
                // image, a link can point to a site.
                foreach (var path in FilesOf(result, projectPath).Where(p => p.EndsWith(".md", StringComparison.OrdinalIgnoreCase)))
                {
                    if (excluded.Contains(path)) continue;
                    if (folders.Any(f => path.StartsWith(f, StringComparison.OrdinalIgnoreCase))) continue;
                    if (!foundBy.TryGetValue(path, out var keywords)) foundBy[path] = keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    keywords.Add(keyword);
                }
            }

            var filesPerKeyword = searches
                .GroupBy(s => s.Keyword, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => foundBy.Count(f => f.Value.Contains(g.Key)), StringComparer.OrdinalIgnoreCase);
            var files = Math.Max(1, foundBy.Count);

            return foundBy
                .Select(f => (Path: f.Key, Keywords: f.Value.OrderBy(k => k).ToList(),
                    Weight: f.Value.Sum(k => Math.Log(1 + (double)files / Math.Max(1, filesPerKeyword[k])))))
                .OrderByDescending(f => f.Weight)
                .ThenBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
                .Take(MaxSources)
                .Select(f => new ProjectSource(f.Path, f.Keywords, Math.Round(f.Weight, 2),
                    Passages(readFile(f.Path), f.Keywords, MaxCharsPerSource)))
                .ToList();
        }

        /// <summary>
        /// The paragraphs of <paramref name="text"/> holding the most keywords, in file order, up to
        /// <paramref name="maxChars"/>. A paragraph is a run of lines between blank lines, a table row
        /// or a list item (with its continuation lines); the front matter is not one.
        /// </summary>
        public static IReadOnlyList<ProjectPassage> Passages(string text, IReadOnlyCollection<string> keywords, int maxChars)
        {
            if (string.IsNullOrEmpty(text) || keywords.Count == 0) return Array.Empty<ProjectPassage>();

            // Whole words, as the search finds them: «anno» is not in «annota» (measured 25/09/2026:
            // with a plain Contains the passages were paragraphs about something else).
            var matchers = keywords
                .Select(k => new Regex(@"(?<![\p{L}\p{N}_])" + Regex.Escape(k) + @"(?![\p{L}\p{N}_])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .ToList();
            var lines = text.Replace("\r\n", "\n").Split('\n');
            var paragraphs = new List<(int Line, string Text, int Hits)>();
            var start = FrontMatterEnd(lines);
            var current = new List<string>();
            var currentLine = 0;

            void Close()
            {
                if (current.Count == 0) return;
                var paragraph = string.Join("\n", current).Trim();
                var hits = matchers.Count(m => m.IsMatch(paragraph));
                if (hits > 0) paragraphs.Add((currentLine, paragraph, hits));
                current.Clear();
            }

            for (var i = start; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    Close();
                    continue;
                }
                // A row of a table and an item of a list are passages of their own: a whole table
                // is one paragraph, and cut to size it lost the very row the keywords were in
                // (measured in F2: «il passaggio che ho è troncato»).
                if (StartsUnit.IsMatch(lines[i])) Close();
                if (current.Count == 0) currentLine = i + 1;
                current.Add(lines[i]);
            }
            Close();

            var chosen = new List<(int Line, string Text)>();
            var used = 0;
            foreach (var p in paragraphs.OrderByDescending(p => p.Hits).ThenBy(p => p.Line))
            {
                var passage = p.Text.Length <= MaxCharsPerPassage ? p.Text : p.Text.Substring(0, MaxCharsPerPassage) + "…";
                if (used + passage.Length > maxChars && chosen.Count > 0) break;
                chosen.Add((p.Line, passage));
                used += passage.Length;
            }
            return chosen.OrderBy(p => p.Line).Select(p => new ProjectPassage(p.Line, p.Text)).ToList();
        }

        /// <summary>The files a search found: by content, by name, and the targets of the links whose title matched.</summary>
        private static IEnumerable<string> FilesOf(SearchResult result, string projectPath)
        {
            if (result == null) yield break;
            foreach (var c in result.Contents ?? new()) yield return Relative(c.Path, projectPath);
            foreach (var f in result.Files ?? new()) yield return Relative(f.Path, projectPath);
            foreach (var l in result.Links ?? new())
            {
                if (!string.IsNullOrWhiteSpace(l.FullPath)) yield return Relative(l.FullPath, projectPath);
            }
        }

        /// <summary>The path relative to the project root, with '/' — as Mark Search's <c>toRootRelative</c>.</summary>
        public static string Relative(string path, string projectPath)
        {
            var normalized = (path ?? string.Empty).Replace('\\', '/');
            var root = (projectPath ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            if (root.Length > 0 && normalized.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(root.Length);
            return normalized.TrimStart('/');
        }

        private static int FrontMatterEnd(string[] lines)
        {
            if (lines.Length == 0 || lines[0].TrimEnd() != "---") return 0;
            for (var i = 1; i < lines.Length; i++)
            {
                if (lines[i].TrimEnd() is "---" or "...") return i + 1;
            }
            return 0;
        }
    }
}
