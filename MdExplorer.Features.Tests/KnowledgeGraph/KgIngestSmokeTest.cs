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

            var cobolKg = Path.Combine(cobolDir, "comp3.kg.md");
            File.WriteAllText(cobolKg, @"
# Knowledge graph — comp3.md
## 🖼️ Graph (PlantUML)
```plantuml
@startuml
[COMP-3] --> [Packed decimal] : encodes
@enduml
```
## 🗃️ Graph (Neo4j)
### Concepts
| Name |
|------|
| COMP-3 |
| Packed decimal |

### Relationships
| From | Type | To |
|------|------|----|
| COMP-3 | USES | Packed decimal |
");

            var planKg = Path.Combine(planDir, "migrazione.kg.md");
            File.WriteAllText(planKg, @"
# Knowledge graph — migrazione.md
## 🗃️ Graph (Neo4j)
### Concepts
| Name |
|------|
| Migrate COMP-3 to decimal |

### Relationships
| From | Type | To |
|------|------|----|
| Migrate COMP-3 to decimal | REFERENCES | COMP-3 |
");

            var projectId = Guid.NewGuid();
            var driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass ?? string.Empty));
            try
            {
                await using var session = driver.AsyncSession();
                var ingest = new KgIngestService(NullLogger<KgIngestService>.Instance);

                // ---- Reset for this fresh projectId ----
                await ingest.ResetProjectAsync(projectId, session);

                // ---- Batch ingest of both files (global two-pass) ----
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

                // ---- Verify nodes + cross-graph edge ----
                var verifyCursor = await session.RunAsync(@"
                    MATCH (n:Concept {projectId: $pid})
                    RETURN n.name AS name, n.graph AS graph, n.sourceDocs AS docs
                    ORDER BY graph, name
                ", new { pid = projectId.ToString() });
                var rows = await verifyCursor.ToListAsync();
                Assert.AreEqual(3, rows.Count, "expected 3 concepts");

                var crossEdgeCursor = await session.RunAsync(@"
                    MATCH (from:Concept {projectId: $pid, graph: 'impl-plan', name: 'Migrate COMP-3 to decimal'})
                          -[r:REFERENCES]->
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

                // ---- Rigid cross-graph: ingest with missing target ----
                var badDir = Path.Combine(projectRoot, "docs", "bad", ".mde-doc");
                Directory.CreateDirectory(badDir);
                var badKg = Path.Combine(badDir, "bad.kg.md");
                File.WriteAllText(badKg, @"
# bad
### Concepts
| Name |
|------|
| Bad Source |
### Relationships
| From | Type | To |
|------|------|----|
| Bad Source | REFERENCES | DOES_NOT_EXIST |
");
                var badResults = await ingest.IngestKgFilesAsync(projectId, projectRoot,
                    new List<KgBatchFile> { new() { KgFileAbsolutePath = badKg, GraphNamespace = "bad-graph" } },
                    session);
                Assert.IsTrue(badResults[0].HasError, "missing cross-graph target must fail file");

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
