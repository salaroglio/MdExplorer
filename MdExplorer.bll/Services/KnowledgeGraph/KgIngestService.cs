using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                var hash = ComputeMd5(content);
                result.ContentHash = hash;

                if (!string.IsNullOrEmpty(f.PreviousHash) && string.Equals(f.PreviousHash, hash, StringComparison.Ordinal))
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

        private static string MakeSourceDocPath(string projectRootPath, string kgFileAbsolutePath)
        {
            var rel = Path.GetRelativePath(projectRootPath, kgFileAbsolutePath);
            return rel.Replace('\\', '/');
        }
    }
}
