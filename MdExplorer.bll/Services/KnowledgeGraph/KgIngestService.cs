using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class KgIngestService : IKgIngestService
    {
        private static readonly HashSet<string> AllowedRelationshipTypes = new(StringComparer.Ordinal)
        {
            "USES", "EXTENDS", "CONTRADICTS", "DERIVES_FROM", "IS_INSTANCE_OF",
            "DEPENDS_ON", "REQUIRES", "MITIGATES", "RELATED_TO", "REFERENCES"
        };

        private readonly ILogger<KgIngestService> _logger;

        public KgIngestService(ILogger<KgIngestService> logger)
        {
            _logger = logger;
        }

        // ===================================================================
        //   Public API
        // ===================================================================

        public async Task<KgIngestResult> IngestKgFileAsync(
            Guid projectId,
            string projectRootPath,
            string kgFileAbsolutePath,
            string previousHash,
            string graphNamespace,
            IAsyncSession session,
            CancellationToken ct = default)
        {
            var batch = new[] { new KgBatchFile {
                KgFileAbsolutePath = kgFileAbsolutePath,
                PreviousHash = previousHash,
                GraphNamespace = graphNamespace
            }};
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

            // ---- Phase A: read, hash, parse, validate types ----
            var prepared = new List<PreparedFile>();
            var results = new List<KgIngestResult>();
            foreach (var f in files ?? Array.Empty<KgBatchFile>())
            {
                if (f == null || string.IsNullOrEmpty(f.KgFileAbsolutePath)) continue;
                if (!File.Exists(f.KgFileAbsolutePath))
                {
                    results.Add(new KgIngestResult
                    {
                        SourceDocPath = MakeSourceDocPath(projectRootPath, f.KgFileAbsolutePath),
                        GraphNamespace = f.GraphNamespace,
                        Error = "File not found on disk"
                    });
                    continue;
                }

                var content = await File.ReadAllTextAsync(f.KgFileAbsolutePath, ct).ConfigureAwait(false);
                var hash = ComputeMd5(content);
                var sourceDoc = MakeSourceDocPath(projectRootPath, f.KgFileAbsolutePath);

                if (!string.IsNullOrEmpty(f.PreviousHash) && string.Equals(f.PreviousHash, hash, StringComparison.Ordinal))
                {
                    results.Add(new KgIngestResult
                    {
                        SourceDocPath = sourceDoc,
                        ContentHash = hash,
                        GraphNamespace = f.GraphNamespace,
                        Skipped = true
                    });
                    continue;
                }

                var parsed = KgFileParser.Parse(content);
                var invalidTypes = parsed.Relationships
                    .Where(r => !AllowedRelationshipTypes.Contains(r.Type))
                    .Select(r => $"'{r.Type}' (in edge {r.From} -> {r.To})")
                    .Distinct()
                    .ToList();
                if (invalidTypes.Count > 0)
                {
                    results.Add(new KgIngestResult
                    {
                        SourceDocPath = sourceDoc,
                        ContentHash = hash,
                        GraphNamespace = f.GraphNamespace,
                        Error = $"Invalid relationship type(s) — closed vocabulary is USES/EXTENDS/CONTRADICTS/DERIVES_FROM/IS_INSTANCE_OF/DEPENDS_ON/REQUIRES/MITIGATES/RELATED_TO/REFERENCES.",
                        ErrorDetails = invalidTypes
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(f.GraphNamespace))
                {
                    results.Add(new KgIngestResult
                    {
                        SourceDocPath = sourceDoc,
                        ContentHash = hash,
                        Error = "Folder has no knowledgeGraph.namespace in .development.yml; cannot ingest."
                    });
                    continue;
                }

                prepared.Add(new PreparedFile
                {
                    SourceDoc = sourceDoc,
                    Hash = hash,
                    GraphNamespace = f.GraphNamespace,
                    Concepts = parsed.Concepts,
                    Relationships = parsed.Relationships
                });
            }

            if (prepared.Count == 0)
            {
                return results;
            }

            // ---- Phase B: Pass 1 — global cleanup + MERGE all concepts in a single tx ----
            await session.ExecuteWriteAsync<bool>(async tx =>
            {
                foreach (var p in prepared)
                {
                    await CleanupSourceDocAsync(tx, projectId, p.SourceDoc).ConfigureAwait(false);
                    await MergeConceptsAsync(tx, projectId, p.GraphNamespace, p.SourceDoc, p.Concepts).ConfigureAwait(false);
                }
                return true;
            }).ConfigureAwait(false);

            // ---- Phase C: Pass 2 per-file (separate tx for isolation) ----
            foreach (var p in prepared)
            {
                var fileResult = new KgIngestResult
                {
                    SourceDocPath = p.SourceDoc,
                    ContentHash = p.Hash,
                    GraphNamespace = p.GraphNamespace,
                    ConceptCount = p.Concepts.Count
                };

                try
                {
                    await session.ExecuteWriteAsync<bool>(async tx =>
                    {
                        var validation = await ValidateRelationshipTargetsAsync(tx, projectId, p.Relationships).ConfigureAwait(false);
                        if (validation.Count > 0)
                        {
                            throw new KgIngestException(
                                $"{p.SourceDoc}: {validation.Count} cross-graph reference issue(s). Sync target graph(s) first or fix concept names.",
                                validation);
                        }
                        await MergeRelationshipsAsync(tx, projectId, p.GraphNamespace, p.SourceDoc, p.Relationships).ConfigureAwait(false);
                        return true;
                    }).ConfigureAwait(false);

                    fileResult.RelationshipCount = p.Relationships.Count;
                }
                catch (KgIngestException ex)
                {
                    fileResult.Error = ex.Message;
                    fileResult.ErrorDetails = ex.Details.ToList();
                    _logger.LogWarning("[KgIngest] {Doc}: {Err}", p.SourceDoc, ex.Message);
                }

                results.Add(fileResult);
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
                    MATCH (n:Concept {projectId: $pid})
                    DETACH DELETE n
                ", new { pid = projectId.ToString() });
                return true;
            }).ConfigureAwait(false);
            _logger.LogInformation("[KgIngest] Reset project {ProjectId}", projectId);
        }

        // ===================================================================
        //   Private helpers
        // ===================================================================

        private class PreparedFile
        {
            public string SourceDoc { get; set; }
            public string Hash { get; set; }
            public string GraphNamespace { get; set; }
            public List<KgConceptRow> Concepts { get; set; }
            public List<KgRelationshipRow> Relationships { get; set; }
        }

        private static async Task CleanupSourceDocAsync(IAsyncQueryRunner tx, Guid projectId, string sourceDoc)
        {
            // Concepts: shrink sourceDocs, drop orphans with no relations.
            await tx.RunAsync(@"
                MATCH (n:Concept {projectId: $pid})
                WHERE $doc IN n.sourceDocs
                SET n.sourceDocs = [d IN n.sourceDocs WHERE d <> $doc]
                WITH n
                WHERE size(n.sourceDocs) = 0 AND NOT (n)--()
                DETACH DELETE n
            ", new { pid = projectId.ToString(), doc = sourceDoc });

            // Relationships: shrink sourceDocs, drop empty edges.
            await tx.RunAsync(@"
                MATCH (:Concept {projectId: $pid})-[r {projectId: $pid}]->(:Concept {projectId: $pid})
                WHERE $doc IN r.sourceDocs
                SET r.sourceDocs = [d IN r.sourceDocs WHERE d <> $doc]
                WITH r
                WHERE size(r.sourceDocs) = 0
                DELETE r
            ", new { pid = projectId.ToString(), doc = sourceDoc });
        }

        private static async Task MergeConceptsAsync(IAsyncQueryRunner tx, Guid projectId, string graphNamespace, string sourceDoc, List<KgConceptRow> concepts)
        {
            if (concepts == null || concepts.Count == 0) return;
            var rows = concepts.Select(c => new Dictionary<string, object> { ["name"] = c.Name }).ToList();
            await tx.RunAsync(@"
                UNWIND $rows AS row
                MERGE (n:Concept {projectId: $pid, graph: $graph, name: row.name})
                ON CREATE SET n.sourceDocs = [$doc]
                ON MATCH  SET n.sourceDocs = CASE WHEN $doc IN coalesce(n.sourceDocs, []) THEN n.sourceDocs ELSE coalesce(n.sourceDocs, []) + $doc END
            ", new { pid = projectId.ToString(), graph = graphNamespace, doc = sourceDoc, rows });
        }

        private static async Task<List<string>> ValidateRelationshipTargetsAsync(IAsyncQueryRunner tx, Guid projectId, List<KgRelationshipRow> relationships)
        {
            var issues = new List<string>();
            foreach (var r in relationships)
            {
                var cursor = await tx.RunAsync(@"
                    MATCH (to:Concept {projectId: $pid, name: $name})
                    RETURN collect(to.graph) AS graphs
                ", new { pid = projectId.ToString(), name = r.To });
                var record = await cursor.SingleAsync();
                var graphs = record["graphs"].As<List<object>>();
                if (graphs.Count == 0)
                {
                    issues.Add($"missing target: {r.From} -[{r.Type}]-> {r.To}");
                }
                else if (graphs.Count > 1)
                {
                    issues.Add($"ambiguous target '{r.To}' (in graphs [{string.Join(", ", graphs)}]) for edge {r.From} -[{r.Type}]-> {r.To}");
                }
            }
            return issues;
        }

        private static async Task MergeRelationshipsAsync(IAsyncQueryRunner tx, Guid projectId, string graphNamespace, string sourceDoc, List<KgRelationshipRow> relationships)
        {
            foreach (var r in relationships)
            {
                // Relationship type interpolation is safe — every type was whitelisted in Phase A.
                var cypher = $@"
                    MATCH (from:Concept {{projectId: $pid, graph: $graph, name: $from}})
                    MATCH (to:Concept   {{projectId: $pid, name: $to}})
                    MERGE (from)-[edge:{r.Type} {{projectId: $pid}}]->(to)
                    ON CREATE SET edge.sourceDocs = [$doc]
                    ON MATCH  SET edge.sourceDocs = CASE WHEN $doc IN coalesce(edge.sourceDocs, []) THEN edge.sourceDocs ELSE coalesce(edge.sourceDocs, []) + $doc END
                ";
                await tx.RunAsync(cypher, new
                {
                    pid = projectId.ToString(),
                    graph = graphNamespace,
                    from = r.From,
                    to = r.To,
                    doc = sourceDoc
                });
            }
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
