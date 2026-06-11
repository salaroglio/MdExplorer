using System.Collections.Generic;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class FolderKgConfig
    {
        /// <summary>
        /// Logical knowledge-graph namespace (e.g. "cobol-domain", "impl-plan").
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// When false the folder is skipped by sync operations even if a namespace is set.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Absolute path of the folder this config applies to (only populated by enumeration).
        /// </summary>
        public string FolderAbsolutePath { get; set; }
    }

    public interface IFolderKgConfigResolver
    {
        /// <summary>
        /// Looks up <c>.development.yml</c> at the project root and returns the
        /// <c>knowledgeGraph</c> configuration for the folder whose absolute path is
        /// <paramref name="folderAbsolutePath"/>, or <c>null</c> if the folder has no
        /// such entry or the file does not exist.
        /// </summary>
        FolderKgConfig Resolve(string projectPath, string folderAbsolutePath);

        /// <summary>
        /// Enumerates every folder in <c>.development.yml</c> that declares a non-empty
        /// <c>knowledgeGraph.namespace</c> and is not explicitly disabled.
        /// Each returned config has <see cref="FolderKgConfig.FolderAbsolutePath"/> set.
        /// </summary>
        IList<FolderKgConfig> EnumerateConfiguredFolders(string projectPath);
    }
}
