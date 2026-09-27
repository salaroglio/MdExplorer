using System;
using System.IO;

namespace MdExplorer.Features.E2e
{
    /// <summary>A resolved setting and where it comes from: a file path, or null for MdExplorer's default.</summary>
    public sealed record E2eSetting(bool Value, string Source);

    public sealed record E2eEffectiveRunSettings(E2eSetting DedicatedSession, E2eSetting CommitAfterRun, E2eSetting Headless);

    /// <summary>
    /// The settings a run of a test uses (D21-D24): each key is taken, one by one, from the first place
    /// that says it — the test's own front matter, then the <c>&lt;folder&gt;.md.directory</c> of its
    /// folder, then of every folder above it up to the project root — and otherwise from MdExplorer's
    /// defaults. The particular wins over the general.
    /// </summary>
    public static class E2eRunSettingsResolver
    {
        public static readonly E2eRunSettings Defaults = new(DedicatedSession: true, CommitAfterRun: false, Headless: true);

        /// <summary>The settings file of a folder: <c>&lt;folder&gt;/&lt;folder name&gt;.md.directory</c>.</summary>
        public static string FolderSettingsPath(string folder)
        {
            var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder));
            return Path.Combine(full, Path.GetFileName(full) + ".md.directory");
        }

        /// <summary>The settings for a test file; for a folder pass <paramref name="path"/> = the folder.</summary>
        public static E2eEffectiveRunSettings Resolve(string path, string projectRoot)
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectRoot));
            var full = Path.GetFullPath(path);
            var isFolder = Directory.Exists(full);

            if (!IsInside(full, root))
                throw new ArgumentException($"'{full}' non è dentro il progetto '{root}'.", nameof(path));

            E2eSetting session = null, commit = null, headless = null;
            void Take(E2eRunSettings run, string source)
            {
                if (session == null && run.DedicatedSession != null) session = new E2eSetting(run.DedicatedSession.Value, source);
                if (commit == null && run.CommitAfterRun != null) commit = new E2eSetting(run.CommitAfterRun.Value, source);
                if (headless == null && run.Headless != null) headless = new E2eSetting(run.Headless.Value, source);
            }

            if (!isFolder) Take(ReadFile(full), full);

            var folder = Path.TrimEndingDirectorySeparator(isFolder ? full : Path.GetDirectoryName(full));
            while (true)
            {
                var settings = FolderSettingsPath(folder);
                if (File.Exists(settings)) Take(ReadFile(settings), settings);
                if (string.Equals(folder, root, PathComparison) || session != null && commit != null && headless != null) break;
                folder = Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(folder));
            }

            return new E2eEffectiveRunSettings(
                session ?? new E2eSetting(Defaults.DedicatedSession.Value, null),
                commit ?? new E2eSetting(Defaults.CommitAfterRun.Value, null),
                headless ?? new E2eSetting(Defaults.Headless.Value, null));
        }

        private static E2eRunSettings ReadFile(string path) =>
            E2eFrontMatter.ReadRunSettings(File.ReadAllText(path), Path.GetFileName(path));

        private static bool IsInside(string path, string root) =>
            string.Equals(Path.TrimEndingDirectorySeparator(path), root, PathComparison)
            || path.StartsWith(root + Path.DirectorySeparatorChar, PathComparison);

        private static StringComparison PathComparison =>
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    }
}
