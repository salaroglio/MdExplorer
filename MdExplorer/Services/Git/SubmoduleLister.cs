using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>One submodule of a repository, with its path relative to the root that was asked.</summary>
    public sealed class ListedSubmodule
    {
        public string Path { get; init; }
        /// <summary>Recorded by the repository that holds it, but never downloaded into its folder.</summary>
        public bool NotInitialized { get; init; }
    }

    /// <summary>
    /// Which submodules a repository has, nested ones included, and whether each is downloaded.
    /// <para>
    /// It stands in for <c>git submodule status --recursive</c> where only those two answers are needed.
    /// That command also works out a readable name for every submodule's commit, with up to four
    /// <c>git describe</c> each: 16 processes for 4 submodules (traced on 23/11/2026), and on Windows,
    /// where starting a process is slow, 20–44 seconds on a machine that asked it twice a minute.
    /// Here it is one <c>git ls-files</c> per repository that has a <c>.gitmodules</c>, and none at all
    /// for a repository without one.
    /// </para>
    /// </summary>
    public static class SubmoduleLister
    {
        private const string GitlinkMode = "160000";

        public static async Task<List<ListedSubmodule>> ListAsync(INativeGitRunner git, string root, ILogger logger, CancellationToken ct = default)
        {
            var list = new List<ListedSubmodule>();
            await CollectAsync(git, root, string.Empty, list, logger, ct);
            return list;
        }

        private static async Task CollectAsync(INativeGitRunner git, string dir, string prefix, List<ListedSubmodule> list, ILogger logger, CancellationToken ct)
        {
            // git itself refuses a submodule that .gitmodules does not describe: without the file there is none.
            if (!File.Exists(Path.Combine(dir, ".gitmodules"))) return;

            // The index, like 'submodule status': a submodule is an entry of mode 160000, one per stage when
            // it is in conflict.
            var res = await git.RunAsync(dir, new[] { "ls-files", "--stage", "-z" }, ct);
            if (!res.Ok)
            {
                logger?.LogWarning("[Submodule] 'ls-files --stage' failed in '{Dir}': its submodules are not listed. {Why}", dir, res.Describe());
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in (res.Stdout ?? string.Empty).Split('\0', StringSplitOptions.RemoveEmptyEntries))
            {
                // "<mode> <sha> <stage>\t<path>"
                if (!entry.StartsWith(GitlinkMode + " ", StringComparison.Ordinal)) continue;
                var tab = entry.IndexOf('\t');
                if (tab < 0 || tab == entry.Length - 1) continue;
                var relative = entry.Substring(tab + 1);
                if (!seen.Add(relative)) continue;

                var subDir = Path.Combine(dir, relative.Replace('/', Path.DirectorySeparatorChar));
                var downloaded = IsDownloaded(subDir);
                list.Add(new ListedSubmodule { Path = prefix + relative, NotInitialized = !downloaded });
                if (downloaded) await CollectAsync(git, subDir, prefix + relative + "/", list, logger, ct);
            }
        }

        /// <summary>
        /// A downloaded submodule has a <c>.git</c> in its folder: a folder of its own, or a file that says
        /// where its repository is kept (<c>gitdir: ../.git/modules/…</c>), and that place must exist.
        /// </summary>
        internal static bool IsDownloaded(string subDir)
        {
            var dotGit = Path.Combine(subDir, ".git");
            if (Directory.Exists(dotGit)) return true;
            if (!File.Exists(dotGit)) return false;

            string first;
            try { using var reader = new StreamReader(dotGit); first = reader.ReadLine(); }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }

            const string marker = "gitdir:";
            if (first == null || !first.StartsWith(marker, StringComparison.Ordinal)) return false;
            var target = first.Substring(marker.Length).Trim();
            if (target.Length == 0) return false;
            if (!Path.IsPathRooted(target)) target = Path.Combine(subDir, target);
            return Directory.Exists(target);
        }
    }
}
