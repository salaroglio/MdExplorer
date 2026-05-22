using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class KgSyncOrchestrator : IKgSyncOrchestrator
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IPasswordProtector _passwordProtector;
        private readonly INeo4jConnectionPool _connectionPool;
        private readonly IKgIngestService _ingestService;
        private readonly IFolderKgConfigResolver _folderResolver;
        private readonly IFolderKgConfigWriter _folderWriter;
        private readonly ILogger<KgSyncOrchestrator> _logger;

        public KgSyncOrchestrator(
            IUserSettingsDB userSettingsDB,
            IPasswordProtector passwordProtector,
            INeo4jConnectionPool connectionPool,
            IKgIngestService ingestService,
            IFolderKgConfigResolver folderResolver,
            IFolderKgConfigWriter folderWriter,
            ILogger<KgSyncOrchestrator> logger)
        {
            _userSettingsDB = userSettingsDB;
            _passwordProtector = passwordProtector;
            _connectionPool = connectionPool;
            _ingestService = ingestService;
            _folderResolver = folderResolver;
            _folderWriter = folderWriter;
            _logger = logger;
        }

        public Task<KgSyncOutcome> SyncFolderAsync(string folderAbsolutePath, KgSyncTrigger trigger, CancellationToken ct = default)
            => SyncInternalAsync(folderAbsolutePath, isFolder: true, trigger, ct);

        public Task<KgSyncOutcome> SyncFileAsync(string kgFileAbsolutePath, KgSyncTrigger trigger, CancellationToken ct = default)
            => SyncInternalAsync(kgFileAbsolutePath, isFolder: false, trigger, ct);

        private async Task<KgSyncOutcome> SyncInternalAsync(string absolutePath, bool isFolder, KgSyncTrigger trigger, CancellationToken ct)
        {
            var outcome = new KgSyncOutcome();
            if (string.IsNullOrEmpty(absolutePath))
            {
                outcome.Reason = "empty path";
                return outcome;
            }

            try
            {
                // ---- 1) Lookup project (longest matching Path) + settings in a single transaction ----
                Project project;
                ProjectNeo4jSettings settings;
                try
                {
                    _userSettingsDB.BeginTransaction();
                    var allProjects = _userSettingsDB.GetDal<Project>().GetList().ToList();
                    project = allProjects
                        .Where(p => !string.IsNullOrEmpty(p.Path) && PathContains(p.Path, absolutePath))
                        .OrderByDescending(p => p.Path.Length)
                        .FirstOrDefault();

                    settings = project == null ? null : _userSettingsDB.GetDal<ProjectNeo4jSettings>().GetList()
                        .FirstOrDefault(s => s.Project.Id == project.Id);
                    _userSettingsDB.Commit();
                }
                catch
                {
                    try { _userSettingsDB.Rollback(); } catch { }
                    throw;
                }

                if (project == null) { outcome.Reason = "no project owns this path"; return outcome; }
                if (settings == null || !settings.Enabled) { outcome.Reason = "KG disabled for project"; return outcome; }

                // ---- 2) Per-trigger flag gate ----
                if (trigger == KgSyncTrigger.TocGeneration && !settings.SyncOnTocGeneration) { outcome.Reason = "SyncOnTocGeneration disabled"; return outcome; }
                if (trigger == KgSyncTrigger.KgFileSave && !settings.SyncOnKgFileSave) { outcome.Reason = "SyncOnKgFileSave disabled"; return outcome; }
                if (string.IsNullOrEmpty(settings.PasswordEncrypted)) { outcome.Reason = "Neo4j password not set"; return outcome; }

                // ---- 3) Resolve folder + namespace ----
                string folderAbs;
                if (isFolder)
                {
                    folderAbs = absolutePath;
                }
                else
                {
                    // .kg.cypher lives in <folder>/.mde-doc/<name>.kg.cypher  → folder is the grandparent
                    var mdeDocDir = Path.GetDirectoryName(absolutePath);
                    folderAbs = Path.GetDirectoryName(mdeDocDir);
                }
                var folderCfg = _folderResolver.Resolve(project.Path, folderAbs);
                if (folderCfg == null)
                {
                    // KG is enabled for the project (checked above) but this folder has
                    // no namespace yet. Rather than silently skip, create one with a
                    // deterministic default so the .kg.cypher always has somewhere to go.
                    folderCfg = _folderWriter.EnsureFolderConfig(project.Path, folderAbs);
                    if (folderCfg != null && !string.IsNullOrWhiteSpace(folderCfg.Namespace))
                    {
                        outcome.AutoCreatedNamespace = folderCfg.Namespace;
                        _logger.LogInformation("[KgSync] auto-created KG namespace '{Ns}' for folder {Folder}",
                            folderCfg.Namespace, folderAbs);
                    }
                }
                if (folderCfg == null || !folderCfg.Enabled || string.IsNullOrWhiteSpace(folderCfg.Namespace))
                {
                    outcome.Reason = "folder has no knowledgeGraph.namespace";
                    return outcome;
                }

                // ---- 4) Build batch ----
                var batch = new List<KgBatchFile>();
                if (isFolder)
                {
                    var mdeDocDir = Path.Combine(folderAbs, ".mde-doc");
                    if (!Directory.Exists(mdeDocDir)) { outcome.Reason = "no .mde-doc/ folder"; return outcome; }
                    var kgFiles = Directory.GetFiles(mdeDocDir, "*.kg.cypher", SearchOption.TopDirectoryOnly);
                    foreach (var f in kgFiles)
                    {
                        batch.Add(new KgBatchFile
                        {
                            KgFileAbsolutePath = f,
                            GraphNamespace = folderCfg.Namespace,
                            PreviousHash = LookupHash(project.Id, MakeRelative(project.Path, f))
                        });
                    }
                }
                else
                {
                    batch.Add(new KgBatchFile
                    {
                        KgFileAbsolutePath = absolutePath,
                        GraphNamespace = folderCfg.Namespace,
                        PreviousHash = LookupHash(project.Id, MakeRelative(project.Path, absolutePath))
                    });
                }
                if (batch.Count == 0) { outcome.Reason = "no .kg.cypher files in scope"; return outcome; }

                // ---- 5) Decrypt password, open session, run ingest ----
                var password = _passwordProtector.Unprotect(settings.PasswordEncrypted);
                var driver = _connectionPool.GetOrCreateDriver(project.Id, settings.Uri, settings.Username, password ?? string.Empty);
                IList<KgIngestResult> results;
                await using (var session = driver.AsyncSession(b =>
                {
                    if (!string.IsNullOrWhiteSpace(settings.Database)) b.WithDatabase(settings.Database);
                    b.WithDefaultAccessMode(AccessMode.Write);
                }))
                {
                    results = await _ingestService.IngestKgFilesAsync(project.Id, project.Path, batch, session, ct);
                }

                // ---- 6) Persist KgIngestState rows ----
                PersistIngestState(project, results);

                outcome.Triggered = true;
                outcome.SucceededFiles = results.Count(r => !r.HasError && !r.Skipped);
                outcome.SkippedFiles = results.Count(r => r.Skipped);
                outcome.FailedFiles = results.Count(r => r.HasError);
                outcome.FirstError = results.FirstOrDefault(r => r.HasError)?.Error;
                _logger.LogInformation("[KgSync] trigger={Trigger} folder={Folder} → {Ok} ok, {Sk} skipped, {Fail} failed",
                    trigger, isFolder ? folderAbs : absolutePath, outcome.SucceededFiles, outcome.SkippedFiles, outcome.FailedFiles);
                return outcome;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgSync] failed for {Path}", absolutePath);
                outcome.FirstError = ex.Message;
                outcome.Reason = "exception";
                return outcome;
            }
        }

        // ---- helpers ----

        private static bool PathContains(string projectPath, string candidate)
        {
            var pNorm = Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar);
            var cNorm = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar);
            if (string.Equals(pNorm, cNorm, StringComparison.OrdinalIgnoreCase)) return true;
            return cNorm.StartsWith(pNorm + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static string MakeRelative(string projectPath, string absolutePath)
            => Path.GetRelativePath(projectPath, absolutePath).Replace('\\', '/');

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
                try { _userSettingsDB.Rollback(); } catch { }
                return null;
            }
        }

        private void PersistIngestState(Project project, IList<KgIngestResult> results)
        {
            if (results == null || results.Count == 0) return;
            try
            {
                _userSettingsDB.BeginTransaction();
                var dal = _userSettingsDB.GetDal<KgIngestState>();
                var existing = dal.GetList().Where(s => s.Project.Id == project.Id).ToList();
                foreach (var r in results)
                {
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
                    row.NodeCount = r.NodeCount;
                    row.EdgeCount = r.EdgeCount;
                    dal.Save(row);
                }
                _userSettingsDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KgSync] PersistIngestState failed for {ProjectId}", project.Id);
                try { _userSettingsDB.Rollback(); } catch { }
            }
        }
    }
}
