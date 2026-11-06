using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Service.Models;

namespace MdExplorer.Service.Services
{
    /// <summary>
    /// The folders tagged <c>program</c> in <c>.development.yml</c>, as absolute paths. The tree shows a folder only
    /// when it holds markdown, with one exception (19/10/2026): a program folder that exists and holds files shows
    /// anyway — its markdown may have been deleted, or it never had any, and without the row there is no way to
    /// reach «Ambiente di sviluppo» and «Grafo del codice». The exception is the folder's alone: it does not make
    /// the folders above it visible (they follow the markdown rule, the eye reveals them).
    /// </summary>
    public sealed class ProgramFolders
    {
        private const string ProgramTag = "program";
        private readonly HashSet<string> _paths;

        private ProgramFolders(HashSet<string> paths) => _paths = paths;

        /// <summary>Absolute paths of the program folders, the project root excluded (the root always shows).</summary>
        public IReadOnlyCollection<string> Paths => _paths;

        public static ProgramFolders From(string projectPath, IEnumerable<DevelopmentFolder> folders)
        {
            var root = Normalize(projectPath);
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in folders ?? Enumerable.Empty<DevelopmentFolder>())
            {
                if (string.IsNullOrWhiteSpace(f?.Path) || f.Tags == null
                    || !f.Tags.Contains(ProgramTag, StringComparer.OrdinalIgnoreCase))
                    continue;
                var relative = f.Path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)
                    .Trim(Path.DirectorySeparatorChar);
                var full = Normalize(Path.Combine(projectPath, relative));
                if (!full.Equals(root, StringComparison.OrdinalIgnoreCase))
                    paths.Add(full);
            }
            return new ProgramFolders(paths);
        }

        /// <summary>Reads <c>.development.yml</c> at the project root (strict read); no file = no program folders.</summary>
        public static ProgramFolders Load(string projectPath)
        {
            var path = Path.Combine(projectPath, ".development.yml");
            if (!File.Exists(path))
                return From(projectPath, null);
            return From(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path)?.Folders);
        }

        public bool IsProgram(string folder) => _paths.Contains(Normalize(folder));

        /// <summary>
        /// True when <paramref name="folder"/> is a program folder that holds at least one file through
        /// non-ignored folders: the tree shows it even without markdown.
        /// </summary>
        public bool ShowsWithoutMarkdown(string folder, string projectPath, FoldersIgnoreService foldersIgnore) =>
            IsProgram(folder) && FolderRevealableContent.HasAnyFile(folder, projectPath, foldersIgnore);

        private static string Normalize(string path) =>
            Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
