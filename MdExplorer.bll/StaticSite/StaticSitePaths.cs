using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MdExplorer.Features.StaticSite
{
    /// <summary>
    /// Where each file goes in the export, and how one exported file points at another. The zip keeps
    /// the project's folders, so a relative link written by the author (<c>../costi.md</c>) stays right
    /// once the extension changes; MdExplorer's own files (reveal.js, the scripts) go under
    /// <see cref="AssetFolder"/>. Paths here are relative, with <c>/</c>, never starting with one.
    /// </summary>
    public static class StaticSitePaths
    {
        /// <summary>The folder of MdExplorer's files (a copy of the service's wwwroot, the part the pages use).</summary>
        public const string AssetFolder = "_mde";

        /// <summary>
        /// The zip path of a project file. A markdown file becomes its page: <c>x.md</c> → <c>x.html</c>, or
        /// <c>x.md.html</c> when the project already has an <c>x.html</c> (<paramref name="projectHas"/>);
        /// a folder summary <c>x.md.directory</c> → <c>x.md.directory.html</c>. Any other file keeps its path.
        /// </summary>
        public static string OutputOf(string projectPath, Func<string, bool> projectHas)
        {
            if (IsMarkdown(projectPath))
            {
                if (projectPath.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase))
                {
                    return projectPath + ".html";
                }
                var page = projectPath.Substring(0, projectPath.Length - ".md".Length) + ".html";
                return projectHas(page) ? projectPath + ".html" : page;
            }
            return projectPath;
        }

        /// <summary>The zip path of one of MdExplorer's files (<c>reveal/dist/reveal.js</c>).</summary>
        public static string OutputOfAsset(string assetPath) => AssetFolder + "/" + assetPath;

        public static bool IsMarkdown(string path)
            => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase);

        public static bool IsHtml(string path)
            => path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);

        public static bool IsCss(string path) => path.EndsWith(".css", StringComparison.OrdinalIgnoreCase);

        /// <summary>The folder of a path (<c>a/b/c.md</c> → <c>a/b</c>), empty at the root.</summary>
        public static string FolderOf(string path)
        {
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }

        /// <summary>
        /// <paramref name="relative"/> (decoded, with <c>/</c>) resolved against <paramref name="folder"/>:
        /// <c>.</c> and <c>..</c> taken out. Null when it climbs above the root: a file outside the project.
        /// </summary>
        public static string Combine(string folder, string relative)
        {
            var parts = new List<string>();
            foreach (var segment in (folder + "/" + relative).Split('/'))
            {
                if (segment.Length == 0 || segment == ".") continue;
                if (segment == "..")
                {
                    if (parts.Count == 0) return null;
                    parts.RemoveAt(parts.Count - 1);
                    continue;
                }
                parts.Add(segment);
            }
            return string.Join("/", parts);
        }

        /// <summary>
        /// How the file at <paramref name="from"/> points at the one at <paramref name="to"/> (both zip paths):
        /// relative, each segment encoded as a URL (<c>../Costi%20Q3.html</c>).
        /// </summary>
        public static string Link(string from, string to)
        {
            var fromParts = FolderOf(from).Split('/', StringSplitOptions.RemoveEmptyEntries);
            var toParts = to.Split('/');
            var common = 0;
            while (common < fromParts.Length && common < toParts.Length - 1 && fromParts[common] == toParts[common])
            {
                common++;
            }
            var up = Enumerable.Repeat("..", fromParts.Length - common);
            return string.Join("/", up.Concat(toParts.Skip(common).Select(Uri.EscapeDataString)));
        }

        /// <summary>From a zip path up to the zip's root: <c>a/b/c.html</c> → <c>../../</c>, empty at the root.</summary>
        public static string ToRoot(string from)
            => string.Concat(Enumerable.Repeat("../", FolderOf(from).Split('/', StringSplitOptions.RemoveEmptyEntries).Length));

        /// <summary>A path on disk under <paramref name="root"/>, from a relative one with <c>/</c>.</summary>
        public static string OnDisk(string root, string path)
            => Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
    }
}
