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
        private readonly ShellRegistry _registry;
        private readonly ServiceRegistry _services;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<ServiceRunner> _logger;

        public ServiceRunner(
            ShellRegistry registry,
            ServiceRegistry services,
            IHubContext<MonitorMDHub> hub,
            ILogger<ServiceRunner> logger)
        {
            _registry = registry;
            _services = services;
            _hub = hub;
            _logger = logger;
        }

        public RunningService StartService(
            string code,
            string language,
            string workingDirectory,
            IReadOnlyDictionary<string, string> environment,
            string blockId,
            string projectPath)
        {
            if (!_registry.TryGet(language, out var descriptor))
            {
                throw new InvalidOperationException(
                    $"Unsupported shell language: '{language}'. Supported: {string.Join(", ", _registry.SupportedLanguages)}");
            }

            var scriptBody = descriptor.NormalizeToLf
                ? (code ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n")
                : (code ?? string.Empty);

            var tempFile = Path.Combine(
                Path.GetTempPath(),
                $"mde-svc-{Guid.NewGuid():N}{descriptor.ScriptExtension}");
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

            var service = new RunningService
            {
                Id = Guid.NewGuid().ToString("N"),
                BlockId = blockId,
                ProjectPath = projectPath,
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
                _services.TryRemove(service.Id, out _);
                try { process.Dispose(); } catch { /* already gone */ }
            };

            service.Process = process;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            service.Pid = process.Id;

            _services.TryAdd(service);
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
                if (service.Process != null && !service.Process.HasExited)
                {
                    service.Process.Kill(entireProcessTree: true);
                }
                else
                {
                    // Already exited but Exited handler may not have run — clean up defensively.
                    _ = SafeSend("service.stopped", service.ToDto());
                    TryDeleteTemp(service.TempScriptPath);
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
