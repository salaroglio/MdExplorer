using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Utilities;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Esito del prepare-before-run di un worktree (Fase 7c/7d).</summary>
    public sealed class WorktreePrepareResult
    {
        public bool Success { get; private set; }
        public string WorktreePath { get; private set; }
        /// <summary>Il merge del branch di handoff è in conflitto → il chiamante lo mappa su not-ready/merge-conflict-with-main.</summary>
        public bool MergeConflict { get; private set; }
        /// <summary>La sync al ref/commit di handoff è fallita (ref assente/fetch ko, Fase 7d.5) → not-ready/git-sync-failed.</summary>
        public bool SyncFailed { get; private set; }
        public string Error { get; private set; }

        public static WorktreePrepareResult Ok(string path) => new() { Success = true, WorktreePath = path };
        public static WorktreePrepareResult Conflict(string path) => new() { Success = false, MergeConflict = true, WorktreePath = path, Error = "merge-conflict-with-main" };
        public static WorktreePrepareResult SyncFail(string path) => new() { Success = false, SyncFailed = true, WorktreePath = path, Error = "git-sync-failed" };
        public static WorktreePrepareResult Fail(string error) => new() { Success = false, Error = error };
    }

    /// <summary>Esito del commit+push del branch d'attività di un agente (Fase 7d.2/7d.5).</summary>
    public sealed class HandoffPushResult
    {
        /// <summary>Nome del branch pushato (ref COMPLETO da usare come handoffRef, es. <c>agent/&lt;A&gt;/&lt;id&gt;</c>).</summary>
        public string Branch { get; init; }
        /// <summary>Sha del commit di testa pushato (baseCommit a cui il ricevente deve sincronizzarsi).</summary>
        public string HeadSha { get; init; }
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

        /// <summary>
        /// Fase 7d.2/7d.5 — nel worktree dell'agente: <c>commit -a</c> (identità dell'agente, salta
        /// se pulito) poi push del branch d'attività su origin (refspec, non upstream-only).
        /// Ritorna branch (ref completo) + sha di testa, o <c>null</c> se non c'è worktree o il push
        /// fallisce. È il "commit → push" che precede la richiesta federata (§6).
        /// </summary>
        Task<HandoffPushResult> CommitAndPushBranchAsync(string projectPath, string agentName, string commitMessage, CancellationToken ct = default);

        /// <summary>Fase 7d.1 — merge esplicito <paramref name="sourceRef"/> in <paramref name="intoBranch"/> (native). Metodo, NON auto-innescato (7g).</summary>
        Task<bool> MergeBranchAsync(string projectPath, string sourceRef, string intoBranch, CancellationToken ct = default);

        /// <summary>Fase 7d.1 — cancella un branch (locale e, se <paramref name="remoteToo"/>, sul remote).</summary>
        Task DeleteBranchAsync(string projectPath, string branch, bool remoteToo, CancellationToken ct = default);

        /// <summary>Fase 7d.1/7d.4 — branch <c>agent/*</c> già fusi in <paramref name="intoBranch"/> (native <c>git branch --merged</c>).</summary>
        Task<IReadOnlyList<string>> ListMergedAgentBranchesAsync(string projectPath, string intoBranch, CancellationToken ct = default);

        /// <summary>
        /// Fase 7e.1 — path relativi dei submodule "toccati" (sporchi) nel worktree, per il gate del
        /// codice (§6bis). Legge i submodule dal <c>.gitmodules</c> e li incrocia con
        /// <c>git status --porcelain</c>. Vuoto se il worktree non ha submodule o sono puliti.
        /// </summary>
        Task<IReadOnlyList<string>> GetDirtySubmodulesAsync(string worktreePath, CancellationToken ct = default);

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

                // 4) merge dell'handoff (ref COMPLETO): sync fallita (ref assente) → git-sync-failed;
                //    conflitto di merge → merge-conflict-with-main. NON auto-risolvere (7d.5).
                if (!string.IsNullOrWhiteSpace(handoffRef))
                {
                    var remoteRef = "origin/" + handoffRef.Trim();
                    // Il ref di handoff deve esistere sul remote (dopo il fetch): se manca, la sync
                    // al lavoro di A è fallita a monte (branch non pushato/ref sbagliato).
                    var (vc, _, _) = await GitAsync(worktreePath, $"rev-parse --verify --quiet {Quote(remoteRef + "^{commit}")}", ct);
                    if (vc != 0)
                    {
                        _logger.LogWarning("[Worktree] ref di handoff '{Ref}' assente sul remote per '{Agent}': git-sync-failed.", remoteRef, agentName);
                        return WorktreePrepareResult.SyncFail(worktreePath);
                    }
                    var (mc, _, me) = await GitAsync(worktreePath, $"merge {Quote(remoteRef)}", ct);
                    if (mc != 0)
                    {
                        await GitAsync(worktreePath, "merge --abort", ct);   // ripristina lo stato pulito
                        _logger.LogWarning("[Worktree] merge di '{Ref}' in conflitto per '{Agent}': not-ready. {Err}", remoteRef, agentName, me);
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

        public async Task<HandoffPushResult> CommitAndPushBranchAsync(string projectPath, string agentName, string commitMessage, CancellationToken ct = default)
        {
            var worktreePath = WorktreePathFor(projectPath, agentName);
            if (!Directory.Exists(worktreePath))
                return null;

            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var identity = AgentGitIdentity.EnvFor(agentName);
                await GitAsync(worktreePath, "add -A", ct);
                // commit -a: se non c'è nulla da committare git ritorna != 0 (non è un errore) → si prosegue.
                var msg = string.IsNullOrWhiteSpace(commitMessage) ? "agent deliverable" : commitMessage.Replace("\"", "'");
                await GitAsync(worktreePath, $"commit -m {Quote(msg)}", ct, identity);

                var (bc, branchOut, _) = await GitAsync(worktreePath, "rev-parse --abbrev-ref HEAD", ct);
                var (hc, shaOut, _) = await GitAsync(worktreePath, "rev-parse HEAD", ct);
                if (bc != 0 || hc != 0)
                    return null;
                var branch = branchOut.Trim();
                var headSha = shaOut.Trim();
                if (string.IsNullOrWhiteSpace(branch) || branch == "HEAD")
                    return null;   // detached: nessun branch d'attività da pushare

                // Push per REFSPEC (PushAsync di LibGit2Sharp è upstream-only): pubblica il branch nuovo.
                var (pc, _, pe) = await GitAsync(worktreePath, $"push origin {Quote(branch + ":refs/heads/" + branch)}", ct);
                if (pc != 0)
                {
                    _logger.LogWarning("[Worktree] push del branch '{Branch}' per '{Agent}' fallito: {Err}", branch, agentName, Describe(pc, pe));
                    return null;
                }

                _logger.LogInformation("[Worktree] deliverable di '{Agent}' pubblicato: {Branch}@{Sha}", agentName, branch, headSha);
                return new HandoffPushResult { Branch = branch, HeadSha = headSha };
            }
            finally { gate.Release(); }
        }

        public async Task<bool> MergeBranchAsync(string projectPath, string sourceRef, string intoBranch, CancellationToken ct = default)
        {
            // ⚠️ Muta la working tree del repo (checkout + merge): metodo DORMIENTE in 7d, lo
            // innescherà 7g (gate del merge) con le sue guardie. Non è chiamato automaticamente ora.
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var (cc, _, ce) = await GitAsync(projectPath, $"checkout {Quote(intoBranch)}", ct);
                if (cc != 0) { _logger.LogWarning("[Worktree] merge: checkout '{Into}' fallito: {Err}", intoBranch, ce); return false; }
                var (mc, _, me) = await GitAsync(projectPath, $"merge {Quote(sourceRef)}", ct);
                if (mc != 0)
                {
                    await GitAsync(projectPath, "merge --abort", ct);
                    _logger.LogWarning("[Worktree] merge di '{Src}' in '{Into}' fallito: {Err}", sourceRef, intoBranch, me);
                    return false;
                }
                return true;
            }
            finally { gate.Release(); }
        }

        public async Task DeleteBranchAsync(string projectPath, string branch, bool remoteToo, CancellationToken ct = default)
        {
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var (lc, _, le) = await GitAsync(projectPath, $"branch -D {Quote(branch)}", ct);
                if (lc != 0) _logger.LogWarning("[Worktree] cancellazione branch locale '{Branch}' fallita: {Err}", branch, le);
                if (remoteToo)
                {
                    var (rc, _, re) = await GitAsync(projectPath, $"push origin --delete {Quote(branch)}", ct);
                    if (rc != 0) _logger.LogWarning("[Worktree] cancellazione branch remoto '{Branch}' fallita: {Err}", branch, re);
                }
            }
            finally { gate.Release(); }
        }

        public async Task<IReadOnlyList<string>> ListMergedAgentBranchesAsync(string projectPath, string intoBranch, CancellationToken ct = default)
        {
            var into = string.IsNullOrWhiteSpace(intoBranch) ? await ResolveDefaultBranchAsync(projectPath, ct) : intoBranch.Trim();
            var (code, outp, _) = await GitAsync(projectPath, $"branch --merged {Quote(into)}", ct);
            if (code != 0) return System.Array.Empty<string>();
            var result = new List<string>();
            foreach (var raw in outp.Split('\n'))
            {
                var line = raw.TrimStart('*', ' ', '+').Trim();   // '*' = branch corrente, '+' = in un worktree
                if (line.StartsWith("agent/", System.StringComparison.Ordinal))
                    result.Add(line);
            }
            return result;
        }

        public async Task<IReadOnlyList<string>> GetDirtySubmodulesAsync(string worktreePath, CancellationToken ct = default)
        {
            var submodulePaths = ReadSubmodulePaths(worktreePath);
            if (submodulePaths.Count == 0)
                return System.Array.Empty<string>();

            var (code, outp, _) = await GitAsync(worktreePath, "status --porcelain --ignore-submodules=none", ct);
            if (code != 0)
                return System.Array.Empty<string>();

            var dirty = new List<string>();
            foreach (var raw in outp.Split('\n'))
            {
                if (raw.Length < 4) continue;
                var status = raw.Substring(0, 2);
                if (status == "  ") continue;                 // pulito
                var path = raw.Substring(3).Trim().Trim('"'); // porcelain: 2 char stato + spazio + path
                var slash = path.IndexOf(" -> ", System.StringComparison.Ordinal);
                if (slash >= 0) path = path.Substring(slash + 4);   // rename: prendi la destinazione
                path = path.TrimEnd('/');
                if (submodulePaths.Contains(path) && !dirty.Contains(path))
                    dirty.Add(path);
            }
            return dirty;
        }

        /// <summary>Legge i path dei submodule dal <c>.gitmodules</c> del worktree (normalizzati a '/').</summary>
        private static HashSet<string> ReadSubmodulePaths(string worktreePath)
        {
            var set = new HashSet<string>(System.StringComparer.Ordinal);
            var gm = Path.Combine(worktreePath, ".gitmodules");
            if (!File.Exists(gm)) return set;
            try
            {
                foreach (var line in File.ReadAllLines(gm))
                {
                    var t = line.Trim();
                    if (!t.StartsWith("path", System.StringComparison.OrdinalIgnoreCase)) continue;
                    var eq = t.IndexOf('=');
                    if (eq < 0) continue;
                    var p = t.Substring(eq + 1).Trim().Replace('\\', '/').TrimEnd('/');
                    if (p.Length > 0) set.Add(p);
                }
            }
            catch { /* best-effort */ }
            return set;
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
        private async Task<(int ExitCode, string Stdout, string Stderr)> GitAsync(
            string workingDirectory, string arguments, CancellationToken ct,
            IReadOnlyDictionary<string, string> extraEnv = null)
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
            if (extraEnv != null)
                foreach (var kv in extraEnv)
                    process.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;

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
