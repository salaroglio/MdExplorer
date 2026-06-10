using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// A long-running "service" started from a runnable fenced code block.
    /// Unlike a one-shot <see cref="ShellRunner"/> run, its process is detached from the
    /// HTTP request: it has no timeout and is not bound to RequestAborted. It stays alive
    /// until explicitly stopped or until the backend process dies (it remains a child of
    /// the backend, so the Electron tree-kill on app quit terminates it).
    /// </summary>
    public sealed class RunningService
    {
        public string Id { get; init; }
        public string BlockId { get; init; }
        public string ProjectPath { get; init; }
        public string DocumentPath { get; init; }   // full path of the .md that owns the block
        public string Language { get; init; }
        public string CodePreview { get; init; }
        public int Pid { get; set; }
        public DateTimeOffset StartedAt { get; init; }
        public string Status { get; set; }            // "running" | "exited" | "killed"
        public int? ExitCode { get; set; }
        public int? DetectedPort { get; set; }
        public string TempScriptPath { get; init; }

        /// <summary>
        /// True when this entry was rediscovered on startup from the marker store rather than
        /// started in the current backend run. Such an entry has NO <see cref="Process"/> handle
        /// (we can't recapture stdout of an already-running process) — it is stopped via the OS
        /// marker / discovered PIDs instead.
        /// </summary>
        public bool IsReattached { get; init; }

        /// <summary>Marker-bearing root PIDs found at discovery (reattached entries only).</summary>
        public List<int> DiscoveredPids { get; init; }

        // Kept alive intentionally — NOT disposed until the process exits or is killed.
        internal Process Process { get; set; }

        private const int RingCapacity = 500;
        private readonly object _ringLock = new();
        private readonly Queue<string> _ring = new();

        public void AppendOutput(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;
            lock (_ringLock)
            {
                _ring.Enqueue(chunk);
                while (_ring.Count > RingCapacity) _ring.Dequeue();
            }
        }

        public string[] SnapshotOutput()
        {
            lock (_ringLock)
            {
                return _ring.ToArray();
            }
        }

        /// <summary>
        /// Serialization-safe projection (no <see cref="Process"/>) for SignalR / HTTP.
        /// </summary>
        public object ToDto() => new
        {
            id = Id,
            blockId = BlockId,
            projectPath = ProjectPath,
            language = Language,
            codePreview = CodePreview,
            pid = Pid,
            startedAt = StartedAt,
            status = Status,
            exitCode = ExitCode,
            detectedPort = DetectedPort,
            uptimeMs = (long)(DateTimeOffset.UtcNow - StartedAt).TotalMilliseconds,
            reattached = IsReattached,
            documentPath = DocumentPath,                       // full path — used to re-link a block on reload
            documentRelativePath = ComputeDocumentRelativePath(),
        };

        /// <summary>The owning document's path relative to the project root, for display.
        /// Null when unknown (e.g. an old rediscovered service persisted before this was tracked).</summary>
        private string ComputeDocumentRelativePath()
        {
            if (string.IsNullOrEmpty(DocumentPath)) return null;
            if (string.IsNullOrEmpty(ProjectPath)) return DocumentPath;
            try
            {
                var rel = Path.GetRelativePath(ProjectPath, DocumentPath);
                // GetRelativePath returns the input unchanged if it can't relativize (different root).
                return rel.Replace('\\', '/');
            }
            catch { return DocumentPath; }
        }
    }

    /// <summary>
    /// Singleton registry of running services. Thread-safe.
    /// Must be a singleton so it persists across requests for the lifetime of the backend.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly ConcurrentDictionary<string, RunningService> _byId = new();

        public bool TryAdd(RunningService service) => _byId.TryAdd(service.Id, service);

        public bool TryGet(string id, out RunningService service) => _byId.TryGetValue(id, out service);

        public bool TryRemove(string id, out RunningService service) => _byId.TryRemove(id, out service);

        public IReadOnlyCollection<RunningService> List() => _byId.Values.ToArray();

        public RunningService FindByBlockId(string blockId) =>
            _byId.Values.FirstOrDefault(x => x.BlockId == blockId && x.Status == "running");
    }
}
