using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Com'è andata un'azione su un repository.</summary>
    public sealed class RepoActionResult
    {
        public bool Success { get; init; }

        /// <summary>
        /// Perché non si è nemmeno partiti. <c>null</c> = si è partiti. È il motivo che la riga
        /// mostrava già sul pulsante spento: qui lo si ripete, perché a rifiutare è il servizio e
        /// non l'interfaccia.
        /// </summary>
        public string Refused { get; init; }

        public string Message { get; init; }

        /// <summary>Cose da dire a operazione riuscita: un submodule lasciato dov'era, e perché.</summary>
        public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

        /// <summary>I file cambiati sul disco, relativi alla radice del progetto: servono a chi deve ricaricare.</summary>
        public IReadOnlyList<string> ChangedFiles { get; init; } = Array.Empty<string>();

        /// <summary>Vero se sul disco è cambiato qualcosa, anche quando l'elenco dei file non è calcolabile.</summary>
        public bool ContentChanged { get; init; }
    }

    public sealed class RepoFetchOutcome
    {
        /// <summary>Vuoto per la radice; per i submodule il percorso relativo.</summary>
        public string Repo { get; init; }
        public string Label { get; init; }
        public bool Ok { get; init; }
        /// <summary>Vero se non c'era un remoto da interrogare.</summary>
        public bool Skipped { get; init; }
        public string Error { get; init; }
    }

    public interface IRepoSyncService
    {
        /// <summary>
        /// Chiede a ogni remoto cosa c'è di nuovo. Senza, un submodule non sa mai di essere
        /// indietro: il fetch del progetto non aggiorna i riferimenti dei figli.
        /// </summary>
        Task<IReadOnlyList<RepoFetchOutcome>> FetchAllAsync(string projectPath, CancellationToken ct = default);

        /// <summary>Pubblica <b>un</b> repository.</summary>
        Task<RepoActionResult> PushAsync(string projectPath, string agentName, string repo, CancellationToken ct = default);

        /// <summary>
        /// Scarica dal remoto di <b>un</b> repository. Sulla radice è lo scaricamento del progetto,
        /// seguito dall'allineamento dei submodule; su un submodule è «Aggiorna all'ultima».
        /// </summary>
        Task<RepoActionResult> PullAsync(string projectPath, string repo, CancellationToken ct = default);

        /// <summary>Porta un submodule (o tutti, con <paramref name="repo"/> vuoto) alla versione registrata, solo in avanti.</summary>
        Task<RepoActionResult> AlignAsync(string projectPath, string repo, CancellationToken ct = default);

        /// <summary>Scarica il progetto, se c'è da scaricare, e allinea i submodule.</summary>
        Task<RepoActionResult> PullAllAsync(string projectPath, CancellationToken ct = default);

        /// <summary>Annulla un'unione rimasta a metà: si torna a prima dello scaricamento.</summary>
        Task<RepoActionResult> AbortMergeAsync(string projectPath, string repo, CancellationToken ct = default);
    }

    /// <summary>
    /// Le azioni per riga dei pannelli git: pubblica, scarica, aggiorna all'ultima, allinea,
    /// annulla l'unione.
    /// <para>
    /// Ognuna <b>rilegge la vista</b> e si rifiuta con lo stesso motivo che la riga mostrava sul
    /// pulsante spento. La vista è l'unico posto dove si decide cosa è permesso: qui non c'è una
    /// seconda copia delle regole, che prima o poi direbbe qualcosa di diverso.
    /// </para>
    /// </summary>
    public sealed class RepoSyncService : IRepoSyncService
    {
        private readonly IWorkingChangesService _changes;
        private readonly IModernGitService _git;
        private readonly INativeGitTransport _transport;
        private readonly INativeGitRunner _runner;
        private readonly ISubmoduleAligner _aligner;
        private readonly IRepoRemoteState _remotes;
        private readonly ILogger<RepoSyncService> _logger;

        public RepoSyncService(
            IWorkingChangesService changes,
            IModernGitService git,
            INativeGitTransport transport,
            INativeGitRunner runner,
            ISubmoduleAligner aligner,
            IRepoRemoteState remotes,
            ILogger<RepoSyncService> logger)
        {
            _changes = changes;
            _git = git;
            _transport = transport;
            _runner = runner;
            _aligner = aligner;
            _remotes = remotes;
            _logger = logger;
        }

        public async Task<IReadOnlyList<RepoFetchOutcome>> FetchAllAsync(string projectPath, CancellationToken ct = default)
        {
            var outcomes = new List<RepoFetchOutcome>();
            var view = await _changes.GetAsync(projectPath, null, ct);
            if (view.Problem != null || view.NotAGitRepository || view.Repos == null) return outcomes;

            foreach (var repo in view.Repos)
            {
                if (repo.NotInitialized) continue;
                var dir = DirOf(view.RootPath, repo.Path);

                var remotes = await _runner.RunAsync(dir, new[] { "remote" }, ct);
                var hasOrigin = remotes.Ok && (remotes.Stdout ?? string.Empty)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries).Any(x => x.Trim() == "origin");
                if (!hasOrigin)
                {
                    _remotes.Set(dir, null);
                    outcomes.Add(new RepoFetchOutcome { Repo = repo.Path, Label = repo.Label, Ok = true, Skipped = true });
                    continue;
                }

                var fetch = await _transport.FetchAsync(dir, "origin", ct);
                // Un remoto che non risponde non ferma gli altri: lo si scrive sulla sua riga.
                _remotes.Set(dir, fetch.Ok ? null : $"Il remoto non ha risposto: {fetch.Error}");
                outcomes.Add(new RepoFetchOutcome { Repo = repo.Path, Label = repo.Label, Ok = fetch.Ok, Error = fetch.Ok ? null : fetch.Error });
            }
            return outcomes;
        }

        public async Task<RepoActionResult> PushAsync(string projectPath, string agentName, string repo, CancellationToken ct = default)
        {
            var (view, row, refused) = await FindAsync(projectPath, agentName, repo, ct);
            if (refused != null) return Refuse(refused);
            if (row.PushBlocker != null) return Refuse(row.PushBlocker);
            if (row.Ahead <= 0) return Refuse($"In '{row.Label}' non c'è niente da pubblicare.");

            var res = await _git.PushAsync(DirOf(view.RootPath, row.Path));
            return res.Success
                ? new RepoActionResult { Success = true, Message = $"'{row.Label}' pubblicato." }
                : new RepoActionResult { Success = false, Message = res.ErrorMessage ?? "Pubblicazione non riuscita." };
        }

        public async Task<RepoActionResult> PullAsync(string projectPath, string repo, CancellationToken ct = default)
        {
            var (view, row, refused) = await FindAsync(projectPath, null, repo, ct);
            if (refused != null) return Refuse(refused);
            if (row.PullBlocker != null) return Refuse(row.PullBlocker);

            var before = await HeadsAsync(view, ct);
            var dir = DirOf(view.RootPath, row.Path);
            RepoActionResult result;

            if (row.Depth == 0)
            {
                // Lo scaricamento del progetto allinea già i submodule, solo in avanti (ModernGitService).
                var res = await _git.PullAsync(dir);
                result = res.Success
                    ? new RepoActionResult
                    {
                        Success = true,
                        Message = res.HasChanges ? $"'{row.Label}' aggiornato." : $"'{row.Label}' era già aggiornato.",
                        Warnings = res.Warnings ?? Array.Empty<string>(),
                    }
                    : new RepoActionResult { Success = false, Message = await DescribeFailedPullAsync(dir, row.Label, res.ErrorMessage, ct) };
            }
            else
            {
                result = await UpdateToLatestAsync(dir, row, ct);
            }

            return await WithChangesAsync(result, view, before, ct);
        }

        /// <summary>
        /// «Aggiorna all'ultima»: porta un submodule in cima al ramo che segue. È un cambio di
        /// versione deciso dall'utente; dopo, il repository contenitore ha una versione nuova da
        /// registrare.
        /// </summary>
        private async Task<RepoActionResult> UpdateToLatestAsync(string dir, RepoChanges row, CancellationToken ct)
        {
            if (row.Detached)
            {
                // HEAD staccato: si torna sul ramo principale del submodule. Il blocco della riga ha
                // già garantito che il commit in checkout stia su quel ramo.
                var target = row.DetachedTarget;
                var fetch = await _transport.FetchAsync(dir, "origin", ct);
                if (!fetch.Ok) return Fail($"'{row.Label}': il remoto non ha risposto ({fetch.Error}).");

                var local = await _runner.RunAsync(dir, new[] { "show-ref", "--verify", "--quiet", $"refs/heads/{target}" }, ct);
                var checkout = local.Ok
                    ? await _runner.RunAsync(dir, new[] { "checkout", target }, ct)
                    : await _runner.RunAsync(dir, new[] { "checkout", "-b", target, "--track", $"origin/{target}" }, ct);
                if (!checkout.Ok) return Fail($"'{row.Label}' non è tornato sul ramo '{target}': {checkout.Describe()}");

                var ff = await _runner.RunAsync(dir, new[] { "merge", "--ff-only", $"origin/{target}" }, ct);
                if (!ff.Ok)
                    return new RepoActionResult
                    {
                        Success = true,
                        Message = $"'{row.Label}' è tornato sul ramo '{target}'.",
                        Warnings = new[] { $"'{row.Label}': il ramo '{target}' ha commit tuoi che il remoto non ha, quindi non è stato portato in cima. Scarica di nuovo da qui per unirli." },
                    };
            }
            else
            {
                var pull = await _transport.PullAsync(dir, ct);
                if (!pull.Ok) return Fail(await DescribeFailedPullAsync(dir, row.Label, pull.Error, ct));
            }

            // I submodule di questo submodule seguono, solo in avanti.
            var nested = await _aligner.AlignAsync(dir, SubmoduleAlignMode.Forward, null, ct);
            return new RepoActionResult
            {
                Success = true,
                Message = $"'{row.Label}' aggiornato all'ultima versione.",
                Warnings = nested.Notes,
            };
        }

        public async Task<RepoActionResult> AlignAsync(string projectPath, string repo, CancellationToken ct = default)
        {
            var view = await _changes.GetAsync(projectPath, null, ct);
            if (view.Problem != null) return Refuse(view.Problem);
            if (view.NotAGitRepository) return Refuse("Questa cartella non è un repository git.");

            string only = null;
            if (!string.IsNullOrWhiteSpace(repo))
            {
                var row = Find(view, repo);
                if (row == null || row.Depth == 0) return Refuse($"'{repo}' non è un submodule di questo progetto.");
                if (row.AlignBlocker != null) return Refuse(row.AlignBlocker);
                only = row.Path;
            }
            else
            {
                var blocked = view.Repos.FirstOrDefault(r => r.Depth > 0 && r.AlignBlocker != null);
                if (blocked != null) return Refuse($"'{blocked.Label}': {blocked.AlignBlocker}");
            }

            var before = await HeadsAsync(view, ct);
            var aligned = await _aligner.AlignAsync(view.RootPath, SubmoduleAlignMode.Forward, only, ct);
            return await WithChangesAsync(DescribeAlign(aligned), view, before, ct);
        }

        public async Task<RepoActionResult> PullAllAsync(string projectPath, CancellationToken ct = default)
        {
            await FetchAllAsync(projectPath, ct);

            var view = await _changes.GetAsync(projectPath, null, ct);
            if (view.Problem != null) return Refuse(view.Problem);
            if (view.NotAGitRepository) return Refuse("Questa cartella non è un repository git.");
            if (view.Repos == null || view.Repos.Count == 0) return Refuse("Nessun repository da aggiornare.");

            var root = view.Repos[0];
            if (root.RemoteProblem != null) return Refuse($"'{root.Label}': {root.RemoteProblem}");

            // Il progetto ha da scaricare: lo scaricamento allinea anche i submodule.
            if (root.Behind > 0) return await PullAsync(projectPath, string.Empty, ct);

            return await AlignAsync(projectPath, null, ct);
        }

        public async Task<RepoActionResult> AbortMergeAsync(string projectPath, string repo, CancellationToken ct = default)
        {
            var (view, row, refused) = await FindAsync(projectPath, null, repo, ct);
            if (refused != null) return Refuse(refused);
            if (!row.MergeInProgress) return Refuse($"In '{row.Label}' non c'è nessuna unione in corso.");

            var dir = DirOf(view.RootPath, row.Path);
            var abort = await _runner.RunAsync(dir, new[] { "merge", "--abort" }, ct);
            if (!abort.Ok) return Fail($"L'unione in '{row.Label}' non è stata annullata: {abort.Describe()}");

            _logger.LogInformation("[Flusso] unione annullata in '{Repo}'.", row.Label);
            return new RepoActionResult
            {
                Success = true,
                Message = $"Unione annullata: '{row.Label}' è tornato a prima dello scaricamento.",
                ContentChanged = true,
                ChangedFiles = row.Conflicts.Select(p => Prefixed(row.Path, p)).ToList(),
            };
        }

        // ---- infrastruttura ----

        private async Task<(WorkingChangesView View, RepoChanges Row, string Refused)> FindAsync(
            string projectPath, string agentName, string repo, CancellationToken ct)
        {
            var view = await _changes.GetAsync(projectPath, agentName, ct);
            if (view.Problem != null) return (view, null, view.Problem);
            if (view.NotAGitRepository) return (view, null, "Questa cartella non è un repository git.");

            var row = Find(view, repo);
            return row == null
                ? (view, null, $"'{repo}' non è un repository di questo progetto.")
                : (view, row, null);
        }

        private static RepoChanges Find(WorkingChangesView view, string repo)
        {
            var wanted = (repo ?? string.Empty).Replace('\\', '/').Trim('/');
            return (view.Repos ?? Array.Empty<RepoChanges>())
                .FirstOrDefault(r => string.Equals(r.Path ?? string.Empty, wanted, StringComparison.OrdinalIgnoreCase));
        }

        private static string DirOf(string root, string relativePath)
            => string.IsNullOrEmpty(relativePath)
                ? root
                : Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        private static string Prefixed(string repoPath, string file)
            => string.IsNullOrEmpty(repoPath) ? file : repoPath + "/" + file;

        /// <summary>Dove sta HEAD in ogni repository: il «prima» con cui confrontare il «dopo».</summary>
        private async Task<Dictionary<string, string>> HeadsAsync(WorkingChangesView view, CancellationToken ct)
        {
            var heads = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var repo in view.Repos)
            {
                if (repo.NotInitialized) continue;
                var head = await _runner.RunAsync(DirOf(view.RootPath, repo.Path), new[] { "rev-parse", "--verify", "--quiet", "HEAD" }, ct);
                if (head.Ok && !string.IsNullOrWhiteSpace(head.Stdout)) heads[repo.Path ?? string.Empty] = head.Stdout.Trim();
            }
            return heads;
        }

        /// <summary>
        /// Aggiunge al risultato i file cambiati sul disco, repository per repository: chi ha il
        /// documento aperto deve poterlo ricaricare, anche quando sta dentro un submodule.
        /// </summary>
        private async Task<RepoActionResult> WithChangesAsync(RepoActionResult result, WorkingChangesView before,
            Dictionary<string, string> headsBefore, CancellationToken ct)
        {
            var after = await _changes.GetAsync(before.RootPath, null, ct);
            if (after.Repos == null) return result;

            var changed = new List<string>();
            var moved = false;
            foreach (var repo in after.Repos)
            {
                if (repo.NotInitialized) continue;
                var dir = DirOf(after.RootPath, repo.Path);
                var head = await _runner.RunAsync(dir, new[] { "rev-parse", "--verify", "--quiet", "HEAD" }, ct);
                var now = head.Ok ? head.Stdout?.Trim() : null;
                if (string.IsNullOrEmpty(now)) continue;

                if (!headsBefore.TryGetValue(repo.Path ?? string.Empty, out var was))
                {
                    moved = true;   // appena popolato: l'elenco dei file non ha un «prima»
                    continue;
                }
                if (string.Equals(was, now, StringComparison.OrdinalIgnoreCase)) continue;

                moved = true;
                var diff = await _runner.RunAsync(dir, new[] { "diff", "--name-only", "--ignore-submodules=all", was, now }, ct);
                if (!diff.Ok) continue;
                changed.AddRange((diff.Stdout ?? string.Empty)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Prefixed(repo.Path, x.Trim().Trim('"'))));
            }

            return new RepoActionResult
            {
                Success = result.Success,
                Refused = result.Refused,
                Message = result.Message,
                Warnings = result.Warnings,
                ContentChanged = moved || result.ContentChanged,
                ChangedFiles = changed,
            };
        }

        private static RepoActionResult DescribeAlign(SubmoduleAlignResult aligned)
        {
            var done = aligned.Steps.Count(s => s.Outcome is SubmoduleAlignStep.Aligned or SubmoduleAlignStep.Populated);
            var failed = aligned.Steps.Where(s => s.Outcome == SubmoduleAlignStep.Failed).ToList();
            if (failed.Count > 0)
                return new RepoActionResult
                {
                    Success = false,
                    Message = string.Join(" ", failed.Select(f => f.Note)),
                    Warnings = aligned.Steps.Where(s => s.Outcome == SubmoduleAlignStep.LeftDiverged).Select(s => s.Note).ToList(),
                };

            return new RepoActionResult
            {
                Success = true,
                Message = done == 0
                    ? "I submodule erano già alla versione che il progetto registra."
                    : done == 1 ? "1 submodule allineato alla versione che il progetto registra."
                    : $"{done} submodule allineati alla versione che il progetto registra.",
                Warnings = aligned.Notes,
            };
        }

        /// <summary>
        /// Uno scaricamento non riuscito lascia due situazioni molto diverse: niente è cambiato,
        /// oppure un'unione è rimasta a metà. La seconda va detta col suo nome e con l'uscita.
        /// </summary>
        private async Task<string> DescribeFailedPullAsync(string dir, string label, string error, CancellationToken ct)
        {
            var merging = await _runner.RunAsync(dir, new[] { "rev-parse", "--quiet", "--verify", "MERGE_HEAD" }, ct);
            if (merging.Ok)
                return $"In '{label}' il tuo lavoro e quello scaricato toccano le stesse righe: l'unione è rimasta a metà. " +
                       "Correggi i file in conflitto e committa, oppure annulla l'unione per tornare a prima.";
            return $"'{label}' non è stato aggiornato e niente è cambiato: {error}";
        }

        private static RepoActionResult Refuse(string why) => new RepoActionResult { Success = false, Refused = why };
        private static RepoActionResult Fail(string why) => new RepoActionResult { Success = false, Message = why };
    }
}
