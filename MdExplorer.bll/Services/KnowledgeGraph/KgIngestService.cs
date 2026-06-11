using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    /// <summary>
    /// Executes per-document <c>.kg.cypher</c> scripts against a Neo4j instance.
    ///
    /// Contract with the skill that emits the file:
    /// - Every <c>MERGE</c>/<c>CREATE</c> of a node or edge MUST include the
    ///   properties <c>{sourceDoc: $doc, projectId: $pid, graph: $graph}</c>
    ///   so this service can locate and remove the document's previous
    ///   contribution before re-running the file. Nodes/edges WITHOUT these
    ///   tags are not tracked and will accumulate stale state.
    /// - Every <c>MERGE</c>/<c>CREATE</c> should set a <c>description</c>
    ///   property (free-text, in the project language) explaining the
    ///   semantic meaning of the node/edge for AI consumers.
    /// - Statements are separated by <c>;</c>. Skill authors should keep
    ///   <c>;</c> out of string literals.
    ///
    /// This service does NOT validate the Cypher — it trusts the skill. The
    /// only guard is the cleanup-before-execute pass that keeps Neo4j in sync
    /// with the latest version of each .kg.cypher file.
    /// </summary>
    public class KgIngestService : IKgIngestService
    {
        private readonly ILogger<KgIngestService> _logger;

        public KgIngestService(ILogger<KgIngestService> logger)
        {
            _logger = logger;
        }

        public async Task<KgIngestResult> IngestKgFileAsync(
            Guid projectId,
            string projectRootPath,
            string kgFileAbsolutePath,
            string previousHash,
            string graphNamespace,
            IAsyncSession session,
            CancellationToken ct = default)
        {
            var batch = new[]
            {
                new KgBatchFile
                {
                    KgFileAbsolutePath = kgFileAbsolutePath,
                    PreviousHash = previousHash,
                    GraphNamespace = graphNamespace
                }
            };
            var results = await IngestKgFilesAsync(projectId, projectRootPath, batch, session, ct).ConfigureAwait(false);
            return results[0];
        }

        public async Task<List<KgIngestResult>> IngestKgFilesAsync(
            Guid projectId,
            string projectRootPath,
            IEnumerable<KgBatchFile> files,
            IAsyncSession session,
            CancellationToken ct = default)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("projectId is required", nameof(projectId));
            if (string.IsNullOrEmpty(projectRootPath)) throw new ArgumentException("projectRootPath is required", nameof(projectRootPath));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var results = new List<KgIngestResult>();
            foreach (var f in files ?? Array.Empty<KgBatchFile>())
            {
                if (f == null || string.IsNullOrEmpty(f.KgFileAbsolutePath)) continue;
                ct.ThrowIfCancellationRequested();

                var sourceDoc = MakeSourceDocPath(projectRootPath, f.KgFileAbsolutePath);
                var result = new KgIngestResult
                {
                    SourceDocPath = sourceDoc,
                    GraphNamespace = f.GraphNamespace
                };

                if (!File.Exists(f.KgFileAbsolutePath))
                {
                    result.Error = "File not found on disk";
                    results.Add(result);
                    continue;
                }

                var content = await File.ReadAllTextAsync(f.KgFileAbsolutePath, ct).ConfigureAwait(false);
                var hashPre = ComputeMd5(content);
                result.ContentHash = hashPre;

                if (!string.IsNullOrEmpty(f.PreviousHash) && string.Equals(f.PreviousHash, hashPre, StringComparison.Ordinal))
                {
                    result.Skipped = true;
                    results.Add(result);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(f.GraphNamespace))
                {
                    result.Error = "Folder has no knowledgeGraph.namespace in .development.yml; cannot ingest.";
                    results.Add(result);
                    continue;
                }

                var statements = SplitCypherStatements(content);
                var parameters = new Dictionary<string, object>
                {
                    ["doc"] = sourceDoc,
                    ["pid"] = projectId.ToString(),
                    ["graph"] = f.GraphNamespace
                };

                try
                {
                    var counts = await session.ExecuteWriteAsync(async tx =>
                    {
                        // Cleanup: drop everything previously tagged with this sourceDoc
                        // so re-execution produces a clean state, even when concepts
                        // were renamed/removed between versions.
                        await tx.RunAsync(@"
                            MATCH (n {sourceDoc: $doc, projectId: $pid})
                            DETACH DELETE n
                        ", parameters).ConfigureAwait(false);

                        await tx.RunAsync(@"
                            MATCH ()-[r {sourceDoc: $doc, projectId: $pid}]->()
                            DELETE r
                        ", parameters).ConfigureAwait(false);

                        int nodes = 0;
                        int edges = 0;
                        foreach (var stmt in statements)
                        {
                            var cursor = await tx.RunAsync(stmt, parameters).ConfigureAwait(false);
                            var summary = await cursor.ConsumeAsync().ConfigureAwait(false);
                            nodes += summary.Counters.NodesCreated;
                            edges += summary.Counters.RelationshipsCreated;
                        }
                        return (nodes, edges);
                    }).ConfigureAwait(false);

                    result.NodeCount = counts.nodes;
                    result.EdgeCount = counts.edges;
                    _logger.LogInformation("[KgIngest] {Doc} (graph={Graph}): +{N} nodes, +{E} edges",
                        sourceDoc, f.GraphNamespace, counts.nodes, counts.edges);

                    // Post-ingest writeback of "// sourceDocHash: <md5>" header.
                    // Must come AFTER a successful ingest, and the new file MD5
                    // must overwrite result.ContentHash — that hash is what
                    // KgSyncOrchestrator persists, and the watcher then uses it
                    // to gate the self-write event (file MD5 == DB hash → skip).
                    if (!string.IsNullOrEmpty(f.SourceMdAbsolutePath) && File.Exists(f.SourceMdAbsolutePath))
                    {
                        try
                        {
                            var mdContent = await File.ReadAllTextAsync(f.SourceMdAbsolutePath, ct).ConfigureAwait(false);
                            var mdHash = ComputeMd5(mdContent);
                            var newContent = UpdateSourceDocHashHeader(content, mdHash);
                            if (!string.Equals(newContent, content, StringComparison.Ordinal))
                            {
                                await File.WriteAllTextAsync(f.KgFileAbsolutePath, newContent, ct).ConfigureAwait(false);
                                result.ContentHash = ComputeMd5(newContent);
                            }
                        }
                        catch (Exception writeEx)
                        {
                            // Writeback failure is non-fatal: ingest already succeeded.
                            // ContentHash stays = hashPre so the next watcher event will
                            // see "file unchanged" and skip, while the drift detector
                            // gracefully reports stale (header missing).
                            _logger.LogWarning(writeEx, "[KgIngest] sourceDocHash writeback failed for {Doc}", sourceDoc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                    _logger.LogWarning(ex, "[KgIngest] {Doc} failed", sourceDoc);
                }

                results.Add(result);
            }

            return results;
        }

        public async Task ResetProjectAsync(Guid projectId, IAsyncSession session, CancellationToken ct = default)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("projectId is required", nameof(projectId));
            if (session == null) throw new ArgumentNullException(nameof(session));

            await session.ExecuteWriteAsync<bool>(async tx =>
            {
                await tx.RunAsync(@"
                    MATCH (n {projectId: $pid})
                    DETACH DELETE n
                ", new { pid = projectId.ToString() }).ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
            _logger.LogInformation("[KgIngest] Reset project {ProjectId}", projectId);
        }

        // ===================================================================
        //   Private helpers
        // ===================================================================

        /// <summary>
        /// Splits a Cypher script into statements on <c>;</c> boundaries, with a
        /// state machine that ignores semicolons appearing inside string literals
        /// (<c>'...'</c> / <c>"..."</c>, honoring backslash escapes), inside
        /// <c>//</c> line comments, and inside <c>/* ... *\/</c> block comments.
        /// Comments are stripped; empty statements are dropped.
        /// A naive <c>Split(';')</c> is NOT safe here: skill-generated
        /// <c>description</c> values routinely contain natural-language semicolons.
        /// </summary>
        private static List<string> SplitCypherStatements(string content)
        {
            var statements = new List<string>();
            if (string.IsNullOrEmpty(content)) return statements;

            var sb = new StringBuilder();
            int i = 0, n = content.Length;
            while (i < n)
            {
                char c = content[i];

                // Line comment: // ... up to end of line
                if (c == '/' && i + 1 < n && content[i + 1] == '/')
                {
                    while (i < n && content[i] != '\n') i++;
                    continue;
                }
                // Block comment: /* ... */
                if (c == '/' && i + 1 < n && content[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < n && !(content[i] == '*' && content[i + 1] == '/')) i++;
                    i = Math.Min(i + 2, n);
                    continue;
                }
                // String literal: ' ... ' or " ... " — semicolons inside are literal text
                if (c == '\'' || c == '"')
                {
                    char quote = c;
                    sb.Append(c);
                    i++;
                    while (i < n)
                    {
                        char s = content[i];
                        if (s == '\\' && i + 1 < n)
                        {
                            sb.Append(s).Append(content[i + 1]);
                            i += 2;
                            continue;
                        }
                        sb.Append(s);
                        i++;
                        if (s == quote) break;
                    }
                    continue;
                }
                // Statement terminator (only reached when outside string/comment)
                if (c == ';')
                {
                    var stmt = sb.ToString().Trim();
                    if (stmt.Length > 0) statements.Add(stmt);
                    sb.Clear();
                    i++;
                    continue;
                }
                sb.Append(c);
                i++;
            }
            var tail = sb.ToString().Trim();
            if (tail.Length > 0) statements.Add(tail);
            return statements;
        }

        private static string ComputeMd5(string content)
        {
            using var md5 = MD5.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty);
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Public helper used by the drift detector to compute MD5 of an arbitrary file's content.</summary>
        public static string ComputeFileMd5(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath)) return null;
            var content = File.ReadAllText(absolutePath);
            return ComputeMd5(content);
        }

        private static string MakeSourceDocPath(string projectRootPath, string kgFileAbsolutePath)
        {
            var rel = Path.GetRelativePath(projectRootPath, kgFileAbsolutePath);
            return rel.Replace('\\', '/');
        }

        // ===================================================================
        //   sourceDocHash header — parser + writer
        // ===================================================================
        //
        // We write a single comment line at the very top of every .kg.cypher
        // after a successful ingest:
        //
        //     // sourceDocHash: <md5 of the .md source>
        //
        // The drift detector (FileSystemWatcherManager) re-reads this header
        // whenever the .md changes and compares it with the current MD5 of
        // the .md to decide whether the graph is still aligned.
        //
        // Stored INSIDE the .kg.cypher (not in UserDB) so it travels with the
        // file across clones and stays accurate on machines that didn't run
        // the ingest.

        private static readonly Regex SourceDocHashLineRegex = new Regex(
            @"^\s*//\s*sourceDocHash\s*:\s*(?<hash>[A-Za-z0-9]+)\s*$",
            RegexOptions.Compiled);

        /// <summary>
        /// Returns the hash inside the leading <c>// sourceDocHash: ...</c> line, or null
        /// if the header is missing/malformed. Inspects only the first ~5 non-empty lines.
        /// </summary>
        public static string ExtractSourceDocHash(string kgFileContent)
        {
            if (string.IsNullOrEmpty(kgFileContent)) return null;
            using var reader = new StringReader(kgFileContent);
            for (int i = 0; i < 5; i++)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                var m = SourceDocHashLineRegex.Match(line);
                if (m.Success) return m.Groups["hash"].Value.ToLowerInvariant();
                // First non-empty line is not our header → no header at all (don't scan deeper
                // to avoid matching a sourceDocHash mention buried inside a description).
                return null;
            }
            return null;
        }

        /// <summary>
        /// Returns <paramref name="content"/> with the leading <c>// sourceDocHash: ...</c>
        /// line replaced by one containing <paramref name="newHash"/>. If no header is
        /// present the new one is prepended. Preserves the original line ending style.
        /// </summary>
        public static string UpdateSourceDocHashHeader(string content, string newHash)
        {
            content ??= string.Empty;
            var newline = content.Contains("\r\n") ? "\r\n" : "\n";
            var lines = content.Split(new[] { newline }, StringSplitOptions.None).ToList();

            // Strip any leading sourceDocHash line(s) — defensive: in theory there's at
            // most one, but a malformed file could have more.
            while (lines.Count > 0 && SourceDocHashLineRegex.IsMatch(lines[0]))
                lines.RemoveAt(0);

            // Collapse a single leading blank line if present, so the header sits right
            // before the original first line of meaningful content.
            if (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
                lines.RemoveAt(0);

            var header = "// sourceDocHash: " + (newHash ?? string.Empty);
            return header + newline + string.Join(newline, lines);
        }
    }
}
