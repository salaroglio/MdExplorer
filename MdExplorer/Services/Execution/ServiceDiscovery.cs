using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// On backend startup, rediscovers long-running services that this machine's MdExplorer
    /// spawned in a previous run (and that survived a crash / forced kill / web restart) and
    /// re-adopts them into the live <see cref="ServiceRegistry"/> so they appear in
    /// Settings → Services and can be stopped.
    ///
    /// The OS-level marker is the source of truth for liveness/identity:
    ///   • Linux  — the env var <c>MDE_SERVICE_ID</c> (inherited by the whole process tree, so an
    ///              orphaned grandchild whose shell has died is still found). Read from
    ///              <c>/proc/&lt;pid&gt;/environ</c>.
    ///   • Windows — the <c>mde-svc-&lt;id&gt;</c> token in the command line of the spawned shell,
    ///              queried via WMI <c>Win32_Process</c>. (Command lines are NOT inherited, so a
    ///              grandchild orphaned by a dead intermediate shell is NOT rediscovered on
    ///              Windows — logged below. On Windows the Electron will-quit tree-kill remains
    ///              the normal-quit safety net.)
    ///
    /// The persisted <see cref="ServiceMarkerStore"/> only enriches the display metadata; records
    /// whose marker is no longer live are pruned.
    /// </summary>
    public sealed class ServiceDiscovery
    {
        private readonly ServiceMarkerStore _markerStore;
        private readonly ServiceRunner _serviceRunner;
        private readonly ILogger<ServiceDiscovery> _logger;

        public ServiceDiscovery(
            ServiceMarkerStore markerStore,
            ServiceRunner serviceRunner,
            ILogger<ServiceDiscovery> logger)
        {
            _markerStore = markerStore;
            _serviceRunner = serviceRunner;
            _logger = logger;
        }

        public void DiscoverAndReattach()
        {
            try
            {
                var live = ScanLiveMarkers(); // id -> live PIDs carrying the marker
                var records = _markerStore.Snapshot();
                var byId = records
                    .GroupBy(r => r.Id)
                    .ToDictionary(g => g.Key, g => g.First());

                var reattached = 0;
                foreach (var kv in live)
                {
                    var id = kv.Key;
                    var pids = kv.Value;
                    if (pids.Count == 0) continue;

                    var rec = byId.TryGetValue(id, out var found)
                        ? found
                        : SynthesizeMinimal(id, pids[0]);

                    var svc = _serviceRunner.RegisterReattached(rec, pids, rec.LastPort);
                    if (svc != null) reattached++;
                }

                // Prune records whose marker is no longer alive (service died while we were down).
                var prunedDead = 0;
                foreach (var rec in records)
                {
                    if (!live.ContainsKey(rec.Id))
                    {
                        _markerStore.Remove(rec.Id);
                        prunedDead++;
                    }
                }

                if (reattached > 0 || prunedDead > 0)
                    _logger.LogInformation(
                        "[ServiceDiscovery] Reattached {Reattached} service(s), pruned {Pruned} dead record(s).",
                        reattached, prunedDead);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ServiceDiscovery] Discovery failed");
            }
        }

        private Dictionary<string, List<int>> ScanLiveMarkers()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return ScanLinux();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return ScanWindows();
            return new Dictionary<string, List<int>>(); // macOS: not supported
        }

        // ── Linux: /proc/<pid>/environ (NUL-separated KEY=VALUE) ──────────────────
        private Dictionary<string, List<int>> ScanLinux()
        {
            var result = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            var prefix = ServiceRunner.MarkerEnvIdKey + "="; // "MDE_SERVICE_ID="

            foreach (var dir in Directory.EnumerateDirectories("/proc"))
            {
                var name = Path.GetFileName(dir);
                if (!int.TryParse(name, out var pid)) continue;
                string id;
                try
                {
                    var environ = File.ReadAllText($"/proc/{pid}/environ");
                    id = ExtractEnvValue(environ, prefix);
                }
                catch { continue; } // process vanished or environ unreadable
                if (string.IsNullOrEmpty(id)) continue;

                if (!result.TryGetValue(id, out var list)) { list = new List<int>(); result[id] = list; }
                list.Add(pid);
            }
            return result;
        }

        private static string ExtractEnvValue(string environ, string prefix)
        {
            // environ is a NUL-separated list of "KEY=VALUE" entries.
            foreach (var entry in environ.Split('\0'))
            {
                if (entry.StartsWith(prefix, StringComparison.Ordinal))
                    return entry.Substring(prefix.Length);
            }
            return null;
        }

        // ── Windows: WMI Win32_Process.CommandLine LIKE '%mde-svc-%' ──────────────
        [SupportedOSPlatform("windows")]
        private Dictionary<string, List<int>> ScanWindows()
        {
            var result = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT ProcessId, CommandLine FROM Win32_Process WHERE CommandLine LIKE '%" +
                    ServiceRunner.TempScriptPrefix + "%'");
                foreach (var o in searcher.Get())
                {
                    using var mo = (System.Management.ManagementObject)o;
                    var cmd = mo["CommandLine"]?.ToString();
                    if (string.IsNullOrEmpty(cmd)) continue;
                    if (mo["ProcessId"] is not uint pidU) continue;
                    var pid = (int)pidU;

                    var id = ExtractIdFromCommandLine(cmd);
                    if (string.IsNullOrEmpty(id)) continue;

                    if (!result.TryGetValue(id, out var list)) { list = new List<int>(); result[id] = list; }
                    list.Add(pid);
                }

                _logger.LogDebug(
                    "[ServiceDiscovery] Windows command-line scan: command lines are not inherited, so a " +
                    "server grandchild orphaned by a dead intermediate shell would NOT be rediscovered.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ServiceDiscovery] WMI scan failed");
            }
            return result;
        }

        // Pulls the 32-hex-char guid that follows "mde-svc-" in the temp-script path.
        private static string ExtractIdFromCommandLine(string cmd)
        {
            var idx = cmd.IndexOf(ServiceRunner.TempScriptPrefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var start = idx + ServiceRunner.TempScriptPrefix.Length;
            var sb = new System.Text.StringBuilder(32);
            for (var i = start; i < cmd.Length && sb.Length < 32; i++)
            {
                var c = cmd[i];
                var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!isHex) break;
                sb.Append(c);
            }
            return sb.Length == 32 ? sb.ToString() : null;
        }

        private static ServiceMarkerRecord SynthesizeMinimal(string id, int pid) => new()
        {
            Id = id,
            BlockId = null,
            ProjectPath = string.Empty,
            Language = "unknown",
            CodePreview = "(rediscovered process — no metadata)",
            Pid = pid,
            StartedAt = null,
            LastPort = null,
        };
    }
}
