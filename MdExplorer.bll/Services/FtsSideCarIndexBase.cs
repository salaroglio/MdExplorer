using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Features.Services
{
    /// <summary>
    /// Shared FTS5 (trigram) side-car index engine. Two independent, non-overlapping
    /// indexes derive from this base:
    ///   • <see cref="MarkdownFtsService"/> — the markdown world (MdEngineFts_{hash}.db),
    ///   • <see cref="TextFtsService"/>     — the separate non-markdown text world
    ///     (MdEngineTextFts_{hash}.db).
    /// Each subclass only supplies its database-file prefix and table name; the SQL
    /// is identical, so the two worlds share code but never share data.
    ///
    /// Why a side-car file at all: the engine database is owned by NHibernate over
    /// System.Data.SQLite, whose native binary ships WITHOUT the FTS5 module. This
    /// base uses Microsoft.Data.Sqlite (e_sqlite3, FTS5 + trigram built in) on its
    /// own file. Connections are short-lived and pooled; schema is ensured on every
    /// open. A creation failure is a hard error (no silent fallback).
    /// </summary>
    public abstract class FtsSideCarIndexBase
    {
        private readonly string _appDataPath;
        private readonly ILogger _logger;

        /// <summary>Side-car database filename prefix, e.g. "MdEngineFts_" (hash + ".db" appended).</summary>
        protected abstract string DbFilePrefix { get; }

        /// <summary>Virtual FTS5 table name inside the side-car database.</summary>
        protected abstract string TableName { get; }

        /// <summary>Human-readable tag used in log lines, e.g. "MarkdownFts" / "TextFts".</summary>
        protected abstract string LogTag { get; }

        protected FtsSideCarIndexBase(string appDataPath, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(appDataPath))
            {
                throw new ArgumentException("appDataPath is required", nameof(appDataPath));
            }
            _appDataPath = appDataPath;
            _logger = logger;
        }

        public string GetDatabasePath(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                throw new InvalidOperationException(
                    "Full-text search requires an open project: projectPath is empty. " +
                    "Pass the ConnectionId of a connection that opened a project.");
            }
            var normalizedPath = Path.GetFullPath(projectPath);
            var hash = Helper.HGetHashString(normalizedPath);
            return Path.Combine(_appDataPath, $"{DbFilePrefix}{hash}.db");
        }

        private SqliteConnection OpenConnection(string projectPath)
        {
            var connection = new SqliteConnection($"Data Source={GetDatabasePath(projectPath)}");
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000; PRAGMA synchronous = NORMAL; " +
                $"CREATE VIRTUAL TABLE IF NOT EXISTS {TableName} " +
                "USING fts5(content, markdownFileId UNINDEXED, path UNINDEXED, fileName UNINDEXED, tokenize='trigram');";
            cmd.ExecuteNonQuery();
            return connection;
        }

        /// <summary>Atomically replaces the whole index for a project.</summary>
        protected void RebuildIndexCore(string projectPath,
            IEnumerable<(Guid Id, string Path, string FileName, string Content)> entries)
        {
            using var connection = OpenConnection(projectPath);
            using var transaction = connection.BeginTransaction();

            using (var delete = connection.CreateCommand())
            {
                delete.Transaction = transaction;
                delete.CommandText = $"DELETE FROM {TableName}";
                delete.ExecuteNonQuery();
            }

            var count = 0;
            using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText =
                    $"INSERT INTO {TableName} (content, markdownFileId, path, fileName) " +
                    "VALUES ($content, $id, $path, $fileName)";
                var pContent = insert.Parameters.Add("$content", SqliteType.Text);
                var pId = insert.Parameters.Add("$id", SqliteType.Text);
                var pPath = insert.Parameters.Add("$path", SqliteType.Text);
                var pFileName = insert.Parameters.Add("$fileName", SqliteType.Text);

                foreach (var entry in entries)
                {
                    pContent.Value = entry.Content ?? string.Empty;
                    pId.Value = entry.Id.ToString();
                    pPath.Value = entry.Path;
                    pFileName.Value = entry.FileName;
                    insert.ExecuteNonQuery();
                    count++;
                }
            }

            transaction.Commit();
            _logger?.LogInformation("[{Tag}] Index rebuilt for '{ProjectPath}': {Count} files", LogTag, projectPath, count);
        }

        /// <summary>Inserts or replaces the row for a single file (keyed by full path).</summary>
        public void UpsertFile(string projectPath, Guid fileId, string fullPath, string fileName, string content)
        {
            using var connection = OpenConnection(projectPath);
            using var transaction = connection.BeginTransaction();

            using (var delete = connection.CreateCommand())
            {
                delete.Transaction = transaction;
                delete.CommandText = $"DELETE FROM {TableName} WHERE path = $path";
                delete.Parameters.AddWithValue("$path", fullPath);
                delete.ExecuteNonQuery();
            }

            using (var insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText =
                    $"INSERT INTO {TableName} (content, markdownFileId, path, fileName) " +
                    "VALUES ($content, $id, $path, $fileName)";
                insert.Parameters.AddWithValue("$content", content ?? string.Empty);
                insert.Parameters.AddWithValue("$id", fileId.ToString());
                insert.Parameters.AddWithValue("$path", fullPath);
                insert.Parameters.AddWithValue("$fileName", fileName);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        /// <summary>Removes the row for a file. Safe to call when no row exists.</summary>
        public void DeleteFileByPath(string projectPath, string fullPath)
        {
            using var connection = OpenConnection(projectPath);
            using var delete = connection.CreateCommand();
            delete.CommandText = $"DELETE FROM {TableName} WHERE path = $path";
            delete.Parameters.AddWithValue("$path", fullPath);
            delete.ExecuteNonQuery();
        }

        /// <summary>
        /// Substring search (trigram). Terms shorter than 3 characters return an
        /// empty list (physical limit of the trigram tokenizer). Snippets are
        /// HTML-escaped with &lt;mark&gt;…&lt;/mark&gt; around matches.
        /// </summary>
        public List<ContentSearchResult> SearchContent(string projectPath, string searchTerm, int maxResults = 50)
        {
            var results = new List<ContentSearchResult>();
            var term = searchTerm?.Trim();

            // The trigram tokenizer cannot match terms shorter than 3 characters.
            if (string.IsNullOrEmpty(term) || term.Length < 3)
            {
                return results;
            }

            // Quoted phrase with embedded quotes doubled: substring semantics under
            // trigram, and no FTS5 query-syntax errors on characters like - . ( ).
            var ftsQuery = "\"" + term.Replace("\"", "\"\"") + "\"";

            using var connection = OpenConnection(projectPath);
            using var select = connection.CreateCommand();
            // char(1)/char(2) are snippet markers that can never appear in
            // HTML-escaped content; EscapeSnippet turns them into <mark> tags.
            select.CommandText =
                "SELECT markdownFileId, path, fileName, " +
                $"snippet({TableName}, 0, char(1), char(2), '…', 12) AS snip, " +
                $"bm25({TableName}) AS score " +
                $"FROM {TableName} WHERE {TableName} MATCH $q " +
                "ORDER BY score LIMIT $max";
            select.Parameters.AddWithValue("$q", ftsQuery);
            select.Parameters.AddWithValue("$max", maxResults);

            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new ContentSearchResult
                {
                    MarkdownFileId = Guid.TryParse(reader.GetString(0), out var id) ? id : Guid.Empty,
                    Path = reader.GetString(1),
                    FileName = reader.GetString(2),
                    Snippet = EscapeSnippet(reader.GetString(3)),
                    Score = reader.GetDouble(4)
                });
            }

            return results;
        }

        private static string EscapeSnippet(string snippet)
        {
            if (string.IsNullOrEmpty(snippet))
            {
                return snippet;
            }
            var escaped = System.Net.WebUtility.HtmlEncode(snippet);
            return escaped.Replace("\u0001", "<mark>").Replace("\u0002", "</mark>");
        }
    }
}
