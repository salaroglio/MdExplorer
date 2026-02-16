using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Hubs;
using MdExplorer.Services;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Service.Services;
using Ad.Tools.Dal.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private static volatile RagIndexingProgress _currentIndexingProgress;

        private readonly IProjectDB _projectDB;
        private readonly IEngineDB _engineDB;
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorSearchService _vectorSearchService;
        private readonly IMarkdownChunkingService _chunkingService;
        private readonly IModelDownloadService _downloadService;
        private readonly IEmbeddingConfigService _embeddingConfigService;
        private readonly IRagIndexingService _ragIndexingService;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IDatabaseManager _databaseManager;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<RagController> _logger;

        public RagController(
            IProjectDB projectDB,
            IEngineDB engineDB,
            IUserSettingsDB userSettingsDB,
            IEmbeddingService embeddingService,
            IVectorSearchService vectorSearchService,
            IMarkdownChunkingService chunkingService,
            IModelDownloadService downloadService,
            IEmbeddingConfigService embeddingConfigService,
            IRagIndexingService ragIndexingService,
            IHubContext<MonitorMDHub> hubContext,
            IDatabaseManager databaseManager,
            IMdIgnoreService mdIgnoreService,
            FoldersIgnoreService foldersIgnoreService,
            ILoggerFactory loggerFactory,
            ILogger<RagController> logger)
        {
            _projectDB = projectDB;
            _engineDB = engineDB;
            _userSettingsDB = userSettingsDB;
            _embeddingService = embeddingService;
            _vectorSearchService = vectorSearchService;
            _chunkingService = chunkingService;
            _downloadService = downloadService;
            _embeddingConfigService = embeddingConfigService;
            _ragIndexingService = ragIndexingService;
            _hubContext = hubContext;
            _databaseManager = databaseManager;
            _mdIgnoreService = mdIgnoreService;
            _foldersIgnoreService = foldersIgnoreService;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        [HttpPost("enable")]
        public IActionResult EnableRag()
        {
            try
            {
                SetRagEnabled(true);
                _logger.LogInformation("[RAG] RAG enabled for current project");
                return Ok(new { success = true, message = "RAG enabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Error enabling RAG");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("disable")]
        public IActionResult DisableRag()
        {
            try
            {
                SetRagEnabled(false);
                _logger.LogInformation("[RAG] RAG disabled for current project");
                return Ok(new { success = true, message = "RAG disabled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Error disabling RAG");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus([FromQuery] string project = null)
        {
            IEngineDB isolatedDB = null;
            try
            {
                var engineDB = _engineDB;
                if (!string.IsNullOrEmpty(project))
                {
                    var projectPath = ResolveProjectPath(project);
                    if (projectPath == null)
                        return NotFound(new { error = $"Project not found: {project}" });
                    isolatedDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);
                    engineDB = isolatedDB;
                }

                var enabled = string.IsNullOrEmpty(project) ? IsRagEnabled() : true;
                var modelInstalled = _downloadService.GetInstalledEmbeddingModelPath() != null;
                var modelLoaded = _embeddingService.IsModelLoaded();

                int chunksCount = 0;
                try
                {
                    var chunkDal = engineDB.GetDal<DocumentChunk>();
                    chunksCount = chunkDal.GetList().Count();
                }
                catch { /* Engine DB might not be initialized yet */ }

                int embeddedCount = 0;
                try
                {
                    var chunkDal = engineDB.GetDal<DocumentChunk>();
                    embeddedCount = chunkDal.GetList().Count(c => c.Embedding != null);
                }
                catch { }

                var currentProgress = _currentIndexingProgress;
                var isIndexing = currentProgress != null
                    && currentProgress.Status != "completed"
                    && currentProgress.Status != "error";

                return Ok(new
                {
                    enabled,
                    modelInstalled,
                    modelLoaded,
                    chunksCount,
                    embeddedCount,
                    embeddingDimension = _embeddingService.GetEmbeddingDimension(),
                    isIndexing,
                    indexingProgress = currentProgress != null ? new
                    {
                        status = currentProgress.Status,
                        processed = currentProgress.Processed,
                        total = currentProgress.Total,
                        message = currentProgress.Message
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Error getting status");
                return StatusCode(500, new { error = ex.Message });
            }
            finally
            {
                (isolatedDB as IDisposable)?.Dispose();
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int topK = 5, [FromQuery] string project = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { error = "Query parameter 'q' is required" });
                }

                // Auto-load embedding model if not loaded
                if (!_embeddingService.IsModelLoaded())
                {
                    var modelPath = _downloadService.GetInstalledEmbeddingModelPath();

                    if (modelPath == null)
                        return Ok(new { enabled = false, message = "No embedding model installed", results = Array.Empty<object>() });

                    var config = _embeddingConfigService.GetConfig();
                    var loaded = await _embeddingService.LoadModelAsync(modelPath, config.ContextSize, config.BatchSize, config.MaxEmbeddingChars);
                    if (!loaded)
                        return Ok(new { enabled = false, message = "Failed to load embedding model", results = Array.Empty<object>() });
                }

                IVectorSearchService searchService;
                IEngineDB isolatedDB = null;

                if (!string.IsNullOrEmpty(project))
                {
                    var projectPath = ResolveProjectPath(project);
                    if (projectPath == null)
                        return NotFound(new { error = $"Project not found: {project}" });

                    isolatedDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);
                    searchService = new VectorSearchService(
                        _embeddingService, isolatedDB,
                        _loggerFactory.CreateLogger<VectorSearchService>());
                }
                else
                {
                    if (!IsRagEnabled())
                        return Ok(new { enabled = false, message = "RAG not enabled for this project", results = Array.Empty<object>() });

                    searchService = _vectorSearchService;
                }

                try
                {
                    var results = await searchService.SearchAsync(q, topK);

                    return Ok(new
                    {
                        enabled = true,
                        query = q,
                        count = results.Count,
                        results = results.Select(r => new
                        {
                            text = r.ChunkText,
                            filePath = r.FilePath,
                            sectionTitle = r.SectionTitle,
                            score = r.SimilarityScore,
                            startLine = r.StartLine,
                            endLine = r.EndLine,
                            chunkType = r.ChunkType,
                            groupId = r.GroupId
                        })
                    });
                }
                finally
                {
                    (isolatedDB as IDisposable)?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Search error");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("reindex")]
        public IActionResult Reindex([FromBody] ReindexRequest request)
        {
            try
            {
                if (!IsRagEnabled())
                {
                    return Ok(new { enabled = false, message = "RAG not enabled for this project" });
                }

                var projectPath = request?.ProjectPath;
                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest(new { error = "projectPath is required" });
                }

                // Start indexing in background with an isolated EngineDB session.
                // forceReindex=true will clear all chunks via raw SQL and skip
                // existing chunk queries (avoids Guid format issues between sessions).
                _ = Task.Run(async () =>
                {
                    IEngineDB isolatedEngineDB = null;
                    try
                    {
                        isolatedEngineDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);

                        var ragService = new RagIndexingService(
                            isolatedEngineDB,
                            _embeddingService,
                            _chunkingService,
                            _vectorSearchService,
                            _downloadService,
                            _embeddingConfigService,
                            _mdIgnoreService,
                            _foldersIgnoreService,
                            _loggerFactory.CreateLogger<RagIndexingService>());

                        _currentIndexingProgress = new RagIndexingProgress
                        {
                            Status = "starting",
                            Processed = 0,
                            Total = 0,
                            Message = "Starting indexing..."
                        };

                        var progress = new Progress<RagIndexingProgress>(p =>
                        {
                            _currentIndexingProgress = p;
                            _hubContext.Clients.All.SendAsync("ragIndexingProgress", new
                            {
                                status = p.Status,
                                processed = p.Processed,
                                total = p.Total,
                                message = p.Message
                            });
                        });
                        await ragService.IndexAllAsync(projectPath, progress, forceReindex: true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RAG] Background reindex error");
                        _currentIndexingProgress = new RagIndexingProgress
                        {
                            Status = "error",
                            Processed = 0,
                            Total = 0,
                            Message = $"Indexing error: {ex.Message}"
                        };
                        await _hubContext.Clients.All.SendAsync("ragIndexingProgress", new
                        {
                            status = "error",
                            processed = 0,
                            total = 0,
                            message = $"Indexing error: {ex.Message}"
                        });
                    }
                    finally
                    {
                        (isolatedEngineDB as IDisposable)?.Dispose();
                    }
                });

                return Ok(new { started = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Reindex error");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("clear")]
        public IActionResult Clear([FromBody] ClearRequest request)
        {
            IEngineDB isolatedEngineDB = null;
            try
            {
                var projectPath = request?.ProjectPath;
                if (string.IsNullOrEmpty(projectPath))
                    return BadRequest(new { error = "projectPath is required" });

                isolatedEngineDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);

                int chunksDeleted = 0;
                try
                {
                    var chunkDal = isolatedEngineDB.GetDal<DocumentChunk>();
                    chunksDeleted = chunkDal.GetList().Count();

                    if (chunksDeleted > 0)
                    {
                        isolatedEngineDB.BeginTransaction();
                        isolatedEngineDB.CreateSQLQuery("DELETE FROM DocumentChunk").ExecuteUpdate();
                        isolatedEngineDB.Commit();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RAG] Error clearing chunks");
                    return StatusCode(500, new { error = ex.Message });
                }

                _vectorSearchService.InvalidateCache();

                _logger.LogInformation("[RAG] Cleared {Count} chunks for project {Path}", chunksDeleted, projectPath);
                return Ok(new { success = true, chunksDeleted });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Error clearing RAG index");
                return StatusCode(500, new { error = ex.Message });
            }
            finally
            {
                (isolatedEngineDB as IDisposable)?.Dispose();
            }
        }

        public class ClearRequest
        {
            public string ProjectPath { get; set; }
        }

        public class ReindexRequest
        {
            public string ProjectPath { get; set; }
        }

        public class IndexFileRequest
        {
            public string FilePath { get; set; }
            public string ProjectPath { get; set; }
            public bool ForceReindex { get; set; }
        }

        public class IndexDirectoryRequest
        {
            public string DirectoryPath { get; set; }
            public string ProjectPath { get; set; }
            public bool ForceReindex { get; set; }
        }

        [HttpPost("index-file")]
        public async Task<IActionResult> IndexFile([FromBody] IndexFileRequest request)
        {
            IEngineDB isolatedEngineDB = null;
            try
            {
                if (string.IsNullOrEmpty(request?.FilePath) || string.IsNullOrEmpty(request?.ProjectPath))
                    return BadRequest(new { error = "filePath and projectPath are required" });

                isolatedEngineDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(request.ProjectPath);

                var ragService = new RagIndexingService(
                    isolatedEngineDB,
                    _embeddingService,
                    _chunkingService,
                    _vectorSearchService,
                    _downloadService,
                    _embeddingConfigService,
                    _mdIgnoreService,
                    _foldersIgnoreService,
                    _loggerFactory.CreateLogger<RagIndexingService>());

                var result = await ragService.IndexFileAsync(request.FilePath, request.ProjectPath, request.ForceReindex);

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    chunksEmbedded = result.ChunksEmbedded,
                    skipped = result.Skipped
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Index file error");
                return StatusCode(500, new { error = ex.Message });
            }
            finally
            {
                (isolatedEngineDB as IDisposable)?.Dispose();
            }
        }

        [HttpPost("index-directory")]
        public IActionResult IndexDirectory([FromBody] IndexDirectoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.DirectoryPath) || string.IsNullOrEmpty(request?.ProjectPath))
                    return BadRequest(new { error = "directoryPath and projectPath are required" });

                // Check if indexing is already in progress
                var currentProgress = _currentIndexingProgress;
                if (currentProgress != null && currentProgress.Status != "completed" && currentProgress.Status != "error")
                    return Conflict(new { error = "Indexing already in progress. Wait for it to complete." });

                _ = Task.Run(async () =>
                {
                    IEngineDB isolatedEngineDB = null;
                    try
                    {
                        isolatedEngineDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(request.ProjectPath);

                        var ragService = new RagIndexingService(
                            isolatedEngineDB,
                            _embeddingService,
                            _chunkingService,
                            _vectorSearchService,
                            _downloadService,
                            _embeddingConfigService,
                            _mdIgnoreService,
                            _foldersIgnoreService,
                            _loggerFactory.CreateLogger<RagIndexingService>());

                        _currentIndexingProgress = new RagIndexingProgress
                        {
                            Status = "starting",
                            Processed = 0,
                            Total = 0,
                            Message = "Starting directory indexing...",
                            Scope = "directory"
                        };

                        var progress = new Progress<RagIndexingProgress>(p =>
                        {
                            _currentIndexingProgress = p;
                            _hubContext.Clients.All.SendAsync("ragIndexingProgress", new
                            {
                                status = p.Status,
                                processed = p.Processed,
                                total = p.Total,
                                message = p.Message,
                                scope = p.Scope ?? "directory"
                            });
                        });

                        await ragService.IndexDirectoryAsync(request.DirectoryPath, request.ProjectPath, progress, request.ForceReindex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RAG] Background directory index error");
                        _currentIndexingProgress = new RagIndexingProgress
                        {
                            Status = "error",
                            Processed = 0,
                            Total = 0,
                            Message = $"Directory indexing error: {ex.Message}",
                            Scope = "directory"
                        };
                        await _hubContext.Clients.All.SendAsync("ragIndexingProgress", new
                        {
                            status = "error",
                            processed = 0,
                            total = 0,
                            message = $"Directory indexing error: {ex.Message}",
                            scope = "directory"
                        });
                    }
                    finally
                    {
                        (isolatedEngineDB as IDisposable)?.Dispose();
                    }
                });

                return Ok(new { started = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Index directory error");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("document")]
        public IActionResult GetDocument([FromQuery] string path, [FromQuery] string project = null)
        {
            IEngineDB isolatedDB = null;
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return BadRequest(new { error = "Path parameter is required" });
                }

                var engineDB = _engineDB;
                if (!string.IsNullOrEmpty(project))
                {
                    var projectPath = ResolveProjectPath(project);
                    if (projectPath == null)
                        return NotFound(new { error = $"Project not found: {project}" });
                    isolatedDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);
                    engineDB = isolatedDB;
                }

                // Find the markdown file in the engine DB
                var mdFileDal = engineDB.GetDal<MarkdownFile>();
                var mdFile = mdFileDal.GetList()
                    .FirstOrDefault(f => f.Path.Contains(path) || f.FileName.Contains(path));

                if (mdFile == null)
                {
                    return NotFound(new { error = $"Document not found: {path}" });
                }

                if (!System.IO.File.Exists(mdFile.Path))
                {
                    return NotFound(new { error = $"File not found on disk: {mdFile.Path}" });
                }

                var content = System.IO.File.ReadAllText(mdFile.Path);

                return Ok(new
                {
                    path = mdFile.Path,
                    fileName = mdFile.FileName,
                    content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RAG] Error getting document");
                return StatusCode(500, new { error = ex.Message });
            }
            finally
            {
                (isolatedDB as IDisposable)?.Dispose();
            }
        }

        #region Helpers

        /// <summary>
        /// Resolves a project name to its filesystem path by looking up the UserSettings DB.
        /// </summary>
        private string ResolveProjectPath(string projectName)
        {
            try
            {
                var projectDal = _userSettingsDB.GetDal<Project>();
                var projects = projectDal.GetList().ToList();
                var project = projects.FirstOrDefault(p => p.Name != null &&
                    p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
                return project?.Path;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RAG] Error resolving project name: {Name}", projectName);
                return null;
            }
        }

        public bool IsRagEnabled()
        {
            try
            {
                var settingsDal = _projectDB.GetDal<ProjectSetting>();
                var setting = settingsDal.GetList()
                    .FirstOrDefault(s => s.Name == "RagEnabled");
                return setting?.ValueBool ?? false;
            }
            catch
            {
                return false;
            }
        }

        private void SetRagEnabled(bool enabled)
        {
            _projectDB.BeginTransaction();
            var settingsDal = _projectDB.GetDal<ProjectSetting>();
            var setting = settingsDal.GetList()
                .FirstOrDefault(s => s.Name == "RagEnabled");

            if (setting != null)
            {
                setting.ValueBool = enabled;
                settingsDal.Save(setting);
            }
            else
            {
                settingsDal.Save(new ProjectSetting
                {
                    Name = "RagEnabled",
                    Description = "Enable RAG (Retrieval Augmented Generation) for this project",
                    ValueBool = enabled
                });
            }
            _projectDB.Commit();
        }

        #endregion
    }
}
