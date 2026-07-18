using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Utilities;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Esito del prepare-before-run di un worktree (Fase 7c).</summary>
    public sealed class WorktreePrepareResult
    {
        public bool Success { get; private set; }
        public string WorktreePath { get; private set; }
        /// <summary>Il merge del branch di handoff è in conflitto → il chiamante lo mappa su not-ready.</summary>
        public bool MergeConflict { get; private set; }
        public string Error { get; private set; }

        public static WorktreePrepareResult Ok(string path) => new() { Success = true, WorktreePath = path };
        public static WorktreePrepareResult Conflict(string path) => new() { Success = false, MergeConflict = true, WorktreePath = path, Error = "merge-conflict-with-main" };
        public static WorktreePrepareResult Fail(string error) => new() { Success = false, Error = error };
    }

    /// <summary>
    /// Isolamento d'esecuzione per-agente (Fase 7c, §7bis): ogni agente ha un <b>worktree git
    /// persistente</b> FUORI dal progetto sorvegliato, in <c>{AppData}/MdExplorer/worktrees/
    /// {project-hash}/{agente}</c>. A ogni risveglio il worktree è "preparato" (fetch + reset +
    /// branch fresco per-attività da <c>origin/&lt;base&gt;</c> + merge dell'eventuale handoff),
    /// così il lavoro dell'agente non tocca la working tree dell'umano.
    /// <para>
    /// <b>LibGit2Sharp non sa fare <c>git worktree</c></b> → tutte le operazioni shellano il
    /// binario <c>git</c> (stesso pattern di <c>ModernGitService.RunNativeGitAsync</c>, con
    /// <c>GIT_TERMINAL_PROMPT=0</c>). Opt-in: gira solo se <c>agentCity.useAgentWorktrees</c>.
    /// </para>
    /// </summary>
    public interface IAgentWorktreeManager
    {
        /// <summary>Crea (se assente) il worktree dell'agente e ne ritorna il path. Idempotente.</summary>
        Task<string> EnsureWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default);

        /// <summary>
        /// Prepara il worktree per un run: fetch, reset --hard + clean -fd (MAI -x), branch
        /// <c>agent/&lt;agente&gt;/&lt;activityId&gt;</c> da <c>origin/&lt;baseBranch&gt;</c>, e —
        /// se <paramref name="handoffRef"/> è dato (ref COMPLETO) — merge di <c>origin/&lt;handoffRef&gt;</c>.
        /// Conflitto di merge → <see cref="WorktreePrepareResult.MergeConflict"/> (non auto-risolve).
        /// </summary>
        Task<WorktreePrepareResult> PrepareForRunAsync(
            string projectPath, string agentName, string activityId,
            string baseBranch = null, string handoffRef = null, CancellationToken ct = default);

        /// <summary>Rimuove il worktree dell'agente (git worktree remove + prune; best-effort se il progetto è sparito).</summary>
        Task RemoveWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default);

        /// <summary>Root dei worktree di un progetto: <c>{AppData}/MdExplorer/worktrees/{project-hash}</c>.</summary>
        string WorktreeRootForProject(string projectPath);

        /// <summary>Path del worktree di un agente (non lo crea).</summary>
        string WorktreePathFor(string projectPath, string agentName);
    }

    public sealed class AgentWorktreeManager : IAgentWorktreeManager, IDisposable
    {
        // git non trovato / timeout: codici sentinella (fuori dal range 0..255 dei veri exit).
        private const int GitNotFoundExit = -9999;
        private const int GitTimeoutExit = -9998;
        private const int DefaultTimeoutMs = 300000;

        private static string WorktreesRoot => Path.Combine(CrossPlatformPath.GetMdExplorerDataDirectory(), "worktrees");

        private readonly ILogger<AgentWorktreeManager> _logger;
        // Un lock per repo (project-hash): git worktree add/remove toccano lo stesso .git/worktrees;
        // e un agente esegue una cosa alla volta (actor model). Serializza per repo, non globale.
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _repoGates = new();
        private bool _disposed;

        public AgentWorktreeManager(ILogger<AgentWorktreeManager> logger)
        {
            _logger = logger;
        }

        public string WorktreeRootForProject(string projectPath)
            => Path.Combine(WorktreesRoot, Helper.HGetHashString(projectPath));

        public string WorktreePathFor(string projectPath, string agentName)
        {
            if (string.IsNullOrWhiteSpace(agentName) || agentName.IndexOfAny(new[] { '/', '\\', ':' }) >= 0)
                throw new ArgumentException($"Nome agente non valido per un path di worktree: '{agentName}'.", nameof(agentName));
            return Path.Combine(WorktreeRootForProject(projectPath), agentName);
        }

        public async Task<string> EnsureWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default)
        {
            var worktreePath = WorktreePathFor(projectPath, agentName);
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                return await EnsureWorktreeUnlockedAsync(projectPath, agentName, worktreePath, ct);
            }
            finally { gate.Release(); }
        }

        private async Task<string> EnsureWorktreeUnlockedAsync(string projectPath, string agentName, string worktreePath, CancellationToken ct)
        {
            // Già un worktree valido? (rev-parse dentro il path riesce)
            if (Directory.Exists(worktreePath))
            {
                var (probe, _, _) = await GitAsync(worktreePath, "rev-parse --is-inside-work-tree", ct);
                if (probe == 0) return worktreePath;

                // Cartella presente ma non è un worktree valido (stale): pulisci e ricrea.
                _logger.LogWarning("[Worktree] '{Path}' esiste ma non è un worktree valido: ricreo.", worktreePath);
                await RemoveWorktreeUnlockedAsync(projectPath, agentName, worktreePath, ct);
            }

            Directory.CreateDirectory(WorktreeRootForProject(projectPath));

            // Detached su HEAD corrente: il prepare farà il checkout del branch d'attività vero.
            var (code, _, err) = await GitAsync(projectPath, $"worktree add --detach {Quote(worktreePath)}", ct);
            if (code != 0)
                throw new InvalidOperationException(
                    $"Creazione worktree per '{agentName}' fallita in '{projectPath}' (è un repo git?): {Describe(code, err)}");

            _logger.LogInformation("[Worktree] creato per '{Agent}': {Path}", agentName, worktreePath);
            return worktreePath;
        }

        public async Task<WorktreePrepareResult> PrepareForRunAsync(
            string projectPath, string agentName, string activityId,
            string baseBranch = null, string handoffRef = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                return WorktreePrepareResult.Fail("activityId mancante: impossibile nominare il branch d'attività.");

            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var worktreePath = await EnsureWorktreeUnlockedAsync(projectPath, agentName,
                    WorktreePathFor(projectPath, agentName), ct);

                // 1) fetch origin
                var (fc, _, fe) = await GitAsync(worktreePath, "fetch origin", ct);
                if (fc != 0) return WorktreePrepareResult.Fail($"git fetch origin fallito: {Describe(fc, fe)}");

                // baseBranch: se non dato, risolvi il default del remote (origin/HEAD), fallback main.
                var resolvedBase = string.IsNullOrWhiteSpace(baseBranch) ? await ResolveDefaultBranchAsync(worktreePath, ct) : baseBranch.Trim();

                // 2) reset --hard + clean -fd (MAI -x: gli untracked ignorati restano)
                var (rc, _, re) = await GitAsync(worktreePath, "reset --hard", ct);
                if (rc != 0) return WorktreePrepareResult.Fail($"git reset --hard fallito: {Describe(rc, re)}");
                await GitAsync(worktreePath, "clean -fd", ct);   // best-effort: pulizia scratch

                // 3) branch fresco per-attività da origin/<base>
                var branch = $"agent/{agentName}/{activityId}";
                var (cc, _, ce) = await GitAsync(worktreePath, $"checkout -B {Quote(branch)} {Quote("origin/" + resolvedBase)}", ct);
                if (cc != 0) return WorktreePrepareResult.Fail($"git checkout -B '{branch}' da 'origin/{resolvedBase}' fallito: {Describe(cc, ce)}");

                // 4) merge dell'handoff (ref COMPLETO): conflitto → not-ready, NON auto-risolvere
                if (!string.IsNullOrWhiteSpace(handoffRef))
                {
                    var (mc, _, me) = await GitAsync(worktreePath, $"merge {Quote("origin/" + handoffRef.Trim())}", ct);
                    if (mc != 0)
                    {
                        await GitAsync(worktreePath, "merge --abort", ct);   // ripristina lo stato pulito
                        _logger.LogWarning("[Worktree] merge di 'origin/{Ref}' in conflitto per '{Agent}': not-ready. {Err}", handoffRef, agentName, me);
                        return WorktreePrepareResult.Conflict(worktreePath);
                    }
                }

                // 5) submodule (il worktree fresco NON li popola): best-effort, no-op se assenti
                var (sc, _, se) = await GitAsync(worktreePath, "submodule update --init", ct);
                if (sc != 0)
                    _logger.LogWarning("[Worktree] submodule update per '{Agent}' non riuscito (best-effort): {Err}", agentName, se);

                _logger.LogInformation("[Worktree] preparato per '{Agent}': branch '{Branch}' da origin/{Base}", agentName, branch, resolvedBase);
                return WorktreePrepareResult.Ok(worktreePath);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return WorktreePrepareResult.Fail(ex.Message);
            }
            finally { gate.Release(); }
        }

        public async Task RemoveWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default)
        {
            var worktreePath = WorktreePathFor(projectPath, agentName);
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try { await RemoveWorktreeUnlockedAsync(projectPath, agentName, worktreePath, ct); }
            finally { gate.Release(); }
        }

        private async Task RemoveWorktreeUnlockedAsync(string projectPath, string agentName, string worktreePath, CancellationToken ct)
        {
            if (!Directory.Exists(worktreePath))
                return;

            // Progetto ancora presente → rimozione pulita via git (aggiorna .git/worktrees).
            if (Directory.Exists(projectPath))
            {
                var (code, _, err) = await GitAsync(projectPath, $"worktree remove --force {Quote(worktreePath)}", ct);
                await GitAsync(projectPath, "worktree prune", ct);
                if (code == 0)
                {
                    _logger.LogInformation("[Worktree] rimosso per '{Agent}': {Path}", agentName, worktreePath);
                    return;
                }
                _logger.LogWarning("[Worktree] 'git worktree remove' fallito per '{Agent}' ({Err}): elimino la cartella direttamente.", agentName, err);
            }

            // Progetto sparito o remove fallito: elimina la cartella best-effort.
            try { Directory.Delete(worktreePath, recursive: true); }
            catch (Exception ex) { _logger.LogWarning(ex, "[Worktree] eliminazione cartella '{Path}' fallita.", worktreePath); }
        }

        private async Task<string> ResolveDefaultBranchAsync(string worktreePath, CancellationToken ct)
        {
            // origin/HEAD → refs/remotes/origin/<default>. Fallback 'main' se non risolvibile.
            var (code, outp, _) = await GitAsync(worktreePath, "symbolic-ref --short refs/remotes/origin/HEAD", ct);
            if (code == 0 && !string.IsNullOrWhiteSpace(outp))
            {
                var name = outp.Trim();
                var slash = name.IndexOf('/');
                return slash >= 0 ? name.Substring(slash + 1) : name;   // "origin/main" → "main"
            }
            _logger.LogWarning("[Worktree] default branch del remote non risolvibile in '{Path}': uso 'main'.", worktreePath);
            return "main";
        }

        private static string RepoKey(string projectPath) => Helper.HGetHashString(projectPath ?? string.Empty);
        private static string Quote(string s) => "\"" + s + "\"";

        private static string Describe(int code, string stderr)
            => code == GitNotFoundExit ? "git non trovato nel PATH"
             : code == GitTimeoutExit ? "timeout"
             : $"exit {code}: {stderr?.Trim()}";

        /// <summary>Esegue git nativo (LibGit2Sharp non fa worktree). GIT_TERMINAL_PROMPT=0 = fail-fast senza hang.</summary>
        private async Task<(int ExitCode, string Stdout, string Stderr)> GitAsync(string workingDirectory, string arguments, CancellationToken ct)
        {
            _logger.LogDebug("[Worktree] git -C {Dir}: git {Args}", workingDirectory, arguments);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            process.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";

            try { process.Start(); }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, "[Worktree] git non trovato nel PATH");
                return (GitNotFoundExit, string.Empty, ex.Message);
            }

            // Leggere PRIMA di attendere: leggere dopo WaitForExit può deadlockare col buffer pieno.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            var completed = await Task.Run(() => process.WaitForExit(DefaultTimeoutMs), ct);
            if (!completed)
            {
                try { process.Kill(true); } catch { }
                _logger.LogError("[Worktree] git {Args} in timeout ({Ms}ms)", arguments, DefaultTimeoutMs);
                return (GitTimeoutExit, string.Empty, "timeout");
            }

            return (process.ExitCode, await stdoutTask, await stderrTask);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var g in _repoGates.Values)
                g.Dispose();
            _repoGates.Clear();
        }
    }
}
