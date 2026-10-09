using Markdig;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.E2e
{
    /// <summary>What <see cref="E2eSupportFiles.Ensure"/> did, paths relative to the project.</summary>
    public sealed class E2eSupportUpdate
    {
        public List<string> Created { get; } = new();
        public List<string> Updated { get; } = new();

        /// <summary>Support files without MdExplorer's marker: the user's, left as they are.</summary>
        public List<string> Customized { get; } = new();
    }

    /// <summary>
    /// The support files of the scripts (<c>E2eTests.csproj</c>, <c>E2eSupport.cs</c>, <c>e2e.runsettings</c>) are
    /// written by MdExplorer, not copied by the agent from the skill (P4 of sprint 2026-09-28-Script-E2E-Affidabili):
    /// the same everywhere, and updated with MdExplorer. Their text is the skill's own examples — the one source, the
    /// one the skill's tests check — taken from the skill MdExplorer carries.
    /// </summary>
    public static class E2eSupportFiles
    {
        /// <summary>File name → the id of the skill's example that holds its text.</summary>
        public static readonly IReadOnlyList<(string File, string Example)> Files = new[]
        {
            ("E2eTests.csproj", "csproj"),
            ("E2eSupport.cs", "supporto"),
            ("e2e.runsettings", "runsettings"),
        };

        /// <summary>In every support file MdExplorer writes: without it the file is the user's.</summary>
        public const string Marker = "MdExplorer, skill mde-e2e";

        /// <summary>
        /// The runsettings the agent copied from the skill up to its version 4, which had no marker (with Chrome, or
        /// Edge as the skill allowed): MdExplorer's too, so brought up to date instead of kept as the user's.
        /// </summary>
        private static readonly string[] UnmarkedPrevious =
        {
            "<RunSettings><Playwright><BrowserName>chromium</BrowserName><LaunchOptions><Channel>chrome</Channel><Headless>true</Headless></LaunchOptions></Playwright></RunSettings>",
            "<RunSettings><Playwright><BrowserName>chromium</BrowserName><LaunchOptions><Channel>msedge</Channel><Headless>true</Headless></LaunchOptions></Playwright></RunSettings>",
        };

        /// <summary>The text of each support file, from the examples of the mde-e2e skill.</summary>
        public static IReadOnlyDictionary<string, string> FromSkill(string skill)
        {
            var document = Markdown.Parse(skill, new MarkdownPipelineBuilder().UseYamlFrontMatter().Build());
            var examples = document.Descendants<FencedCodeBlock>()
                .Where(b => (b.Arguments ?? "").StartsWith("esempio="))
                .ToDictionary(b => b.Arguments.Substring("esempio=".Length).Trim(), b => b.Lines.ToString() + "\n");
            var files = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (file, example) in Files)
            {
                if (!examples.TryGetValue(example, out var text))
                    throw new InvalidOperationException($"La skill mde-e2e incorporata non ha l'esempio '{example}' ({file}).");
                if (!text.Contains(Marker, StringComparison.Ordinal))
                    throw new InvalidOperationException($"L'esempio '{example}' della skill mde-e2e non contiene il marcatore «{Marker}».");
                files[file] = text;
            }
            return files;
        }

        /// <summary>
        /// Writes the support files for <paramref name="testFiles"/>: next to the tests project that already serves
        /// a test (in its folder or above), otherwise — when <paramref name="create"/> — in the test's folder.
        /// Shallower folders first, so a folder launch never creates a project below one it has just created (two
        /// projects would compile the same scripts twice). A file is written only when its text differs: a csproj
        /// rewritten for nothing would look newer than its packages and ask for a new download.
        /// </summary>
        public static E2eSupportUpdate Ensure(IEnumerable<string> testFiles, string projectRoot, IReadOnlyDictionary<string, string> contents, bool create)
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectRoot));
            var update = new E2eSupportUpdate();
            var done = new HashSet<string>(StringComparer.Ordinal);
            foreach (var test in testFiles.Select(Path.GetFullPath).OrderBy(t => t.Count(c => c == Path.DirectorySeparatorChar)))
            {
                var project = E2eReplay.FindProject(test, root);
                if (project == null && !create) continue;
                var folder = project != null ? Path.GetDirectoryName(project)! : Path.GetDirectoryName(test)!;
                if (!done.Add(folder)) continue;

                foreach (var (file, _) in Files)
                {
                    var path = Path.Combine(folder, file);
                    var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
                    var wanted = contents[file];
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, wanted);
                        update.Created.Add(relative);
                        continue;
                    }
                    var current = File.ReadAllText(path);
                    if (Normalize(current) == Normalize(wanted)) continue;
                    if (!current.Contains(Marker, StringComparison.Ordinal) && !UnmarkedPrevious.Contains(Compact(current)))
                    {
                        update.Customized.Add(relative);
                        continue;
                    }
                    File.WriteAllText(path, wanted);
                    update.Updated.Add(relative);
                }
            }
            return update;
        }

        private static string Compact(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", "");

        /// <summary>Line endings do not make a file different (git on Windows may turn them into CRLF).</summary>
        private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd();
    }
}
