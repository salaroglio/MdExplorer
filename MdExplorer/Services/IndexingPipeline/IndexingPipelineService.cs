using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Service.Services;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
    /// Pipeline di indicizzazione INCREMENTALE (singleton).
    /// Vedi <see cref="IIndexingPipelineService"/> per il contratto.
    ///
    /// Diff su fingerprint persistiti su MarkdownFile (mtime+size → hash, stesso
    /// modello two-tier di RagIndexingService):
    ///   - FileHash  = identità del contenuto come ultimo osservato
    ///   - LinksHash = FileHash all'ultimo parse link+TLDR riuscito
    ///   - FtsHash   = FileHash all'ultimo upsert FTS riuscito
    /// Un sottosistema è aggiornato per un file sse il suo hash == FileHash:
    /// ogni fase è incrementale E riprendibile dopo una run cancellata, senza
    /// marker globali (le transazioni per-file aggiornano gli hash man mano).
    ///
    /// Gli Id dei MarkdownFile sono STABILI tra aperture (upsert per Path,
    /// UNIQUE index UX_MarkdownFile_Path): questo rende finalmente efficace lo
    /// skip per FileHash dei DocumentChunk (Phase 4) anche tra una run e l'altra.
    ///
    /// Pattern preso da ReEmbedFileAsync (FileSystemWatcherManager): IsolatedEngineDB
    /// per il long-lived, scope solo per i singleton (Embedding/Chunking/VectorSearch).
    /// </summary>
    public class IndexingPipelineService : IIndexingPipelineService
    {
        private readonly ILogger<IndexingPipelineService> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _fileSystemWatcherManager;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IWorkLink[] _linkParsers;
        private readonly IHelper _helper;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEmbeddingService _embeddingService;
        private readonly IMarkdownChunkingService _chunkingService;
        private readonly IModelDownloadService _downloadService;
        private readonly IMarkdownFtsService _markdownFtsService;
        // IVectorSearchService NON iniettato qui: è registrato come Scoped in Startup.cs:145.
        // Singleton non può consumare Scoped. Si ottiene via _serviceScopeFactory.CreateScope()
        // alla fine della fase Embed per InvalidateCache. Stesso pattern di ReEmbedFileAsync (FSW).

        // ── Serializzazione per projectPath ──
        // Due pipeline concorrenti sullo stesso progetto interleavano le scritture
        // sullo stesso SQLite (duplicati/record persi).
        // Una nuova RunAsync per lo stesso path CANCELLA la run precedente e
        // ATTENDE che abbia rilasciato il DB prima di partire. Mai due attive.
        private sealed class PipelineRun
        {
            public CancellationTokenSource Cts { get; init; }
            // TCS (not the raw Task) so a successor can await release even if it
            // observes this run between registration and core start.
            public TaskCompletionSource<bool> Done { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        private readonly object _runsLock = new object();
        private readonly Dictionary<string, PipelineRun> _activeRuns =
            new Dictionary<string, PipelineRun>(StringComparer.OrdinalIgnoreCase);

        // Fingerprint di un MarkdownFile come letto dal DB (proiezione, mai entità:
        // la sessione non deve idratare l'intero progetto nella first-level cache).
        private sealed class StoredFingerprint
        {
            public Guid Id { get; set; }
            public string Path { get; set; }
            public string FileLastWriteUtc { get; set; }
            public long? FileSize { get; set; }
            public string FileHash { get; set; }
            public string LinksHash { get; set; }
            public string FtsHash { get; set; }
        }

        // Stato di un file sul filesystem dopo la fase di diff.
        private sealed class PipelineFile
        {
            public Guid Id { get; set; }
            public string Path { get; set; }
            public string FileName { get; set; }
            public string StatMtime { get; set; }   // "o" format
            public long StatSize { get; set; }
            public string FileHash { get; set; }    // hash effettivo corrente (null se file illeggibile)
            public string LinksHash { get; set; }   // come memorizzato prima di questa run
            public bool IsNew { get; set; }
            public bool ContentChanged { get; set; }
            public bool NeedsLinks => FileHash != null && (LinksHash == null || LinksHash != FileHash);
        }

        private sealed class DiffStats
        {
            public int New, Changed, StatOnly, Unchanged, Unreadable, FtsUpserts;
        }

        public IndexingPipelineService(
            ILogger<IndexingPipelineService> logger,
            IDatabaseManager databaseManager,
            IFileSystemWatcherManager fileSystemWatcherManager,
            IHubContext<MonitorMDHub> hubContext,
            IWorkLink[] linkParsers,
            IHelper helper,
            IMdIgnoreService mdIgnoreService,
            FoldersIgnoreService foldersIgnoreService,
            IServiceScopeFactory serviceScopeFactory,
            IEmbeddingService embeddingService,
            IMarkdownChunkingService chunkingService,
            IModelDownloadService downloadService,
            IMarkdownFtsService markdownFtsService)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _fileSystemWatcherManager = fileSystemWatcherManager;
            _hubContext = hubContext;
            _linkParsers = linkParsers;
            _helper = helper;
            _mdIgnoreService = mdIgnoreService;
            _foldersIgnoreService = foldersIgnoreService;
            _serviceScopeFactory = serviceScopeFactory;
            _embeddingService = embeddingService;
            _chunkingService = chunkingService;
            _downloadService = downloadService;
            _markdownFtsService = markdownFtsService;
        }

        public async Task RunAsync(string connectionId, string projectPath, bool linkIndexingEnabled, bool forceFullReindex = false, CancellationToken ct = default)
        {
            // FIRE-AND-FORGET CORRECTNESS — vedi diagnosi 2026-05-23 sui log di anagrafica_reale.
            //
            // Il chiamante è MdFilesController.GetShallowStructure che usa
            //   _ = _indexingPipelineService.RunAsync(...);
            // per "staccare" la pipeline dalla response HTTP.
            //
            // Senza Task.Yield qui, l'intero metodo gira sincrono sul thread del controller
            // finché non incontra un await che effettivamente cede (SignalR SendAsync e le
            // DB ops NHibernate spesso completano sincrone). Task.Yield posta la
            // continuazione sul ThreadPool: il chiamante riprende immediatamente.
            await Task.Yield();

            // ── Serializzazione per projectPath ──
            // Una sola run attiva per progetto: la nuova CANCELLA la precedente e
            // ATTENDE che abbia rilasciato l'IsolatedEngineDB prima di partire.
            var run = new PipelineRun { Cts = CancellationTokenSource.CreateLinkedTokenSource(ct) };
            PipelineRun previous;
            lock (_runsLock)
            {
                _activeRuns.TryGetValue(projectPath, out previous);
                _activeRuns[projectPath] = run;
            }

            if (previous != null)
            {
                _logger.LogWarning(
                    "[IndexingPipeline] A run is already active for '{ProjectPath}' — cancelling it and waiting for DB release",
                    projectPath);
                previous.Cts.Cancel();
                await previous.Done.Task;
            }

            try
            {
                await RunCoreAsync(connectionId, projectPath, linkIndexingEnabled, forceFullReindex, run.Cts.Token);
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

        private async Task RunCoreAsync(string connectionId, string projectPath, bool linkIndexingEnabled, bool force, CancellationToken ct)
        {
            _logger.LogInformation(
                "[IndexingPipeline] STARTED projectPath='{ProjectPath}' connectionId='{ConnectionId}' linkIndexingEnabled={LinkIndexingEnabled} force={Force}",
                projectPath, connectionId, linkIndexingEnabled, force);

            IEngineDB isolatedDB = null;
            try
            {
                if (string.IsNullOrEmpty(projectPath) || projectPath == AppDomain.CurrentDomain.BaseDirectory)
                {
                    _logger.LogWarning("[IndexingPipeline] Invalid project path, skipping");
                    return;
                }

                isolatedDB = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);

                // Scan filesystem (unica visita ricorsiva)
                ct.ThrowIfCancellationRequested();
                var fsFiles = ScanFileSystem(projectPath);

                // Fingerprint memorizzati (proiezione, niente entità in sessione)
                var (stored, duplicates) = LoadFingerprints(isolatedDB);

                // Phase 1: RECONCILE — file nel DB ma non più su disco + duplicati difensivi
                ct.ThrowIfCancellationRequested();
                var deletedCount = Reconcile(isolatedDB, projectPath, stored, duplicates, fsFiles);

                // Phase 2: DIFF + UPSERT (+ FTS catch-up)
                ct.ThrowIfCancellationRequested();
                var stats = new DiffStats();
                var files = DiffUpsertAndFts(isolatedDB, projectPath, stored, fsFiles, force, stats, ct);

                // Phase 3: parse link + TLDR, solo sul work set
                var work = force ? files.Where(f => f.FileHash != null).ToList()
                                 : files.Where(f => f.NeedsLinks).ToList();
                var linksParsed = 0;
                if (linkIndexingEnabled && work.Count > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    await SafeSendAsync(connectionId, "parsingProjectStart", "process started");
                    linksParsed = await ParseLinksWithFolderEvents(isolatedDB, work, projectPath, connectionId, ct);
                    await SafeSendAsync(connectionId, "parsingProjectStop", "process completed");
                }

                // Phase 4: embedding two-tier su TUTTI i file (skip economici per gli invariati)
                var embedded = 0;
                if (linkIndexingEnabled && files.Count > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    embedded = await EmbedDocumentsIfEnabled(isolatedDB, files, projectPath, connectionId, force, ct);
                }

                // Riga di contratto per diagnostica e verifica automatica.
                _logger.LogInformation(
                    "[IndexingPipeline] SUMMARY new={New} changed={Changed} statOnly={StatOnly} unchanged={Unchanged} deleted={Deleted} unreadable={Unreadable} ftsUpserts={FtsUpserts} linksParsed={LinksParsed} embedded={Embedded} forced={Forced}",
                    stats.New, stats.Changed, stats.StatOnly, stats.Unchanged, deletedCount, stats.Unreadable, stats.FtsUpserts, linksParsed, embedded, force);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[IndexingPipeline] CANCELLED projectPath='{ProjectPath}'", projectPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IndexingPipeline] ERROR projectPath='{ProjectPath}'", projectPath);
            }
            finally
            {
                // Always dispose the isolated session
                try
                {
                    (isolatedDB as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Error disposing IsolatedEngineDB");
                }

                // Always re-enable FSW even if pipeline failed
                try
                {
                    _fileSystemWatcherManager?.SetWatcherEnabled(connectionId, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Could not re-enable FSW for {ConnectionId}", connectionId);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Filesystem scan (stessi filtri della vecchia IndexFiles)
        // ─────────────────────────────────────────────────────────────────────
        private List<string> ScanFileSystem(string projectPath)
        {
            var allMdFiles = Directory.GetFiles(projectPath, "*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".md" + Path.DirectorySeparatorChar))
                .Where(f => !_mdIgnoreService.ShouldIgnorePath(f, projectPath))
                .Where(f => !IsInIgnoredFolder(f, projectPath))
                .ToList();

            _logger.LogInformation("[IndexingPipeline] Scan: found {Count} markdown files", allMdFiles.Count);
            return allMdFiles;
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

        // ─────────────────────────────────────────────────────────────────────
        // Fingerprint load: proiezione a 7 colonne, mai entità.
        // Duplicati per Path (non dovrebbero esistere post-migration UNIQUE):
        // teniamo il primo e mettiamo gli altri in coda di cancellazione.
        // ─────────────────────────────────────────────────────────────────────
        private (Dictionary<string, StoredFingerprint> stored, List<StoredFingerprint> duplicates)
            LoadFingerprints(IEngineDB engineDB)
        {
            var rows = engineDB.GetDal<MarkdownFile>().GetList()
                .Select(m => new StoredFingerprint
                {
                    Id = m.Id,
                    Path = m.Path,
                    FileLastWriteUtc = m.FileLastWriteUtc,
                    FileSize = m.FileSize,
                    FileHash = m.FileHash,
                    LinksHash = m.LinksHash,
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
            if (duplicates.Count > 0)
            {
                _logger.LogWarning("[IndexingPipeline] Found {Count} duplicate MarkdownFile paths (will be removed)", duplicates.Count);
            }
            return (stored, duplicates);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 1: RECONCILE — cancella righe di file spariti dal disco (+ duplicati).
        // Raw SQL: le righe morte non devono entrare nella session cache.
        // ─────────────────────────────────────────────────────────────────────
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

            _logger.LogInformation("[IndexingPipeline] Reconcile: removing {Count} stale/duplicate records", toDelete.Count);
            try
            {
                engineDB.BeginTransaction();
                foreach (var dead in toDelete)
                {
                    engineDB.CreateSQLQuery("DELETE FROM LinkInsideMarkdown WHERE MarkdownFileId = :id")
                        .SetParameter("id", dead.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                    engineDB.CreateSQLQuery("DELETE FROM DocumentChunk WHERE MarkdownFileId = :id")
                        .SetParameter("id", dead.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                    engineDB.CreateSQLQuery("DELETE FROM MarkdownFile WHERE Id = :id")
                        .SetParameter("id", dead.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                }
                engineDB.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IndexingPipeline] Reconcile failed - rolling back");
                try { engineDB.Rollback(); } catch { }
                throw;
            }

            // Post-commit: FTS side-car (DB separato, idempotente)
            foreach (var dead in toDelete)
            {
                _markdownFtsService.DeleteFileByPath(projectPath, dead.Path);
                stored.Remove(dead.Path);
            }

            return toDelete.Count;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 2: DIFF + UPSERT (+ FTS catch-up).
        //
        // Per ogni file su disco:
        //   stat match (mtime+size) e FileHash noto  → UNCHANGED, zero letture
        //   contenuto letto, hash == stored.FileHash → STAT-ONLY (update stat batch)
        //   altrimenti                               → CHANGED/NEW (upsert riga)
        // FTS catch-up indipendente: FtsHash != FileHash → UpsertFile + set FtsHash
        // (copre anche le run precedenti cancellate a metà).
        // Il contenuto vive UNA iterazione: mai liste di contenuti in memoria
        // (eccetto force, che usa RebuildIndex atomico come la vecchia pipeline).
        // ─────────────────────────────────────────────────────────────────────
        private List<PipelineFile> DiffUpsertAndFts(
            IEngineDB engineDB,
            string projectPath,
            Dictionary<string, StoredFingerprint> stored,
            List<string> fsFiles,
            bool force,
            DiffStats stats,
            CancellationToken ct)
        {
            var files = new List<PipelineFile>(fsFiles.Count);
            var statOnlyQueue = new List<PipelineFile>();
            var ftsRebuildEntries = force ? new List<MarkdownFtsEntry>() : null;

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
                    _logger.LogWarning(statEx, "[IndexingPipeline] Diff: cannot stat '{Path}', skipping", filePath);
                    stats.Unreadable++;
                    continue;
                }

                stored.TryGetValue(filePath, out var prev);
                var pf = new PipelineFile
                {
                    Id = prev?.Id ?? Guid.Empty,
                    Path = filePath,
                    FileName = Path.GetFileName(filePath),
                    StatMtime = statMtime,
                    StatSize = statSize,
                    LinksHash = prev?.LinksHash,
                    IsNew = prev == null
                };

                string content = null;

                if (!force && prev != null && prev.FileHash != null
                    && prev.FileLastWriteUtc == statMtime && prev.FileSize == statSize)
                {
                    // UNCHANGED: nessuna lettura (trust model stat, come git index)
                    pf.FileHash = prev.FileHash;
                    stats.Unchanged++;
                }
                else
                {
                    try
                    {
                        content = SharedFileReader.ReadAllText(filePath);
                    }
                    catch (Exception readEx)
                    {
                        _logger.LogWarning(readEx,
                            "[IndexingPipeline] Diff: cannot read '{Path}', record kept without content fingerprint", filePath);
                    }

                    if (content == null)
                    {
                        // File illeggibile: per i nuovi creiamo comunque la riga (hash null,
                        // la prossima run riprova); per gli esistenti non tocchiamo nulla.
                        stats.Unreadable++;
                        if (prev == null)
                        {
                            var newId = InsertMarkdownFile(engineDB, pf, fileHash: null);
                            if (newId == Guid.Empty) { continue; }
                            pf.Id = newId;
                        }
                        files.Add(pf);
                        continue;
                    }

                    pf.FileHash = ContentFingerprint.ComputeHash(content);

                    if (!force && prev != null && pf.FileHash == prev.FileHash)
                    {
                        // STAT-ONLY: contenuto identico, solo il timestamp è cambiato (touch).
                        statOnlyQueue.Add(pf);
                        stats.StatOnly++;
                    }
                    else
                    {
                        pf.ContentChanged = true;
                        if (prev == null)
                        {
                            var newId = InsertMarkdownFile(engineDB, pf, pf.FileHash);
                            if (newId == Guid.Empty) { continue; }
                            pf.Id = newId;
                            stats.New++;
                        }
                        else
                        {
                            UpdateMarkdownFileFingerprint(engineDB, pf);
                            stats.Changed++;
                        }
                    }
                }

                // ── FTS catch-up (DB side-car, indipendente dalle tx engine) ──
                if (force)
                {
                    ftsRebuildEntries.Add(new MarkdownFtsEntry
                    {
                        MarkdownFileId = pf.Id,
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
                        _markdownFtsService.UpsertFile(projectPath, pf.Id, pf.Path, pf.FileName, content);
                        ExecuteSmallUpdate(engineDB,
                            "UPDATE MarkdownFile SET FtsHash = :hash WHERE Id = :id",
                            q => q.SetParameter("hash", pf.FileHash).SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid));
                        stats.FtsUpserts++;
                    }
                }

                files.Add(pf);
            }

            // Batch degli update stat-only (un'unica transazione: sono UPDATE puntuali)
            if (statOnlyQueue.Count > 0)
            {
                try
                {
                    engineDB.BeginTransaction();
                    foreach (var pf in statOnlyQueue)
                    {
                        engineDB.CreateSQLQuery(
                            "UPDATE MarkdownFile SET FileLastWriteUtc = :m, FileSize = :s WHERE Id = :id")
                            .SetParameter("m", pf.StatMtime)
                            .SetParameter("s", pf.StatSize)
                            .SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid)
                            .ExecuteUpdate();
                    }
                    engineDB.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[IndexingPipeline] Diff: stat-only batch update failed - rolling back");
                    try { engineDB.Rollback(); } catch { }
                    throw;
                }
            }

            // Force: ricostruzione FTS atomica + allineamento FtsHash in un colpo solo
            if (force)
            {
                _markdownFtsService.RebuildIndex(projectPath, ftsRebuildEntries.Where(e => e.Content != null).ToList());
                ExecuteSmallUpdate(engineDB, "UPDATE MarkdownFile SET FtsHash = FileHash", q => q);
                stats.FtsUpserts = ftsRebuildEntries.Count;
            }

            return files;
        }

        /// <summary>
        /// Insert di un nuovo MarkdownFile in una piccola transazione dedicata.
        /// Una violazione UNIQUE(Path) (FSW di una seconda connessione in corsa)
        /// viene convertita in update deterministico. Ritorna l'Id della riga
        /// (nuova o esistente), Guid.Empty se l'upsert è fallito del tutto.
        /// </summary>
        private Guid InsertMarkdownFile(IEngineDB engineDB, PipelineFile pf, string fileHash)
        {
            try
            {
                engineDB.BeginTransaction();
                var mdf = new MarkdownFile
                {
                    FileName = pf.FileName,
                    Path = pf.Path,
                    FileType = "file",
                    FileLastWriteUtc = pf.StatMtime,
                    FileSize = pf.StatSize,
                    FileHash = fileHash
                };
                engineDB.GetDal<MarkdownFile>().Save(mdf);
                engineDB.Commit();

                // Evict: l'entità NON deve restare nella first-level cache, altrimenti
                // i raw UPDATE successivi (FtsHash, stat) verrebbero sovrascritti dal
                // prossimo Save dell'istanza cached (Phase 3 ricarica fresca dal DB).
                engineDB.Evict(mdf);
                return mdf.Id;
            }
            catch (Exception ex)
            {
                try { engineDB.Rollback(); } catch { }
                _logger.LogWarning(ex,
                    "[IndexingPipeline] Insert failed for '{Path}' (likely UNIQUE race) — converting to update", pf.Path);
                try
                {
                    // AddScalar tipizzato: la colonna Id è un Guid binario (BinaryGuid
                    // di System.Data.SQLite), una lettura come stringa non funzionerebbe.
                    var existingId = engineDB.CreateSQLQuery("SELECT Id FROM MarkdownFile WHERE Path = :path")
                        .AddScalar("Id", NHibernate.NHibernateUtil.Guid)
                        .SetParameter("path", pf.Path)
                        .List<Guid>()
                        .FirstOrDefault();
                    if (existingId != Guid.Empty)
                    {
                        pf.Id = existingId;
                        UpdateMarkdownFileFingerprint(engineDB, pf);
                        return existingId;
                    }
                }
                catch (Exception requeryEx)
                {
                    _logger.LogError(requeryEx, "[IndexingPipeline] Requery-by-path failed for '{Path}'", pf.Path);
                }
                return Guid.Empty;
            }
        }

        private void UpdateMarkdownFileFingerprint(IEngineDB engineDB, PipelineFile pf)
        {
            ExecuteSmallUpdate(engineDB,
                "UPDATE MarkdownFile SET FileName = :name, FileLastWriteUtc = :m, FileSize = :s, FileHash = :hash WHERE Id = :id",
                q => q.SetParameter("name", pf.FileName)
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
                _logger.LogError(ex, "[IndexingPipeline] Update failed: {Sql}", sql);
                try { engineDB.Rollback(); } catch { }
                throw;
            }
        }

        private string SafeReadAllText(string path)
        {
            try { return SharedFileReader.ReadAllText(path); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[IndexingPipeline] Cannot read '{Path}'", path);
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 3: Parse links + TLDR per folder, SOLO sul work set.
        //
        // Dentro la transazione per-file viene aggiornato anche LinksHash:
        // una run cancellata riprende esattamente dai file mancanti.
        // Dopo ogni commit viene emesso fileIndexed per quel file (delta-only,
        // sostituisce il vecchio sweep finale NotifyFilesIndexed).
        // ─────────────────────────────────────────────────────────────────────
        private async Task<int> ParseLinksWithFolderEvents(
            IEngineDB engineDB,
            List<PipelineFile> work,
            string projectPath,
            string connectionId,
            CancellationToken ct)
        {
            _logger.LogInformation("[IndexingPipeline] ParseLinks: starting for {Count} changed files", work.Count);

            // Carica le entità SOLO per il work set (chunk ≤500: limite parametri SQLite)
            var entitiesById = new Dictionary<Guid, MarkdownFile>();
            var dal = engineDB.GetDal<MarkdownFile>();
            foreach (var idChunk in work.Select(w => w.Id).Distinct().Chunk(500))
            {
                foreach (var entity in dal.GetList().Where(m => idChunk.Contains(m.Id)).ToList())
                {
                    entitiesById[entity.Id] = entity;
                }
            }

            var byFolder = work
                .GroupBy(f => Path.GetDirectoryName(f.Path) ?? string.Empty)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var linkDal = engineDB.GetDal<LinkInsideMarkdown>();
            var processedCount = 0;
            var totalFolders = byFolder.Count;
            var foldersDone = 0;

            // Emit kickoff progress so the frontend bar shows 0% from the start
            await SafeSendAsync(connectionId, "knowledgeProgress", new
            {
                processed = 0,
                total = totalFolders,
                percent = 0
            });

            foreach (var folderGroup in byFolder)
            {
                ct.ThrowIfCancellationRequested();
                var folderPath = folderGroup.Key;

                await SafeSendAsync(connectionId, "folderIndexingStart", new { path = folderPath, status = "indexing" });

                foreach (var pf in folderGroup)
                {
                    if (!entitiesById.TryGetValue(pf.Id, out var mdf))
                    {
                        _logger.LogWarning("[IndexingPipeline] ParseLinks: entity not found for '{Path}', skipping", pf.Path);
                        continue;
                    }

                    try
                    {
                        engineDB.BeginTransaction();

                        // Cancellazione preventiva dei link esistenti del file
                        var existingLinks = linkDal.GetList().Where(_ => _.MarkdownFile == mdf).ToList();
                        foreach (var link in existingLinks)
                        {
                            linkDal.Delete(link);
                        }

                        foreach (var parser in _linkParsers)
                        {
                            var linksToStore = parser.GetLinksFromFile(mdf.Path);
                            foreach (var singleLink in linksToStore)
                            {
                                var fullPath = Path.GetDirectoryName(mdf.Path)
                                    + Path.DirectorySeparatorChar
                                    + singleLink.FullPath.Replace('/', Path.DirectorySeparatorChar);

                                var mdContext = (Path.GetDirectoryName(mdf.Path) ?? string.Empty)
                                    .Replace(projectPath, string.Empty)
                                    .Replace(Path.DirectorySeparatorChar, '/');

                                linkDal.Save(new LinkInsideMarkdown
                                {
                                    FullPath = _helper.NormalizePath(fullPath),
                                    Path = singleLink.FullPath,
                                    Source = parser.GetType().Name,
                                    LinkedCommand = singleLink.LinkedCommand,
                                    SectionIndex = singleLink.SectionIndex,
                                    MarkdownFile = mdf,
                                    MdContext = mdContext
                                });
                            }
                        }

                        // Extract TLDR; block from the markdown and persist it on MarkdownFile.
                        // Silently ignore IO errors: a missing or unreadable file leaves Tldr as-is.
                        try
                        {
                            var rawMd = SharedFileReader.ReadAllText(mdf.Path);
                            mdf.Tldr = TldrExtractor.ExtractTldr(rawMd);
                        }
                        catch (Exception tldrEx)
                        {
                            _logger.LogDebug(tldrEx, "[IndexingPipeline] TLDR extraction skipped for '{Path}'", mdf.Path);
                        }

                        // Link + TLDR riusciti per questo contenuto: marca il sottosistema
                        // come aggiornato. Stessa transazione → atomico col parse.
                        mdf.LinksHash = pf.FileHash;
                        engineDB.GetDal<MarkdownFile>().Save(mdf);

                        engineDB.Commit();
                        processedCount++;

                        await SafeSendAsync(connectionId, "fileIndexed", new { path = mdf.Path, isIndexed = true });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[IndexingPipeline] ParseLinks error for '{Path}'", mdf.Path);
                        try { engineDB.Rollback(); } catch { }
                    }
                }

                await SafeSendAsync(connectionId, "folderIndexingComplete", new { path = folderPath, status = "completed" });

                foldersDone++;
                var percent = totalFolders > 0
                    ? (int)Math.Round(foldersDone * 100.0 / totalFolders)
                    : 100;
                await SafeSendAsync(connectionId, "knowledgeProgress", new
                {
                    processed = foldersDone,
                    total = totalFolders,
                    percent = percent
                });
            }

            _logger.LogInformation(
                "[IndexingPipeline] ParseLinks: completed {Processed}/{Total} files across {Folders} folders",
                processedCount, work.Count, byFolder.Count);
            return processedCount;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Phase 4: RAG embedding two-tier (se abilitato).
        //   Tier 1: chunk.FileLastWriteUtc == stat corrente  → skip, zero IO
        //   Tier 2: chunk.FileHash == FileHash corrente      → update timestamp, skip
        //   altrimenti: delete chunk + re-chunk + re-embed
        // Con gli Id stabili lo skip funziona finalmente TRA un'apertura e l'altra.
        // ─────────────────────────────────────────────────────────────────────
        private async Task<int> EmbedDocumentsIfEnabled(
            IEngineDB engineDB,
            List<PipelineFile> files,
            string projectPath,
            string connectionId,
            bool force,
            CancellationToken ct)
        {
            if (_embeddingService == null || _chunkingService == null)
            {
                _logger.LogDebug("[IndexingPipeline] Embed: services unavailable, skipping");
                return 0;
            }

            // Check ProjectDB.RagEnabled — uses a fresh scope to access singleton-like ProjectDB
            try
            {
                using var checkScope = _serviceScopeFactory.CreateScope();
                var dbManager = checkScope.ServiceProvider.GetService<IDatabaseManager>();
                var ctx = dbManager?.GetContext(connectionId);
                if (ctx?.ProjectDB == null)
                {
                    _logger.LogDebug("[IndexingPipeline] Embed: no ProjectDB context, skipping");
                    return 0;
                }
                var settingsDal = ctx.ProjectDB.GetDal<ProjectSetting>();
                var ragSetting = settingsDal.GetList().FirstOrDefault(s => s.Name == "RagEnabled");
                if (ragSetting?.ValueBool != true)
                {
                    _logger.LogDebug("[IndexingPipeline] Embed: RAG not enabled for project, skipping");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[IndexingPipeline] Embed: could not check RAG setting, skipping");
                return 0;
            }

            // Ensure embedding model is loaded
            if (!_embeddingService.IsModelLoaded())
            {
                var modelPath = _downloadService?.GetInstalledEmbeddingModelPath();
                if (modelPath == null)
                {
                    _logger.LogInformation("[IndexingPipeline] Embed: no model installed, skipping");
                    return 0;
                }
                var loaded = await _embeddingService.LoadModelAsync(modelPath);
                if (!loaded)
                {
                    _logger.LogWarning("[IndexingPipeline] Embed: failed to load model, skipping");
                    return 0;
                }
            }

            var chunkDal = engineDB.GetDal<DocumentChunk>();

            int processedCount = 0;
            int skippedCount = 0;
            int embeddedCount = 0;
            bool progressStarted = false;

            foreach (var pf in files)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if (pf.FileHash == null || !File.Exists(pf.Path))
                    {
                        skippedCount++;
                        processedCount++;
                        continue;
                    }

                    var fileId = pf.Id;
                    var sampleChunk = chunkDal.GetList()
                        .Where(c => c.MarkdownFile.Id == fileId)
                        .FirstOrDefault();

                    if (!force && sampleChunk != null)
                    {
                        // Tier 1: timestamp identico → skip senza IO
                        if (sampleChunk.FileLastWriteUtc == pf.StatMtime)
                        {
                            skippedCount++;
                            processedCount++;
                            continue;
                        }
                        // Tier 2: contenuto identico (hash già calcolato in Phase 2) →
                        // aggiorna solo il timestamp memorizzato, niente re-embed.
                        if (sampleChunk.FileHash == pf.FileHash)
                        {
                            ExecuteSmallUpdate(engineDB,
                                "UPDATE DocumentChunk SET FileLastWriteUtc = :ts WHERE MarkdownFileId = :id",
                                q => q.SetParameter("ts", pf.StatMtime).SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid));
                            skippedCount++;
                            processedCount++;
                            continue;
                        }
                    }

                    var content = SafeReadAllText(pf.Path);
                    if (content == null)
                    {
                        skippedCount++;
                        processedCount++;
                        continue;
                    }

                    if (!progressStarted)
                    {
                        progressStarted = true;
                        await SafeSendAsync(connectionId, "embeddingProgress",
                            new { status = "started", total = files.Count, processed = processedCount });
                    }

                    // Delete dei chunk esistenti (raw SQL: niente entità in sessione)
                    ExecuteSmallUpdate(engineDB,
                        "DELETE FROM DocumentChunk WHERE MarkdownFileId = :id",
                        q => q.SetParameter("id", pf.Id, NHibernate.NHibernateUtil.Guid));

                    var mdfRef = engineDB.GetDal<MarkdownFile>().GetList().First(m => m.Id == fileId);
                    var relativePath = pf.Path.Replace(projectPath, "").TrimStart(Path.DirectorySeparatorChar);
                    var chunks = _chunkingService.ChunkFile(relativePath, content);

                    foreach (var chunk in chunks)
                    {
                        ct.ThrowIfCancellationRequested();
                        var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                        var embeddingBytes = VectorSearchService.SerializeEmbedding(embedding);

                        engineDB.BeginTransaction();
                        chunkDal.Save(new DocumentChunk
                        {
                            MarkdownFile = mdfRef,
                            FilePath = chunk.FilePath,
                            SectionTitle = chunk.SectionTitle,
                            Content = chunk.Content,
                            StartLine = chunk.StartLine,
                            EndLine = chunk.EndLine,
                            Embedding = embeddingBytes,
                            EmbeddingDimension = _embeddingService.GetEmbeddingDimension(),
                            LastUpdated = DateTime.UtcNow.ToString("o"),
                            FileHash = pf.FileHash,
                            FileLastWriteUtc = pf.StatMtime,
                            ChunkType = chunk.ChunkType ?? "document",
                            GroupId = chunk.GroupId
                        });
                        engineDB.Commit();
                    }

                    embeddedCount++;
                    processedCount++;

                    if (embeddedCount % 5 == 0)
                    {
                        await SafeSendAsync(connectionId, "embeddingProgress",
                            new { status = "processing", total = files.Count, processed = processedCount });
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Embed error for '{Path}'", pf.Path);
                    processedCount++;
                    try { engineDB.Rollback(); } catch { }
                }
            }

            if (embeddedCount > 0)
            {
                // Invalidate vector search cache via fresh scope (IVectorSearchService is Scoped)
                try
                {
                    using var cacheScope = _serviceScopeFactory.CreateScope();
                    var vectorSearchService = cacheScope.ServiceProvider.GetService<IVectorSearchService>();
                    vectorSearchService?.InvalidateCache();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[IndexingPipeline] Embed: could not invalidate vector search cache");
                }
            }

            if (progressStarted)
            {
                await SafeSendAsync(connectionId, "embeddingProgress",
                    new { status = "completed", total = files.Count, processed = processedCount });
            }

            _logger.LogInformation(
                "[IndexingPipeline] Embed: {Embedded} embedded, {Skipped} skipped of {Total}",
                embeddedCount, skippedCount, files.Count);
            return embeddedCount;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        private async Task SafeSendAsync(string connectionId, string method, object payload)
        {
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(method, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[IndexingPipeline] SignalR send failed for '{Method}'", method);
            }
        }

    }
}
