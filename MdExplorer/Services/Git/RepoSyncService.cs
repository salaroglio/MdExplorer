using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git.Interfaces;
using MdExplorer.Features.Templates;
using MdExplorer.Service.Models;
using MdExplorer.Services.Templates;
using MdExplorer.Utilities;
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

    /// <summary>
    /// The «source» of a project: the repository the project was taken from. Updates are only ever taken from it;
    /// nothing is published to it. It is the remote <c>template-&lt;id&gt;</c> of the project's base template (the row
    /// <c>kind: project</c> of <c>templates:</c> in <c>.development.yml</c>, docs-internal/Sprints/2026-10-17-Template-Di-Progetto.md),
    /// or, for a project that does not come from a template, a remote called <c>upstream</c> (a fork's original, a demo
    /// cloned before the templates).
    /// </summary>
    public sealed class UpstreamStatus
    {
        /// <summary>False for almost every project: no source, nothing to show.</summary>
        public bool HasUpstream { get; init; }
        public string Url { get; init; }
        /// <summary>The source's branch the updates come from (for a template: the branch of the chosen language).</summary>
        public string Branch { get; init; }
        /// <summary>The git remote of the source: <c>template-&lt;id&gt;</c> or <c>upstream</c>.</summary>
        public string Remote { get; init; }
        /// <summary>The project's branch the updates go into.</summary>
        public string LocalBranch { get; init; }
        /// <summary>Commits the source has and this project does not.</summary>
        public int Behind { get; init; }
        /// <summary>Why the source could not be asked or compared, for the person.</summary>
        public string Problem { get; init; }
    }

    public interface IRepoSyncService
    {
        /// <summary>
        /// What the project's source has that the project does not. <paramref name="fetch"/> asks the source
        /// first (network); without it the answer is what was known at the last asking.
        /// </summary>
        Task<UpstreamStatus> UpstreamStatusAsync(string projectPath, bool fetch, CancellationToken ct = default);

        /// <summary>
        /// «Scarica gli aggiornamenti»: merges the source's branch into the project's and brings <c>origin</c>
        /// level with the result, so that whoever starts from <c>origin</c> (the agents) starts from the new
        /// version. A merge that conflicts is undone: the project is left as it was.
        /// </summary>
        Task<RepoActionResult> PullUpstreamAsync(string projectPath, CancellationToken ct = default);

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
        private readonly MdExplorer.Services.AgentRun.IAgentMergeRequestService _requests;
        private readonly IModernGitService _git;
        private readonly INativeGitTransport _transport;
        private readonly INativeGitRunner _runner;
        private readonly ISubmoduleAligner _aligner;
        private readonly IRepoRemoteState _remotes;
        private readonly ILogger<RepoSyncService> _logger;
        private readonly MdeAutoCommit _autoCommit;
        private readonly MdExplorer.Services.DevelopmentConfigNotices _configNotices;

        public RepoSyncService(
            IWorkingChangesService changes,
            MdExplorer.Services.AgentRun.IAgentMergeRequestService requests,
            IModernGitService git,
            INativeGitTransport transport,
            INativeGitRunner runner,
            ISubmoduleAligner aligner,
            IRepoRemoteState remotes,
            MdeAutoCommit autoCommit,
            MdExplorer.Services.DevelopmentConfigNotices configNotices,
            ILogger<RepoSyncService> logger)
        {
            _configNotices = configNotices;
            _autoCommit = autoCommit;
            _changes = changes;
            _requests = requests;
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
                _remotes.Set(dir, fetch.Ok ? null : UiText.Say($"Il remoto non ha risposto: {fetch.Error}", $"The remote did not answer: {fetch.Error}"));
                outcomes.Add(new RepoFetchOutcome { Repo = repo.Path, Label = repo.Label, Ok = fetch.Ok, Error = fetch.Ok ? null : fetch.Error });
            }
            return outcomes;
        }

        public async Task<RepoActionResult> PushAsync(string projectPath, string agentName, string repo, CancellationToken ct = default)
        {
            var (view, row, refused) = await FindAsync(projectPath, agentName, repo, ct);
            if (refused != null) return Refuse(refused);
            if (row.PushBlocker != null) return Refuse(row.PushBlocker);
            if (row.Ahead <= 0) return Refuse(UiText.Say($"In '{row.Label}' non c'è niente da pubblicare.", $"In '{row.Label}' there is nothing to push."));

            // La copia di un agente non si pubblica col nome locale del ramo: su origin quel nome non esiste, e
            // nascerebbe un secondo ramo che nessuna richiesta di approvazione guarda.
            if (!string.IsNullOrEmpty(agentName) && string.IsNullOrEmpty(row.Path))
            {
                var problem = await _requests.PublishCopyAsync(projectPath, agentName, view.RootPath, row.Branch, ct);
                return problem == null
                    ? new RepoActionResult { Success = true, Message = UiText.Say($"Lavoro nella copia di '{agentName}' pubblicato.", $"Work in the copy of '{agentName}' pushed.") }
                    : new RepoActionResult { Success = false, Message = UiText.Say($"Pubblicazione non riuscita ({problem}).", $"Push failed ({problem}).") };
            }

            var res = await _git.PushAsync(DirOf(view.RootPath, row.Path));
            return res.Success
                ? new RepoActionResult { Success = true, Message = UiText.Say($"'{row.Label}' pubblicato.", $"'{row.Label}' pushed.") }
                : new RepoActionResult { Success = false, Message = res.ErrorMessage ?? UiText.Say("Pubblicazione non riuscita.", "Push failed.") };
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
                // .development.yml modificato solo qui (MdExplorer lo scrive da sé: il segreto della chat, una scelta): git
                // rifiuterebbe il pull appena gli aggiornamenti toccano il file. Lo si mette da parte e lo si riunisce dopo.
                // Sprint «File di sistema di MDE», S5: prima il commit dei file di MDE rimasti modificati, così il pull non si ferma su di
                // loro. Con il commit automatico spento (o senza identità git) resta il parcheggio di .development.yml qui sotto.
                await _autoCommit.CommitPendingAsync(dir, ct);
                var parked = await ParkDevelopmentYamlAsync(dir, ct);
                if (parked.Refusal != null) return Refuse(parked.Refusal);

                // Lo scaricamento del progetto allinea già i submodule, solo in avanti (ModernGitService).
                var res = await _git.PullAsync(dir);
                IReadOnlyList<string> mdeNotes = Array.Empty<string>();
                bool mergeStopped;
                if (!res.Success && (await _runner.RunAsync(dir, new[] { "rev-parse", "--quiet", "--verify", "MERGE_HEAD" }, ct)).Ok)
                {
                    var resolved = await ResolveMdeConflictsAsync(dir, ct);
                    if (resolved.Problem != null)
                    {
                        // conflitto vero su una voce: si torna a prima, e si dice quale voce
                        await _runner.RunAsync(dir, new[] { "merge", "--abort" }, ct);
                        if (parked.Park != null) await UnparkDevelopmentYamlAsync(dir, parked.Park, false, ct);
                        return await WithChangesAsync(Fail(resolved.Problem), view, before, ct);
                    }
                    mdeNotes = MdeResolutionNotes(resolved).ToList();
                    if (resolved.Done)
                    {
                        var aligned = await _aligner.AlignAsync(dir, SubmoduleAlignMode.Forward, null, ct);
                        res = new GitOperationResult { Success = true, HasChanges = true, Warnings = aligned.Notes };
                    }
                    else if (resolved.Remaining.Count > 0)
                    {
                        mdeNotes = mdeNotes.Append(UiText.Say(
                            $"Restano da sistemare a mano: {string.Join(", ", resolved.Remaining)}.",
                            $"Still to fix by hand: {string.Join(", ", resolved.Remaining)}.")).ToList();
                    }
                }
                // N4 (QA 07/11): l'unione è ancora in corso (si è fermata su file dell'utente) anche quando git ha unito .development.yml
                // da sé, senza conflitto: lo dice MERGE_HEAD, non l'elenco dei file che MDE ha dovuto risolvere.
                mergeStopped = !res.Success && (await _runner.RunAsync(dir, new[] { "rev-parse", "--quiet", "--verify", "MERGE_HEAD" }, ct)).Ok;
                var ymlNotes = parked.Park == null
                    ? Array.Empty<string>()
                    // N4 (QA 07/11): con l'unione a metà la copia messa da parte non torna sopra quella unita, ci si riunisce.
                    : await UnparkDevelopmentYamlAsync(dir, parked.Park, res.Success || mergeStopped, ct, fromWorkingTree: mergeStopped);
                ymlNotes = mdeNotes.Concat(ymlNotes).ToList();
                result = res.Success
                    ? new RepoActionResult
                    {
                        Success = true,
                        Message = res.HasChanges ? UiText.Say($"'{row.Label}' aggiornato.", $"'{row.Label}' updated.") : UiText.Say($"'{row.Label}' era già aggiornato.", $"'{row.Label}' was already up to date."),
                        Warnings = (res.Warnings ?? Array.Empty<string>()).Concat(ymlNotes).ToList(),
                    }
                    : new RepoActionResult { Success = false, Message = string.Join(" ", new[] { await DescribeFailedPullAsync(dir, row.Label, res.ErrorMessage, ct) }.Concat(mdeNotes)) };
            }
            else
            {
                result = await UpdateToLatestAsync(dir, row, ct);
            }

            // Il pull può aver portato un .development.yml scritto da un MDE più nuovo (D4 dello schema): si dice subito.
            _configNotices.Announce(projectPath, always: false, "pull");
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
                if (!fetch.Ok) return Fail(UiText.Say($"'{row.Label}': il remoto non ha risposto ({fetch.Error}).", $"'{row.Label}': the remote did not answer ({fetch.Error})."));

                var local = await _runner.RunAsync(dir, new[] { "show-ref", "--verify", "--quiet", $"refs/heads/{target}" }, ct);
                var checkout = local.Ok
                    ? await _runner.RunAsync(dir, new[] { "checkout", target }, ct)
                    : await _runner.RunAsync(dir, new[] { "checkout", "-b", target, "--track", $"origin/{target}" }, ct);
                if (!checkout.Ok) return Fail(UiText.Say($"'{row.Label}' non è tornato sul ramo '{target}': {checkout.Describe()}", $"'{row.Label}' did not go back to branch '{target}': {checkout.Describe()}"));

                var ff = await _runner.RunAsync(dir, new[] { "merge", "--ff-only", $"origin/{target}" }, ct);
                if (!ff.Ok)
                    return new RepoActionResult
                    {
                        Success = true,
                        Message = UiText.Say($"'{row.Label}' è tornato sul ramo '{target}'.", $"'{row.Label}' is back on branch '{target}'."),
                        Warnings = new[] { UiText.Say($"'{row.Label}': il ramo '{target}' ha commit tuoi che il remoto non ha, quindi non è stato portato in cima. Scarica di nuovo da qui per unirli.", $"'{row.Label}': branch '{target}' has commits of yours that the remote does not have, so it was not brought to the tip. Pull again from here to merge them.") },
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
                Message = UiText.Say($"'{row.Label}' aggiornato all'ultima versione.", $"'{row.Label}' updated to the latest version."),
                Warnings = nested.Notes,
            };
        }

        public async Task<RepoActionResult> AlignAsync(string projectPath, string repo, CancellationToken ct = default)
        {
            var view = await _changes.GetAsync(projectPath, null, ct);
            if (view.Problem != null) return Refuse(view.Problem);
            if (view.NotAGitRepository) return Refuse(UiText.Say("Questa cartella non è un repository git.", "This folder is not a git repository."));

            string only = null;
            if (!string.IsNullOrWhiteSpace(repo))
            {
                var row = Find(view, repo);
                if (row == null || row.Depth == 0) return Refuse(UiText.Say($"'{repo}' non è un submodule di questo progetto.", $"'{repo}' is not a submodule of this project."));
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
            if (view.NotAGitRepository) return Refuse(UiText.Say("Questa cartella non è un repository git.", "This folder is not a git repository."));
            if (view.Repos == null || view.Repos.Count == 0) return Refuse(UiText.Say("Nessun repository da aggiornare.", "No repository to update."));

            var root = view.Repos[0];
            if (root.RemoteProblem != null) return Refuse($"'{root.Label}': {root.RemoteProblem}");

            // Il progetto ha da scaricare: lo scaricamento allinea anche i submodule.
            if (root.Behind > 0) return await PullAsync(projectPath, string.Empty, ct);

            return await AlignAsync(projectPath, null, ct);
        }

        public const string UpstreamRemote = "upstream";

        public async Task<UpstreamStatus> UpstreamStatusAsync(string projectPath, bool fetch, CancellationToken ct = default)
        {
            var (view, _, refused) = await FindAsync(projectPath, null, string.Empty, ct);
            if (refused != null) return new UpstreamStatus();
            var dir = view.RootPath;

            var local = await CurrentBranchAsync(dir, ct);
            var source = await SourceAsync(dir, local, ct);
            if (source == null) return new UpstreamStatus();
            if (source.Problem != null) return source;
            if (local == null)
                return With(source, UiText.Say("Il progetto non è su un ramo: gli aggiornamenti della sorgente si scaricano stando su un ramo.", "The project is not on a branch: updates from the source are pulled while on a branch."));

            if (fetch)
            {
                var fetched = await _transport.FetchAsync(dir, source.Remote, ct);
                if (!fetched.Ok)
                    return With(source, UiText.Say($"La sorgente non risponde: {fetched.Error}", $"The source does not answer: {fetched.Error}"));
            }

            var count = await _runner.RunAsync(dir, new[] { "rev-list", "--count", $"HEAD..{source.Remote}/{source.Branch}" }, ct);
            if (!count.Ok || !int.TryParse(count.Stdout.Trim(), out var behind))
                // Never asked yet, or the source has no branch with this name: nothing to offer, and no alarm.
                return source;

            return new UpstreamStatus { HasUpstream = true, Url = source.Url, Branch = source.Branch, Remote = source.Remote, LocalBranch = local, Behind = behind };
        }

        private static UpstreamStatus With(UpstreamStatus s, string problem)
            => new() { HasUpstream = true, Url = s.Url, Branch = s.Branch, Remote = s.Remote, LocalBranch = s.LocalBranch, Problem = problem };

        /// <summary>
        /// Where the project's updates come from, without asking the network: only the old <c>upstream</c> remote of a project
        /// that does not come from a template. A project with templates (P3), or whose <c>upstream</c> branch now carries a
        /// project template's manifest (an old demo to migrate), takes its updates from the templates' panel instead
        /// (<see cref="MdExplorer.Services.Templates.IProjectTemplateUpdates"/>). Null = no source here.
        /// </summary>
        private async Task<UpstreamStatus> SourceAsync(string dir, string local, CancellationToken ct)
        {
            var yml = Path.Combine(dir, DevelopmentConfigCleanup.FileName);
            try
            {
                if ((File.Exists(yml) ? DevelopmentConfigFile.Config(yml).Templates : null)?.Count > 0) return null;
            }
            catch (InvalidOperationException ex)
            {
                return new UpstreamStatus { HasUpstream = true, Problem = ex.Message };
            }

            var url = await _runner.RunAsync(dir, new[] { "remote", "get-url", UpstreamRemote }, ct);
            if (!url.Ok || string.IsNullOrWhiteSpace(url.Stdout)) return null;
            var manifests = await _runner.RunAsync(dir, new[] { "ls-tree", "-r", "--name-only", $"{UpstreamRemote}/{local}", "--", TemplateManifest.Folder }, ct);
            if (manifests.Ok && (manifests.Stdout ?? "").Split('\n').Any(f => f.Trim().EndsWith("/" + TemplateManifest.FileName, StringComparison.Ordinal)))
                return null;
            return new UpstreamStatus { HasUpstream = true, Url = url.Stdout.Trim(), Remote = UpstreamRemote, Branch = local, LocalBranch = local };
        }

        public async Task<RepoActionResult> PullUpstreamAsync(string projectPath, CancellationToken ct = default)
        {
            var (view, row, refused) = await FindAsync(projectPath, null, string.Empty, ct);
            if (refused != null) return Refuse(refused);
            if (row.MergeInProgress) return Refuse(UiText.Say($"In '{row.Label}' c'è un'unione rimasta a metà: chiudila o annullala prima di scaricare gli aggiornamenti.", $"In '{row.Label}' a merge was left half done: finish or abort it before downloading the updates."));
            var dir = view.RootPath;

            var status = await UpstreamStatusAsync(projectPath, fetch: true, ct);
            if (!status.HasUpstream) return Refuse(UiText.Say("Questo progetto non ha una sorgente da cui scaricare aggiornamenti: non viene da un template e non ha un remoto chiamato 'upstream'.", "This project has no source to download updates from: it does not come from a template and has no remote called 'upstream'."));
            if (status.Problem != null) return Fail(status.Problem);
            if (status.Behind == 0)
                return new RepoActionResult { Success = true, Message = UiText.Say("Il progetto ha già l'ultima versione della sorgente.", "The project already has the latest version of the source.") };

            var before = await HeadsAsync(view, ct);
            var merge = await _runner.RunAsync(dir, new[] { "merge", "--no-edit", $"{status.Remote}/{status.Branch}" }, ct);
            var notes = new List<string>();
            if (!merge.Ok)
            {
                var resolved = await ResolveMdeConflictsAsync(dir, ct);
                if (resolved.Done)
                {
                    merge = resolved.Commit.Value;
                    notes.AddRange(MdeResolutionNotes(resolved));
                }
                else if (resolved.Problem != null)
                {
                    await _runner.RunAsync(dir, new[] { "merge", "--abort" }, ct);
                    return Fail(resolved.Problem);
                }
            }
            if (!merge.Ok)
            {
                // Conflicting with one's own changes is not something to leave half done behind a button
                // that says «scarica»: back to before, and the person is told which files are in the way.
                var conflicts = await _runner.RunAsync(dir, new[] { "diff", "--name-only", "--diff-filter=U" }, ct);
                var merging = await _runner.RunAsync(dir, new[] { "rev-parse", "--quiet", "--verify", "MERGE_HEAD" }, ct);
                if (merging.Ok) await _runner.RunAsync(dir, new[] { "merge", "--abort" }, ct);

                var files = (conflicts.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
                return Fail(files.Count > 0
                    ? UiText.Say(
                        $"Gli aggiornamenti toccano file che hai cambiato anche tu ({string.Join(", ", files)}): non ho scaricato niente e il progetto è com'era. " +
                        "Committa o annulla le tue modifiche a quei file e riprova.",
                        $"The updates touch files you changed too ({string.Join(", ", files)}): I downloaded nothing and the project is as it was. " +
                        "Commit or undo your changes to those files and try again.")
                    : UiText.Say(
                        $"Gli aggiornamenti non sono stati scaricati e il progetto è com'era: {merge.Describe()}",
                        $"The updates were not downloaded and the project is as it was: {merge.Describe()}"));
            }

            _logger.LogInformation("[Flusso] '{Repo}': {Count} aggiornamenti dalla sorgente ({Branch}).", row.Label, status.Behind, status.Branch);

            // origin level with the result: the agents' desks start from origin, not from this folder.
            var warnings = new List<string>(notes);
            var hasOrigin = await _runner.RunAsync(dir, new[] { "remote", "get-url", "origin" }, ct);
            if (hasOrigin.Ok)
            {
                var push = await _transport.PushAsync(dir, "origin", status.LocalBranch, ct);
                if (!push.Ok)
                    warnings.Add(UiText.Say($"Gli aggiornamenti sono nella tua cartella, ma non sono riuscito a portarli su 'origin': {push.Error}. Pubblicali dal pannello «in attesa di push».", $"The updates are in your folder, but I could not push them to 'origin': {push.Error}. Push them from the «To push» panel."));
            }

            return await WithChangesAsync(new RepoActionResult
            {
                Success = true,
                Message = status.Behind == 1
                    ? UiText.Say("Scaricato 1 aggiornamento dalla sorgente.", "Downloaded 1 update from the source.")
                    : UiText.Say($"Scaricati {status.Behind} aggiornamenti dalla sorgente.", $"Downloaded {status.Behind} updates from the source."),
                Warnings = warnings,
            }, view, before, ct);
        }

        /// <summary>La versione locale di .development.yml messa da parte prima del pull.</summary>
        private sealed record ParkedYaml(string BaseText, string Ours, string BackupPath);

        private const string DevYaml = ".development.yml";

        /// <summary>
        /// Prima del pull (06/11/2026, progetto A bloccato da 18 giorni): se l'UNICO file modificato e non salvato è
        /// <c>.development.yml</c>, lo si mette da parte. MdExplorer lo scrive da sé (il segreto della chat creato da
        /// <c>EnsureRoomSecret</c>, «servono commit e push») e git rifiuta il pull appena gli aggiornamenti toccano il file:
        /// «Your local changes … would be overwritten by merge». La correzione del 07/10 (<see cref="ResolveDevelopmentYamlAsync"/>)
        /// non c'entrava: lavora su un'unione già partita, qui non parte nemmeno.
        /// <para>
        /// Prima si prova l'unione a vuoto contro il ramo remoto appena scaricato: un conflitto vero, o dei commenti (che
        /// l'unione voce per voce perderebbe), fermano TUTTO senza toccare niente. Poi la versione locale si salva dentro la
        /// cartella git, il file torna quello dell'ultimo commit, e dopo il pull <see cref="UnparkDevelopmentYamlAsync"/> la
        /// rimette voce per voce sopra quella scaricata.
        /// </para>
        /// </summary>
        private async Task<(ParkedYaml Park, string Refusal)> ParkDevelopmentYamlAsync(string dir, CancellationToken ct)
        {
            var status = await _runner.RunAsync(dir, new[] { "status", "--porcelain=v1", "--untracked-files=no" }, ct);
            if (!status.Ok) return (null, null);
            var changed = (status.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Length > 3 ? GitPaths.Unquote(l.Substring(3)) : string.Empty).ToList();
            if (changed.Count != 1 || changed[0] != DevYaml) return (null, null); // niente, o altri file: il pull va come sempre

            var path = Path.Combine(dir, DevYaml);
            var ours = await File.ReadAllTextAsync(path, ct);
            var baseShow = await _runner.RunAsync(dir, new[] { "show", "HEAD:" + DevYaml }, ct);
            if (!baseShow.Ok) return (null, null); // il file non è nell'ultimo commit: niente da proteggere
            var baseText = baseShow.Stdout;

            if (HasYamlComments(ours) || HasYamlComments(baseText))
                return (null, UiText.Say(
                    $"Hai cambiato {DevYaml} senza salvarlo in un commit, e il file contiene dei commenti: unendolo voce per voce li " +
                    "perderei. Non ho scaricato niente: fai il commit del file, poi scarica.",
                    $"You changed {DevYaml} without committing it, and the file contains comments: merging it entry by entry would " +
                    "lose them. I downloaded nothing: commit the file, then pull."));

            // L'unione a vuoto contro il ramo remoto: se fallirebbe, ci si ferma prima di toccare qualsiasi cosa.
            var fetch = await _runner.RunAsync(dir, new[] { "fetch", "--quiet" }, ct);
            var upstream = await _runner.RunAsync(dir, new[] { "show", "@{u}:" + DevYaml }, ct);
            if (fetch.Ok && upstream.Ok && upstream.Stdout != baseText)
            {
                if (HasYamlComments(upstream.Stdout))
                    return (null, UiText.Say(
                        $"Gli aggiornamenti cambiano {DevYaml}, che hai cambiato anche tu senza commit, e la loro versione contiene " +
                        "dei commenti che unendola voce per voce perderei. Non ho scaricato niente: fai il commit del file, poi scarica.",
                        $"The updates change {DevYaml}, which you changed too without committing, and their version contains comments " +
                        "that an entry-by-entry merge would lose. I downloaded nothing: commit the file, then pull."));
                string trial, conflict;
                try
                {
                    trial = MdExplorer.Features.Git.DevelopmentYamlMerge.Merge(baseText, ours, upstream.Stdout,
                        new[] { MdExplorer.Features.Git.DevelopmentYamlMerge.RoomSecret }, out _, out conflict);
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is YamlDotNet.Core.YamlException)
                {
                    return (null, UiText.Say(
                        $"Hai cambiato {DevYaml} senza commit e non riesco a unirlo con gli aggiornamenti ({ex.Message}). " +
                        "Non ho scaricato niente: fai il commit del file, poi scarica.",
                        $"You changed {DevYaml} without committing and I cannot merge it with the updates ({ex.Message}). " +
                        "I downloaded nothing: commit the file, then pull."));
                }
                if (trial == null)
                    return (null, UiText.Say(
                        $"Gli aggiornamenti cambiano in {DevYaml} la voce «{conflict}», che hai cambiato anche tu in un altro modo " +
                        "(senza commit). Non ho scaricato niente: decidi quale tenere, fai il commit, poi scarica.",
                        $"The updates change the entry «{conflict}» in {DevYaml}, which you changed too in another way (without " +
                        "committing). I downloaded nothing: decide which one to keep, commit, then pull."));
            }

            // La copia di sicurezza, dentro la cartella git: non finisce mai in un commit e non sporca il progetto.
            var gitDir = await _runner.RunAsync(dir, new[] { "rev-parse", "--absolute-git-dir" }, ct);
            if (!gitDir.Ok) return (null, null);
            var backupDir = Path.Combine(gitDir.Stdout.Trim(), "mde");
            Directory.CreateDirectory(backupDir);
            var backup = Path.Combine(backupDir, $"development.yml.{DateTime.Now:yyyyMMdd-HHmmss}.local");
            await File.WriteAllTextAsync(backup, ours, ct);

            var restore = await _runner.RunAsync(dir, new[] { "checkout", "--", DevYaml }, ct);
            if (!restore.Ok)
            {
                await File.WriteAllTextAsync(path, ours, ct);
                return (null, UiText.Say($"Non riesco a mettere da parte {DevYaml}: {restore.Describe()}. Non ho scaricato niente.",
                                         $"I cannot set {DevYaml} aside: {restore.Describe()}. I downloaded nothing."));
            }
            _logger.LogInformation("[Flusso] {File} modificato senza commit messo da parte prima del pull in {Dir} (copia: {Backup})", DevYaml, dir, backup);
            return (new ParkedYaml(baseText, ours, backup), null);
        }

        /// <summary>
        /// Dopo il pull: la versione locale di <c>.development.yml</c> torna sopra quella scaricata, voce per voce, e resta una
        /// modifica non salvata come prima. Pull fallito: torna identica, byte per byte. Se l'unione non riesce (raro: è già
        /// stata provata a vuoto) resta la versione scaricata e si dice dov'è la copia della propria.
        /// </summary>
        /// <param name="fromWorkingTree">L'unione è rimasta a metà (file dell'utente in conflitto): la versione scaricata e già unita da MDE
        /// è quella sul disco, non quella di HEAD, che è ancora la vecchia.</param>
        private async Task<IReadOnlyList<string>> UnparkDevelopmentYamlAsync(string dir, ParkedYaml park, bool pulled, CancellationToken ct, bool fromWorkingTree = false)
        {
            var path = Path.Combine(dir, DevYaml);
            if (!pulled)
            {
                await File.WriteAllTextAsync(path, park.Ours, ct);
                return Array.Empty<string>();
            }

            var now = fromWorkingTree
                ? (File.Exists(path) ? new GitResult { ExitCode = 0, Stdout = await File.ReadAllTextAsync(path, ct), Stderr = "" } : new GitResult { ExitCode = 1, Stdout = "", Stderr = "missing" })
                : await _runner.RunAsync(dir, new[] { "show", "HEAD:" + DevYaml }, ct);
            if (!now.Ok || now.Stdout == park.BaseText)
            {
                // Gli aggiornamenti non hanno toccato il file (o l'hanno tolto): la tua versione torna com'era.
                await File.WriteAllTextAsync(path, park.Ours, ct);
                return Array.Empty<string>();
            }

            string merged = null, conflict = null;
            IReadOnlyList<string> tookSource = Array.Empty<string>();
            try
            {
                if (!HasYamlComments(now.Stdout))
                    merged = MdExplorer.Features.Git.DevelopmentYamlMerge.Merge(park.BaseText, park.Ours, now.Stdout,
                        new[] { MdExplorer.Features.Git.DevelopmentYamlMerge.RoomSecret }, out tookSource, out conflict);
                else
                    conflict = UiText.Say("la versione scaricata contiene commenti", "the downloaded version contains comments");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is YamlDotNet.Core.YamlException)
            {
                conflict = ex.Message;
            }
            if (merged == null)
            {
                _logger.LogWarning("[Flusso] {File} non riunito dopo il pull in {Dir} ({Conflict}): resta lo scaricato, copia in {Backup}", DevYaml, dir, conflict, park.BackupPath);
                return new[] { UiText.Say(
                    $"Le tue modifiche a {DevYaml} non si sono potute riunire a quelle scaricate ({conflict}): " +
                    $"il file ora è quello scaricato, e la tua versione è salvata in {park.BackupPath}.",
                    $"Your changes to {DevYaml} could not be merged with the downloaded ones ({conflict}): " +
                    $"the file is now the downloaded one, and your version is saved in {park.BackupPath}.") };
            }

            await File.WriteAllTextAsync(path, merged, ct);
            _logger.LogInformation("[Flusso] {File} modificato senza commit riunito voce per voce dopo il pull in {Dir}", DevYaml, dir);
            var notes = new List<string> { UiText.Say(
                $"Avevi cambiato {DevYaml} senza commit e gli aggiornamenti lo cambiavano anche loro: li ho uniti voce per voce. " +
                "Le tue modifiche restano da salvare con un commit.",
                $"You had changed {DevYaml} without committing and the updates changed it too: I merged them entry by entry. " +
                "Your changes are still to be committed.") };
            if (tookSource.Contains(MdExplorer.Features.Git.DevelopmentYamlMerge.RoomSecret))
                notes.Add(UiText.Say(
                    $"Questo computer aveva creato un suo segreto della chat, diverso da quello pubblicato: vale quello pubblicato, che i " +
                    $"colleghi usano già. Il tuo è nella copia {park.BackupPath}.",
                    $"This computer had created its own chat secret, different from the published one: the published one, which your " +
                    $"colleagues already use, is kept. Yours is in the copy {park.BackupPath}."));
            return notes;
        }

        /// <summary>
        /// Un commento YAML: <c>#</c> a inizio riga o dopo uno spazio. Approssimato per eccesso (un <c> #</c> dentro una stringa
        /// conta): sbagliare in questo verso ferma il pull con un messaggio, nell'altro cancellerebbe un commento. La riga
        /// <c># mde-schema: N</c> non conta: la scrive MDE (schema di .development.yml, F3) e l'unione voce per voce la rimette.
        /// </summary>
        internal static bool HasYamlComments(string yaml)
            => !string.IsNullOrEmpty(yaml) && System.Text.RegularExpressions.Regex.IsMatch(
                   SchemaHeader.Replace(yaml, string.Empty), @"(^|\s)#", System.Text.RegularExpressions.RegexOptions.Multiline);

        private static readonly System.Text.RegularExpressions.Regex SchemaHeader =
            new(@"^#\s*mde-schema:\s*\d+\s*$", System.Text.RegularExpressions.RegexOptions.Multiline);

        /// <summary>Esito di <see cref="ResolveMdeConflictsAsync"/>.</summary>
        private sealed record MdeConflictResolution(bool Done, GitResult? Commit, string Problem, IReadOnlyList<string> TookSource,
            IReadOnlyList<string> Resolved, IReadOnlyList<string> Remaining)
        {
            public static readonly MdeConflictResolution None = new(false, default, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        }

        /// <summary>
        /// S5 dello sprint «File di sistema di MDE»: un'unione ferma sui file di MDE (<see cref="MdeOwnedFiles"/>) la risolve MDE, con la
        /// regola di ciascuno. MDE li scrive da sé a ogni macchina, e git si ferma su due aggiunte vicine anche quando niente si contraddice.
        /// <list type="bullet">
        /// <item><c>.development.yml</c>: voce per voce (<see cref="MdExplorer.Features.Git.DevelopmentYamlMerge"/>); il segreto della chat
        /// già pubblicato vince; la stessa voce cambiata in due modi è un conflitto vero → <c>Problem</c> con il nome della voce.</item>
        /// <item>file di istruzioni: fuori dal blocco di MDE l'unione normale di git; dentro, il blocco con la versione più alta (a parità
        /// quello di qui). Righe del gruppo cambiate in due modi: il file resta in conflitto per l'utente (non è di MDE).</item>
        /// </list>
        /// Se restano in conflitto solo file di MDE risolti, l'unione si chiude con il commit; altrimenti i file di MDE sono risolti e
        /// aggiunti, e gli altri restano all'utente (<c>Remaining</c>).
        /// </summary>
        private async Task<MdeConflictResolution> ResolveMdeConflictsAsync(string dir, CancellationToken ct)
        {
            var conflicts = await _runner.RunAsync(dir, new[] { "diff", "--name-only", "--diff-filter=U" }, ct);
            var files = (conflicts.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
            var mde = files.Where(f => MdeOwnedFiles.Find(f) != null).ToList();
            if (mde.Count == 0) return MdeConflictResolution.None;

            var merged = new Dictionary<string, string>();
            var tookSource = new List<string>();
            var remaining = files.Except(mde).ToList();
            foreach (var file in mde)
            {
                var baseText = await _runner.RunAsync(dir, new[] { "show", ":1:" + file }, ct);
                var ours = await _runner.RunAsync(dir, new[] { "show", ":2:" + file }, ct);
                var theirs = await _runner.RunAsync(dir, new[] { "show", ":3:" + file }, ct);
                if (!ours.Ok || !theirs.Ok) { remaining.Add(file); continue; } // cancellato da un lato: lo decide l'utente

                if (MdeOwnedFiles.Find(file).Kind == MdeMergeKind.YamlEntries)
                {
                    string text, conflict;
                    IReadOnlyList<string> took;
                    // Il segreto della stanza creato anche qui non è un conflitto: vale quello pubblicato, che i colleghi usano già.
                    try { text = MdExplorer.Features.Git.DevelopmentYamlMerge.Merge(baseText.Ok ? baseText.Stdout : null, ours.Stdout, theirs.Stdout,
                              new[] { MdExplorer.Features.Git.DevelopmentYamlMerge.RoomSecret }, out took, out conflict); }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is YamlDotNet.Core.YamlException)
                    {
                        return MdeConflictResolution.None with { Problem = UiText.Say(
                            $"Gli aggiornamenti toccano {file}, che è cambiato anche qui, e non l'ho potuto unire voce per voce ({ex.Message}): " +
                            "non ho scaricato niente e il progetto è com'era.",
                            $"The updates touch {file}, which changed here too, and I could not merge it entry by entry ({ex.Message}): " +
                            "I downloaded nothing and the project is as it was.") };
                    }
                    if (text == null)
                        return MdeConflictResolution.None with { Problem = UiText.Say(
                            $"Gli aggiornamenti cambiano in {file} la voce «{conflict}», che è cambiata anche qui in un altro modo: " +
                            "non ho scaricato niente e il progetto è com'era. Decidi tu quale tenere, fai il commit e riprova.",
                            $"The updates change the entry «{conflict}» in {file}, which changed here too in another way: " +
                            "I downloaded nothing and the project is as it was. Decide which one to keep, commit, and try again.") };
                    merged[file] = text;
                    tookSource.AddRange(took);
                }
                else if (MdeOwnedFiles.Find(file).Kind == MdeMergeKind.Versioned)
                {
                    // Una skill, un agente o un prompt mde-*: vince la versione più alta, a parità quella di qui (09/11/2026). Un file
                    // di accompagnamento senza blocco mde: segue la versione del SKILL.md della sua cartella.
                    var mine = await MdeVersionAsync(dir, "HEAD", file, ours.Stdout, ct);
                    var source = await MdeVersionAsync(dir, "MERGE_HEAD", file, theirs.Stdout, ct);
                    merged[file] = source > mine ? theirs.Stdout : ours.Stdout;
                }
                else if (MdeOwnedFiles.Find(file).Kind == MdeMergeKind.Copy)
                {
                    // La copia di una skill di un template: la si rifà dalla zona del template a ogni apertura; qui vale quella pubblicata.
                    merged[file] = theirs.Stdout;
                }
                else
                {
                    var layout = HarnessLayout.All.FirstOrDefault(l => string.Equals(l.InstructionsFile, file, StringComparison.OrdinalIgnoreCase));
                    var text = await MergeBlockFileAsync(dir, layout?.SkillRelativePath("mde-doc"), baseText.Ok ? baseText.Stdout : string.Empty, ours.Stdout, theirs.Stdout, ct);
                    if (text == null) { remaining.Add(file); continue; }
                    merged[file] = text;
                }
            }

            foreach (var (file, text) in merged)
                await File.WriteAllTextAsync(Path.Combine(dir, file.Replace('/', Path.DirectorySeparatorChar)), text, ct);
            if (merged.Count > 0)
            {
                var add = await _runner.RunAsync(dir, new[] { "add", "--" }.Concat(merged.Keys).ToArray(), ct);
                if (!add.Ok) return MdeConflictResolution.None with { Problem = UiText.Say($"{string.Join(", ", merged.Keys)} uniti, ma non riesco ad aggiungerli all'unione: {add.Describe()}", $"{string.Join(", ", merged.Keys)} merged, but I cannot add them to the merge: {add.Describe()}") };
            }
            var resolved = merged.Keys.ToList();
            if (remaining.Count > 0) return new MdeConflictResolution(false, default, null, tookSource, resolved, remaining);

            var commit = await _runner.RunAsync(dir, new[] { "commit", "--no-edit" }, ct);
            if (!commit.Ok) return MdeConflictResolution.None with { Problem = UiText.Say($"{string.Join(", ", resolved)} uniti, ma l'unione non si chiude: {commit.Describe()}", $"{string.Join(", ", resolved)} merged, but the merge does not close: {commit.Describe()}") };
            _logger.LogInformation("[Flusso] file di MDE uniti da MDE in {Dir}: {Files}", dir, string.Join(", ", resolved));
            return new MdeConflictResolution(true, commit, null, tookSource, resolved, Array.Empty<string>());
        }

        /// <summary>
        /// La <c>mde: version</c> di un file <c>mde-*</c> su un lato dell'unione (<paramref name="side"/>: HEAD o MERGE_HEAD). Se il file
        /// non ha il blocco (un file di accompagnamento di una skill), quella del <c>SKILL.md</c> della sua cartella su quel lato. 0 se
        /// non si trova.
        /// </summary>
        private async Task<int> MdeVersionAsync(string dir, string side, string file, string text, CancellationToken ct)
        {
            var own = MdeSkillUpdater.ExtractMdeMarker(text ?? string.Empty).Version;
            if (own != null) return own.Value;
            var slash = file.LastIndexOf('/');
            if (slash < 0) return 0;
            var skill = await _runner.RunAsync(dir, new[] { "show", $"{side}:{file.Substring(0, slash)}/SKILL.md" }, ct);
            return skill.Ok ? MdeSkillUpdater.ExtractMdeMarker(skill.Stdout).Version ?? 0 : 0;
        }

        /// <summary>
        /// Il file di istruzioni unito: il blocco di MDE si toglie dai tre lati (al suo posto una riga segnaposto), il resto lo unisce git
        /// (<c>git merge-file</c>), poi al segnaposto va il blocco con la versione più alta, a parità quello di qui. Null = le righe del
        /// gruppo sono cambiate in due modi: decide l'utente.
        /// </summary>
        private async Task<string> MergeBlockFileAsync(string dir, string skillPath, string baseText, string ours, string theirs, CancellationToken ct)
        {
            const string Placeholder = "<!-- mde:skill (posto del blocco di MDE durante l'unione) -->";
            // Il paragrafo delle versioni precedenti si migra al blocco su ogni lato, base compresa: altrimenti il paragrafo vecchio
            // nella base e il blocco nei due lati sembrano a git due cambi della stessa zona, e una riga del gruppo accanto va in conflitto.
            string Lf(string t)
            {
                var text = (t ?? string.Empty).Replace("\r\n", "\n");
                if (skillPath == null) return text;
                try { return MdeInstructionsBlock.Apply(text, skillPath); } catch (InvalidOperationException) { return text; }
            }
            static (string Text, string Block, int Version) Strip(string text)
            {
                var m = MdeInstructionsBlock.FindBlock(text);
                return m.Success
                    ? (text.Substring(0, m.Index) + Placeholder + text.Substring(m.Index + m.Length), m.Value, MdeInstructionsBlock.VersionOf(m))
                    : (text, null, -1);
            }
            var b = Strip(Lf(baseText)); var o = Strip(Lf(ours)); var t = Strip(Lf(theirs));

            var gitDir = (await _runner.RunAsync(dir, new[] { "rev-parse", "--absolute-git-dir" }, ct)).Stdout?.Trim();
            var work = Path.Combine(string.IsNullOrEmpty(gitDir) ? Path.GetTempPath() : gitDir, "mde", "merge-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(work);
            try
            {
                string Put(string name, string text) { var f = Path.Combine(work, name); File.WriteAllText(f, text); return f; }
                var of = Put("ours", o.Text); var bf = Put("base", b.Text); var tf = Put("theirs", t.Text);
                var result = await _runner.RunAsync(dir, new[] { "merge-file", "-p", of, bf, tf }, ct);
                if (result.ExitCode != 0) return null; // >0 conflitti sulle righe del gruppo, <0 errore
                var text = result.Stdout ?? string.Empty;
                var winner = o.Block == null ? t.Block : t.Block == null ? o.Block : t.Version > o.Version ? t.Block : o.Block;
                if (text.Contains(Placeholder)) text = text.Replace(Placeholder, winner ?? string.Empty);
                return text;
            }
            finally { try { Directory.Delete(work, recursive: true); } catch { /* è dentro .git/mde, innocuo */ } }
        }

        /// <summary>Che cosa ha unito MDE da solo, da dire all'utente.</summary>
        private static IEnumerable<string> MdeResolutionNotes(MdeConflictResolution resolved)
        {
            if (resolved.Resolved.Count == 0) yield break;
            var files = string.Join(", ", resolved.Resolved);
            yield return UiText.Say(
                $"Gli aggiornamenti e questo computer avevano cambiato tutti e due {files}, file di MDE: li ho uniti io " +
                "(.development.yml voce per voce, tenendo le tue voci e aggiungendo quelle nuove; nelle istruzioni il blocco di MDE più recente, il resto com'era).",
                $"The updates and this computer had both changed {files}, MDE files: I merged them myself " +
                "(.development.yml entry by entry, keeping your entries and adding the new ones; in the instructions the most recent MDE block, the rest as it was).");
            if (resolved.TookSource.Contains(MdExplorer.Features.Git.DevelopmentYamlMerge.RoomSecret))
                yield return UiText.Say(
                    "Questo computer aveva creato un suo segreto della chat, diverso da quello pubblicato: vale quello pubblicato, " +
                    "che i colleghi usano già, e il tuo è stato scartato.",
                    "This computer had created its own chat secret, different from the published one: the published one, which " +
                    "your colleagues already use, is kept and yours was discarded.");
        }

        /// <summary>The branch HEAD is on; null when detached.</summary>
        private async Task<string> CurrentBranchAsync(string dir, CancellationToken ct)
        {
            var head = await _runner.RunAsync(dir, new[] { "symbolic-ref", "--quiet", "--short", "HEAD" }, ct);
            return head.Ok && !string.IsNullOrWhiteSpace(head.Stdout) ? head.Stdout.Trim() : null;
        }

        public async Task<RepoActionResult> AbortMergeAsync(string projectPath, string repo, CancellationToken ct = default)
        {
            var (view, row, refused) = await FindAsync(projectPath, null, repo, ct);
            if (refused != null) return Refuse(refused);
            if (!row.MergeInProgress) return Refuse(UiText.Say($"In '{row.Label}' non c'è nessuna unione in corso.", $"In '{row.Label}' there is no merge in progress."));

            var dir = DirOf(view.RootPath, row.Path);
            var abort = await _runner.RunAsync(dir, new[] { "merge", "--abort" }, ct);
            if (!abort.Ok) return Fail(UiText.Say($"L'unione in '{row.Label}' non è stata annullata: {abort.Describe()}", $"The merge in '{row.Label}' was not aborted: {abort.Describe()}"));

            _logger.LogInformation("[Flusso] unione annullata in '{Repo}'.", row.Label);
            return new RepoActionResult
            {
                Success = true,
                Message = UiText.Say($"Unione annullata: '{row.Label}' è tornato a prima dello scaricamento.", $"Merge aborted: '{row.Label}' is back to before the pull."),
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
            if (view.NotAGitRepository) return (view, null, UiText.Say("Questa cartella non è un repository git.", "This folder is not a git repository."));

            var row = Find(view, repo);
            return row == null
                ? (view, null, UiText.Say($"'{repo}' non è un repository di questo progetto.", $"'{repo}' is not a repository of this project."))
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
                    .Select(x => Prefixed(repo.Path, GitPaths.Unquote(x))));
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
                    ? UiText.Say("I submodule erano già alla versione che il progetto registra.", "The submodules were already at the version the project records.")
                    : done == 1 ? UiText.Say("1 submodule allineato alla versione che il progetto registra.", "1 submodule aligned to the version the project records.")
                    : UiText.Say($"{done} submodule allineati alla versione che il progetto registra.", $"{done} submodules aligned to the version the project records."),
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
                return UiText.Say(
                    $"In '{label}' il tuo lavoro e quello scaricato toccano le stesse righe: l'unione è rimasta a metà. " +
                    "Correggi i file in conflitto e fai il commit, oppure annulla l'unione per tornare a prima.",
                    $"In '{label}' your work and the pulled work touch the same lines: the merge was left half done. " +
                    "Fix the conflicting files and commit, or abort the merge to go back.");
            return UiText.Say($"'{label}' non è stato aggiornato e niente è cambiato: {error}", $"'{label}' was not updated and nothing changed: {error}");
        }

        private static RepoActionResult Refuse(string why) => new RepoActionResult { Success = false, Refused = why };
        private static RepoActionResult Fail(string why) => new RepoActionResult { Success = false, Message = why };
    }
}
