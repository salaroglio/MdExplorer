using System;
using System.IO;
using System.Linq;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.Service.Services
{
    /// <summary>
    /// Whether the tree's eye has something to show in a folder, in one place for the two that need it: the
    /// structure sent when a folder is loaded (<c>MdFilesController</c>) and the live update sent when files
    /// appear or disappear on disk (<c>FileSystemWatcherManager</c>, 28/09/2026). Two copies would drift, and the
    /// eye would say one thing on load and another after a change. Two rules: <see cref="HasHidden"/> for a
    /// folder of the tree, <see cref="Has"/> for a revealed (green) one.
    /// </summary>
    public static class FolderRevealableContent
    {
        /// <summary>
        /// The eye of a folder of the tree (the load rule of <c>ExploreNodes</c>): it hides something when it
        /// holds a file that is not markdown (the TOC sidecar excepted), or a non-ignored subfolder with no
        /// markdown anywhere below it — both are left out of the tree. Markdown files are shown, so they do not
        /// count. For a folder that is itself revealed (green) the rule is <see cref="Has"/>.
        /// </summary>
        public static bool HasHidden(string folder, string projectPath, IMdIgnoreService mdIgnore, FoldersIgnoreService foldersIgnore)
        {
            try
            {
                if (Directory.GetFiles(folder).Any(IsExtraFile))
                    return true;
                foreach (var d in Directory.GetDirectories(folder).Where(_ => !_.Contains(".md")))
                {
                    if (foldersIgnore.ShouldIgnoreFolderForProject(d, projectPath))
                        continue;
                    if (!HasVisibleMarkdown(d, projectPath, mdIgnore, foldersIgnore))
                        return true;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Unreadable folder (gone, locked) → no eye rather than a lie.
            }
            return false;
        }

        /// <summary>The same test as the tree's load: not markdown, and not the TOC sidecar.</summary>
        public static bool IsExtraFile(string filePath) =>
            Path.GetExtension(filePath) != ".md"
            && !filePath.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase);

        /// <summary>A non-ignored markdown file in <paramref name="folder"/> or below, through non-ignored folders.</summary>
        private static bool HasVisibleMarkdown(string folder, string projectPath, IMdIgnoreService mdIgnore, FoldersIgnoreService foldersIgnore)
        {
            if (Directory.GetFiles(folder, "*.md").Any(f => Path.GetExtension(f) == ".md" && !mdIgnore.ShouldIgnorePath(f, projectPath)))
                return true;
            return Directory.GetDirectories(folder).Where(_ => !_.Contains(".md"))
                .Where(d => !foldersIgnore.ShouldIgnoreFolderForProject(d, projectPath))
                .Any(d => HasVisibleMarkdown(d, projectPath, mdIgnore, foldersIgnore));
        }

        /// <summary>
        /// True when a reveal of <paramref name="folder"/> would return at least one node — any non-ignored
        /// markdown file, any non-markdown file (excluding the TOC sidecar), or any non-ignored direct subfolder.
        /// An unreadable folder has no eye rather than a lie.
        /// </summary>
        public static bool Has(string folder, string projectPath, IMdIgnoreService mdIgnore, FoldersIgnoreService foldersIgnore)
        {
            try
            {
                foreach (var f in Directory.GetFiles(folder))
                {
                    if (f.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (Path.GetExtension(f) == ".md")
                    {
                        if (!mdIgnore.ShouldIgnorePath(f, projectPath))
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                foreach (var d in Directory.GetDirectories(folder).Where(_ => !_.Contains(".md")))
                {
                    if (!foldersIgnore.ShouldIgnoreFolderForProject(d, projectPath))
                        return true;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Unreadable folder (gone, locked) → no eye rather than a lie.
            }
            return false;
        }
    }
}
