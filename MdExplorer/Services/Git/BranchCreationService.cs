using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Com'è il repository prima di creare un ramo: quello che la finestra «Nuovo ramo» mostra.</summary>
    public sealed class NewBranchPreview
    {
        /// <summary>Il ramo da cui si parte; null con HEAD staccato.</summary>
        public string CurrentBranch { get; init; }
        public string HeadShort { get; init; }
        /// <summary>I commit del ramo attuale non ancora su GitHub, che il ramo nuovo si porta dietro.</summary>
        public int UnpublishedCommits { get; init; }
        /// <summary>I file non registrati (git status), che vengono con te nel ramo nuovo.</summary>
        public IReadOnlyList<string> CarriedFiles { get; init; } = Array.Empty<string>();
        public bool HasOrigin { get; init; }
        public bool HasSubmodules { get; init; }
        /// <summary>Perché adesso non si può creare un ramo (unione a metà…); null se si può.</summary>
        public string Blocker { get; init; }
        /// <summary>Il nome suggerito: la data di oggi, come vogliono i nomi dei rami del gruppo.</summary>
        public string SuggestedPrefix { get; init; }
    }

    /// <summary>Com'è andata la creazione.</summary>
    public sealed class NewBranchResult
    {
        public bool Created { get; init; }
        public string Branch { get; init; }
        /// <summary>Pubblicato su GitHub con l'upstream impostato.</summary>
        public bool Published { get; init; }
        /// <summary>Perché non è pubblicato (nessun remoto, push rifiutato…); il ramo c'è lo stesso in locale.</summary>
        public string PublishProblem { get; init; }
        /// <summary>Perché non è stato creato; null se è stato creato.</summary>
        public string Error { get; init; }
        /// <summary>Cose da sapere (HEAD staccato, submodule lasciati dove sono, GitHub non controllato…).</summary>
        public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> CarriedFiles { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// «Nuovo ramo» dalla title bar (sprint 2026-11-25-Nuovo-Ramo-E-Lavoro-All-Agente, A1). Il ramo nasce dal punto in cui si
    /// è (U2), con <c>git switch -c</c>, che non perde mai il lavoro aperto: modificati, in coda e nuovi vengono con te così come
    /// sono (U3, verificato in F0-P1). Poi si pubblica subito con <c>push -u</c> (U4); se il push non riesce il ramo resta in
    /// locale e lo si dice. Prima di creare: nome valido e libero in locale E su GitHub (F0-P2), nessuna unione, rebase o
    /// cherry-pick a metà; HEAD staccato e submodule si dicono.
    /// </summary>
    public sealed class BranchCreationService
    {
        private readonly INativeGitRunner _git;
        private readonly INativeGitTransport _transport;
        private readonly IRepoWorkflowGuard _guard;
        private readonly ILogger<BranchCreationService> _logger;

        public BranchCreationService(INativeGitRunner git, INativeGitTransport transport, IRepoWorkflowGuard guard, ILogger<BranchCreationService> logger)
        {
            _git = git;
            _transport = transport;
            _guard = guard;
            _logger = logger;
        }

        public async Task<NewBranchPreview> PreviewAsync(string repositoryPath, CancellationToken ct = default)
        {
            Require(repositoryPath);
            var branch = await _git.RunAsync(repositoryPath, new[] { "symbolic-ref", "--quiet", "--short", "HEAD" }, ct);
            var head = await _git.RunAsync(repositoryPath, new[] { "rev-parse", "--short", "HEAD" }, ct);
            var origin = await _git.RunAsync(repositoryPath, new[] { "remote", "get-url", "origin" }, ct);
            var unpublished = 0;
            if (branch.Ok)
            {
                var ahead = await _git.RunAsync(repositoryPath, new[] { "rev-list", "--count", "@{upstream}..HEAD" }, ct);
                if (ahead.Ok) int.TryParse(ahead.Stdout.Trim(), out unpublished);
                else if (origin.Ok)
                {
                    // Un ramo che non segue niente: tutto quello che non è su nessun ramo di origin.
                    var notOnOrigin = await _git.RunAsync(repositoryPath, new[] { "rev-list", "--count", "HEAD", "--not", "--remotes=origin" }, ct);
                    if (notOnOrigin.Ok) int.TryParse(notOnOrigin.Stdout.Trim(), out unpublished);
                }
            }
            return new NewBranchPreview
            {
                CurrentBranch = branch.Ok ? branch.Stdout.Trim() : null,
                HeadShort = head.Ok ? head.Stdout.Trim() : null,
                UnpublishedCommits = unpublished,
                CarriedFiles = await StatusFilesAsync(repositoryPath, ct),
                HasOrigin = origin.Ok,
                HasSubmodules = File.Exists(Path.Combine(repositoryPath, ".gitmodules")),
                Blocker = await BlockerAsync(repositoryPath, ct),
                SuggestedPrefix = DateTime.Now.ToString("yyyy-MM-dd") + "-",
            };
        }

        public async Task<NewBranchResult> CreateAsync(string repositoryPath, string name, CancellationToken ct = default)
        {
            Require(repositoryPath);
            name = (name ?? string.Empty).Trim();
            var blocker = await BlockerAsync(repositoryPath, ct);
            if (blocker != null) return Refused(blocker);

            var nameProblem = await NameProblemAsync(repositoryPath, name, ct);
            if (nameProblem != null) return Refused(nameProblem);

            var notes = new List<string>();
            var hasOrigin = (await _git.RunAsync(repositoryPath, new[] { "remote", "get-url", "origin" }, ct)).Ok;
            if (hasOrigin)
            {
                // Un ramo con quel nome solo su GitHub: in locale switch -c passerebbe, e poi il push lo mescolerebbe (F0-P2).
                var remote = await _transport.LsRemoteAsync(repositoryPath, "origin", ct);
                if (!remote.Ok)
                    notes.Add(UiText.Say($"Non ho potuto controllare su GitHub che il nome sia libero ({remote.Error}).",
                        $"I could not check on GitHub that the name is free ({remote.Error})."));
                else if (remote.Stdout.Split('\n').Any(l => l.TrimEnd().EndsWith("\trefs/heads/" + name, StringComparison.Ordinal)))
                    return Refused(UiText.Say($"Su GitHub c'è già un ramo «{name}»: per lavorarci usa il cambio ramo, oppure scegli un altro nome.",
                        $"GitHub already has a branch «{name}»: to work on it switch to it, or choose another name."));
            }

            var detached = !(await _git.RunAsync(repositoryPath, new[] { "symbolic-ref", "--quiet", "HEAD" }, ct)).Ok;
            if (detached)
            {
                var head = await _git.RunAsync(repositoryPath, new[] { "rev-parse", "--short", "HEAD" }, ct);
                notes.Add(UiText.Say($"Il ramo parte dal commit {head.Stdout.Trim()}, non da un ramo (HEAD era staccato).",
                    $"The branch starts from commit {head.Stdout.Trim()}, not from a branch (HEAD was detached)."));
            }
            if (File.Exists(Path.Combine(repositoryPath, ".gitmodules")))
                notes.Add(UiText.Say("Il ramo nasce solo in questo repository: i submodule restano sui loro rami.",
                    "The branch is created in this repository only: the submodules stay on their branches."));

            var carried = await StatusFilesAsync(repositoryPath, ct);
            var switched = await _git.RunAsync(repositoryPath, new[] { "switch", "-c", name }, ct);
            if (!switched.Ok) return Refused(UiText.Say("Il ramo non è stato creato: ", "The branch was not created: ") + switched.Describe());
            _logger.LogInformation("[NuovoRamo] {Repo}: creato {Branch}, {Files} file portati", repositoryPath, name, carried.Count);

            if (!hasOrigin)
                return new NewBranchResult
                {
                    Created = true, Branch = name, Notes = notes, CarriedFiles = carried,
                    PublishProblem = UiText.Say("Il progetto non ha un remoto «origin»: il ramo resta su questo computer.",
                        "The project has no «origin» remote: the branch stays on this computer."),
                };

            var push = await _transport.PushAsync(repositoryPath, "origin", name, ct);
            if (!push.Ok) _logger.LogWarning("[NuovoRamo] {Repo}: {Branch} creato ma non pubblicato: {Error}", repositoryPath, name, push.Error);
            return new NewBranchResult
            {
                Created = true, Branch = name, Notes = notes, CarriedFiles = carried, Published = push.Ok,
                PublishProblem = push.Ok ? null : UiText.Say($"Il ramo è creato e ci sei sopra, ma non è pubblicato: {push.Error}",
                    $"The branch is created and you are on it, but it is not published: {push.Error}"),
            };
        }

        /// <summary>Pubblica un ramo creato quando il push non era riuscito (il pulsante «Pubblica» della finestra).</summary>
        public async Task<NewBranchResult> PublishAsync(string repositoryPath, string name, CancellationToken ct = default)
        {
            Require(repositoryPath);
            var push = await _transport.PushAsync(repositoryPath, "origin", name, ct);
            return new NewBranchResult
            {
                Created = true, Branch = name, Published = push.Ok,
                PublishProblem = push.Ok ? null : UiText.Say($"Non pubblicato: {push.Error}", $"Not published: {push.Error}"),
            };
        }

        /// <summary>Perché adesso non si crea un ramo: un'unione, un rebase o un cherry-pick a metà.</summary>
        private async Task<string> BlockerAsync(string repositoryPath, CancellationToken ct)
        {
            if ((await _guard.ReadMergeAsync(repositoryPath, ct)).InProgress)
                return UiText.Say("C'è un'unione rimasta a metà: va conclusa con un commit, oppure annullata, prima di creare un ramo.",
                    "A merge was left half done: finish it with a commit, or abort it, before creating a branch.");
            foreach (var (path, it, en) in new[]
            {
                ("rebase-merge", "un rebase", "a rebase"), ("rebase-apply", "un rebase", "a rebase"),
                ("CHERRY_PICK_HEAD", "un cherry-pick", "a cherry-pick"), ("REVERT_HEAD", "un revert", "a revert"),
            })
            {
                var where = await _git.RunAsync(repositoryPath, new[] { "rev-parse", "--git-path", path }, ct);
                if (!where.Ok) continue;
                var full = Path.IsPathRooted(where.Stdout.Trim()) ? where.Stdout.Trim() : Path.Combine(repositoryPath, where.Stdout.Trim());
                if (File.Exists(full) || Directory.Exists(full))
                    return UiText.Say($"C'è {it} rimasto a metà: va concluso o annullato prima di creare un ramo.",
                        $"There is {en} left half done: finish or abort it before creating a branch.");
            }
            return null;
        }

        private async Task<string> NameProblemAsync(string repositoryPath, string name, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(name)) return UiText.Say("Manca il nome del ramo.", "The branch name is missing.");
            var format = await _git.RunAsync(repositoryPath, new[] { "check-ref-format", "--branch", name }, ct);
            if (!format.Ok)
                return UiText.Say($"«{name}» non è un nome di ramo valido per git (niente spazi, «..», punto finale o trattino iniziale).",
                    $"«{name}» is not a valid git branch name (no spaces, «..», trailing dot or leading hyphen).");
            if ((await _git.RunAsync(repositoryPath, new[] { "show-ref", "--verify", "--quiet", "refs/heads/" + name }, ct)).Ok)
                return UiText.Say($"Il ramo «{name}» esiste già: per lavorarci usa il cambio ramo, oppure scegli un altro nome.",
                    $"The branch «{name}» already exists: to work on it switch to it, or choose another name.");
            return null;
        }

        /// <summary>I file che git status elenca (con il nuovo nome per i rinominati): quelli che vengono nel ramo nuovo.</summary>
        private async Task<IReadOnlyList<string>> StatusFilesAsync(string repositoryPath, CancellationToken ct)
        {
            var status = await _git.RunAsync(repositoryPath, new[] { "status", "--porcelain", "-z", "--untracked-files=all" }, ct);
            if (!status.Ok) return Array.Empty<string>();
            var entries = status.Stdout.Split('\0');
            var files = new List<string>();
            for (var i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e.Length < 4) continue;
                files.Add(e.Substring(3));
                if (e[0] == 'R' || e[0] == 'C') i++;   // il nome di prima segue: non è un file in più
            }
            return files;
        }

        private static NewBranchResult Refused(string why) => new() { Created = false, Error = why };

        private static void Require(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath) || !Directory.Exists(repositoryPath))
                throw new ArgumentException(UiText.Say("La cartella del repository non c'è.", "The repository folder does not exist."), nameof(repositoryPath));
        }
    }
}
