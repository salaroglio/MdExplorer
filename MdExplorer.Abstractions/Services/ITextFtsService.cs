using System;
using System.Collections.Generic;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Full-text content index over the project's NON-markdown text files
    /// (see <see cref="TextFileClassifier"/> for eligibility). Completely separate
    /// from <see cref="IMarkdownFtsService"/>: its own side-car SQLite database
    /// (MdEngineTextFts_{hash}.db, FTS5 trigram) and its own background job, so the
    /// markdown world is never affected. Only built/maintained when the project has
    /// <see cref="Entities.UserDB.Project.IndexAllTextFiles"/> ON.
    /// </summary>
    public interface ITextFtsService
    {
        /// <summary>Full path of the side-car database file for a project (existence not implied).</summary>
        string GetDatabasePath(string projectPath);

        /// <summary>Atomically replaces the whole text index for a project.</summary>
        void RebuildIndex(string projectPath, IReadOnlyCollection<TextFtsEntry> entries);

        /// <summary>Inserts or replaces the row for a single file (keyed by full path).</summary>
        void UpsertFile(string projectPath, Guid textFileId, string fullPath, string fileName, string content);

        /// <summary>Removes the row for a file. Safe to call when no row exists.</summary>
        void DeleteFileByPath(string projectPath, string fullPath);

        /// <summary>
        /// Substring search (trigram). Terms shorter than 3 characters return an
        /// empty list. Snippets are HTML-escaped with &lt;mark&gt;…&lt;/mark&gt; around matches.
        /// </summary>
        List<ContentSearchResult> SearchContent(string projectPath, string searchTerm, int maxResults = 50);
    }

    public class TextFtsEntry
    {
        public Guid TextFileId { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}
