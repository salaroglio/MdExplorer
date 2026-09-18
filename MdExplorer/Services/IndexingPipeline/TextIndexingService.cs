using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Service.Services;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.IndexingPipeline
{
    /// <summary>
    /// See <see cref="ITextIndexingService"/>. A slim, FTS-only twin of
    /// <see cref="IndexingPipelineService"/> for non-markdown text files. Shares the
    /// same isolated-engine-DB + fingerprint diff machinery, but writes to the
    /// TextFile table and the text FTS side-car only — the markdown world is untouched.
    /// </summary>
    public class TextIndexingService : ITextIndexingService
    {
        private readonly ILogger<TextIndexingService> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly ITextFtsService _textFtsService;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        private readonly IHubContext<MonitorMDHub> _hubContext;

        // One run per project: a new run cancels the previous and waits for DB release.
        private sealed class TextRun
        {
            public CancellationTokenSource Cts { get; init; }
            public TaskCompletionSource<bool> Done { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        private readonly object _runsLock = new object();
        private readonly Dictionary<string, TextRun> _activeRuns =
            new Dictionary<string, TextRun>(StringComparer.OrdinalIgnoreCase);

        private sealed class StoredFingerprint
        {
            public Guid Id { get; set; }
            public string Path { get; set; }
            public string FileLastWriteUtc { get; set; }
            public long? FileSize { get; set; }
            public string FileHash { get; set; }
            public string FtsHash { get; set; }
        }

        private sealed class TextPipelineFile
        {
            public Guid Id { get; set; }
            public string Path { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }
            public string StatMtime { get; set; }
            public long StatSize { get; set; }
            public string FileHash { get; set; }
        }

        private sealed class DiffStats
        {
            public int New, Changed, StatOnly, Unchanged, Unreadable, FtsUpserts;
        }

        public TextIndexingService(
            ILogger<TextIndexingService> logger,
            IDatabaseManager databaseManager,
            ITextFtsService textFtsService,
            IMdIgnoreService mdIgnoreService,
            FoldersIgnoreService foldersIgnoreService,
            IHubContext<MonitorMDHub> hubContext)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _textFtsService = textFtsService;
            _mdIgnoreService = mdIgnoreService;
            _foldersIgnoreService = foldersIgnoreService;
            _hubContext = hubContext;
        }

        public async Task RunAsync(string connectionId, string projectPath, IReadOnlyCollection<string> extensions,
            bool forceFullReindex = false, CancellationToken ct = default)
        {
            // Detach from the caller (see IndexingPipelineService for the rationale).
            await Task.Yield();

            var effective = new HashSet<string>(
                extensions ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var md in TextFileClassifier.MarkdownExtensions)
            {
                effective.Remove(md);
            }
            if (effective.Count == 0)
            {
                _logger.LogInformation("[TextIndexing] Empty allow-list for '{ProjectPath}', nothing to index", projectPath);
                return;
            }

            var run = new TextRun { Cts = CancellationTokenSource.CreateLinkedTokenSource(ct) };
            TextRun previous;
            lock (_runsLock)
            {
                _activeRuns.TryGetValue(projectPath, out previous);
                _activeRuns[projectPath] = run;
            }

            if (previous != null)
            {
                _logger.LogWarning("[TextIndexing] A run is already active for '{ProjectPath}' — cancelling it", projectPath);
                previous.Cts.Cancel();
                await previous.Done.Task;
            }

            try
            {
                await RunCoreAsync(connectionId, projectPath, effective, forceFullReindex, run.Cts.Token);
            }
            finally
            {
                lock (_runsLock)
                {
                    if (_activeRuns.TryGetValue(projectPath, out var current) && ReferenceEquals(current, run))
                    {
                        _activeRuns.Remove(projectPath);
                    }
                }
                run.Done.TrySetResult(true);
                run.Cts.Dispose();
            }
        }

        private async Task RunCoreAsync(string connectionId, string projectPath,
            HashSet<string> extensions, bool force, CancellationToken ct)
        {
            _logger.LogInformation(
                "[TextIndexing] STARTED projectPath='{ProjectPath}' connectionId='{ConnectionId}' force={Force} extensions={Count}",
                projectPath, connectionId, force, extensions.Count);

            IEngineDB isolatedDB = null;
            try
            {
                if (string.IsNullOrEmpty(projectPath) || projectPath == AppDomain.CurrentDomain.BaseDirectory)
                {
                    _logger.LogWarning("[TextIndexing] Invalid project path, skipping");
                    return;
                }

                isolatedDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);

                ct.ThrowIfCancellationRequested();
                var fsFiles = ScanFileSystem(projectPath, extensions);

                var (stored, duplicates) = LoadFingerprints(isolatedDB);

                ct.ThrowIfCancellationRequested();
                var deletedCount = Reconcile(isolatedDB, projectPath, stored, duplicates, fsFiles);

                ct.ThrowIfCancellationRequested();
                var stats = new DiffStats();
                DiffUpsertAndFts(isolatedDB, projectPath, stored, fsFiles, force, stats, ct);

                _logger.LogInformation(
                    "[TextIndexing] SUMMARY new={New} changed={Changed} statOnly={StatOnly} unchanged={Unchanged} deleted={Deleted} unreadable={Unreadable} ftsUpserts={FtsUpserts} forced={Forced}",
                    stats.New, stats.Changed, stats.StatOnly, stats.Unchanged, deletedCount, stats.Unreadable, stats.FtsUpserts, force);

                await SafeSendAsync(connectionId, "textIndexingCompleted", new
                {
                    indexed = fsFiles.Count,
                    changed = stats.New + stats.Changed
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[TextIndexing] CANCELLED projectPath='{ProjectPath}'", projectPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TextIndexing] ERROR projectPath='{ProjectPath}'", projectPath);
            }
            finally
            {
                try { (isolatedDB as IDisposable)?.Dispose(); }
                catch (Exception ex) { _logger.LogWarning(ex, "[TextIndexing] Error disposing IsolatedEngineDB"); }
            }
        }

        // ─── Filesystem scan: allow-list, same ignore filters as the markdown scan ───
        private List<string> ScanFileSystem(string projectPath, HashSet<string> extensions)
        {
            var files = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".md" + Path.DirectorySeparatorChar))
                .Where(f => TextFileClassifier.IsEligibleTextFile(f, extensions))
                .Where(f => !_mdIgnoreService.ShouldIgnorePath(f, projectPath))
                .Where(f => !IsInIgnoredFolder(f, projectPath))
                .ToList();

            _logger.LogInformation("[TextIndexing] Scan: found {Count} text files", files.Count);
            return files;
        }

        private bool IsInIgnoredFolder(string filePath, string projectPath)
        {
            var directory = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrEmpty(directory) && directory.Length > projectPath.Length)
            {
                if (_foldersIgnoreService.ShouldIgnoreFolderForProject(directory, projectPath))
                    return true;
                directory = Path.GetDirectoryName(directory);
            }
            return false;
        }

        private (Dictionary<string, StoredFingerprint> stored, List<StoredFingerprint> duplicates)
            LoadFingerprints(IEngineDB engineDB)
        {
            var rows = engineDB.GetDal<TextFile>().GetList()
                .Select(m => new StoredFingerprint
                {
                    Id = m.Id,
                    Path = m.Path,
                    FileLastWriteUtc = m.FileLastWriteUtc,
                    FileSize = m.FileSize,
                    FileHash = m.FileHash,
                    FtsHash = m.FtsHash
                })
                .ToList();

            var stored = new Dictionary<string, StoredFingerprint>(ContentFingerprint.PathComparer);
            var duplicates = new List<StoredFingerprint>();
            foreach (var row in rows)
            {
                if (!stored.TryAdd(row.Path, row))
                {
                    duplicates.Add(row);
                }
            }
            return (stored, duplicates);
        }

        private int Reconcile(
            IEngineDB engineDB,
            string projectPath,
            Dictionary<string, StoredFingerprint> stored,
            List<StoredFingerprint> duplicates,
            List<string> fsFiles)
        {
            var fsSet = new HashSet<string>(fsFiles, ContentFingerprint.PathComparer);
            var toDelete = stored.Values.Where(f => !fsSet.Contains(f.Path)).ToList();
            toDelete.AddRange(duplicates);

            if (toDelete.Count == 0)
            {
                return 0;
            }

            _logger.LogInformation("[TextIndexing] Reconcile: removing {Count} stale/duplicate records", toDelete.Count);
            try
            {
                engineDB.BeginTransaction();
                foreach (var dead in toDelete)
                {
                    engineDB.CreateSQLQuery("DELETE FROM TextFile WHERE Id = :id")
                        .SetParameter("id", dead.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                }
                engineDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TextIndexing] Reconcile failed - rolling back");
                try { engineDB.Rollback(); } catch { }
                throw;
            }

            foreach (var dead in toDelete)
            {
                _textFtsService.DeleteFileByPath(projectPath, dead.Path);
                stored.Remove(dead.Path);
            }

            return toDelete.Count;
        }

        private void DiffUpsertAndFts(
            IEngineDB engineDB,
            string projectPath,
            Dictionary<string, StoredFingerprint> stored,
            List<string> fsFiles,
            bool force,
            DiffStats stats,
            CancellationToken ct)
        {
            var statOnlyQueue = new List<TextPipelineFile>();
            var ftsRebuildEntries = force ? new List<TextFtsEntry>() : null;

            foreach (var filePath in fsFiles)
            {
                ct.ThrowIfCancellationRequested();

                string statMtime;
                long statSize;
                try
                {
                    var fi = new FileInfo(filePath);
                    statMtime = fi.LastWriteTimeUtc.ToString("o");
                    statSize = fi.Length;
                }
                catch (Exception statEx)
                {
                    _logger.LogWarning(statEx, "[TextIndexing] Diff: cannot stat '{Path}', skipping", filePath);
                    stats.Unreadable++;
                    continue;
                }

                stored.TryGetValue(filePath, out var prev);
                var pf = new TextPipelineFile
                {
                    Id = prev?.Id ?? Guid.Empty,
                    Path = filePath,
                    FileName = Path.GetFileName(filePath),
                    Extension = Path.GetExtension(filePath)?.ToLowerInvariant(),
                    StatMtime = statMtime,
                    StatSize = statSize
                };

                string content = null;

                if (!force && prev != null && prev.FileHash != null
                    && prev.FileLastWriteUtc == statMtime && prev.FileSize == statSize)
                {
                    pf.FileHash = prev.FileHash;
                    stats.Unchanged++;
                }
                else
                {
                    content = SafeReadAllText(filePath);
                    if (content == null)
                    {
                        stats.Unreadable++;
                        if (prev == null)
                        {
                            var newId = InsertTextFile(engineDB, pf, fileHash: null);
                            if (newId == Guid.Empty) { continue; }
                            pf.Id = newId;
                        }
                        continue;
                    }

                    pf.FileHash = ContentFingerprint.ComputeHash(content);

                    if (!force && prev != null && pf.FileHash == prev.FileHash)
                    {
                        statOnlyQueue.Add(pf);
                        stats.StatOnly++;
                    }
                    else if (prev == null)
                    {
                        var newId = InsertTextFile(engineDB, pf, pf.FileHash);
                        if (newId == Guid.Empty) { continue; }
                        pf.Id = newId;
                        stats.New++;
                    }
                    else
                    {
                        UpdateTextFileFingerprint(engineDB, pf);
                        stats.Changed++;
                    }
                }

                // FTS catch-up (side-car text DB, independent of engine transactions)
                if (force)
                {
                    ftsRebuildEntries.Add(new TextFtsEntry
                    {
                        TextFileId = pf.Id,
                        Path = pf.Path,
                        FileName = pf.FileName,
                        Content = content ?? SafeReadAllText(pf.Path)
                    });
                }
                else if (pf.FileHash != null && prev?.FtsHash != pf.FileHash)
                {
                    content ??= SafeReadAllText(pf.Path);
                    if (content != null)
                    {
                        _textFtsService.UpsertFile(projectPath, pf.Id, pf.Path, pf.FileName, content);
                        ExecuteSmallUpdate(engineDB,
                            "UPDATE TextFile SET FtsHash = :hash WHERE Id = :id",
                            q => q.SetParameter("hash", pf.FileHash).SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid));
                        stats.FtsUpserts++;
                    }
                }
            }

            if (statOnlyQueue.Count > 0)
            {
                try
                {
                    engineDB.BeginTransaction();
                    foreach (var pf in statOnlyQueue)
                    {
                        engineDB.CreateSQLQuery(
                            "UPDATE TextFile SET FileLastWriteUtc = :m, FileSize = :s WHERE Id = :id")
                            .SetParameter("m", pf.StatMtime)
                            .SetParameter("s", pf.StatSize)
                            .SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid)
                            .ExecuteUpdate();
                    }
                    engineDB.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TextIndexing] Diff: stat-only batch update failed - rolling back");
                    try { engineDB.Rollback(); } catch { }
                    throw;
                }
            }

            if (force)
            {
                _textFtsService.RebuildIndex(projectPath, ftsRebuildEntries.Where(e => e.Content != null).ToList());
                ExecuteSmallUpdate(engineDB, "UPDATE TextFile SET FtsHash = FileHash", q => q);
                stats.FtsUpserts = ftsRebuildEntries.Count;
            }
        }

        private Guid InsertTextFile(IEngineDB engineDB, TextPipelineFile pf, string fileHash)
        {
            try
            {
                engineDB.BeginTransaction();
                var tf = new TextFile
                {
                    FileName = pf.FileName,
                    Path = pf.Path,
                    Extension = pf.Extension,
                    FileLastWriteUtc = pf.StatMtime,
                    FileSize = pf.StatSize,
                    FileHash = fileHash
                };
                engineDB.GetDal<TextFile>().Save(tf);
                engineDB.Commit();
                engineDB.Evict(tf);
                return tf.Id;
            }
            catch (Exception ex)
            {
                try { engineDB.Rollback(); } catch { }
                _logger.LogWarning(ex,
                    "[TextIndexing] Insert failed for '{Path}' (likely UNIQUE race) — converting to update", pf.Path);
                try
                {
                    var existingId = engineDB.CreateSQLQuery("SELECT Id FROM TextFile WHERE Path = :path")
                        .AddScalar("Id", NHibernate.NHibernateUtil.Guid)
                        .SetParameter("path", pf.Path)
                        .List<Guid>()
                        .FirstOrDefault();
                    if (existingId != Guid.Empty)
                    {
                        pf.Id = existingId;
                        UpdateTextFileFingerprint(engineDB, pf);
                        return existingId;
                    }
                }
                catch (Exception requeryEx)
                {
                    _logger.LogError(requeryEx, "[TextIndexing] Requery-by-path failed for '{Path}'", pf.Path);
                }
                return Guid.Empty;
            }
        }

        private void UpdateTextFileFingerprint(IEngineDB engineDB, TextPipelineFile pf)
        {
            ExecuteSmallUpdate(engineDB,
                "UPDATE TextFile SET FileName = :name, Extension = :ext, FileLastWriteUtc = :m, FileSize = :s, FileHash = :hash WHERE Id = :id",
                q => q.SetParameter("name", pf.FileName)
                      .SetParameter("ext", pf.Extension)
                      .SetParameter("m", pf.StatMtime)
                      .SetParameter("s", pf.StatSize)
                      .SetParameter("hash", pf.FileHash)
                      .SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid));
        }

        private void ExecuteSmallUpdate(IEngineDB engineDB, string sql, Func<NHibernate.ISQLQuery, NHibernate.IQuery> bind)
        {
            try
            {
                engineDB.BeginTransaction();
                bind(engineDB.CreateSQLQuery(sql)).ExecuteUpdate();
                engineDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TextIndexing] Update failed: {Sql}", sql);
                try { engineDB.Rollback(); } catch { }
                throw;
            }
        }

        private string SafeReadAllText(string path)
        {
            try { return SharedFileReader.ReadAllText(path); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TextIndexing] Cannot read '{Path}'", path);
                return null;
            }
        }

        private async Task SafeSendAsync(string connectionId, string method, object arg)
        {
            if (string.IsNullOrEmpty(connectionId)) { return; }
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(method, arg);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TextIndexing] Could not send '{Method}' to {ConnectionId}", method, connectionId);
            }
        }
    }
}
