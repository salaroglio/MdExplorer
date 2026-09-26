using System;
using System.IO;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    /// <summary>What the Knowledge Graph needs to show and open a file node.</summary>
    public sealed class KnowledgeGraphFileInfo
    {
        /// <summary>markdown, json, text, word, powerpoint, excel, pdf, image, other — the icon.</summary>
        public string Kind { get; init; }

        /// <summary><see cref="KnowledgeGraphFiles.OpenInPage"/>, <see cref="KnowledgeGraphFiles.OpenWithApplication"/> or <see cref="KnowledgeGraphFiles.CannotOpen"/>.</summary>
        public string OpenWith { get; init; }

        public bool Exists { get; init; }
    }

    /// <summary>
    /// Paths and kinds of the files in the Knowledge Graph of links between documents.
    ///
    /// <para>
    /// Paths. <c>LinkInsideMarkdown</c> keeps a linked path as it was built when indexing: on
    /// Linux with "\" (measured 17/09/2026: <c>\tmp\…\beta.md</c> in a <c>/tmp/…</c> project),
    /// and with ".." for a relative link. Compared as they are, the project root is not found,
    /// the node's relative path comes out absolute and the node cannot be opened, and its label
    /// is the whole path. Every path is normalized the same way before any comparison.
    /// </para>
    ///
    /// <para>
    /// Opening, as the page does it: a .md, and a text file the page shows as colored source
    /// (<c>MdExplorerController.TextFileToShow</c>), open in the page; the extensions of the
    /// project's <c>.mdapplicationtoopen</c> open with their application, as
    /// <c>FromLinkToApplication</c> does for the links of the page; any other existing file
    /// opens with the system's application too — the page could only show its raw bytes. A
    /// missing file does not open.
    /// </para>
    /// </summary>
    public static class KnowledgeGraphFiles
    {
        public const string OpenInPage = "page";
        public const string OpenWithApplication = "application";
        public const string OpenInBrowser = "browser";
        public const string CannotOpen = "none";

        /// <summary>Separators of this system; a rooted path also loses its "." and ".." segments.</summary>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            var separated = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            return Path.IsPathRooted(separated) ? Path.GetFullPath(separated) : separated;
        }

        /// <summary>The path inside the project, "/"-separated; null when the file is not inside it.</summary>
        public static string ToProjectRelative(string fullPath, string projectRoot)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(projectRoot))
            {
                return null;
            }
            var full = NormalizePath(fullPath);
            var root = NormalizePath(projectRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return full.Substring(root.Length).Replace(Path.DirectorySeparatorChar, '/');
        }

        /// <param name="fullPath">The file, already inside the project for <see cref="OpenInPage"/> to make sense.</param>
        /// <param name="opensWithApplication">Whether the project opens this extension (no dot) with its application.</param>
        public static KnowledgeGraphFileInfo Describe(string fullPath, Func<string, bool> opensWithApplication)
        {
            if (opensWithApplication == null)
            {
                throw new ArgumentNullException(nameof(opensWithApplication));
            }

            var path = NormalizePath(fullPath);
            var extension = Path.GetExtension(path ?? string.Empty).TrimStart('.').ToLowerInvariant();
            var exists = !string.IsNullOrEmpty(path) && File.Exists(path);
            var isText = exists && extension != "md" && IsText(path);

            string openWith;
            if (!exists)
            {
                openWith = CannotOpen;
            }
            else if (extension == "md")
            {
                openWith = OpenInPage;
            }
            else if (opensWithApplication(extension) || RenderedByTheBrowser(extension))
            {
                openWith = OpenWithApplication;
            }
            else
            {
                openWith = isText ? OpenInPage : OpenWithApplication;
            }

            return new KnowledgeGraphFileInfo { Kind = KindOf(extension, isText), OpenWith = openWith, Exists = exists };
        }

        /// <summary>
        /// A file that exists but cannot be read (locked, no permission) is not text: it opens
        /// with its application, which will say what is wrong — one unreadable file must not
        /// take the whole graph down.
        /// </summary>
        private static bool IsText(string path)
        {
            try
            {
                return TextFileView.IsText(path);
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        // What the page does not show as colored source even though it is text.
        private static bool RenderedByTheBrowser(string extension)
            => extension == "html" || extension == "htm" || extension == "svg";

        private static string KindOf(string extension, bool isText)
        {
            switch (extension)
            {
                case "md": return "markdown";
                case "json": case "jsonld": return "json";
                case "doc": case "docx": return "word";
                case "ppt": case "pptx": return "powerpoint";
                case "xls": case "xlsx": return "excel";
                case "pdf": return "pdf";
                case "png": case "jpg": case "jpeg": case "gif": case "svg": case "webp": case "bmp": return "image";
                default: return isText ? "text" : "other";
            }
        }
    }
}
