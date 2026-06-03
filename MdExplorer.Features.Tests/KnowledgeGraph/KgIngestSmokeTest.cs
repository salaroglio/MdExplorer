using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4j.Driver;

namespace MdExplorer.Features.Tests.KnowledgeGraph
{
    /// <summary>
    /// End-to-end smoke test of the Knowledge Graph ingest pipeline against a real local
    /// Neo4j instance. Skipped (Inconclusive) unless the following env vars are set:
    ///   MDE_NEO4J_URI    e.g. bolt://localhost:7687
    ///   MDE_NEO4J_USER   e.g. neo4j
    ///   MDE_NEO4J_PASS   the password
    /// To run: set the three vars and execute
    ///   dotnet test --filter "FullyQualifiedName~KgIngestSmokeTest"
    /// </summary>
    [TestClass]
    public class KgIngestSmokeTest
    {
        [TestMethod]
        public async Task Smoke_CrossGraph_IngestAndVerify()
        {
            var uri = Environment.GetEnvironmentVariable("MDE_NEO4J_URI");
            var user = Environment.GetEnvironmentVariable("MDE_NEO4J_USER");
            var pass = Environment.GetEnvironmentVariable("MDE_NEO4J_PASS");
            if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(user))
            {
                Assert.Inconclusive("Smoke skipped — set MDE_NEO4J_URI, MDE_NEO4J_USER, MDE_NEO4J_PASS to run.");
                return;
            }

            var projectRoot = Path.Combine(Path.GetTempPath(), $"kg-smoke-{Guid.NewGuid():N}");
            Directory.CreateDirectory(projectRoot);
            var cobolDir = Path.Combine(projectRoot, "docs", "cobol", ".mde-doc");
            var planDir = Path.Combine(projectRoot, "docs", "plan", ".mde-doc");
            Directory.CreateDirectory(cobolDir);
            Directory.CreateDirectory(planDir);

            var cobolKg = Path.Combine(cobolDir, "comp3.kg.cypher");
            File.WriteAllText(cobolKg, @"
// comp3 in the cobol-domain graph
MERGE (a:Concept {name: 'COMP-3', projectId: $pid, graph: $graph, sourceDoc: $doc})
  ON CREATE SET a.description = 'COBOL packed-decimal numeric storage format.';

MERGE (b:Concept {name: 'Packed decimal', projectId: $pid, graph: $graph, sourceDoc: $doc})
  ON CREATE SET b.description = 'Generic packed-decimal encoding used by COMP-3 and similar formats.';

MERGE (a)-[r:USES {sourceDoc: $doc, projectId: $pid}]->(b)
  ON CREATE SET r.description = 'COMP-3 leverages packed decimal as its underlying encoding.';
");

            var planKg = Path.Combine(planDir, "migrazione.kg.cypher");
            File.WriteAllText(planKg, @"
// Cross-graph reference: a plan task targets the COMP-3 concept from cobol-domain.
MATCH (target:Concept {name: 'COMP-3', projectId: $pid})
MERGE (task:Concept {name: 'Migrate COMP-3 to decimal', projectId: $pid, graph: $graph, sourceDoc: $doc})
  ON CREATE SET task.description = 'Plan task to migrate COMP-3 fields to native decimal in the modernized code base.';
MERGE (task)-[r:REFERENCES {sourceDoc: $doc, projectId: $pid}]->(target)
  ON CREATE SET r.description = 'The migration task acts on the COMP-3 concept.';
");

            var projectId = Guid.NewGuid();
            var driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass ?? string.Empty));
            try
            {
                await using var session = driver.AsyncSession();
                var ingest = new KgIngestService(NullLogger<KgIngestService>.Instance);

                // ---- Reset for this fresh projectId ----
                await ingest.ResetProjectAsync(projectId, session);

                // ---- Ingest both files (cobol first so plan's MATCH can resolve COMP-3) ----
                var batch = new List<KgBatchFile>
                {
                    new() { KgFileAbsolutePath = cobolKg, GraphNamespace = "cobol-domain" },
                    new() { KgFileAbsolutePath = planKg,  GraphNamespace = "impl-plan"    }
                };
                var results = await ingest.IngestKgFilesAsync(projectId, projectRoot, batch, session);

                Assert.AreEqual(2, results.Count);
                foreach (var r in results)
                {
                    Assert.IsFalse(r.HasError, $"{r.SourceDocPath}: {r.Error}");
                    Assert.IsFalse(r.Skipped);
                }

                // ---- Verify nodes (3 concepts across both graphs) ----
                var verifyCursor = await session.RunAsync(@"
                    MATCH (n:Concept {projectId: $pid})
                    RETURN n.name AS name, n.graph AS graph, n.description AS description
                    ORDER BY graph, name
                ", new { pid = projectId.ToString() });
                var rows = await verifyCursor.ToListAsync();
                Assert.AreEqual(3, rows.Count, "expected 3 concepts");
                foreach (var row in rows)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(row["description"].As<string>()),
                        $"concept {row["name"]} is missing description");
                }

                // ---- Verify cross-graph edge ----
                var crossEdgeCursor = await session.RunAsync(@"
                    MATCH (from:Concept {projectId: $pid, graph: 'impl-plan', name: 'Migrate COMP-3 to decimal'})
                          -[r:REFERENCES {projectId: $pid}]->
                          (to:Concept {projectId: $pid, graph: 'cobol-domain', name: 'COMP-3'})
                    RETURN count(r) AS c
                ", new { pid = projectId.ToString() });
                var c = (await crossEdgeCursor.SingleAsync())["c"].As<long>();
                Assert.AreEqual(1, c, "cross-graph REFERENCES edge should exist");

                // ---- Idempotency: second ingest with same hashes -> skipped ----
                var second = await ingest.IngestKgFilesAsync(projectId, projectRoot,
                    new List<KgBatchFile>
                    {
                        new() { KgFileAbsolutePath = cobolKg, GraphNamespace = "cobol-domain", PreviousHash = results[0].ContentHash },
                        new() { KgFileAbsolutePath = planKg,  GraphNamespace = "impl-plan",    PreviousHash = results[1].ContentHash }
                    },
                    session);
                Assert.IsTrue(second[0].Skipped);
                Assert.IsTrue(second[1].Skipped);

                // ---- Re-ingest with modified content: cleanup pass removes the old node ----
                File.WriteAllText(cobolKg, @"
// comp3 — rewritten with a different shape; the old 'Packed decimal' node must be cleaned up.
MERGE (a:Concept {name: 'COMP-3', projectId: $pid, graph: $graph, sourceDoc: $doc})
  ON CREATE SET a.description = 'COBOL packed-decimal numeric storage format (revised).';
");
                var third = await ingest.IngestKgFilesAsync(projectId, projectRoot,
                    new List<KgBatchFile> { new() { KgFileAbsolutePath = cobolKg, GraphNamespace = "cobol-domain" } },
                    session);
                Assert.IsFalse(third[0].HasError, third[0].Error);
                var afterCursor = await session.RunAsync(@"
                    MATCH (n:Concept {projectId: $pid, graph: 'cobol-domain'})
                    RETURN n.name AS name
                    ORDER BY name
                ", new { pid = projectId.ToString() });
                var afterRows = await afterCursor.ToListAsync();
                Assert.AreEqual(1, afterRows.Count, "after re-ingest the old 'Packed decimal' node must be gone");
                Assert.AreEqual("COMP-3", afterRows[0]["name"].As<string>());

                // ---- Cleanup ----
                await ingest.ResetProjectAsync(projectId, session);
            }
            finally
            {
                await driver.DisposeAsync();
                try { Directory.Delete(projectRoot, true); } catch { /* best effort */ }
            }
        }
    }
}
