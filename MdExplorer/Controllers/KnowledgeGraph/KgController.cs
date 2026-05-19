using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace MdExplorer.Service.Controllers.KnowledgeGraph
{
    [Route("api/kg")]
    [ApiController]
    public class KgController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IPasswordProtector _passwordProtector;
        private readonly INeo4jConnectionPool _connectionPool;
        private readonly IKgIngestService _kgIngestService;
        private readonly IFolderKgConfigResolver _folderKgConfigResolver;
        private readonly ILogger<KgController> _logger;

        private const string PasswordMask = "********";

        public KgController(
            IUserSettingsDB userSettingsDB,
            IPasswordProtector passwordProtector,
            INeo4jConnectionPool connectionPool,
            IKgIngestService kgIngestService,
            IFolderKgConfigResolver folderKgConfigResolver,
            ILogger<KgController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _passwordProtector = passwordProtector;
            _connectionPool = connectionPool;
            _kgIngestService = kgIngestService;
            _folderKgConfigResolver = folderKgConfigResolver;
            _logger = logger;
        }

        // ============================================================
        //   GET /api/kg/settings/{projectId}
        // ============================================================
        [HttpGet("settings/{projectId}")]
        public IActionResult GetSettings(Guid projectId)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return NotFound(new { error = $"Project {projectId} not found" });
                }
                var settings = _userSettingsDB.GetDal<ProjectNeo4jSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == projectId);
                _userSettingsDB.Commit();

                return Ok(new
                {
                    projectId,
                    enabled = settings?.Enabled ?? false,
                    uri = settings?.Uri ?? "bolt://localhost:7687",
                    database = settings?.Database ?? "neo4j",
                    username = settings?.Username ?? "neo4j",
                    hasPassword = !string.IsNullOrEmpty(settings?.PasswordEncrypted),
                    syncOnTocGeneration = settings?.SyncOnTocGeneration ?? true,
                    syncOnKgFileSave = settings?.SyncOnKgFileSave ?? true,
                    lastTestedAt = settings?.LastTestedAt,
                    lastTestSuccess = settings?.LastTestSuccess
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] GetSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class Neo4jSettingsRequest
        {
            public bool Enabled { get; set; }
            public string Uri { get; set; }
            public string Database { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }   // plaintext; empty/null = keep existing
            public bool SyncOnTocGeneration { get; set; } = true;
            public bool SyncOnKgFileSave { get; set; } = true;
        }

        // ============================================================
        //   PUT /api/kg/settings/{projectId}
        // ============================================================
        [HttpPut("settings/{projectId}")]
        public IActionResult SaveSettings(Guid projectId, [FromBody] Neo4jSettingsRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return NotFound(new { error = $"Project {projectId} not found" });
                }
                var settingsDal = _userSettingsDB.GetDal<ProjectNeo4jSettings>();
                var settings = settingsDal.GetList().FirstOrDefault(s => s.Project.Id == projectId);
                bool isNew = settings == null;
                if (isNew)
                {
                    settings = new ProjectNeo4jSettings { Project = project };
                }

                settings.Enabled = req.Enabled;
                settings.Uri = string.IsNullOrWhiteSpace(req.Uri) ? settings.Uri : req.Uri.Trim();
                settings.Database = string.IsNullOrWhiteSpace(req.Database) ? settings.Database : req.Database.Trim();
                settings.Username = string.IsNullOrWhiteSpace(req.Username) ? settings.Username : req.Username.Trim();
                settings.SyncOnTocGeneration = req.SyncOnTocGeneration;
                settings.SyncOnKgFileSave = req.SyncOnKgFileSave;

                if (!string.IsNullOrEmpty(req.Password) && req.Password != PasswordMask)
                {
                    settings.PasswordEncrypted = _passwordProtector.Protect(req.Password);
                }

                settingsDal.Save(settings);
                _userSettingsDB.Commit();

                // Invalidate any cached driver — credentials may have changed.
                _connectionPool.Invalidate(projectId);

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] SaveSettings failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class TestConnectionRequest
        {
            public Guid? ProjectId { get; set; }    // when present, also persists LastTested*
            public string Uri { get; set; }
            public string Database { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }    // plaintext; if "********", reuse stored password
        }

        // ============================================================
        //   POST /api/kg/test-connection
        // ============================================================
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Uri))
                return BadRequest(new { error = "uri required" });

            string passwordPlain = req.Password;
            if (req.ProjectId.HasValue && (string.IsNullOrEmpty(passwordPlain) || passwordPlain == PasswordMask))
            {
                _userSettingsDB.BeginTransaction();
                var stored = _userSettingsDB.GetDal<ProjectNeo4jSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == req.ProjectId.Value);
                _userSettingsDB.Commit();
                if (stored != null && !string.IsNullOrEmpty(stored.PasswordEncrypted))
                    passwordPlain = _passwordProtector.Unprotect(stored.PasswordEncrypted);
            }

            var sw = Stopwatch.StartNew();
            bool ok = false;
            string error = null;
            try
            {
                ok = await _connectionPool.TestConnectionAsync(req.Uri, req.Username ?? "neo4j", passwordPlain ?? string.Empty, req.Database ?? "neo4j");
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            sw.Stop();

            if (req.ProjectId.HasValue)
            {
                try
                {
                    _userSettingsDB.BeginTransaction();
                    var dal = _userSettingsDB.GetDal<ProjectNeo4jSettings>();
                    var stored = dal.GetList().FirstOrDefault(s => s.Project.Id == req.ProjectId.Value);
                    if (stored != null)
                    {
                        stored.LastTestedAt = DateTime.UtcNow;
                        stored.LastTestSuccess = ok;
                        dal.Save(stored);
                    }
                    _userSettingsDB.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[KgController] could not persist LastTested* for {ProjectId}", req.ProjectId.Value);
                }
            }

            return Ok(new { success = ok, error, latencyMs = sw.ElapsedMilliseconds });
        }

        public class IngestFileRequest { public Guid ProjectId { get; set; } public string RelativeKgPath { get; set; } }
        public class IngestFolderRequest { public Guid ProjectId { get; set; } public string RelativeFolderPath { get; set; } }
        public class IngestProjectRequest { public Guid ProjectId { get; set; } }

        // ============================================================
        //   POST /api/kg/ingest/file
        // ============================================================
        [HttpPost("ingest/file")]
        public async Task<IActionResult> IngestFile([FromBody] IngestFileRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            var ctx = OpenContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            var kgAbs = Path.Combine(ctx.ProjectPath, req.RelativeKgPath?.Replace('/', Path.DirectorySeparatorChar) ?? string.Empty);
            if (!System.IO.File.Exists(kgAbs))
                return BadRequest(new { error = $"kg file not found: {req.RelativeKgPath}" });

            // Resolve namespace from parent folder of .mde-doc/ (.mde-doc is sibling of the document folder).
            var mdeDocDir = Path.GetDirectoryName(kgAbs);
            var folderAbs = Path.GetDirectoryName(mdeDocDir);
            var cfg = _folderKgConfigResolver.Resolve(ctx.ProjectPath, folderAbs);
            if (cfg == null || !cfg.Enabled || string.IsNullOrWhiteSpace(cfg.Namespace))
                return BadRequest(new { error = "folder has no knowledgeGraph.namespace in .development.yml" });

            var previousHash = LookupHash(req.ProjectId, MakeRelative(ctx.ProjectPath, kgAbs));

            await using var session = ctx.OpenSession();
            var batch = new[] { new KgBatchFile {
                KgFileAbsolutePath = kgAbs,
                PreviousHash = previousHash,
                GraphNamespace = cfg.Namespace
            }};
            var results = await _kgIngestService.IngestKgFilesAsync(req.ProjectId, ctx.ProjectPath, batch, session);
            PersistIngestState(req.ProjectId, results);
            return Ok(new { results });
        }

        // ============================================================
        //   POST /api/kg/ingest/folder
        // ============================================================
        [HttpPost("ingest/folder")]
        public async Task<IActionResult> IngestFolder([FromBody] IngestFolderRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            var ctx = OpenContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            var folderAbs = Path.Combine(ctx.ProjectPath, req.RelativeFolderPath?.Replace('/', Path.DirectorySeparatorChar) ?? string.Empty);
            if (!Directory.Exists(folderAbs))
                return BadRequest(new { error = $"folder not found: {req.RelativeFolderPath}" });

            var cfg = _folderKgConfigResolver.Resolve(ctx.ProjectPath, folderAbs);
            if (cfg == null || !cfg.Enabled || string.IsNullOrWhiteSpace(cfg.Namespace))
                return BadRequest(new { error = "folder has no knowledgeGraph.namespace in .development.yml" });

            var batch = EnumerateBatchForFolder(ctx.ProjectPath, folderAbs, cfg.Namespace, req.ProjectId);
            if (batch.Count == 0)
                return Ok(new { results = Array.Empty<object>(), message = "no .kg.md files in folder" });

            await using var session = ctx.OpenSession();
            var results = await _kgIngestService.IngestKgFilesAsync(req.ProjectId, ctx.ProjectPath, batch, session);
            PersistIngestState(req.ProjectId, results);
            return Ok(new { results });
        }

        // ============================================================
        //   POST /api/kg/ingest/project
        // ============================================================
        [HttpPost("ingest/project")]
        public async Task<IActionResult> IngestProject([FromBody] IngestProjectRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            var ctx = OpenContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            var configured = _folderKgConfigResolver.EnumerateConfiguredFolders(ctx.ProjectPath);
            if (configured.Count == 0)
                return Ok(new { results = Array.Empty<object>(), message = "no folders with knowledgeGraph.namespace" });

            var batch = new List<KgBatchFile>();
            foreach (var folderCfg in configured)
            {
                batch.AddRange(EnumerateBatchForFolder(ctx.ProjectPath, folderCfg.FolderAbsolutePath, folderCfg.Namespace, req.ProjectId));
            }
            if (batch.Count == 0)
                return Ok(new { results = Array.Empty<object>(), message = "no .kg.md files found" });

            await using var session = ctx.OpenSession();
            var results = await _kgIngestService.IngestKgFilesAsync(req.ProjectId, ctx.ProjectPath, batch, session);
            PersistIngestState(req.ProjectId, results);
            return Ok(new { results, totalFiles = batch.Count, namespaces = configured.Select(c => c.Namespace).Distinct().ToArray() });
        }

        public class ResetRequest { public Guid ProjectId { get; set; } public bool Confirm { get; set; } }

        // ============================================================
        //   POST /api/kg/reset
        // ============================================================
        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromBody] ResetRequest req)
        {
            if (req == null) return BadRequest(new { error = "body required" });
            if (!req.Confirm) return BadRequest(new { error = "confirm flag must be true" });
            var ctx = OpenContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            await using (var session = ctx.OpenSession())
            {
                await _kgIngestService.ResetProjectAsync(req.ProjectId, session);
            }

            // Wipe all KgIngestState rows for this project so the next sync re-ingests everything.
            try
            {
                _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<KgIngestState>();
                var rows = dal.GetList().Where(s => s.Project.Id == req.ProjectId).ToList();
                foreach (var r in rows) dal.Delete(r);
                _userSettingsDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] reset: KgIngestState wipe failed");
                return StatusCode(500, new { error = ex.Message });
            }

            return Ok(new { ok = true });
        }

        // ============================================================
        //   GET /api/kg/query/namespaces/{projectId}
        // ============================================================
        [HttpGet("query/namespaces/{projectId}")]
        public async Task<IActionResult> QueryNamespaces(Guid projectId)
        {
            var ctx = OpenContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();
                var cursor = await session.RunAsync(@"
                    MATCH (n:Concept {projectId: $pid})
                    RETURN n.graph AS graph, count(n) AS conceptCount
                    ORDER BY graph
                ", new { pid = projectId.ToString() });
                var rows = await cursor.ToListAsync(r => new
                {
                    graph = r["graph"].As<string>(),
                    conceptCount = r["conceptCount"].As<long>()
                });
                return Ok(new { projectId, namespaces = rows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] QueryNamespaces failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   GET /api/kg/query/schema/{projectId}?ns=...
        // ============================================================
        [HttpGet("query/schema/{projectId}")]
        public async Task<IActionResult> QuerySchema(Guid projectId, [FromQuery] string ns)
        {
            if (string.IsNullOrWhiteSpace(ns)) return BadRequest(new { error = "ns query param required" });
            var ctx = OpenContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();
                var countCursor = await session.RunAsync(@"
                    MATCH (n:Concept {projectId: $pid, graph: $ns})
                    RETURN count(n) AS c
                ", new { pid = projectId.ToString(), ns });
                var conceptCount = (await countCursor.SingleAsync())["c"].As<long>();

                var typesCursor = await session.RunAsync(@"
                    MATCH (:Concept {projectId: $pid, graph: $ns})-[r {projectId: $pid}]->(:Concept {projectId: $pid})
                    RETURN type(r) AS type, count(r) AS c
                    ORDER BY c DESC
                ", new { pid = projectId.ToString(), ns });
                var relTypes = await typesCursor.ToListAsync(r => new
                {
                    type = r["type"].As<string>(),
                    count = r["c"].As<long>()
                });

                var topCursor = await session.RunAsync(@"
                    MATCH (n:Concept {projectId: $pid, graph: $ns})
                    WITH n, size([(n)--() | 1]) AS deg
                    ORDER BY deg DESC, n.name
                    LIMIT 10
                    RETURN n.name AS name, deg AS degree, n.sourceDocs AS sourceDocs
                ", new { pid = projectId.ToString(), ns });
                var topConcepts = await topCursor.ToListAsync(r => new
                {
                    name = r["name"].As<string>(),
                    degree = r["degree"].As<long>(),
                    sourceDocs = r["sourceDocs"].As<List<object>>()
                });

                return Ok(new
                {
                    projectId,
                    graphNamespace = ns,
                    conceptCount,
                    relationshipsByType = relTypes,
                    topConcepts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] QuerySchema failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   GET /api/kg/query/concepts/search?projectId=&q=&ns=&limit=
        // ============================================================
        [HttpGet("query/concepts/search")]
        public async Task<IActionResult> QueryConceptsSearch(
            [FromQuery] Guid projectId,
            [FromQuery] string q,
            [FromQuery] string ns = null,
            [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "q query param required" });
            if (limit <= 0 || limit > 200) limit = 20;
            var ctx = OpenContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();
                var cypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH (n:Concept {projectId: $pid})
                        WHERE toLower(n.name) CONTAINS toLower($q)
                        RETURN n.name AS name, n.graph AS graph, n.sourceDocs AS sourceDocs
                        ORDER BY n.graph, n.name
                        LIMIT $limit"
                    : @"
                        MATCH (n:Concept {projectId: $pid, graph: $ns})
                        WHERE toLower(n.name) CONTAINS toLower($q)
                        RETURN n.name AS name, n.graph AS graph, n.sourceDocs AS sourceDocs
                        ORDER BY n.name
                        LIMIT $limit";
                var cursor = await session.RunAsync(cypher, new { pid = projectId.ToString(), q, ns, limit });
                var rows = await cursor.ToListAsync(r => new
                {
                    name = r["name"].As<string>(),
                    graph = r["graph"].As<string>(),
                    sourceDocs = r["sourceDocs"].As<List<object>>()
                });
                return Ok(new { projectId, query = q, graphNamespace = ns, results = rows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] QueryConceptsSearch failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   GET /api/kg/query/concepts/{name}/related?projectId=&ns=&depth=
        // ============================================================
        [HttpGet("query/concepts/{name}/related")]
        public async Task<IActionResult> QueryConceptRelated(
            string name,
            [FromQuery] Guid projectId,
            [FromQuery] string ns = null,
            [FromQuery] int depth = 1)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { error = "name required" });
            if (depth < 1 || depth > 3) depth = 1;        // hard cap (safe interpolation below)
            var ctx = OpenContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();
                // depth is safe — clamped to 1..3 integer above.
                var nsClause = string.IsNullOrWhiteSpace(ns) ? string.Empty : ", graph: $ns";
                var cypher = $@"
                    MATCH (origin:Concept {{projectId: $pid, name: $name{nsClause}}})
                    MATCH p = (origin)-[*1..{depth}]-(other:Concept {{projectId: $pid}})
                    WHERE other <> origin
                    WITH other, length(p) AS dist
                    RETURN DISTINCT other.name AS name, other.graph AS graph, min(dist) AS distance, other.sourceDocs AS sourceDocs
                    ORDER BY distance, graph, name
                    LIMIT 50";
                var cursor = await session.RunAsync(cypher, new { pid = projectId.ToString(), name, ns });
                var rows = await cursor.ToListAsync(r => new
                {
                    name = r["name"].As<string>(),
                    graph = r["graph"].As<string>(),
                    distance = r["distance"].As<long>(),
                    sourceDocs = r["sourceDocs"].As<List<object>>()
                });
                return Ok(new { projectId, origin = name, graphNamespace = ns, depth, related = rows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] QueryConceptRelated failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class CypherRequest { public Guid ProjectId { get; set; } public string Query { get; set; } public Dictionary<string, object> Parameters { get; set; } }

        // ============================================================
        //   POST /api/kg/query/cypher (read-only)
        // ============================================================
        [HttpPost("query/cypher")]
        public async Task<IActionResult> QueryCypher([FromBody] CypherRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Query))
                return BadRequest(new { error = "query body required" });
            // Defense-in-depth: refuse obvious write keywords. AccessMode.Read also denies writes at driver level.
            var lower = req.Query.ToLowerInvariant();
            string[] forbidden = { " create ", " merge ", " delete ", " detach ", " set ", " remove ", " drop ", " call apoc" };
            foreach (var k in forbidden)
            {
                if (lower.Contains(k))
                    return BadRequest(new { error = $"Read-only endpoint: query contains forbidden keyword '{k.Trim()}'." });
            }

            var ctx = OpenContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();
                // Force the caller's projectId into the parameters so cross-project leakage is impossible.
                var parameters = req.Parameters != null
                    ? new Dictionary<string, object>(req.Parameters)
                    : new Dictionary<string, object>();
                parameters["pid"] = req.ProjectId.ToString();
                var cursor = await session.RunAsync(req.Query, parameters);
                var rows = await cursor.ToListAsync(r =>
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var k in r.Keys) dict[k] = r[k];
                    return dict;
                });
                return Ok(new { rows });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[KgController] QueryCypher failed");
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================================
        //   GET /api/kg/graph/{projectId}?ns=
        //   Returns nodes + links in force-graph format for the visual KG view.
        // ============================================================
        [HttpGet("graph/{projectId}")]
        public async Task<IActionResult> GetGraph(Guid projectId, [FromQuery] string ns = null)
        {
            var ctx = OpenContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();
                var nodeCypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH (n:Concept {projectId: $pid})
                        RETURN n.graph + '::' + n.name AS id, n.name AS name, n.graph AS graph, n.sourceDocs AS sourceDocs
                        ORDER BY graph, name"
                    : @"
                        MATCH (n:Concept {projectId: $pid, graph: $ns})
                        RETURN n.graph + '::' + n.name AS id, n.name AS name, n.graph AS graph, n.sourceDocs AS sourceDocs
                        ORDER BY name";
                var nodeCursor = await session.RunAsync(nodeCypher, new { pid = projectId.ToString(), ns });
                var nodes = await nodeCursor.ToListAsync(r => new
                {
                    id = r["id"].As<string>(),
                    name = r["name"].As<string>(),
                    graph = r["graph"].As<string>(),
                    sourceDocs = r["sourceDocs"].As<List<object>>()
                });

                var linkCypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH (from:Concept {projectId: $pid})-[r {projectId: $pid}]->(to:Concept {projectId: $pid})
                        RETURN from.graph + '::' + from.name AS source,
                               to.graph   + '::' + to.name   AS target,
                               type(r) AS type,
                               r.sourceDocs AS sourceDocs"
                    : @"
                        MATCH (from:Concept {projectId: $pid, graph: $ns})-[r {projectId: $pid}]->(to:Concept {projectId: $pid})
                        RETURN from.graph + '::' + from.name AS source,
                               to.graph   + '::' + to.name   AS target,
                               type(r) AS type,
                               r.sourceDocs AS sourceDocs";
                var linkCursor = await session.RunAsync(linkCypher, new { pid = projectId.ToString(), ns });
                var links = await linkCursor.ToListAsync(r => new
                {
                    source = r["source"].As<string>(),
                    target = r["target"].As<string>(),
                    type = r["type"].As<string>(),
                    sourceDocs = r["sourceDocs"].As<List<object>>()
                });

                return Ok(new { projectId, graphNamespace = ns, nodes, links });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] GetGraph failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   GET /api/kg/sanity/{projectId}?ns=
        //   Returns sanity metrics for the visual KG view.
        // ============================================================
        [HttpGet("sanity/{projectId}")]
        public async Task<IActionResult> GetSanity(Guid projectId, [FromQuery] string ns = null, [FromQuery] int hotDegreeThreshold = 10)
        {
            var ctx = OpenContext(projectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;
            try
            {
                await using var session = ctx.OpenReadSession();

                // Orphans: concepts with no relationship.
                var orphansCypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH (n:Concept {projectId: $pid})
                        WHERE NOT (n)--()
                        RETURN n.name AS name, n.graph AS graph, n.sourceDocs AS sourceDocs
                        ORDER BY graph, name"
                    : @"
                        MATCH (n:Concept {projectId: $pid, graph: $ns})
                        WHERE NOT (n)--()
                        RETURN n.name AS name, n.graph AS graph, n.sourceDocs AS sourceDocs
                        ORDER BY name";
                var orphansCursor = await session.RunAsync(orphansCypher, new { pid = projectId.ToString(), ns });
                var orphans = await orphansCursor.ToListAsync(r => new
                {
                    name = r["name"].As<string>(),
                    graph = r["graph"].As<string>(),
                    sourceDocs = r["sourceDocs"].As<List<object>>()
                });

                // Hot concepts: degree > threshold.
                var hotCypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH (n:Concept {projectId: $pid})
                        WITH n, size([(n)--() | 1]) AS degree
                        WHERE degree > $threshold
                        RETURN n.name AS name, n.graph AS graph, degree
                        ORDER BY degree DESC, n.graph, n.name"
                    : @"
                        MATCH (n:Concept {projectId: $pid, graph: $ns})
                        WITH n, size([(n)--() | 1]) AS degree
                        WHERE degree > $threshold
                        RETURN n.name AS name, n.graph AS graph, degree
                        ORDER BY degree DESC, n.name";
                var hotCursor = await session.RunAsync(hotCypher, new { pid = projectId.ToString(), ns, threshold = hotDegreeThreshold });
                var hot = await hotCursor.ToListAsync(r => new
                {
                    name = r["name"].As<string>(),
                    graph = r["graph"].As<string>(),
                    degree = r["degree"].As<long>()
                });

                // RELATED_TO ratio: count RELATED_TO vs total edges.
                var ratioCypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH ()-[r {projectId: $pid}]->()
                        RETURN type(r) AS type, count(r) AS c"
                    : @"
                        MATCH (:Concept {projectId: $pid, graph: $ns})-[r {projectId: $pid}]->(:Concept {projectId: $pid})
                        RETURN type(r) AS type, count(r) AS c";
                var ratioCursor = await session.RunAsync(ratioCypher, new { pid = projectId.ToString(), ns });
                var typeCounts = await ratioCursor.ToListAsync(r => new
                {
                    type = r["type"].As<string>(),
                    count = r["c"].As<long>()
                });
                long totalEdges = typeCounts.Sum(t => t.count);
                long relatedToEdges = typeCounts.FirstOrDefault(t => t.type == "RELATED_TO")?.count ?? 0;
                double relatedToRatio = totalEdges > 0 ? (double)relatedToEdges / totalEdges : 0;

                // Casing collisions: concept names that match modulo case (toLower).
                var casingCypher = string.IsNullOrWhiteSpace(ns)
                    ? @"
                        MATCH (n:Concept {projectId: $pid})
                        WITH toLower(n.name) AS k, collect({name: n.name, graph: n.graph}) AS items
                        WHERE size(items) > 1
                        WITH k, [it IN items | it.name] AS names, [it IN items | it.graph] AS graphs
                        WHERE size([n IN names WHERE n <> head(names) | 1]) > 0
                        RETURN k AS key, names, graphs"
                    : @"
                        MATCH (n:Concept {projectId: $pid, graph: $ns})
                        WITH toLower(n.name) AS k, collect(n.name) AS names
                        WHERE size(names) > 1
                        WITH k, names
                        WHERE size([n IN names WHERE n <> head(names) | 1]) > 0
                        RETURN k AS key, names, [] AS graphs";
                var casingCursor = await session.RunAsync(casingCypher, new { pid = projectId.ToString(), ns });
                var casingCollisions = await casingCursor.ToListAsync(r => new
                {
                    key = r["key"].As<string>(),
                    names = r["names"].As<List<object>>(),
                    graphs = r["graphs"].As<List<object>>()
                });

                return Ok(new
                {
                    projectId,
                    graphNamespace = ns,
                    orphans,
                    hot,
                    relatedToRatio = new { totalEdges, relatedToEdges, ratio = relatedToRatio },
                    typeCounts,
                    casingCollisions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] GetSanity failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class RoundTripRequest { public Guid ProjectId { get; set; } public string KgFileRelativePath { get; set; } }

        // ============================================================
        //   POST /api/kg/verify-roundtrip
        //   Re-builds the expected ParsedKgFile from Neo4j (filtered by sourceDoc) and
        //   diffs it against the on-disk .kg.md. Zero diff = loader is faithful.
        // ============================================================
        [HttpPost("verify-roundtrip")]
        public async Task<IActionResult> VerifyRoundTrip([FromBody] RoundTripRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.KgFileRelativePath))
                return BadRequest(new { error = "kgFileRelativePath required" });
            var ctx = OpenContext(req.ProjectId);
            if (ctx.ErrorResult != null) return ctx.ErrorResult;

            // Load on-disk content + parse.
            var kgAbs = Path.Combine(ctx.ProjectPath, req.KgFileRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(kgAbs))
                return BadRequest(new { error = $"file not found: {req.KgFileRelativePath}" });
            var content = await System.IO.File.ReadAllTextAsync(kgAbs);
            var diskParsed = KgFileParser.Parse(content);
            var diskConcepts = diskParsed.Concepts.Select(c => c.Name).ToHashSet(StringComparer.Ordinal);
            var diskRelationships = diskParsed.Relationships
                .Select(r => $"{r.From}|{r.Type}|{r.To}")
                .ToHashSet(StringComparer.Ordinal);

            // Pull from Neo4j what we expect this sourceDoc to have produced.
            await using var session = ctx.OpenReadSession();
            var nodeCursor = await session.RunAsync(@"
                MATCH (n:Concept {projectId: $pid})
                WHERE $doc IN n.sourceDocs
                RETURN n.name AS name, n.graph AS graph
            ", new { pid = req.ProjectId.ToString(), doc = req.KgFileRelativePath });
            var neoConceptRows = await nodeCursor.ToListAsync(r => new { name = r["name"].As<string>(), graph = r["graph"].As<string>() });
            var neoConcepts = neoConceptRows.Select(x => x.name).ToHashSet(StringComparer.Ordinal);

            var edgeCursor = await session.RunAsync(@"
                MATCH (from:Concept {projectId: $pid})-[r {projectId: $pid}]->(to:Concept {projectId: $pid})
                WHERE $doc IN r.sourceDocs
                RETURN from.name AS f, type(r) AS t, to.name AS dst
            ", new { pid = req.ProjectId.ToString(), doc = req.KgFileRelativePath });
            var neoEdgeRows = await edgeCursor.ToListAsync(r => $"{r["f"].As<string>()}|{r["t"].As<string>()}|{r["dst"].As<string>()}");
            var neoEdges = neoEdgeRows.ToHashSet(StringComparer.Ordinal);

            var conceptsMissingInNeo = diskConcepts.Except(neoConcepts).ToList();
            var conceptsExtraInNeo = neoConcepts.Except(diskConcepts).ToList();
            var edgesMissingInNeo = diskRelationships.Except(neoEdges).ToList();
            var edgesExtraInNeo = neoEdges.Except(diskRelationships).ToList();
            bool faithful = conceptsMissingInNeo.Count == 0 && conceptsExtraInNeo.Count == 0
                         && edgesMissingInNeo.Count == 0 && edgesExtraInNeo.Count == 0;

            return Ok(new
            {
                req.ProjectId,
                kgFile = req.KgFileRelativePath,
                faithful,
                conceptsMissingInNeo,
                conceptsExtraInNeo,
                edgesMissingInNeo,
                edgesExtraInNeo,
                diskCounts = new { concepts = diskConcepts.Count, relationships = diskRelationships.Count },
                neoCounts = new { concepts = neoConcepts.Count, relationships = neoEdges.Count }
            });
        }

        // ============================================================
        //   GET /api/kg/state/{projectId}
        // ============================================================
        [HttpGet("state/{projectId}")]
        public IActionResult GetState(Guid projectId)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                var rows = _userSettingsDB.GetDal<KgIngestState>().GetList()
                    .Where(s => s.Project.Id == projectId)
                    .ToList();
                _userSettingsDB.Commit();

                var totals = new
                {
                    files = rows.Count,
                    concepts = rows.Sum(r => r.NodeCount),
                    relationships = rows.Sum(r => r.EdgeCount),
                    lastSyncAt = rows.Count == 0 ? (DateTime?)null : rows.Max(r => r.LastIngestedAt)
                };
                var perNamespace = rows
                    .GroupBy(r => r.GraphNamespace)
                    .Select(g => new
                    {
                        graphNamespace = g.Key,
                        files = g.Count(),
                        concepts = g.Sum(r => r.NodeCount),
                        relationships = g.Sum(r => r.EdgeCount),
                        lastSyncAt = g.Max(r => r.LastIngestedAt)
                    })
                    .ToList();
                return Ok(new { projectId, totals, perNamespace });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] GetState failed for {ProjectId}", projectId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        //   Internal helpers
        // ============================================================

        private class IngestContext
        {
            public IActionResult ErrorResult { get; set; }
            public string ProjectPath { get; set; }
            public ProjectNeo4jSettings Settings { get; set; }
            public string DecryptedPassword { get; set; }
            public Guid ProjectId { get; set; }
            public INeo4jConnectionPool Pool { get; set; }

            public IAsyncSession OpenSession()
            {
                var driver = Pool.GetOrCreateDriver(ProjectId, Settings.Uri, Settings.Username, DecryptedPassword ?? string.Empty);
                return driver.AsyncSession(b =>
                {
                    if (!string.IsNullOrWhiteSpace(Settings.Database)) b.WithDatabase(Settings.Database);
                    b.WithDefaultAccessMode(AccessMode.Write);
                });
            }

            public IAsyncSession OpenReadSession()
            {
                var driver = Pool.GetOrCreateDriver(ProjectId, Settings.Uri, Settings.Username, DecryptedPassword ?? string.Empty);
                return driver.AsyncSession(b =>
                {
                    if (!string.IsNullOrWhiteSpace(Settings.Database)) b.WithDatabase(Settings.Database);
                    b.WithDefaultAccessMode(AccessMode.Read);
                });
            }
        }

        private IngestContext OpenContext(Guid projectId)
        {
            var ctx = new IngestContext { ProjectId = projectId, Pool = _connectionPool };
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                var settings = _userSettingsDB.GetDal<ProjectNeo4jSettings>().GetList()
                    .FirstOrDefault(s => s.Project.Id == projectId);
                _userSettingsDB.Commit();

                if (project == null) { ctx.ErrorResult = NotFound(new { error = $"Project {projectId} not found" }); return ctx; }
                if (settings == null || !settings.Enabled)
                {
                    ctx.ErrorResult = BadRequest(new { error = "Neo4j is disabled for this project (Project Settings → Knowledge Graph)" });
                    return ctx;
                }
                if (string.IsNullOrEmpty(settings.PasswordEncrypted))
                {
                    ctx.ErrorResult = BadRequest(new { error = "Neo4j password not set" });
                    return ctx;
                }
                ctx.ProjectPath = project.Path;
                ctx.Settings = settings;
                ctx.DecryptedPassword = _passwordProtector.Unprotect(settings.PasswordEncrypted);
                return ctx;
            }
            catch (Exception ex)
            {
                ctx.ErrorResult = StatusCode(500, new { error = ex.Message });
                return ctx;
            }
        }

        private List<KgBatchFile> EnumerateBatchForFolder(string projectPath, string folderAbs, string graphNamespace, Guid projectId)
        {
            var batch = new List<KgBatchFile>();
            var mdeDocDir = Path.Combine(folderAbs, ".mde-doc");
            if (!Directory.Exists(mdeDocDir)) return batch;
            var files = Directory.GetFiles(mdeDocDir, "*.kg.md", SearchOption.TopDirectoryOnly)
                .Where(f => !string.Equals(Path.GetFileName(f), "_aggregate.kg.md", StringComparison.OrdinalIgnoreCase));
            foreach (var f in files)
            {
                var rel = MakeRelative(projectPath, f);
                var previousHash = LookupHash(projectId, rel);
                batch.Add(new KgBatchFile
                {
                    KgFileAbsolutePath = f,
                    PreviousHash = previousHash,
                    GraphNamespace = graphNamespace
                });
            }
            return batch;
        }

        private static string MakeRelative(string projectPath, string absolutePath)
        {
            return Path.GetRelativePath(projectPath, absolutePath).Replace('\\', '/');
        }

        private string LookupHash(Guid projectId, string relativeKgPath)
        {
            try
            {
                _userSettingsDB.BeginTransaction();
                var row = _userSettingsDB.GetDal<KgIngestState>().GetList()
                    .FirstOrDefault(s => s.Project.Id == projectId && s.KgFilePath == relativeKgPath);
                _userSettingsDB.Commit();
                return row?.ContentHash;
            }
            catch
            {
                return null;
            }
        }

        private void PersistIngestState(Guid projectId, IList<KgIngestResult> results)
        {
            if (results == null || results.Count == 0) return;
            try
            {
                _userSettingsDB.BeginTransaction();
                var project = _userSettingsDB.GetDal<Project>().GetList().FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    _userSettingsDB.Commit();
                    return;
                }
                var dal = _userSettingsDB.GetDal<KgIngestState>();
                var existing = dal.GetList().Where(s => s.Project.Id == projectId).ToList();
                foreach (var r in results)
                {
                    // Skip rows that didn't successfully ingest (so we retry on next sync).
                    if (r.HasError) continue;
                    if (r.Skipped) continue;
                    if (string.IsNullOrEmpty(r.SourceDocPath) || string.IsNullOrEmpty(r.ContentHash)) continue;

                    var row = existing.FirstOrDefault(s => string.Equals(s.KgFilePath, r.SourceDocPath, StringComparison.Ordinal));
                    if (row == null)
                    {
                        row = new KgIngestState { Project = project, KgFilePath = r.SourceDocPath };
                        existing.Add(row);
                    }
                    row.ContentHash = r.ContentHash;
                    row.GraphNamespace = r.GraphNamespace ?? row.GraphNamespace ?? string.Empty;
                    row.LastIngestedAt = DateTime.UtcNow;
                    row.NodeCount = r.ConceptCount;
                    row.EdgeCount = r.RelationshipCount;
                    dal.Save(row);
                }
                _userSettingsDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgController] PersistIngestState failed for {ProjectId}", projectId);
            }
        }
    }
}
