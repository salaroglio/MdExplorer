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
    /// FTS5 (trigram) full-text index in a side-car SQLite database.
    ///
    /// Why a side-car file: the engine database is owned by NHibernate over
    /// System.Data.SQLite, whose native binary ships WITHOUT the FTS5 module
    /// (verified: ENABLE_FTS5=0 up to and including 1.0.119, all platforms).
    /// This service therefore uses Microsoft.Data.Sqlite (e_sqlite3, FTS5 +
    /// trigram built in) on its own database file MdEngineFts_{hash}.db, where
    /// {hash} matches the engine DB hash for the same project path.
    ///
    /// Connections are short-lived and pooled (Microsoft.Data.Sqlite pools by
    /// default), so the service is safe to call from any thread. Schema is
    /// ensured on every connection open; a creation failure is a hard error.
    /// </summary>
    public class MarkdownFtsService : IMarkdownFtsService
    {
        private const string TableName = "MarkdownContentFts";
        private readonly string _appDataPath;
        private readonly ILogger<MarkdownFtsService> _logger;

        public MarkdownFtsService(string appDataPath, ILogger<MarkdownFtsService> logger)
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
            return Path.Combine(_appDataPath, $"MdEngineFts_{hash}.db");
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

        public void RebuildIndex(string projectPath, IReadOnlyCollection<MarkdownFtsEntry> entries)
        {
            using var connection = OpenConnection(projectPath);
            using var transaction = connection.BeginTransaction();

            using (var delete = connection.CreateCommand())
            {
                delete.Transaction = transaction;
                delete.CommandText = $"DELETE FROM {TableName}";
                delete.ExecuteNonQuery();
            }

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
                    pId.Value = entry.MarkdownFileId.ToString();
                    pPath.Value = entry.Path;
                    pFileName.Value = entry.FileName;
                    insert.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            _logger.LogInformation("[MarkdownFts] Index rebuilt for '{ProjectPath}': {Count} files", projectPath, entries.Count);
        }

        public void UpsertFile(string projectPath, Guid markdownFileId, string fullPath, string fileName, string content)
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
                insert.Parameters.AddWithValue("$id", markdownFileId.ToString());
                insert.Parameters.AddWithValue("$path", fullPath);
                insert.Parameters.AddWithValue("$fileName", fileName);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public void DeleteFileByPath(string projectPath, string fullPath)
        {
            using var connection = OpenConnection(projectPath);
            using var delete = connection.CreateCommand();
            delete.CommandText = $"DELETE FROM {TableName} WHERE path = $path";
            delete.Parameters.AddWithValue("$path", fullPath);
            delete.ExecuteNonQuery();
        }

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
