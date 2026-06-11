using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.Execution
{
    public record ExecutionResult(int ExitCode, TimeSpan Duration, bool TimedOut);

    /// <summary>
    /// Executes a shell script by writing it to a temp file and spawning the appropriate shell.
    /// Output streams line-by-line via the onStdout/onStderr callbacks.
    /// </summary>
    public class ShellRunner
    {
        private readonly ShellRegistry _registry;
        private readonly ILogger<ShellRunner> _logger;

        public ShellRunner(ShellRegistry registry, ILogger<ShellRunner> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        public async Task<ExecutionResult> RunAsync(
            string code,
            string language,
            string workingDirectory,
            IReadOnlyDictionary<string, string> environment,
            Func<string, Task> onStdout,
            Func<string, Task> onStderr,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (!_registry.TryGet(language, out var descriptor))
            {
                throw new InvalidOperationException($"Unsupported shell language: '{language}'. Supported: {string.Join(", ", _registry.SupportedLanguages)}");
            }

            // Guard against empty scripts — the process would exit 0 with no output; still fine to run.
            var scriptBody = descriptor.NormalizeToLf
                ? (code ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n")
                : (code ?? string.Empty);

            var tempFile = Path.Combine(
                Path.GetTempPath(),
                $"mde-exec-{Guid.NewGuid():N}{descriptor.ScriptExtension}");

            await File.WriteAllTextAsync(tempFile, scriptBody, new UTF8Encoding(false), cancellationToken);

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

            // Overlay machine + user environment variables read fresh from the Windows registry.
            // This makes user changes to PATH / JENA_HOME / ANY env var take effect on the next
            // runnable block *without* requiring an MDE restart. Without this, the child shell
            // inherits whatever env MDE captured at launch time — which can be stale (Electron
            // window opened before the user installed Jena, web backend running as a service
            // that never picks up user-level env, etc.).
            ShellEnvironment.RefreshEnvironmentFromRegistry(psi, _logger);

            if (environment != null)
            {
                foreach (var kv in environment)
                {
                    if (kv.Key == null) continue;
                    psi.Environment[kv.Key] = kv.Value ?? string.Empty;
                }
            }

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                // Fire-and-forget — ordering is preserved by SignalR's per-connection queue
                _ = SafeInvoke(onStdout, e.Data + "\n");
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                _ = SafeInvoke(onStderr, e.Data + "\n");
            };

            var started = Stopwatch.StartNew();
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (timeout > TimeSpan.Zero) linked.CancelAfter(timeout);

                var timedOut = false;
                try
                {
                    await process.WaitForExitAsync(linked.Token);
                }
                catch (OperationCanceledException)
                {
                    timedOut = !cancellationToken.IsCancellationRequested;
                    try { process.Kill(entireProcessTree: true); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[ShellRunner] Failed to kill process tree on cancellation");
                    }
                    // Wait briefly for process to finish flushing output after kill
                    try { await process.WaitForExitAsync(CancellationToken.None); } catch { }
                }

                // Drain any remaining async output
                try { process.WaitForExit(500); } catch { }

                return new ExecutionResult(
                    ExitCode: process.HasExited ? process.ExitCode : -1,
                    Duration: started.Elapsed,
                    TimedOut: timedOut);
            }
            finally
            {
                try { File.Delete(tempFile); }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "[ShellRunner] Could not delete temp script {TempFile}", tempFile);
                }
            }
        }

        private async Task SafeInvoke(Func<string, Task> callback, string chunk)
        {
            if (callback == null) return;
            try
            {
                await callback(chunk);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ShellRunner] Output callback threw");
            }
        }

    }
}
