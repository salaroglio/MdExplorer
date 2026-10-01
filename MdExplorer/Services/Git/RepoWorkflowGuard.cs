using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Un'unione rimasta a metà dopo uno scaricamento: cosa è in conflitto, e cosa lo è ancora.</summary>
    public sealed class RepoMergeState
    {
        public static readonly RepoMergeState None = new RepoMergeState();

        public bool InProgress { get; init; }
        /// <summary>I percorsi che git segna come non uniti.</summary>
        public IReadOnlyList<string> Conflicts { get; init; } = Array.Empty<string>();
        /// <summary>Fra quelli, i file che contengono ancora i segni di conflitto: non sono stati corretti.</summary>
        public IReadOnlyList<string> StillMarked { get; init; } = Array.Empty<string>();
        /// <summary>Fra quelli, i submodule: il conflitto è sulla versione registrata, non su un file.</summary>
        public IReadOnlyList<string> SubmoduleConflicts { get; init; } = Array.Empty<string>();
    }

    /// <summary>Le versioni di un submodule viste dal repository che lo contiene.</summary>
    public sealed class SubmodulePointer
    {
        /// <summary>Il commit registrato nell'ultimo commit del repository che lo contiene.</summary>
        public string Recorded { get; init; }
        /// <summary>Il commit in checkout nel submodule.</summary>
        public string CheckedOut { get; init; }
        /// <summary>Uno dei valori di <see cref="SubmoduleRelation"/>; <c>null</c> se non si è potuto leggere.</summary>
        public string Relation { get; init; }
    }

    public interface IRepoWorkflowGuard
    {
        Task<RepoMergeState> ReadMergeAsync(string repositoryPath, CancellationToken ct = default);

        Task<SubmodulePointer> ReadPointerAsync(string parentDir, string relativePath, string submoduleDir,
            CancellationToken ct = default);

        /// <summary>
        /// Perché qui non si può committare adesso, per le ragioni che il commit stesso deve
        /// rifiutare: <c>null</c> = si può.
        /// </summary>
        Task<string> CommitBlockerAsync(string repositoryPath, CancellationToken ct = default);

        /// <summary>
        /// Il ramo su cui rimettere un submodule con HEAD staccato: il ramo principale del suo
        /// remoto, se il commit in checkout ci sta sopra. <c>null</c> = non c'è un ramo su cui
        /// tornare senza perdere di vista quel commit.
        /// </summary>
        Task<string> DetachedTargetAsync(string submoduleDir, CancellationToken ct = default);

        /// <summary>
        /// Scaricando adesso, git si fermerebbe con un conflitto sulla versione di un submodule?
        /// Succede quando <b>sia tu sia il remoto</b> avete registrato per lo stesso submodule due
        /// versioni che divergono. <c>null</c> = no; altrimenti il motivo, con cosa fare prima.
        /// </summary>
        Task<string> PointerConflictAheadAsync(string repositoryPath, string upstream, CancellationToken ct = default);
    }

    /// <summary>
    /// L'esito dell'ultima interrogazione del remoto, per repository. Sta in memoria: serve solo a
    /// scrivere sulla riga «il remoto non ha risposto» finché non si riprova.
    /// </summary>
    public interface IRepoRemoteState
    {
        void Set(string repositoryDir, string problem);
        string Get(string repositoryDir);
    }

    public sealed class RepoRemoteState : IRepoRemoteState
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _problems
            = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string Key(string dir)
            => Path.GetFullPath(dir ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        public void Set(string repositoryDir, string problem)
        {
            if (string.IsNullOrEmpty(problem)) _problems.TryRemove(Key(repositoryDir), out _);
            else _problems[Key(repositoryDir)] = problem;
        }

        public string Get(string repositoryDir)
            => _problems.TryGetValue(Key(repositoryDir), out var problem) ? problem : null;
    }

    /// <summary>
    /// I controlli del flusso di lavoro <b>in un posto solo</b>.
    /// <para>
    /// La vista per repository li usa per dire all'utente cosa può fare e perché no; le azioni li
    /// rieseguono prima di agire e rifiutano con lo stesso motivo. Così un pulsante spento
    /// nell'interfaccia non è l'unica difesa: un client vecchio o un doppio clic non scavalcano
    /// niente.
    /// </para>
    /// </summary>
    public sealed class RepoWorkflowGuard : IRepoWorkflowGuard
    {
        private readonly INativeGitRunner _git;
        private readonly ILogger<RepoWorkflowGuard> _logger;

        public RepoWorkflowGuard(INativeGitRunner git, ILogger<RepoWorkflowGuard> logger)
        {
            _git = git;
            _logger = logger;
        }

        public async Task<RepoMergeState> ReadMergeAsync(string repositoryPath, CancellationToken ct = default)
        {
            var merging = await _git.RunAsync(repositoryPath, new[] { "rev-parse", "--quiet", "--verify", "MERGE_HEAD" }, ct);
            if (!merging.Ok) return RepoMergeState.None;

            var unmerged = await _git.RunAsync(repositoryPath, new[] { "diff", "--name-only", "--diff-filter=U" }, ct);
            var conflicts = (unmerged.Stdout ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().Trim('"')).Where(x => x.Length > 0)
                .Distinct(StringComparer.Ordinal).ToList();

            var marked = new List<string>();
            var submodules = new List<string>();
            foreach (var path in conflicts)
            {
                var full = Path.Combine(repositoryPath, path.Replace('/', Path.DirectorySeparatorChar));
                if (Directory.Exists(full)) { submodules.Add(path); continue; }
                if (File.Exists(full) && HasConflictMarkers(full)) marked.Add(path);
            }

            return new RepoMergeState
            {
                InProgress = true, Conflicts = conflicts, StillMarked = marked, SubmoduleConflicts = submodules,
            };
        }

        public async Task<SubmodulePointer> ReadPointerAsync(string parentDir, string relativePath, string submoduleDir,
            CancellationToken ct = default)
        {
            var recorded = await _git.RunAsync(parentDir, new[] { "rev-parse", "--verify", "--quiet", $"HEAD:{relativePath}" }, ct);
            var head = await _git.RunAsync(submoduleDir, new[] { "rev-parse", "--verify", "--quiet", "HEAD" }, ct);
            var r = recorded.Ok ? recorded.Stdout?.Trim() : null;
            var c = head.Ok ? head.Stdout?.Trim() : null;
            return new SubmodulePointer
            {
                Recorded = string.IsNullOrEmpty(r) ? null : r,
                CheckedOut = string.IsNullOrEmpty(c) ? null : c,
                Relation = await SubmoduleRelation.ReadAsync(_git, submoduleDir, r, c, ct),
            };
        }

        public async Task<string> CommitBlockerAsync(string repositoryPath, CancellationToken ct = default)
        {
            var merge = await ReadMergeAsync(repositoryPath, ct);
            var blocker = MergeCommitBlocker(merge);
            if (blocker != null) return blocker;

            if (!File.Exists(Path.Combine(repositoryPath, ".gitmodules"))) return null;
            var status = await _git.RunAsync(repositoryPath, new[] { "submodule", "status" }, ct);
            if (!status.Ok) return null;

            foreach (var line in SubmoduleStatusLine.Parse(status.Stdout))
            {
                if (line.Prefix == '-') continue;
                var dir = Path.Combine(repositoryPath, line.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(dir)) continue;

                var pointer = await ReadPointerAsync(repositoryPath, line.Path, dir, ct);
                blocker = PointerCommitBlocker(line.Path, pointer.Relation);
                if (blocker != null) return blocker;
            }
            return null;
        }

        public async Task<string> DetachedTargetAsync(string submoduleDir, CancellationToken ct = default)
        {
            var candidates = new List<string>();
            var head = await _git.RunAsync(submoduleDir, new[] { "symbolic-ref", "--short", "-q", "refs/remotes/origin/HEAD" }, ct);
            var value = head.Ok ? head.Stdout?.Trim() : null;
            if (!string.IsNullOrEmpty(value) && value.StartsWith("origin/", StringComparison.Ordinal))
                candidates.Add(value.Substring("origin/".Length));
            candidates.AddRange(new[] { "main", "master" });

            foreach (var name in candidates.Distinct(StringComparer.Ordinal))
            {
                var exists = await _git.RunAsync(submoduleDir, new[] { "rev-parse", "--verify", "--quiet", $"refs/remotes/origin/{name}" }, ct);
                if (!exists.Ok) continue;
                // Solo se il commit in checkout sta su quel ramo: altrimenti tornarci lo lascerebbe indietro.
                return await SubmoduleRelation.IsAncestorAsync(_git, submoduleDir, "HEAD", $"refs/remotes/origin/{name}", ct)
                    ? name
                    : null;
            }
            return null;
        }

        public async Task<string> PointerConflictAheadAsync(string repositoryPath, string upstream, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(upstream)) return null;
            if (!File.Exists(Path.Combine(repositoryPath, ".gitmodules"))) return null;

            var mergeBase = await _git.RunAsync(repositoryPath, new[] { "merge-base", "HEAD", upstream }, ct);
            var baseSha = mergeBase.Ok ? mergeBase.Stdout?.Trim() : null;
            if (string.IsNullOrEmpty(baseSha)) return null;

            var status = await _git.RunAsync(repositoryPath, new[] { "submodule", "status" }, ct);
            if (!status.Ok) return null;

            foreach (var line in SubmoduleStatusLine.Parse(status.Stdout))
            {
                var mine = await TreeEntryAsync(repositoryPath, "HEAD", line.Path, ct);
                var theirs = await TreeEntryAsync(repositoryPath, upstream, line.Path, ct);
                var origin = await TreeEntryAsync(repositoryPath, baseSha, line.Path, ct);
                if (mine == null || theirs == null || origin == null) continue;
                if (mine == theirs || mine == origin || theirs == origin) continue;   // l'ha spostato uno solo dei due

                var dir = Path.Combine(repositoryPath, line.Path.Replace('/', Path.DirectorySeparatorChar));
                var have = Directory.Exists(dir) &&
                           (await _git.RunAsync(dir, new[] { "cat-file", "-e", theirs + "^{commit}" }, ct)).Ok;
                // git se la cava da solo quando una delle due versioni contiene l'altra.
                if (have && (await SubmoduleRelation.IsAncestorAsync(_git, dir, theirs, mine, ct) ||
                             await SubmoduleRelation.IsAncestorAsync(_git, dir, mine, theirs, ct)))
                    continue;

                return $"Tu e il remoto avete portato '{line.Path}' a due versioni diverse. Prima scarica dentro '{line.Path}' " +
                       "(«Aggiorna all'ultima»), poi committa qui la versione unita, e solo dopo scarica questo repository.";
            }
            return null;
        }

        private async Task<string> TreeEntryAsync(string dir, string commitish, string path, CancellationToken ct)
        {
            var res = await _git.RunAsync(dir, new[] { "rev-parse", "--verify", "--quiet", $"{commitish}:{path}" }, ct);
            var sha = res.Ok ? res.Stdout?.Trim() : null;
            return string.IsNullOrEmpty(sha) ? null : sha;
        }

        /// <summary>Il commit che chiude un'unione parte solo quando non resta niente di irrisolto.</summary>
        public static string MergeCommitBlocker(RepoMergeState merge)
        {
            if (merge == null || !merge.InProgress) return null;
            if (merge.SubmoduleConflicts.Count > 0)
                return $"Unione in corso con un conflitto sulla versione di '{merge.SubmoduleConflicts[0]}': " +
                       $"annulla l'unione, scarica prima dentro '{merge.SubmoduleConflicts[0]}', committa il progetto e poi scarica di nuovo.";
            if (merge.StillMarked.Count > 0)
                return "Unione in corso: " + string.Join(", ", merge.StillMarked.Take(5)) +
                       (merge.StillMarked.Count > 5 ? "…" : "") +
                       (merge.StillMarked.Count == 1 ? " contiene" : " contengono") +
                       " ancora i segni di conflitto (<<<<<<<). Correggi, poi committa; oppure annulla l'unione.";
            return null;
        }

        /// <summary>
        /// Committare il repository mentre un suo submodule è più indietro (o diverge) dalla
        /// versione registrata la riporterebbe indietro: il commit registra ciò che è in checkout.
        /// </summary>
        public static string PointerCommitBlocker(string submodulePath, string relation) => relation switch
        {
            SubmoduleRelation.Behind or SubmoduleRelation.Unknown =>
                $"'{submodulePath}' è più indietro della versione che il progetto registra: committando la riporteresti indietro. " +
                $"Prima allinea '{submodulePath}' (pannello «da scaricare»).",
            SubmoduleRelation.Diverged =>
                $"'{submodulePath}' diverge dalla versione che il progetto registra: committando scarteresti quella versione. " +
                $"Prima «Aggiorna all'ultima» dentro '{submodulePath}', che unisce i due lavori.",
            _ => null,
        };

        private bool HasConflictMarkers(string file)
        {
            try
            {
                foreach (var line in File.ReadLines(file))
                    if (line.StartsWith("<<<<<<< ", StringComparison.Ordinal) ||
                        line.StartsWith(">>>>>>> ", StringComparison.Ordinal))
                        return true;
            }
            catch (Exception ex)
            {
                // Non leggibile: non si può dire che sia a posto, quindi resta fra i non risolti.
                _logger.LogDebug(ex, "[Flusso] '{File}' non leggibile durante il controllo dei conflitti.", file);
                return true;
            }
            return false;
        }
    }
}
