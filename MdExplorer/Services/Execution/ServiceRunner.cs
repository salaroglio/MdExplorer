using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Services.Execution
{
    /// <summary>
    /// Starts long-running "services" from runnable fenced code blocks. The spawned process is
    /// DETACHED from the HTTP request: no timeout, no RequestAborted token. It survives document
    /// switches and lives until explicitly stopped (or until the backend dies — it stays a child
    /// of the backend so the Electron tree-kill on app quit terminates it).
    /// </summary>
    public sealed class ServiceRunner
    {
        /// <summary>Env var stamped on every spawned service, inherited by the whole process
        /// tree. On Linux it is the rediscovery marker (read back from /proc/&lt;pid&gt;/environ).</summary>
        public const string MarkerEnvIdKey = "MDE_SERVICE_ID";
        public const string MarkerEnvGeneratedKey = "MDE_GENERATED";

        /// <summary>Temp-script filename prefix. The service Id is embedded as the guid, so the
        /// shell's command line carries <c>mde-svc-&lt;id&gt;</c> — the Windows WMI rediscovery marker.</summary>
        public const string TempScriptPrefix = "mde-svc-";

        private readonly ShellRegistry _registry;
        private readonly ServiceRegistry _services;
        private readonly ServiceMarkerStore _markerStore;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<ServiceRunner> _logger;

        public ServiceRunner(
            ShellRegistry registry,
            ServiceRegistry services,
            ServiceMarkerStore markerStore,
            IHubContext<MonitorMDHub> hub,
            ILogger<ServiceRunner> logger)
        {
            _registry = registry;
            _services = services;
            _markerStore = markerStore;
            _hub = hub;
            _logger = logger;
        }

        public RunningService StartService(
            string code,
            string language,
            string workingDirectory,
            IReadOnlyDictionary<string, string> environment,
            string blockId,
            string projectPath,
            string documentPath = null)
        {
            if (!_registry.TryGet(language, out var descriptor))
            {
                throw new InvalidOperationException(
                    $"Unsupported shell language: '{language}'. Supported: {string.Join(", ", _registry.SupportedLanguages)}");
            }

            var scriptBody = descriptor.NormalizeToLf
                ? (code ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n")
                : (code ?? string.Empty);

            // The service Id IS the temp-script guid: the shell's command line then carries
            // "mde-svc-<id>", which a fresh MdExplorer can rediscover via WMI on Windows.
            var serviceId = Guid.NewGuid().ToString("N");
            var tempFile = Path.Combine(
                Path.GetTempPath(),
                $"{TempScriptPrefix}{serviceId}{descriptor.ScriptExtension}");
            File.WriteAllText(tempFile, scriptBody, new UTF8Encoding(false));

            var psi = new ProcessStartInfo
            {
                FileName = descriptor.Executable,
                Arguments = descriptor.BuildArgs(tempFile),
                WorkingDirectory = ShellEnvironment.SafeWorkingDirectory(workingDirectory),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            ShellEnvironment.RefreshEnvironmentFromRegistry(psi, _logger);
            if (environment != null)
            {
                foreach (var kv in environment)
                {
                    if (kv.Key == null) continue;
                    psi.Environment[kv.Key] = kv.Value ?? string.Empty;
                }
            }

            // OS marker (set AFTER the user overlay so it can't be clobbered). Inherited by the
            // whole descendant tree → on Linux the orphaned grandchild server still carries it,
            // which is how rediscovery finds it even after the shell has died.
            psi.Environment[MarkerEnvIdKey] = serviceId;
            psi.Environment[MarkerEnvGeneratedKey] = "1";

            var service = new RunningService
            {
                Id = serviceId,
                BlockId = blockId,
                ProjectPath = projectPath,
                DocumentPath = documentPath,
                Language = language,
                CodePreview = BuildCodePreview(code),
                StartedAt = DateTimeOffset.UtcNow,
                Status = "running",
                TempScriptPath = tempFile,
            };

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                var chunk = e.Data + "\n";
                service.AppendOutput(chunk);
                _ = SafeSend("service.output", new { serviceId = service.Id, blockId = service.BlockId, stream = "stdout", chunk });
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                var chunk = e.Data + "\n";
                service.AppendOutput(chunk);
                _ = SafeSend("service.output", new { serviceId = service.Id, blockId = service.BlockId, stream = "stderr", chunk });
            };

            process.Exited += (s, e) =>
            {
                // If StopService already flagged it "killed", keep that; otherwise it exited on its own.
                if (service.Status != "killed") service.Status = "exited";
                try { service.ExitCode = process.ExitCode; } catch { service.ExitCode = null; }

                _ = SafeSend("service.stopped", service.ToDto());

                TryDeleteTemp(service.TempScriptPath);
                _markerStore.Remove(service.Id);
                _services.TryRemove(service.Id, out _);
                try { process.Dispose(); } catch { /* already gone */ }
            };

            service.Process = process;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            service.Pid = process.Id;

            _services.TryAdd(service);

            // Persist the marker record so a future MdExplorer run can rediscover this
            // process (and stop it) even though the in-memory Process handle won't survive.
            _markerStore.Upsert(new ServiceMarkerRecord
            {
                Id = service.Id,
                BlockId = service.BlockId,
                ProjectPath = service.ProjectPath,
                DocumentPath = service.DocumentPath,
                Language = service.Language,
                CodePreview = service.CodePreview,
                Pid = service.Pid,
                StartedAt = service.StartedAt.ToString("o"),
                LastPort = null,
            });

            _ = SafeSend("service.started", service.ToDto());

            // Detect the listening port a moment after start (it appears asynchronously).
            _ = Task.Run(() => DetectPortWithRetryAsync(service));

            return service;
        }

        public bool StopService(string id)
        {
            if (!_services.TryGet(id, out var service)) return false;
            try
            {
                service.Status = "killed"; // set BEFORE Kill so the Exited handler reports the right status
                if (service.IsReattached)
                {
                    // No Process handle (rediscovered from a previous run) → kill via the
                    // discovered PIDs and their subtrees. No Exited event will fire, so we
                    // do the registry/marker cleanup here.
                    KillReattached(service);
                    _ = SafeSend("service.stopped", service.ToDto());
                    _markerStore.Remove(service.Id);
                    _services.TryRemove(service.Id, out _);
                }
                else if (service.Process != null && !service.Process.HasExited)
                {
                    service.Process.Kill(entireProcessTree: true); // Exited handler does the cleanup
                }
                else
                {
                    // Already exited but Exited handler may not have run — clean up defensively.
                    _ = SafeSend("service.stopped", service.ToDto());
                    TryDeleteTemp(service.TempScriptPath);
                    _markerStore.Remove(service.Id);
                    _services.TryRemove(service.Id, out _);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ServiceRunner] Failed to stop service {Id}", id);
                return false;
            }
        }

        /// <summary>Stops every running service (used by the graceful shutdown hook).</summary>
        public void StopAll()
        {
            foreach (var svc in _services.List())
            {
                StopService(svc.Id);
            }
        }

        /// <summary>
        /// Re-adopt a service rediscovered on startup (see <see cref="ServiceDiscovery"/>): build a
        /// reattached <see cref="RunningService"/>, register it, announce it via SignalR so it shows
        /// up in Settings → Services, and (best-effort) re-detect its listening port.
        /// </summary>
        public RunningService RegisterReattached(ServiceMarkerRecord rec, List<int> livePids, int? detectedPort)
        {
            if (rec == null || livePids == null || livePids.Count == 0) return null;

            var startedAt = DateTimeOffset.UtcNow;
            if (!string.IsNullOrEmpty(rec.StartedAt) &&
                DateTimeOffset.TryParse(rec.StartedAt, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            {
                startedAt = parsed;
            }

            var service = new RunningService
            {
                Id = rec.Id,
                BlockId = rec.BlockId,
                ProjectPath = rec.ProjectPath,
                DocumentPath = rec.DocumentPath,
                Language = rec.Language,
                CodePreview = rec.CodePreview,
                Pid = livePids[0],
                StartedAt = startedAt,
                Status = "running",
                DetectedPort = detectedPort ?? rec.LastPort,
                IsReattached = true,
                DiscoveredPids = livePids,
            };
            service.AppendOutput(
                "(reattached to a process from a previous MdExplorer session — live output is not available)\n");

            if (!_services.TryAdd(service)) return null;
            _ = SafeSend("service.started", service.ToDto());

            // If we still don't have a port, scan each discovered PID subtree for one.
            if (service.DetectedPort == null)
                _ = Task.Run(() => ReDetectPortForReattached(service));

            return service;
        }

        private async Task ReDetectPortForReattached(RunningService service)
        {
            for (var attempt = 0; attempt < 4; attempt++)
            {
                await Task.Delay(500);
                if (service.Status != "running") return;
                int? found = null;
                foreach (var pid in service.DiscoveredPids ?? new List<int>())
                {
                    try { found = ListeningPortDetector.DetectListeningPort(pid, _logger); }
                    catch { /* ignore */ }
                    if (found != null) break;
                }
                if (found != null)
                {
                    service.DetectedPort = found;
                    _markerStore.UpdatePort(service.Id, found);
                    _ = SafeSend("service.started", service.ToDto());
                    return;
                }
            }
        }

        private void KillReattached(RunningService service)
        {
            foreach (var pid in service.DiscoveredPids ?? new List<int>())
            {
                try
                {
                    using var p = Process.GetProcessById(pid);
                    p.Kill(entireProcessTree: true);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "[ServiceRunner] reattached kill of pid {Pid} failed (already gone?)", pid);
                }
            }
        }

        private async Task DetectPortWithRetryAsync(RunningService service)
        {
            // ~4s window: the server typically binds its port shortly after launch.
            for (var attempt = 0; attempt < 8; attempt++)
            {
                await Task.Delay(500);
                if (service.Status != "running") return; // exited/killed meanwhile
                int? port;
                try { port = ListeningPortDetector.DetectListeningPort(service.Pid, _logger); }
                catch { port = null; }
                if (port != null)
                {
                    service.DetectedPort = port;
                    _markerStore.UpdatePort(service.Id, port);
                    _ = SafeSend("service.started", service.ToDto()); // refresh dto with the port
                    return;
                }
            }
        }

        private async Task SafeSend(string method, object payload)
        {
            try
            {
                await _hub.Clients.All.SendAsync(method, payload);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ServiceRunner] SignalR send '{Method}' failed", method);
            }
        }

        private static void TryDeleteTemp(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { File.Delete(path); } catch { /* best-effort */ }
        }

        private static string BuildCodePreview(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            var firstLine = code.Replace("\r", "").Split('\n').FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim()
                            ?? code.Trim();
            return firstLine.Length > 120 ? firstLine.Substring(0, 117) + "…" : firstLine;
        }
    }
}
