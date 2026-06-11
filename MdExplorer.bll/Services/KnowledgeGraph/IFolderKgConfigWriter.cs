namespace MdExplorer.Features.Services.KnowledgeGraph
{
    /// <summary>
    /// Ensures a folder declares a <c>knowledgeGraph.namespace</c> in
    /// <c>.development.yml</c>, creating one with a deterministic default when it is
    /// missing — so a freshly generated <c>.kg.cypher</c> always has a namespace to
    /// ingest into instead of being silently skipped.
    /// <para>
    /// The write is a full round-trip of the development config (so the other
    /// sections — compatibility, participants, icon … — are preserved), which is why
    /// the implementation lives in the Service project: <c>MdExplorer.bll</c> must not
    /// depend on the full <c>DevelopmentConfig</c> model.
    /// </para>
    /// </summary>
    public interface IFolderKgConfigWriter
    {
        /// <summary>
        /// Returns the folder's KG config. When the folder has no namespace yet, a new
        /// <c>folders[]</c> entry is written (namespace derived from the project folder
        /// name plus the folder's relative path, <c>enabled = true</c>) and returned.
        /// Idempotent: a folder that already declares a namespace is returned unchanged
        /// with no write. Returns <c>null</c> only when the project path is invalid.
        /// </summary>
        FolderKgConfig EnsureFolderConfig(string projectPath, string folderAbsolutePath);
    }
}
