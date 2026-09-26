using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Single source of truth deciding whether a file is a "text file" eligible
    /// for the SEPARATE non-markdown text index.
    ///
    /// Deterministic by design (project rule: no silent fallbacks, no magic):
    /// classification is a pure allow-list match on the file extension. There is
    /// NO binary sniffing — a file is text iff its extension is in the effective
    /// allow-list. Markdown (.md) is ALWAYS excluded here because it is owned by
    /// the markdown world (MarkdownFile / MdEngineFts); the two indexes never overlap.
    ///
    /// The allow-list is per-project (<see cref="Entities.UserDB.Project.TextFileExtensions"/>),
    /// falling back to <see cref="DefaultExtensions"/> when the project leaves it unset.
    /// </summary>
    public static class TextFileClassifier
    {
        /// <summary>
        /// Extensions ALWAYS handled by the markdown world; never eligible for the
        /// text index even if a user adds them to the allow-list.
        /// </summary>
        public static readonly IReadOnlyCollection<string> MarkdownExtensions =
            new[] { ".md", ".markdown" };

        /// <summary>
        /// Central default allow-list applied when a project has no per-project list.
        /// Curated set of common plain-text, code, config and data files. Kept
        /// intentionally conservative — the user can widen it per project.
        /// </summary>
        public static readonly IReadOnlyList<string> DefaultExtensions = new[]
        {
            // plain text / docs
            ".txt", ".text", ".rst", ".adoc", ".asciidoc", ".org", ".log", ".rtf",
            // config / data
            ".json", ".yaml", ".yml", ".xml", ".toml", ".ini", ".cfg", ".conf",
            ".properties", ".env", ".csv", ".tsv",
            // web
            ".html", ".htm", ".css", ".scss", ".sass", ".less",
            // scripting / code
            ".js", ".ts", ".jsx", ".tsx", ".py", ".rb", ".php", ".pl", ".lua",
            ".sh", ".bash", ".zsh", ".ps1", ".bat", ".cmd",
            ".c", ".h", ".cpp", ".hpp", ".cc", ".cs", ".java", ".kt", ".go",
            ".rs", ".swift", ".scala", ".groovy", ".gradle", ".r",
            // data / db / build
            ".sql", ".graphql", ".proto", ".dockerfile", ".makefile",
            // legacy / mainframe (domain-relevant)
            ".cob", ".cbl", ".cpy", ".pli", ".pl1", ".jcl", ".ttl"
        };

        /// <summary>
        /// Resolves the effective, normalized allow-list for a project. Returns a
        /// case-insensitive set of extensions each starting with '.' and lower-cased.
        /// Markdown extensions are stripped defensively.
        /// </summary>
        /// <param name="perProjectCsv">
        /// The project's <c>TextFileExtensions</c> column (comma/semicolon/whitespace
        /// separated). When null/blank the central default applies.
        /// </param>
        public static HashSet<string> GetEffectiveExtensions(string perProjectCsv)
        {
            IEnumerable<string> source = ParseExtensions(perProjectCsv);
            if (!source.Any())
            {
                source = DefaultExtensions;
            }

            var set = new HashSet<string>(source, StringComparer.OrdinalIgnoreCase);
            foreach (var md in MarkdownExtensions)
            {
                set.Remove(md);
            }
            return set;
        }

        /// <summary>
        /// Parses a user-supplied allow-list string into normalized extensions.
        /// Accepts entries with or without a leading dot, separated by comma,
        /// semicolon, or whitespace. Blank/invalid entries are dropped.
        /// </summary>
        public static IEnumerable<string> ParseExtensions(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                yield break;
            }

            var tokens = csv.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in tokens)
            {
                var t = raw.Trim().ToLowerInvariant();
                if (t.Length == 0)
                {
                    continue;
                }
                if (!t.StartsWith("."))
                {
                    t = "." + t;
                }
                if (t.Length > 1)
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// True iff <paramref name="fullPath"/> is an eligible text file for the
        /// given effective allow-list. Never true for markdown extensions.
        /// </summary>
        public static bool IsEligibleTextFile(string fullPath, HashSet<string> effectiveExtensions)
        {
            if (string.IsNullOrEmpty(fullPath) || effectiveExtensions == null || effectiveExtensions.Count == 0)
            {
                return false;
            }
            var ext = Path.GetExtension(fullPath);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }
            if (MarkdownExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            return effectiveExtensions.Contains(ext);
        }

        /// <summary>The central default allow-list as a canonical comma-separated string (for UI seeding).</summary>
        public static string DefaultExtensionsCsv => string.Join(",", DefaultExtensions);
    }
}
