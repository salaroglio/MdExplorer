using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Esegue il <b>git di sistema</b>. Serve dove LibGit2Sharp non arriva — worktree, submodule,
    /// <c>diff</c> testuale — e dove si vuole esattamente ciò che vedrebbe una persona dal
    /// terminale.
    /// <para>
    /// Gli argomenti passano come <see cref="ProcessStartInfo.ArgumentList"/>: ogni token è un
    /// argv <b>letterale</b>, quindi niente quoting e <b>niente injection</b>, nemmeno da un ref
    /// controllato da una città remota. <c>GIT_TERMINAL_PROMPT=0</c> fa fallire subito invece di
    /// restare appeso in attesa di credenziali che nessuno digiterà.
    /// </para>
    /// </summary>
    public interface INativeGitRunner
    {
        Task<GitResult> RunAsync(string workingDirectory, string[] args, CancellationToken ct = default,
            IReadOnlyDictionary<string, string> extraEnv = null, int timeoutMs = NativeGitRunner.DefaultTimeoutMs);
    }

    public readonly struct GitResult
    {
        public int ExitCode { get; init; }
        public string Stdout { get; init; }
        public string Stderr { get; init; }

        public bool Ok => ExitCode == 0;

        /// <summary>Motivo leggibile, per messaggi che l'utente deve poter capire.</summary>
        public string Describe()
            => ExitCode == NativeGitRunner.GitNotFoundExit ? "git non trovato nel PATH"
             : ExitCode == NativeGitRunner.GitTimeoutExit ? "timeout"
             : $"exit {ExitCode}: {Stderr?.Trim()}";
    }

    public sealed class NativeGitRunner : INativeGitRunner
    {
        // Codici sentinella fuori dal range 0..255 dei veri exit di un processo: non possono
        // essere confusi con un esito di git.
        public const int GitNotFoundExit = -9999;
        public const int GitTimeoutExit = -9998;
        public const int DefaultTimeoutMs = 300000;

        private readonly ILogger<NativeGitRunner> _logger;

        public NativeGitRunner(ILogger<NativeGitRunner> logger) => _logger = logger;

        public async Task<GitResult> RunAsync(string workingDirectory, string[] args, CancellationToken ct = default,
            IReadOnlyDictionary<string, string> extraEnv = null, int timeoutMs = DefaultTimeoutMs)
        {
            var pretty = string.Join(" ", args);
            _logger.LogDebug("[git] -C {Dir}: git {Args}", workingDirectory, pretty);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            foreach (var a in args)
                process.StartInfo.ArgumentList.Add(a);
            process.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            if (extraEnv != null)
                foreach (var kv in extraEnv)
                    process.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;

            try { process.Start(); }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, "[git] non trovato nel PATH");
                return new GitResult { ExitCode = GitNotFoundExit, Stdout = string.Empty, Stderr = ex.Message };
            }

            // Leggere PRIMA di attendere: leggere dopo WaitForExit può deadlockare col buffer pieno.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            var completed = await Task.Run(() => process.WaitForExit(timeoutMs), ct);
            if (!completed)
            {
                try { process.Kill(true); } catch { }
                _logger.LogError("[git] {Args} in timeout ({Ms}ms)", pretty, timeoutMs);
                return new GitResult { ExitCode = GitTimeoutExit, Stdout = string.Empty, Stderr = "timeout" };
            }

            return new GitResult { ExitCode = process.ExitCode, Stdout = await stdoutTask, Stderr = await stderrTask };
        }
    }
}
