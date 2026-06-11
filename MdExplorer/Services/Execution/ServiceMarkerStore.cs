using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// One persisted record per long-running service started from a runnable code block.
    /// The file survives backend restarts so a fresh MdExplorer can rediscover the
    /// processes it spawned in a previous run (see <see cref="ServiceDiscovery"/>) and
    /// surface them in Settings → Services. The OS-level marker (env var on Linux,
    /// the <c>mde-svc-&lt;id&gt;</c> token in the command line on Windows) is the source of
    /// truth for liveness/identity; this record only carries the display metadata that
    /// can't be reconstructed from a bare process.
    /// </summary>
    public sealed class ServiceMarkerRecord
    {
        public string Id { get; set; }
        public string BlockId { get; set; }
        public string ProjectPath { get; set; }
        public string DocumentPath { get; set; }
        public string Language { get; set; }
        public string CodePreview { get; set; }
        public int Pid { get; set; }
        public string StartedAt { get; set; }   // ISO-8601 (round-trip)
        public int? LastPort { get; set; }
    }

    /// <summary>
    /// Thread-safe, file-backed registry of started services. Persisted to
    /// <c>%AppData%/MdExplorer/running-services.json</c> (on Docker this resolves to
    /// <c>$XDG_CONFIG_HOME/MdExplorer</c>, i.e. the persistent <c>/data</c> volume).
    /// Singleton. Every mutation rewrites the whole file (the record count is tiny).
    /// </summary>
    public sealed class ServiceMarkerStore
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, ServiceMarkerRecord> _records = new();
        private readonly string _filePath;
        private readonly ILogger<ServiceMarkerStore> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public ServiceMarkerStore(ILogger<ServiceMarkerStore> logger)
        {
            _logger = logger;

            // Same convention as port.txt (Startup.cs). On Docker, ApplicationData maps
            // to XDG_CONFIG_HOME (/data) so the file lands on the persistent volume.
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MdExplorer");
            _filePath = Path.Combine(dir, "running-services.json");

            try { Directory.CreateDirectory(dir); }
            catch (Exception ex) { _logger?.LogWarning(ex, "[ServiceMarkerStore] Could not create dir {Dir}", dir); }

            LoadFromDisk();
        }

        /// <summary>Insert or replace a record, then persist.</summary>
        public void Upsert(ServiceMarkerRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.Id)) return;
            lock (_lock)
            {
                _records[record.Id] = record;
                Persist();
            }
        }

        /// <summary>Patch the last-known listening port of an existing record, then persist.</summary>
        public void UpdatePort(string id, int? port)
        {
            if (string.IsNullOrEmpty(id)) return;
            lock (_lock)
            {
                if (_records.TryGetValue(id, out var r))
                {
                    r.LastPort = port;
                    Persist();
                }
            }
        }

        /// <summary>Remove a record (service exited or stopped), then persist.</summary>
        public void Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            lock (_lock)
            {
                if (_records.Remove(id)) Persist();
            }
        }

        /// <summary>Snapshot copy of all persisted records.</summary>
        public IReadOnlyList<ServiceMarkerRecord> Snapshot()
        {
            lock (_lock)
            {
                return _records.Values
                    .Select(Clone)
                    .ToList();
            }
        }

        public bool TryGet(string id, out ServiceMarkerRecord record)
        {
            lock (_lock)
            {
                if (_records.TryGetValue(id, out var r)) { record = Clone(r); return true; }
                record = null;
                return false;
            }
        }

        // ── internals ─────────────────────────────────────────────────────────

        private void LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json)) return;
                var list = JsonSerializer.Deserialize<List<ServiceMarkerRecord>>(json, JsonOpts);
                if (list == null) return;
                lock (_lock)
                {
                    _records.Clear();
                    foreach (var r in list)
                        if (r != null && !string.IsNullOrEmpty(r.Id))
                            _records[r.Id] = r;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[ServiceMarkerStore] Failed to load {File} — starting empty", _filePath);
            }
        }

        // Caller must hold _lock.
        private void Persist()
        {
            try
            {
                var json = JsonSerializer.Serialize(_records.Values.ToList(), JsonOpts);
                // Write-then-replace for atomicity (avoids a half-written file on crash).
                var tmp = _filePath + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(_filePath)) File.Replace(tmp, _filePath, null);
                else File.Move(tmp, _filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[ServiceMarkerStore] Failed to persist {File}", _filePath);
            }
        }

        private static ServiceMarkerRecord Clone(ServiceMarkerRecord r) => new()
        {
            Id = r.Id,
            BlockId = r.BlockId,
            ProjectPath = r.ProjectPath,
            DocumentPath = r.DocumentPath,
            Language = r.Language,
            CodePreview = r.CodePreview,
            Pid = r.Pid,
            StartedAt = r.StartedAt,
            LastPort = r.LastPort,
        };
    }
}
