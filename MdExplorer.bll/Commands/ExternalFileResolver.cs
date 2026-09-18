using MdExplorer.Abstractions.Models;
using System;
using System.IO;
using System.Text;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Path resolution shared by the include commands — ```text(path), ```html(path) and
    /// ```plantuml(@json|@yaml, path). One place, because the three used to carry three copies
    /// of it and a fix had to be made three times.
    ///
    /// Semantics, unchanged from what the commands documented:
    /// <list type="bullet">
    /// <item><c>./file</c>, <c>../file</c>, <c>file</c> — relative to the .md being rendered</item>
    /// <item><c>/file</c> — relative to the PROJECT root, not to the filesystem root</item>
    /// </list>
    /// Anything landing outside the project is refused.
    ///
    /// The collapsing of <c>.</c> and <c>..</c> is left to <see cref="Path.GetFullPath(string)"/>.
    /// IHelper.NormalizePath, used here before, rejoins the segments with a hardcoded backslash:
    /// on Linux it turned "docs/file.ttl" into "docs\file.ttl" and the file was never found.
    /// </summary>
    internal static class ExternalFileResolver
    {
        /// <summary>
        /// Resolves <paramref name="declaredPath"/> to an absolute path inside the project.
        /// Returns null and fills <paramref name="error"/> when it escapes the project root.
        /// </summary>
        public static string ResolveInsideProject(string declaredPath, RequestInfo requestInfo, out string error)
        {
            error = null;

            // Documents get written on Windows and read on Linux (and the other way round),
            // so accept both separators in the declared path.
            var normalizedDeclaration = (declaredPath ?? string.Empty).Replace('\\', '/').Trim();

            string relativePath;
            if (normalizedDeclaration.StartsWith("/"))
            {
                relativePath = normalizedDeclaration.TrimStart('/');
            }
            else
            {
                // Folder of the .md currently being rendered, relative to the project root.
                var currentFolder = Path.GetDirectoryName(requestInfo.CurrentQueryRequest) ?? string.Empty;
                relativePath = Path.Combine(currentFolder, normalizedDeclaration);
            }

            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            var projectRoot = Path.GetFullPath(requestInfo.CurrentRoot);
            var absolutePath = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

            // Trailing separator on the root, so that a sibling folder whose name merely starts
            // with the root's name is not mistaken for something inside the project.
            var rootWithSeparator = projectRoot.EndsWith(Path.DirectorySeparatorChar)
                ? projectRoot
                : projectRoot + Path.DirectorySeparatorChar;

            if (!absolutePath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                error = "il percorso esce dalla cartella del progetto";
                return null;
            }

            return absolutePath;
        }

        /// <summary>
        /// Resolves and reads the file. Returns null and fills <paramref name="error"/> with a
        /// message meant for the author when the file cannot be used.
        /// </summary>
        public static string ReadInsideProject(string declaredPath, RequestInfo requestInfo, int maxSizeBytes,
                                               out string absoluteFilePath, out string error)
        {
            absoluteFilePath = null;

            var absolutePath = ResolveInsideProject(declaredPath, requestInfo, out error);
            if (absolutePath == null) return null;

            if (!File.Exists(absolutePath))
            {
                error = $"file non trovato — cercato in {absolutePath}";
                return null;
            }

            var fileInfo = new FileInfo(absolutePath);
            if (fileInfo.Length > maxSizeBytes)
            {
                error = $"file troppo grande ({fileInfo.Length / 1024} KB): il limite è {maxSizeBytes / 1024} KB";
                return null;
            }

            absoluteFilePath = absolutePath;
            return File.ReadAllText(absolutePath, Encoding.UTF8);
        }
    }
}
